using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager
{
    public AudioManager manager { get; private set; }

    public bool Playing { get { return musicSource.isPlaying; } }
    public float PlaybackTime { get { return musicSource.timeSamples * MusicSampleRate; } }
    public float MusicLength { get; private set; }
    public float MusicSampleRate { get; private set; }
    public Music CurrentMusic { get { return new Music(currentMusic); } }
    public string CurrentMusicName { get { return currentMusic.name; } }

    private Music[] musicList;
    private AudioSource musicSource;
    private Music currentMusic;

    private bool musicTransitioning;
    private Queue<MusicTransitionRequest> musicRequestQueue;
    private MusicTransitionRequest currentMusicRequest;

    private Coroutine changeMusicPitch;
    private Coroutine changeVolume;

    public MusicManager(AudioManager audioManager)
    {
        if (audioManager == null) return;
        if (audioManager.database == null) return;
        manager = audioManager;
        musicList = audioManager.database.musicList;
        SetupMusic();
    }

    private void SetupMusic()
    {
        // Create AudioSource for music playback
        musicSource = manager.gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.outputAudioMixerGroup = manager.musicMixer;
        // Create queue
        musicRequestQueue = new Queue<MusicTransitionRequest>();
        musicTransitioning = false;
    }

    /// <summary>
    /// Directly play a music without transition.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fromLastTimestamp"></param>
    public void Play(string name, bool fromLastTimestamp)
    {
        if (!TryGetMusic(name, out Music music)) return;

        // Create a dummy request
        var request = new MusicTransitionRequest
        (
            name,
            fromLastTimestamp,
            0f,
            false,
            0,
            0
        );
        currentMusicRequest = request;

        // Only play if current music is different
        if (currentMusic != music)
        {
            SaveTimestamp();
            SetMusic(music);
            ResetVolume();
            if (fromLastTimestamp)
            {
                musicSource.time = music.LastTimestamp;
            }
            else
            {
                musicSource.time = 0f;
            }
            musicSource.Play();
        }
    }

    public void Transition(MusicTransitionRequest request)
    {
        musicRequestQueue.Enqueue(request);
        TryTransitionNext();
    }

    private void TryTransitionNext()
    {
        if (!musicTransitioning && musicRequestQueue.Count > 0)
        {
            // struct, default priority is 0
            int lastPriority = currentMusicRequest.playPriority;
            currentMusicRequest = musicRequestQueue.Dequeue();
            // If next request has a lower priority, skip to next request
            if (currentMusicRequest.startPriority < lastPriority)
            {
                TryTransitionNext();
                return;
            }

            // Otherwise, if we are not playing music or if this is a different music, transition
            if (currentMusic == null || currentMusic.name != currentMusicRequest.name)
            {
                manager.StartCoroutine(HandleTransitionMusic(currentMusicRequest));
            }
            else
            {
                TryTransitionNext();
            }
        }
    }

    private IEnumerator HandleTransitionMusic(MusicTransitionRequest request)
    {
        musicTransitioning = true;

        // If request is empty, transition to silence
        if (string.IsNullOrEmpty(request.name))
        {
            yield return LerpVolume(0f, request.length);
            SaveTimestamp();
            currentMusic = null;
            musicSource.clip = null;
        }
        else if (TryGetMusic(request.name, out Music music))
        {
            // If next music doesn't start from max, use full length to transition
            if (currentMusic != null)
            {
                if (request.startFromMax)
                {
                    yield return LerpVolume(0f, request.length);
                }
                else
                {
                    yield return LerpVolume(0f, request.length / 2);
                }
            }

            SaveTimestamp();
            SetMusic(music);
            musicSource.time = request.fromLastTimestamp ? music.LastTimestamp : 0f;
            musicSource.Play();

            if (request.startFromMax)
            {
                musicSource.volume = 1f;
            }
            else
            {
                yield return LerpVolume(musicSource.volume, request.length / 2);
            }
        }

        musicTransitioning = false;
        TryTransitionNext();
    }

    public void SchedulePlayMusic(float seconds, string name)
    {
        if (TryGetMusic(name, out Music music))
        {
            musicSource.Stop();
            if (currentMusic != music)
            {
                SetMusic(music);
                musicSource.PlayScheduled(AudioSettings.dspTime + seconds);
            }
        }
    }

    private void SetMusic(Music music)
    {
        currentMusic = music;
        musicSource.clip = music.clip;
        MusicLength = music.clip.length;
        MusicSampleRate = 1f / music.clip.frequency;
        AudioManager.OnMusicChange?.Invoke(music);
    }

    private void SaveTimestamp()
    {
        if (currentMusic != null)
        {
            currentMusic.LastTimestamp = musicSource.time;
        }
    }

    public void ChangePitch(float pitch, float length)
    {
        if (changeMusicPitch != null)
        {
            manager.StopCoroutine(changeMusicPitch);
        }
        if (length > 0f)
        {
            changeMusicPitch = manager.StartCoroutine(LerpPitch(pitch, length));
        }
        else
        {
            musicSource.pitch = pitch;
        }
    }

    private IEnumerator LerpPitch(float pitch, float length)
    {
        float start = musicSource.pitch;
        float time = 0f;
        while (musicSource.pitch != pitch)
        {
            musicSource.pitch = Mathf.Lerp(start, pitch, time);
            time += (1 / length) * Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator LerpVolume(float volume, float length)
    {
        if (length <= 0f)
        {
            musicSource.volume = volume;
            yield break;
        }

        float start = musicSource.volume;
        float time = 0f;
        while (time < length)
        {
            musicSource.volume = Mathf.Lerp(start, volume, time / length);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.UnPause();
        }
    }

    public void ResetVolume()
    {
        if (changeVolume != null)
        {
            manager.StopCoroutine(changeVolume);
        }
        musicSource.volume = 1f;
    }

    public void ChangeVolume(float volume, float length)
    {
        if (changeVolume != null)
        {
            manager.StopCoroutine(changeVolume);
        }
        changeVolume = manager.StartCoroutine(LerpVolume(volume, length));
    }

    public void SetPlaybackPosition(float normalizedTime)
    {
        if (musicSource.clip != null)
        {
            musicSource.time = Mathf.Clamp01(normalizedTime) * musicSource.clip.length;
        }
    }

    private bool TryGetMusic(string name, out Music music)
    {
        music = System.Array.Find(musicList, m => m.name == name);
        return music != null;
    }
}

public struct MusicTransitionRequest
{
    public string name;
    public bool fromLastTimestamp;
    public float length;
    public bool startFromMax;
    public int startPriority;
    public int playPriority;

    public MusicTransitionRequest(string name, bool fromLastTimestamp, float length, bool startFromMax, int startPriority, int playPriority)
    {
        this.name = name;
        this.fromLastTimestamp = fromLastTimestamp;
        this.length = length;
        this.startFromMax = startFromMax;
        this.startPriority = startPriority;
        this.playPriority = playPriority;
    }
}
