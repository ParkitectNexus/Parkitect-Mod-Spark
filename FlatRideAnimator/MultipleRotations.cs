using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
[Serializable]
public class MultipleRotations : motor {
    [SerializeField]
    public Transform mainAxis;
    [SerializeField]
    public List<Transform> Axiss = new List<Transform>();
    
    public override void DrawGUI()
    {

#if UNITY_EDITOR
        Identifier = EditorGUILayout.TextField("Name ", Identifier);
        mainAxis = (Transform)EditorGUILayout.ObjectField("MainAxis", mainAxis, typeof(Transform), true);
        Transform Axis = (Transform)EditorGUILayout.ObjectField("Add axis", null, typeof(Transform), true);
        if(Axis)
        {
            Axiss.Add(Axis);
        }
        if (Selection.objects.Length > 0)
        {
            if (GUILayout.Button("Add selection"))
            {
                foreach (GameObject GObj in Selection.objects)
                {
                    Axiss.Add(GObj.transform);
                }

            }
        }
        foreach (Transform T in Axiss)
        {
            if(GUILayout.Button(T.gameObject.name, "ShurikenModuleTitle"))
            {
                if (Event.current.button == 1)
                {
                    Axiss.Remove(T);
                    return;
                }
                else
                {
                    Selection.objects = new GameObject[] { T.gameObject };
                    EditorGUIUtility.PingObject(T.gameObject);
                }
            }
        }
#endif
        base.DrawGUI();
    }
    public override void Reset()
    {
        if (mainAxis)
        {
            foreach (Transform T in Axiss)
            {
                T.localRotation = mainAxis.localRotation;
            }
        }
    }
    public void tick(float dt)
    {
        if (mainAxis)
        {
            foreach (Transform T in Axiss)
            {
                T.localRotation = mainAxis.localRotation;
            }
        }
    }
}
