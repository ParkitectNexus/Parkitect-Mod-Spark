using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ongoing : Product
{
    [SerializeField]
    public int duration;
    public override void DrawGUI()
    {

#if UNITY_EDITOR
        base.DrawGUI();
        duration = EditorGUILayout.IntField("Duration ", duration);
        DrawIngredientsGUI();
#endif
    }
}