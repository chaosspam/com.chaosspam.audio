using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioDatabase))]
public class AudioDatabaseEditor : Editor
{
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
    }
}
