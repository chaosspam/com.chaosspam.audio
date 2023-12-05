using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(AudioDatabase))]
public class AudioDatabaseEditor : Editor
{
    private SoundImport obj;
    private SerializedObject soundImport;
    private SerializedProperty template;
    private SerializedProperty clips;

    private void OnEnable() {
        obj = ScriptableObject.CreateInstance<SoundImport>();
        soundImport = new UnityEditor.SerializedObject(obj);
        template = soundImport.FindProperty("template");
        clips = soundImport.FindProperty("clips");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AudioDatabase database = (AudioDatabase)target;

        if (GUILayout.Button("Sort"))
        {
            database.SortMusic();
            database.SortSounds();
            EditorUtility.SetDirty(database);
        }

        EditorGUILayout.PropertyField(template, new GUIContent("Template"));
        EditorGUILayout.PropertyField(clips, new GUIContent("Clips"));

        if (GUILayout.Button("Add Sounds"))
        {
            int start = database.soundList.Length;
            Array.Resize(ref database.soundList, database.soundList.Length + obj.clips.Length);
            for (int i = 0; i < obj.clips.Length; i++)
            {
                AudioClip clip = obj.clips[i];
                Sound s = new Sound(obj.template, clip);
                database.soundList[start + i] = s;
            }
            EditorUtility.SetDirty(database);
        }

        soundImport.ApplyModifiedProperties();
    }
}

[Serializable]
public class SoundImport : ScriptableObject
{
    public Sound template;
    public AudioClip[] clips;
}

