using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioMixerGroup output;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(-3f, 3f)]
    public float basePitch = 1f;
    public float pitchFluctuation = 0f;

    public bool loop;
    public bool oneShot;
    public bool playOnPause;
}

public class SoundPlayer
{
    private Sound sound;
    private AudioSource source;
    public bool playedThisFrame;

    private float randomPitch
    {
        get => sound.pitchFluctuation == 0 
            ? sound.basePitch 
            : Random.Range(sound.basePitch - sound.pitchFluctuation, sound.basePitch + sound.pitchFluctuation);
    }

    public SoundPlayer(SoundManager soundManager, Sound sound)
    {
        this.sound = sound;

        if (sound.oneShot && sound.pitchFluctuation == 0f)
        {
            source = soundManager.oneShotSource;
        }
        else
        {
            source = soundManager.manager.gameObject.AddComponent<AudioSource>();
            source.loop = sound.loop;
        }

        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.basePitch;
        source.outputAudioMixerGroup = sound.output;
    }

    public void Play(float? pitch, bool bypassRepeat)
    {
        if (source == null) return;
        if (Time.timeScale == 0f && !sound.playOnPause) return;

        float pitchToPlay = pitch ?? randomPitch;

        // Prevent multiple sounds from playing on the same frame
        if (playedThisFrame && !bypassRepeat) return;
        playedThisFrame = true;

        if (sound.oneShot)
        {
            if (sound.pitchFluctuation != 0f)
            {
                source.pitch = pitchToPlay;
            }
            source.outputAudioMixerGroup = sound.output;
            source.PlayOneShot(sound.clip);
        }
        else
        {
            if (source.timeSamples == 0 || source.timeSamples >= sound.clip.samples)
            {
                source.pitch = pitchToPlay;
                source.Play();
            }
        }
    }
}
