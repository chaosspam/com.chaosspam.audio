using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    //todo move this to audio mixer
    [Header("Mixers")]

    public AudioMixer mixer;
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup soundMixer;

    [Header("Audio Data")]

    public AudioDatabase database;

    // Public field
    public static Action OnReady;
    public static Action<Music> OnMusicChange;

    private static float musicVolume = 1f;
    private static float soundVolume = 1f;

    private Dictionary<string, AudioMixerSnapshot> snapshots;

    public string[] snapshotNames;

    public static SoundManager sound { get; private set; }
    public static MusicManager music { get; private set; }
    public PitchManager pitch { get; private set; }

    public UnityEvent<float> OnSetMusicVolume;
    public UnityEvent<float> OnSetSoundVolume;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        sound = new SoundManager(this);
        music = new MusicManager(this);
        pitch = new PitchManager();
        SetupSnapshots();
    }

    private void Start()
    {
        OnReady?.Invoke();
    }

    private void LateUpdate()
    {
        sound.ClearPlayedThisFrame();
    }

    public void TransitionToSnapshot(string name, float time)
    {
        if (!snapshots.ContainsKey(name))
        {
            Debug.LogError($"Snapshot {name} does not exist");
        }
        snapshots[name].TransitionTo(time);
    }

    private void SetupSnapshots()
    {
        snapshots = new Dictionary<string, AudioMixerSnapshot>();
        foreach (string name in snapshotNames)
        {
            snapshots.Add(name, mixer.FindSnapshot(name));
        }
    }

    public static float MusicVolume
    {
        set
        {
            musicVolume = value;
            //GameManager.instance.Stat.musicVolume = value;
            if (musicVolume <= 0)
            {
                musicVolume = 0.001f;
            }

            if(instance != null)
            {
                instance.OnSetMusicVolume?.Invoke(value);
                instance.musicMixer.audioMixer.SetFloat("MusicAttenuation", Mathf.Log(musicVolume) * 20);
            }
        }
        get
        {
            return musicVolume;
        }
    }

    public static float SoundVolume
    {
        set
        {
            soundVolume = value;
            //GameManager.instance.Stat.soundVolume = value;
            if (soundVolume <= 0)
            {
                soundVolume = 0.001f;
            }

            if (instance != null)
            {
                instance.OnSetSoundVolume?.Invoke(value);
                instance.soundMixer.audioMixer.SetFloat("SoundAttenuation", Mathf.Log(soundVolume) * 20);
            }
        }
        get
        {
            return soundVolume;
        }
    }
}
