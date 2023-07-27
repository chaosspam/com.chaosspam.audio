using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio Database", menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    public Music[] musicList;
    public Sound[] soundList;

#if UNITY_EDITOR
    public void SortMusic()
    {
        System.Array.Sort(musicList);
    }
    public void SortSounds()
    {
        System.Array.Sort(soundList, (Sound a, Sound b) => a.name.CompareTo(b.name));
    }
#endif
}
