using UnityEngine;
using System;
using UnityEditor;

[Serializable]
public class wearable : Product
{

    public enum bodylocation { head, face, back }
    [SerializeField]
    public bodylocation BodyLocation = bodylocation.head;
    public override void DrawGUI()
    {
        base.DrawGUI();
        BodyLocation = (bodylocation)EditorGUILayout.EnumPopup("Body Location ", BodyLocation);
        DrawIngredientsGUI();
    }

}