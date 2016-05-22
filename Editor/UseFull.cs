using UnityEngine;
using System.Collections;
using UnityEditor;

public class UseFull : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    [MenuItem("Parkitect/UseFull/AutoPivot", false, 101)]
    static void CreateForAllSelectedPivot()
    {
        Transform Pivot;
        foreach (Transform T in Selection.transforms)
        {
            Undo.SetCurrentGroupName("Pivots");
            Pivot = new GameObject("Pivot-" + T.gameObject.name).transform;
            Undo.RegisterCreatedObjectUndo(Pivot.gameObject, "CreatedPivots");
            Undo.SetTransformParent(Pivot, T, "CreatedPivots");
            Pivot.localRotation = Quaternion.identity;
            Pivot.localPosition = Vector3.zero;
            Pivot.localScale = Vector3.one;
            Undo.SetTransformParent(Pivot, T.parent, "CreatedPivots");
            Undo.SetTransformParent(T, Pivot, "CreatedPivots");

        }
    }
}
