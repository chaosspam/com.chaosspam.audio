using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    public AudioManager manager { get; private set; }
    public AudioSource oneShotSource { get; private set; }
    private SoundPlayer[] soundList;
    private Dictionary<string, SoundPlayer> soundDictionary;

    private float targetSoundPitch;
    private float soundPitchChangeSpeed;

    private Coroutine changeSoundPitch;

    public SoundManager(AudioManager audioManager)
    {
        if (audioManager == null) return;
        if (audioManager.database == null) return;
        manager = audioManager;
        soundDictionary = new Dictionary<string, SoundPlayer>();
        soundList = new SoundPlayer[audioManager.database.soundList.Length];
        SetupSound(audioManager.database);
    }

    private void SetupSound(AudioDatabase database)
    {
        GameObject auxObject = manager.gameObject;
        oneShotSource = auxObject.AddComponent<AudioSource>();

        for (int i = 0; i < database.soundList.Length; i++)
        {
            Sound s = database.soundList[i];

            SoundPlayer soundPlayer = new SoundPlayer(this, s);
            soundList[i] = soundPlayer;
            soundDictionary.Add(s.name, soundPlayer);
        }
    }

    public void ClearPlayedThisFrame()
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].playedThisFrame = false;
        }
    }

    public void Play(string name, float? pitch, bool bypassRepeat = false)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!TryGetSound(name, out SoundPlayer sound))
        {
            Debug.LogErrorFormat("Can not find sound \"{0}\"", name);
            return;
        }
        sound.Play(pitch, bypassRepeat);
    }

    public void Play(string name)
    {
        Play(name, null);
    }

    private bool TryGetSound(string name, out SoundPlayer sound)
    {
        sound = null;
        if (!soundDictionary.ContainsKey(name)) return false;
        sound = soundDictionary[name];
        return true;
    }

    public void ChangeSoundPitch(float pitch, float length)
    {
        if (changeSoundPitch != null)
        {
            manager.StopCoroutine(changeSoundPitch);
        }
        changeSoundPitch = manager.StartCoroutine(LerpSoundPitch(pitch, length));
    }

    private IEnumerator LerpSoundPitch(float pitch, float length)
    {
        float curPitch;
        manager.soundMixer.audioMixer.GetFloat("SoundPitch", out curPitch);
        float start = curPitch;

        float time = 0f;
        while (curPitch != pitch)
        {
            manager.soundMixer.audioMixer.SetFloat("SoundPitch", Mathf.Lerp(start, pitch, time));
            manager.soundMixer.audioMixer.GetFloat("SoundPitch", out curPitch);
            time += (1 / length) * Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
