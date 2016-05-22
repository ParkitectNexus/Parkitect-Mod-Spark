using UnityEngine;
using System.Collections;
using UnityEditor;

public class BoundingBoxWindow
{

    private bool enableEditing = false;
    private bool snap = false;
    private Vector2 scrollPos2;
    public BoundingBox selected;
    public void OnInspectorGUI(ParkitectObject PO)
    {
        Event e = Event.current;

        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, "GroupBox", GUILayout.Height(100));
        for (int i = 0; i < PO.BoundingBoxes.Count; i++)
        {
            Color gui = GUI.color;
            if (PO.BoundingBoxes[i] == selected)
            { GUI.color = Color.red; }

            if (GUILayout.Button("BoudingBox" + (i + 1)))
            {
                if (e.button == 1)
                {
                    PO.BoundingBoxes.RemoveAt(i);
                    return;
                }

                if (selected == PO.BoundingBoxes[i])
                {
                    selected = null;
                    return;
                }
                selected = PO.BoundingBoxes[i];
            }
            GUI.color = gui;
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add BoudingBox"))
        {
            PO.BoundingBoxes.Add(new BoundingBox());
        }
        string caption = "Enable Editing";
        if (enableEditing)
        {
            caption = "Disable Editing";
        }
        if (GUILayout.Button(caption))
        {
            enableEditing = !enableEditing;
        }
        if (enableEditing)
        {
            GUILayout.Label("Hold S - Snap to 0.25");
        }
    }

    public void OnSceneGUI(ParkitectObject PO, ParkitectModManager PMM)
    {
        drawBox(PO);

        if (!enableEditing)
        {
            return;
        }

        EditorUtility.SetDirty(PMM);
        Tools.current = Tool.None;

        int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        switch (Event.current.type)
        {
            case EventType.layout:
                HandleUtility.AddDefaultControl(controlID);
                break;
            case EventType.keyDown:
                if (Event.current.keyCode == KeyCode.S)
                {
                    snap = true;
                }
                break;
            case EventType.keyUp:
                if (Event.current.keyCode == KeyCode.S)
                {
                    snap = false;
                }
                break;
        }

    }

    private void drawBox(ParkitectObject PO)
    {
        if (selected == null || PO == null)
            return;
        foreach (BoundingBox box in PO.BoundingBoxes)
        {


            Vector3 diff = box.bounds.max - box.bounds.min;
            Vector3 diffX = new Vector3(diff.x, 0, 0);
            Vector3 diffY = new Vector3(0, diff.y, 0);
            Vector3 diffZ = new Vector3(0, 0, diff.z);

            Color fill = Color.white;
            fill.a = 0.025f;
            Color outer = Color.gray;
            if (enableEditing && box == selected)
            {
                fill = Color.magenta;
                fill.a = 0.1f;
                outer = Color.black;
            }

            // left
            drawPlane(box.bounds.min, box.bounds.min + diffZ, box.bounds.min + diffZ + diffY, box.bounds.min + diffY, fill, outer, PO);

            //back
            drawPlane(box.bounds.min, box.bounds.min + diffX, box.bounds.min + diffX + diffY, box.bounds.min + diffY, fill, outer, PO);

            //right
            drawPlane(box.bounds.max, box.bounds.max - diffY, box.bounds.max - diffY - diffZ, box.bounds.max - diffZ, fill, outer, PO);

            //forward
            drawPlane(box.bounds.max, box.bounds.max - diffY, box.bounds.max - diffY - diffX, box.bounds.max - diffX, fill, outer, PO);

            //up
            drawPlane(box.bounds.max, box.bounds.max - diffX, box.bounds.max - diffX - diffZ, box.bounds.max - diffZ, fill, outer, PO);

            //down
            drawPlane(box.bounds.min, box.bounds.min + diffX, box.bounds.min + diffX + diffZ, box.bounds.min + diffZ, fill, outer, PO);

            if (enableEditing && box == selected)
            {
                box.bounds.min = handleModifyValue(box.bounds.min, PO.gameObject.transform.InverseTransformPoint(Handles.PositionHandle(PO.gameObject.transform.TransformPoint(box.bounds.min), Quaternion.LookRotation(Vector3.left, Vector3.down))));
                box.bounds.max = handleModifyValue(box.bounds.max, PO.gameObject.transform.InverseTransformPoint(Handles.PositionHandle(PO.gameObject.transform.TransformPoint(box.bounds.max), Quaternion.LookRotation(Vector3.forward))));
                Handles.Label(box.bounds.min, box.bounds.min.ToString("F2"));
                Handles.Label(box.bounds.max, box.bounds.max.ToString("F2"));
            }
        }
    }

    private Vector3 handleModifyValue(Vector3 value, Vector3 newValue)
    {
        if (snap && (newValue - value).magnitude > Mathf.Epsilon)
        {
            if (Mathf.Abs(newValue.x - value.x) > Mathf.Epsilon)
            {
                newValue.x = Mathf.Round(newValue.x * 4) / 4;
            }
            if (Mathf.Abs(newValue.y - value.y) > Mathf.Epsilon)
            {
                newValue.y = Mathf.Round(newValue.y * 4) / 4;
            }
            if (Mathf.Abs(newValue.z - value.z) > Mathf.Epsilon)
            {
                newValue.z = Mathf.Round(newValue.z * 4) / 4;
            }
        }
        return newValue;
    }

    private void drawPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color fill, Color outer, ParkitectObject PO)
    {
        Handles.DrawSolidRectangleWithOutline(new Vector3[] { PO.gameObject.transform.TransformPoint(p1), PO.gameObject.transform.TransformPoint(p2), PO.gameObject.transform.TransformPoint(p3), PO.gameObject.transform.TransformPoint(p4) }, fill, outer);
    }
}