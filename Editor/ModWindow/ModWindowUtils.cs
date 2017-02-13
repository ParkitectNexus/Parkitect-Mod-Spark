using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ModWindowUtils {

    public static ParkitectModManager GetModManager()
    {
        if (GameObject.FindObjectOfType<ParkitectModManager>())
            return GameObject.FindObjectOfType<ParkitectModManager>();
        else
            if (GameObject.Find("ParkitectModManager"))
                return (GameObject.Find("ParkitectModManager")).AddComponent<ParkitectModManager>();
            else
                return (new GameObject("ParkitectModManager")).AddComponent<ParkitectModManager>();
    }

    public static void SelectObject(ParkitectObject PO, ModWindow Window)
    {

        GUI.FocusControl("");

        if (PO.type == ParkitectObject.ObjType.FlatRide && EditorWindow.GetWindow(typeof(FlatRideAnimator)))
        {
            EditorWindow.GetWindow(typeof(FlatRideAnimator)).Repaint();
        }

        //Disable editing
        Window.enableEditing = false;
        Window.BBW.enableEditing = false;

        if (Window.ModManager.asset == PO)
            Window.ModManager.asset = null;
        else
            Window.ModManager.asset = PO;

        EditorGUIUtility.PingObject(PO.gameObject);
        GameObject[] newSelection = new GameObject[1];
        newSelection[0] = PO.gameObject;
        Selection.objects = newSelection;
        if (SceneView.lastActiveSceneView)
            SceneView.lastActiveSceneView.FrameSelected();
        Window.selectedWaypoint = null;
    }


    public static void DeleteObject(ParkitectObject PO, ModWindow Window)
    {
        Window.ModManager.ParkitectObjects.Remove(PO);
        Window.ModManager.asset = null;
        GUI.FocusControl("");
        Window.selectedWaypoint = null;
    }

    public static void DeleteAll(ModWindow Window)
    {
        Window.ModManager.ParkitectObjects.Clear();
        Window.ModManager.asset = null;
    }

    public static void AddObjects(List<Object> objects, ModWindow Window)
    {
        foreach (GameObject GObj in objects)
        {
            foreach (ParkitectObject po in Window.ModManager.ParkitectObjects)
            {
                if (po.gameObject == GObj)
                {
                    UnityEngine.Debug.LogWarning("Object already in list");
                    EditorGUIUtility.PingObject(po.gameObject);
                    return;
                }
                else if (!GameObject.Find(po.gameObject.name))
                {
                    UnityEngine.Debug.LogWarning("Object not in scene");
                    EditorGUIUtility.PingObject(po.gameObject);
                    return;
                }
            }
            ParkitectObject PO = new ParkitectObject();
            PO.gameObject = GObj;
            PO.inGameName = GObj.name;
            float currentX = 0;
            Window.ModManager.ParkitectObjects.Add(PO);
            Window.ModManager.asset = PO;
            foreach (ParkitectObject PObj in Window.ModManager.ParkitectObjects)
            {
                PObj.gameObject.transform.position = new Vector3(currentX, 0, 0);
                currentX += PObj.XSize / 2;
            }
        }
    }

    public static void AddObject(Object OP, ModWindow Window)
    {
        List<Object> objects = new List<Object>() { OP };
        AddObjects(objects, Window);
    }
    public static void recursiveFindTransformsStartingWith(string name, Transform parentTransform, List<Transform> transforms)
    {
        Transform[] componentsInChildren = parentTransform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Transform transform = componentsInChildren[i];
            if (transform.name.StartsWith(name))
                transforms.Add(transform);
        }
    }
}   
