using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ObjectGUI {


    public static void objectGUI(ModWindow Window)
    {
        GUILayout.BeginHorizontal("flow background");
        if (Window.ModManager.asset != null && Window.ModManager.asset.gameObject)
                GUILayout.Label(Window.ModManager.asset.gameObject.name, "LODLevelNotifyText");
        else
            GUILayout.Label("Settings", "LODLevelNotifyText");

        GUILayout.EndHorizontal();

        GUILayout.Label("Settings", "PreToolbar");

        Window.ModManager.asset.type = (ParkitectObject.ObjType)EditorGUILayout.EnumPopup("Type: ", Window.ModManager.asset.type, "MiniPopup");
        if (Window.ModManager.asset.gameObject && Window.ModManager.asset.type != ParkitectObject.ObjType.none)
        {
            if (Window.ModManager.asset.type == ParkitectObject.ObjType.FlatRide)
                FlatRideGUI(Window);
            else
            {
                DecoGUI(Window);
                Window.enableEditing = false;
            }

            GUILayout.Space(20);
            GUILayout.Label("BoudingBox", "PreToolbar");
            Window.BBW.OnInspectorGUI(Window.ModManager.asset);
        }
    }
    public static void DecoGUI(ModWindow Window)
    {
        switch (Window.ModManager.asset.type)
        {
            case ParkitectObject.ObjType.none:
                break;
            case ParkitectObject.ObjType._:
                break;
            case ParkitectObject.ObjType.deco:
                break;
            case ParkitectObject.ObjType.trashbin:
                BasicGUI(Window.ModManager);
                ColorGUI(Window.ModManager);
                return;
            case ParkitectObject.ObjType.seating:
                EditorGUILayout.HelpBox("Make child objects called seat to make a seat (Working seats will display a yellow sphere)", MessageType.Info);
                BasicGUI(Window.ModManager);

                ColorGUI(Window.ModManager);

                //Generate Seats
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create 1 Seats"))
                {
                    GameObject seat1 = new GameObject("Seat");

                    seat1.transform.parent = Window.ModManager.asset.gameObject.transform;

                    seat1.transform.localPosition = new Vector3(0, 0.1f, 0);
                    seat1.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                if (GUILayout.Button("Create 2 Seat"))
                {
                    GameObject seat1 = new GameObject("Seat");
                    GameObject seat2 = new GameObject("Seat");

                    seat1.transform.parent = Window.ModManager.asset.gameObject.transform;
                    seat2.transform.parent = Window.ModManager.asset.gameObject.transform;

                    seat1.transform.localPosition = new Vector3(0.1f, 0.1f, 0);
                    seat1.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    seat2.transform.localPosition = new Vector3(-0.1f, 0.1f, 0);
                    seat2.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                GUILayout.EndHorizontal();
                return;
            case ParkitectObject.ObjType.wall:
                BasicGUI(Window.ModManager);
                Window.ModManager.asset.category = EditorGUILayout.TextField("Category: ", Window.ModManager.asset.category);
                Window.ModManager.asset.heightDelta = EditorGUILayout.FloatField("HeightDelta: ", Window.ModManager.asset.heightDelta);
                ColorGUI(Window.ModManager);
                return;
            case ParkitectObject.ObjType.lamp:
                BasicGUI(Window.ModManager);
                ColorGUI(Window.ModManager);
                return;
            case ParkitectObject.ObjType.fence:
                EditorGUILayout.HelpBox("Fences are in development", MessageType.Warning);
                return;
            case ParkitectObject.ObjType.FlatRide:
                break;
            case ParkitectObject.ObjType.Shop:
                BasicGUI(Window.ModManager);
                ColorGUI(Window.ModManager);
                Window.ModManager.asset.shop.DrawGUI();
                return;
            case ParkitectObject.ObjType.CoasterCar:
                Window.ModManager.asset.inGameName = EditorGUILayout.TextField("In Game name: ", Window.ModManager.asset.inGameName);
                Window.ModManager.asset.CoasterName = EditorGUILayout.TextField("Coaster Name", Window.ModManager.asset.CoasterName);
                Window.ModManager.asset.frontCar = (GameObject)EditorGUILayout.ObjectField("Front Car", Window.ModManager.asset.frontCar, typeof(GameObject), true);
                Window.ModManager.asset.closedAngleRetraints = EditorGUILayout.Vector3Field("Closed Restraints Angle", Window.ModManager.asset.closedAngleRetraints);
                ColorGUI(Window.ModManager);
                return;
            case ParkitectObject.ObjType.PathStyle:
                BasicGUI(Window.ModManager);
                Window.ModManager.asset.pathType = (ParkitectObject.PathType)EditorGUILayout.EnumPopup("Type", Window.ModManager.asset.pathType);
                PathGUI(Window.ModManager);
                return;
            default:
                return;
        }
        BasicGUI(Window.ModManager);
        Window.ModManager.asset.grid = EditorGUILayout.Toggle("GridSnap: ", Window.ModManager.asset.grid);
        Window.ModManager.asset.heightDelta = EditorGUILayout.FloatField("HeightDelta: ", Window.ModManager.asset.heightDelta);
        Window.ModManager.asset.snapCenter = EditorGUILayout.Toggle("SnapCenter: ", Window.ModManager.asset.snapCenter);
        Window.ModManager.asset.gridSubdivision = EditorGUILayout.FloatField("Grid Subdivision", Window.ModManager.asset.gridSubdivision);
        Window.ModManager.asset.category = EditorGUILayout.TextField("Category: ", Window.ModManager.asset.category);
        ColorGUI(Window.ModManager);
    }

    public static void BasicGUI(ParkitectModManager ModManager)
    {
        ModManager.asset.inGameName = EditorGUILayout.TextField("In Game name: ", ModManager.asset.inGameName);
        ModManager.asset.price = EditorGUILayout.FloatField("Price: ", ModManager.asset.price);
    }


    public static void PathGUI(ParkitectModManager ModManager)
    {
        ModManager.asset.PathTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", ModManager.asset.PathTexture, typeof(Texture2D), true);
        if (GUILayout.Button("Create") && ModManager.asset.PathTexture)
        {
            ModManager.asset.PathTexture.alphaIsTransparency = true;
            ModManager.asset.PathTexture.wrapMode = TextureWrapMode.Repeat;
            ModManager.asset.PathTexture.filterMode = FilterMode.Point;

            AssetDatabase.DeleteAsset("Assets/Materials/Paths/" + ModManager.asset.inGameName + ".mat");
            ModManager.asset.gameObject.AddComponent<MeshRenderer>();
            MeshRenderer MR = ModManager.asset.gameObject.GetComponent<MeshRenderer>();

            //Check Folder for the mat
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Paths"))
                AssetDatabase.CreateFolder("Assets/Materials", "Paths");
            Material material = new Material(Shader.Find("Transparent/Diffuse"));
            material.mainTexture = ModManager.asset.PathTexture;
            AssetDatabase.CreateAsset(material, "Assets/Materials/Paths/" + ModManager.asset.inGameName + ".mat");
            MR.material = material;

            ModManager.asset.gameObject.AddComponent<MeshFilter>();
            MeshFilter MF = ModManager.asset.gameObject.GetComponent<MeshFilter>();
            GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            MF.mesh = GO.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(GO);
            ModManager.asset.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }

    public static void FlatRideGUI(ModWindow Window)
    {
        BasicGUI(Window.ModManager);
        ColorGUI(Window.ModManager);
        if (Window.enableEditing)
            GUI.color = Color.grey;

        GUILayout.Space(10);
        GUILayout.Label("Rating", EditorStyles.boldLabel);
        Window.ModManager.asset.Excitement = EditorGUILayout.Slider("Excitement (" + getRatingCategory(Window.ModManager.asset.Excitement) + ")", Window.ModManager.asset.Excitement, 0, 100);
        Window.ModManager.asset.Intensity = EditorGUILayout.Slider("Intensity (" + getRatingCategory(Window.ModManager.asset.Intensity) + ")", Window.ModManager.asset.Intensity, 0, 100);
        Window.ModManager.asset.Nausea = EditorGUILayout.Slider("Nausea (" + getRatingCategory(Window.ModManager.asset.Nausea) + ")", Window.ModManager.asset.Nausea, 0, 100);
        GUILayout.Space(10);
        Window.ModManager.asset.closedAngleRetraints = EditorGUILayout.Vector3Field("Closed Restraints Angle", Window.ModManager.asset.closedAngleRetraints);

        GUILayout.Space(10);
        GUI.color = Color.white;
        Window.ModManager.asset.XSize = (float)EditorGUILayout.IntField("X", (int)Math.Floor(Window.ModManager.asset.XSize));
        Window.ModManager.asset.ZSize = (float)EditorGUILayout.IntField("Z", (int)Math.Floor(Window.ModManager.asset.ZSize));

        GUILayout.Label("Waypoints", "PreToolbar");
        WaypointGUI.OnWaypointGUI(Window);

    }
    public static void ColorGUI(ParkitectModManager ModManager)
    {

        ModManager.asset.Shader = (ParkitectObject.Shaders)EditorGUILayout.EnumPopup("Shader", ModManager.asset.Shader);
        ModManager.asset.recolorable = EditorGUILayout.BeginToggleGroup("Recolorable", ModManager.asset.recolorable);

        if (ModManager.asset.recolorable)
        {
            try
            {
                int colorsUsed = 0;
                ModManager.asset.color1 = EditorGUILayout.ColorField("Color 1", ModManager.asset.color1);
                if (ModManager.asset.color1 != new Color(0.95f, 0, 0))
                {
                    colorsUsed = 1;
                    ModManager.asset.color2 = EditorGUILayout.ColorField("Color 2", ModManager.asset.color2);
                }
                if (ModManager.asset.color2 != new Color(0.32f, 1, 0))
                {
                    colorsUsed = 2;
                    ModManager.asset.color3 = EditorGUILayout.ColorField("Color 3", ModManager.asset.color3);
                }
                if (ModManager.asset.color3 != new Color(0.110f, 0.059f, 1f))
                {
                    colorsUsed = 3;
                    ModManager.asset.color4 = EditorGUILayout.ColorField("Color 4", ModManager.asset.color4);
                }
                if (ModManager.asset.color4 != new Color(1, 0, 1))
                    colorsUsed = 4;
                if (colorsUsed == 0)
                    GUILayout.Label("No custom colors used");
                else if (colorsUsed == 1)
                    GUILayout.Label("You are only using color 1");
                else if (colorsUsed == 2)
                    GUILayout.Label("You are only using color 1 & 2");
                else if (colorsUsed == 3)
                    GUILayout.Label("You are only using color 1 - 3");
                else if (colorsUsed == 4)
                    GUILayout.Label("You are only using color 1 - 4");
                if (GUILayout.Button("Reset"))
                {
                    ModManager.asset.color1 = new Color(0.95f, 0, 0);
                    ModManager.asset.color2 = new Color(0.32f, 1, 0);
                    ModManager.asset.color3 = new Color(0.110f, 0.059f, 1f);
                    ModManager.asset.color4 = new Color(1, 0, 1);
                }
            }
            catch (Exception)
            {
            }
        }
        EditorGUILayout.EndToggleGroup();
    }
    public static string getRatingCategory(float ratingValue)
    {
        ratingValue /= 100f;
        if (ratingValue > 0.9f)
            return "Very High";
        if (ratingValue > 0.6f)
            return "High";
        if (ratingValue > 0.3f)
            return "Medium";
        return "Low";
    }

}
