using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ParkitectModManager))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {

        GUILayout.Label("Not recomend to touch this", "CN EntryError");
    }
}