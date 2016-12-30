using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class wearable : Product
{

    public enum bodylocation { head, face, back }
    [SerializeField]
    public bodylocation BodyLocation = bodylocation.head;
    public override void DrawGUI()
    {

#if UNITY_EDITOR
        base.DrawGUI();
        BodyLocation = (bodylocation)EditorGUILayout.EnumPopup("Body Location ", BodyLocation);
        DrawIngredientsGUI();
#endif
    }

}