using System;
using UnityEngine;

[Serializable]
public class Music: IComparable
{
    public string name;
    public Sprite coverArt;
    public int trackNumber;
    public AudioClip clip;
    public float volume = 1f;
    private float lastTimestamp;

    public Music(Music music)
    {
        if (music != null)
        {
            this.name = music.name;
            this.coverArt = music.coverArt;
            this.trackNumber = music.trackNumber;
            this.clip = music.clip;
            this.volume = music.volume;
            this.lastTimestamp = music.lastTimestamp;
        }
    }

    public float LastTimestamp 
    { 
        get => lastTimestamp;
        set {
            float time = value;
            while (time > clip.length) 
            {
                time -= clip.length;
            }
            if (time < 0f) 
            {
                time = 0f;
            }
            lastTimestamp = time; 
        } 
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Music other = obj as Music;
        if (other != null)
        {
            int trackCompare = trackNumber - other.trackNumber;
            if (trackCompare == 0)
            {
                return name.CompareTo(other.name);
            }
            else
            {
                return trackCompare;
            }
        }
        else
        {
            throw new ArgumentException(string.Format("{0} is not an instance of Music", obj));
        }
    }
}
