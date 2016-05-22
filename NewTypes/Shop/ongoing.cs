using UnityEngine;
using System;
using UnityEditor;

[Serializable]
public class ongoing : Product
{
    [SerializeField]
    public int duration;
    public override void DrawGUI()
    {
        base.DrawGUI();
        duration = EditorGUILayout.IntField("Duration ", duration);
        DrawIngredientsGUI();
    }
}