using UnityEngine;

public class PitchManager
{
    public static readonly float[] chromatic = {
        1f, 1.059f, 1.122f, 1.189f, 1.259f, 1.335f, 1.414f, 1.498f, 1.587f, 1.681f, 1.782f, 1.888f,
        2f, 2.119f, 2.245f, 2.430f, 2.520f, 2.670f, 2.891f, 2.997f, 3.244f, 3.364f, 3.641f, 3.776f,
        4f
    };

    public static readonly int[] majorInterval = {
        0, 2, 2, 1, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 1
    };

    public static readonly int[] minorInterval = {
        0, 2, 1, 2, 2, 1, 3, 1, 2, 1, 2, 2, 1, 3, 1
    };

    public static readonly int[] pentatonicInterval = {
        0, 2, 2, 3, 2, 3, 2, 2, 3, 2, 3
    };

    public static readonly int pitchCount = 8;

    public int noteCount = 0;
    public float noteTime = 2f;
    public bool minor = false;
    private int key = 0;

    public float GetPitch()
    {
        return chromatic[noteCount % chromatic.Length];
    }

    public float GetMajorPitch(int noteOffset)
    {
        int major = key;
        int note = (noteCount + noteOffset) % majorInterval.Length;
        int offset = 0;

        for (int i = 0; i <= note; i++)
        {
            offset += minor ? minorInterval[i] : majorInterval[i];
        }

        major += offset;

        int octave = (major / chromatic.Length + 1);
        float pitch = chromatic[(major + (octave - 1)) % chromatic.Length];

        return octave * pitch;
    }
    public float GetPentatonicPitch(int noteOffset)
    {
        int major = key;
        int note = noteOffset % pentatonicInterval.Length;
        int offset = 0;

        for (int i = 0; i <= note; i++)
        {
            offset += pentatonicInterval[i];
        }

        major += offset;

        int octave = (major / chromatic.Length + 1);
        float pitch = chromatic[(major + (octave - 1)) % chromatic.Length];

        return octave * pitch;
    }


    public float GetMajorPitch()
    {
        return GetMajorPitch(0);
    }

    public void SetNoteCount(int count)
    {
        noteCount = count;
    }

    public void SetKey(int key, bool minor)
    {
        if (key < 0)
        {
            Debug.LogError("Key cannot be negative");
        }

        this.key = key;
        this.minor = minor;
    }

    public int NoteCount
    {
        get
        {
            return noteCount;
        }
    }
}
