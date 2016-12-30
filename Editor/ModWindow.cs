using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml.Linq;
using System;
using System.Linq;
using System.IO;
using System.Xml;



public class ModWindow : EditorWindow
{
    public GUISkin guiSkin;
    public ParkitectModManager ModManager;
    private bool enableEditing = false;
    BoundingBoxWindow BBW = new BoundingBoxWindow();
    Texture2D logo;
    private enum State
    {
        NONE, CONNECT
    }
    public Waypoint selectedWaypoint;
    private Tool currentTool = Tool.None;
    private State state = State.NONE;
    private float helperPlaneY = 0;

    Vector2 scrollPos = new Vector2();
    Vector2 scrollPos2 = new Vector2();

    public void repaintWindow()
    {
        Repaint();
    }


    [MenuItem("Parkitect/Report Problem", false, 112)]
    static void ReportIssue()
    {
        Application.OpenURL("https://github.com/ParkitectNexus/Parkitect-Mod-Spark/issues/new");
    }

    [MenuItem("Parkitect/Wiki", false, 111)]
    static void OpenWiki()
    {
        Application.OpenURL("https://parkitectnexus.com/modding-wiki/Mod-Spark");
    }

    [MenuItem("Parkitect/Mod-Setup", false, 1)]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ModWindow window = (ModWindow)EditorWindow.GetWindow(typeof(ModWindow));
        window.Show();
        window.titleContent.text = "Parkitect Mod";
    }
    void OnEnable()
    {
        logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Parkitect_ModSpark/Textures/MSlogo.png", typeof(Texture2D));
        titleContent.image = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Parkitect_ModSpark/Textures/icon.png", typeof(Texture2D));
        //guiSkin = (GUISkin)(AssetDatabase.LoadAssetAtPath("Assets/Parkitect_ModSpark/Editor/GUI/GUISkin_Spark.guiskin", typeof(GUISkin)));
        //UpdateInfo.Check(false);
    }
    void OnSelectionChange()
    {
        Repaint();

    }

    //              __        ___           _                  ____ _   _ ___ 
    //              \ \      / (_)_ __   __| | _____      __  / ___| | | |_ _|
    //               \ \ /\ / /| | '_ \ / _` |/ _ \ \ /\ / / | |  _| | | || | 
    //                \ V  V / | | | | | (_| | (_) \ V  V /  | |_| | |_| || | 
    //                 \_/\_/  |_|_| |_|\__,_|\___/ \_/\_/    \____|\___/|___|

 
    void OnGUI()
    {
        ModManager.ValidateSelected();
        GUI.changed = false;
        if (!ModManager)
        {
            if (FindObjectOfType<ParkitectModManager>())
            {
                ModManager = FindObjectOfType<ParkitectModManager>();
            }
            else
            {
                if (GameObject.Find("ParkitectModManager"))
                {
                    ModManager = (GameObject.Find("ParkitectModManager")).AddComponent<ParkitectModManager>();
                }
                else
                {

                    ModManager = (new GameObject("ParkitectModManager")).AddComponent<ParkitectModManager>();
                }
            }
        }
        if (ModManager.asset != null)
        {
            if (!ModManager.asset.gameObject)
            {
                ModManager.asset = null;
            }
        }
        if (enableEditing)
        {
            GUI.color = Color.grey;
        }     
        //Show Update 
        if (UpdateInfo.CurNewVersion > UpdateInfo.CurVerion)
        {
            GUI.color = Color.red / 1.4f;
            GUILayout.BeginHorizontal("IN BigTitle");
            GUI.color = Color.white;
            GUILayout.Label("Version " + UpdateInfo.CurNewVersion + " is out! You have version " + UpdateInfo.CurVerion);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update")) { Application.OpenURL(UpdateInfo.NewSite);}
            GUILayout.EndHorizontal();
        }
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        //Show logo
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label(logo, centeredStyle, GUILayout.MaxWidth(Screen.width), GUILayout.ExpandHeight(false));
        //Show Mod name
        GUILayout.BeginHorizontal("flow background");
        GUILayout.Label(ModManager.mod.name, "LODLevelNotifyText");
        GUILayout.EndHorizontal();

        Event e = Event.current;
        GUILayout.Label("Mod setup", "PreToolbar");
        ModGUI();
        GUILayout.Space(10);
        GUILayout.Label("Objects in mod", "PreToolbar");
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, "GroupBox", GUILayout.Height(140));
        foreach (ParkitectObject PO in ModManager.ParkitectObjects)
        {
            if (PO == ModManager.asset)
            {
                GUI.color = Color.red;
            }
            if (!PO.gameObject)
            {
                if (GUILayout.Button("##No game object found##", "minibutton"))
                {
                    if (EditorUtility.DisplayDialog("Are you sure to delete this item?", "Are you sure to delete this missing GameObject item?  ", "Ok", "Cancel"))
                    {
                        ModManager.ParkitectObjects.Remove(PO);
                        ModManager.asset = null;
                    }

                    GUI.FocusControl("");

                    selectedWaypoint = null;
                }

            }
            else
            {

                if (GUILayout.Button(PO.inGameName, "minibutton"))
                {
                    if (e.button == 1)
                    {
                        if (EditorUtility.DisplayDialog("Are you sure to delete this item?", "Are you sure to delete this item? Name: " + PO.gameObject.name, "Ok", "Cancel"))
                        {
                            ModManager.ParkitectObjects.Remove(PO);
                            ModManager.asset = null;
                        }
                        return;
                    }

                    GUI.FocusControl("");

                    if (PO.type == ParkitectObject.ObjType.FlatRide && EditorWindow.GetWindow(typeof(FlatRideAnimator)))
                    {
                        EditorWindow.GetWindow(typeof(FlatRideAnimator)).Repaint();
                    }

                    //Disable editing
                    enableEditing = false;
                    BBW.enableEditing = false;

                    if (ModManager.asset == PO)
                    {
                        ModManager.asset = null;
                    }
                    else
                    {
                        ModManager.asset = PO;
                    }

                    EditorGUIUtility.PingObject(PO.gameObject);
                    GameObject[] newSelection = new GameObject[1];
                    newSelection[0] = PO.gameObject;
                    Selection.objects = newSelection;
                    if (SceneView.lastActiveSceneView)
                    {
                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                    selectedWaypoint = null;

                }

                GUI.color = Color.white;
            }

        }

        EditorGUILayout.EndScrollView();
        if (enableEditing)
        {
            GUI.color = Color.grey;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Add Gameobject:", "GUIEditor.BreadcrumbLeft");

        GUILayout.Space(5);
        GameObject GO = (GameObject)EditorGUILayout.ObjectField(null, typeof(GameObject), true);
        if (GO)
        {

            foreach (ParkitectObject po in ModManager.ParkitectObjects)
            {
                if (po.gameObject == GO)
                {
                    EditorUtility.DisplayDialog("This object is already in the list", "The object that you want to add to the list is already in the list", "Ok");
                    EditorGUIUtility.PingObject(po.gameObject);
                    return;
                }
            }
            ParkitectObject PO = new ParkitectObject();
            PO.gameObject = GO;
            PO.inGameName = GO.name;
            float currentX = 0;
            ModManager.ParkitectObjects.Add(PO);
            ModManager.asset = PO;
            foreach (ParkitectObject PObj in ModManager.ParkitectObjects)
            {
                PObj.gameObject.transform.position = new Vector3(currentX, 0, 0);
                currentX += PObj.XSize / 2;
            }


        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        if (Selection.objects.Count() > 0)
        {
            if (GUILayout.Button("Add selection"))
            {
                foreach (GameObject GObj in Selection.objects)
                {
                    foreach (ParkitectObject po in ModManager.ParkitectObjects)
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
                    ModManager.ParkitectObjects.Add(PO);
                    ModManager.asset = PO;
                    foreach (ParkitectObject PObj in ModManager.ParkitectObjects)
                    {
                        PObj.gameObject.transform.position = new Vector3(currentX, 0, 0);
                        currentX += PObj.XSize / 2;
                    }
                }

            }
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Delete the list"))
        {
            if (EditorUtility.DisplayDialog("Are you sure to delete this list?", "Are you sure to delete this whole list ", "Ok", "Cancel"))
            {
                ModManager.ParkitectObjects.Clear();
                ModManager.asset = null;
            }
        }
        if (ModManager.asset != null && ModManager.asset.gameObject)
        {
            if (GUILayout.Button("Delete this object"))
            {
                if (EditorUtility.DisplayDialog("Are you sure to delete this item?", "Are you sure to delete this item? Name: " + ModManager.asset.gameObject.name, "Ok", "Cancel"))
                {
                    ModManager.ParkitectObjects.Remove(ModManager.asset);
                    ModManager.asset = null;
                }
            }
        }
        if (ModManager.asset != null && ModManager.asset.gameObject)
        {

        }
        if (enableEditing)
        {
            GUI.color = Color.grey;
        }
        if (ModManager.asset != null)
        {
            GUILayout.BeginHorizontal("flow background");
            if (ModManager.asset != null)
            {
                if (ModManager.asset.gameObject)
                {
                    GUILayout.Label(ModManager.asset.gameObject.name, "LODLevelNotifyText");
                }

            }
            else
            {
                GUILayout.Label("Settings", "LODLevelNotifyText");
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Settings", "PreToolbar");

            ModManager.asset.type = (ParkitectObject.ObjType)EditorGUILayout.EnumPopup("Type: ", ModManager.asset.type, "MiniPopup");
            if (ModManager.asset.gameObject && ModManager.asset.type != ParkitectObject.ObjType.none)
            {
                if (ModManager.asset.type == ParkitectObject.ObjType.FlatRide)
                {
                    FlatRideGUI();
                }
                else
                {
                    DecoGUI();
                    enableEditing = false;
                }
                GUILayout.Space(20);
                GUILayout.Label("BoudingBox", "PreToolbar");
                BBW.OnInspectorGUI(ModManager.asset);

            }


        }
                EditorUtility.SetDirty(ModManager);
        GUILayout.Space(50);
        EditorGUILayout.EndScrollView();

        GUILayout.Label("© H-POPS - " + UpdateInfo.VersionName, "PreToolbar");

    }
    void WaypointGUI()
    {
        string caption = "Enable Editing Waypoints";
        if (enableEditing)
        {
            caption = "Disable Editing Waypoints";
        }
        if (enableEditing)
        {
            GUI.color = Color.green;
        }
        bool currentEnableEditing = enableEditing;
        if (GUILayout.Button(caption))
        {
            selectedWaypoint = null;
            enableEditing = !enableEditing;
        }
        if (enableEditing)
        {
            GUI.color = Color.white;
        }
        if (currentEnableEditing != enableEditing)
        {
            if (enableEditing)
            {
                currentTool = Tools.current;
                Tools.current = Tool.None;


            }
            else
            {
                Tools.current = currentTool;
            }
        }
        if (enableEditing)
        {
            GUILayout.Label("S - Snap to axis of connected waypoints");
            helperPlaneY = EditorGUILayout.FloatField("Helper Plane Y", helperPlaneY);

            if (GUILayout.Button("Generate outer grid"))
            {
                generateOuterGrid();
            }
            if (GUILayout.Button("(A)dd Waypoint"))
            {
                addWaypoint();
            }
            if (GUILayout.Button("Rotate 90°"))
            {
                rotateWaypoints();
            }
            if (GUILayout.Button("Clear all"))
            {
                ModManager.asset.waypoints.Clear();
            }
        }

    }

    void FlatRideGUI()
    {
        BasicGUI();
        ColorGUI();
        if (enableEditing)
        {
            GUI.color = Color.grey;
        }

        GUILayout.Space(10);
        GUILayout.Label("Rating", EditorStyles.boldLabel);
        ModManager.asset.Excitement = EditorGUILayout.Slider("Excitement (" + getRatingCategory(ModManager.asset.Excitement) + ")", ModManager.asset.Excitement, 0, 100);
        ModManager.asset.Intensity = EditorGUILayout.Slider("Intensity (" + getRatingCategory(ModManager.asset.Intensity) + ")", ModManager.asset.Intensity, 0, 100);
        ModManager.asset.Nausea = EditorGUILayout.Slider("Nausea (" + getRatingCategory(ModManager.asset.Nausea) + ")", ModManager.asset.Nausea, 0, 100);
        GUILayout.Space(10);
        ModManager.asset.closedAngleRetraints = EditorGUILayout.Vector3Field("Closed Restraints Angle", ModManager.asset.closedAngleRetraints);

        GUILayout.Space(10);
        GUI.color = Color.white;
        ModManager.asset.XSize = (float)EditorGUILayout.IntField("X", (int)Math.Floor(ModManager.asset.XSize));
        ModManager.asset.ZSize = (float)EditorGUILayout.IntField("Z", (int)Math.Floor(ModManager.asset.ZSize));

        GUILayout.Label("Waypoints", "PreToolbar");
        WaypointGUI();

    }
    private string getRatingCategory(float ratingValue)
    {
        ratingValue /= 100f;
        if (ratingValue > 0.9f)
        {
            return "Very High";
        }
        if (ratingValue > 0.6f)
        {
            return "High";
        }
        if (ratingValue > 0.3f)
        {
            return "Medium";
        }
        return "Low";
    }
    void BasicGUI()
    {
        ModManager.asset.inGameName = EditorGUILayout.TextField("In Game name: ", ModManager.asset.inGameName);
        ModManager.asset.price = EditorGUILayout.FloatField("Price: ", ModManager.asset.price);
    }

    void ColorGUI()
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
                if(ModManager.asset.color4 != new Color(1, 0, 1))
                    colorsUsed = 4;
                if(colorsUsed == 0)
                {
                    GUILayout.Label("No custom colors used");
                }
                else if (colorsUsed == 1)
                {
                    GUILayout.Label("You are only using color 1");
                }
                else if (colorsUsed == 2)
                {
                    GUILayout.Label("You are only using color 1 & 2");
                }
                else if (colorsUsed == 3)
                {
                    GUILayout.Label("You are only using color 1 - 3");
                }
                else if (colorsUsed == 4)
                {
                    GUILayout.Label("You are only using color 1 - 4");
                }
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
    void DecoGUI()
    {
        switch (ModManager.asset.type)
        {
            case ParkitectObject.ObjType.none:
                break;
            case ParkitectObject.ObjType._:
                break;
            case ParkitectObject.ObjType.deco:
                break;
            case ParkitectObject.ObjType.trashbin:
                BasicGUI();
                ColorGUI();
                return;
            case ParkitectObject.ObjType.seating:
                EditorGUILayout.HelpBox("Make child objects called seat to make a seat (Working seats will display a yellow sphere)", MessageType.Info);
                BasicGUI();

                ColorGUI();

                //Generate Seats
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create 1 Seats"))
                {
                    GameObject seat1 = new GameObject("Seat");

                    seat1.transform.parent = ModManager.asset.gameObject.transform;

                    seat1.transform.localPosition = new Vector3(0, 0.1f, 0);
                    seat1.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                if (GUILayout.Button("Create 2 Seat"))
                {
                    GameObject seat1 = new GameObject("Seat");
                    GameObject seat2 = new GameObject("Seat");

                    seat1.transform.parent = ModManager.asset.gameObject.transform;
                    seat2.transform.parent = ModManager.asset.gameObject.transform;

                    seat1.transform.localPosition = new Vector3(0.1f, 0.1f, 0);
                    seat1.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    seat2.transform.localPosition = new Vector3(-0.1f, 0.1f, 0);
                    seat2.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                GUILayout.EndHorizontal();
                return;
            case ParkitectObject.ObjType.wall:
                BasicGUI();
                ModManager.asset.category = EditorGUILayout.TextField("Category: ", ModManager.asset.category);
                ModManager.asset.heightDelta = EditorGUILayout.FloatField("HeightDelta: ", ModManager.asset.heightDelta);
                ColorGUI();
                return;
            case ParkitectObject.ObjType.lamp:
                BasicGUI();
                ColorGUI();
                return;
            case ParkitectObject.ObjType.fence:
                EditorGUILayout.HelpBox("Fences are in development", MessageType.Warning);
                return;
            case ParkitectObject.ObjType.FlatRide:
                break;
            case ParkitectObject.ObjType.Shop:
                BasicGUI();
                ColorGUI();
                ModManager.asset.shop.DrawGUI();
                return;
            case ParkitectObject.ObjType.CoasterCar:
                ModManager.asset.inGameName = EditorGUILayout.TextField("In Game name: ", ModManager.asset.inGameName);
                ModManager.asset.CoasterName = EditorGUILayout.TextField("Coaster Name", ModManager.asset.CoasterName);
                ModManager.asset.frontCar = (GameObject)EditorGUILayout.ObjectField("Front Car", ModManager.asset.frontCar, typeof(GameObject), true);
                ModManager.asset.closedAngleRetraints = EditorGUILayout.Vector3Field("Closed Restraints Angle", ModManager.asset.closedAngleRetraints);
                ColorGUI();
                return;
            case ParkitectObject.ObjType.PathStyle:
                BasicGUI();
                ModManager.asset.pathType = (ParkitectObject.PathType)EditorGUILayout.EnumPopup("Type", ModManager.asset.pathType);
                PathGUI();
                return;
            default:
                return;
        }
        BasicGUI();
        ModManager.asset.grid = EditorGUILayout.Toggle("GridSnap: ", ModManager.asset.grid);
        ModManager.asset.heightDelta = EditorGUILayout.FloatField("HeightDelta: ", ModManager.asset.heightDelta);
        ModManager.asset.snapCenter = EditorGUILayout.Toggle("SnapCenter: ", ModManager.asset.snapCenter);
        ModManager.asset.gridSubdivision = EditorGUILayout.FloatField("Grid Subdivision", ModManager.asset.gridSubdivision);
        ModManager.asset.category = EditorGUILayout.TextField("Category: ", ModManager.asset.category);
        ColorGUI();
    }
    void PathGUI()
    {
        ModManager.asset.PathTexture = (Texture2D)EditorGUILayout.ObjectField("Texture",ModManager.asset.PathTexture, typeof(Texture2D), true);
        if(GUILayout.Button("Create") && ModManager.asset.PathTexture)
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
            DestroyImmediate(GO);
            ModManager.asset.gameObject.transform.eulerAngles = new Vector3(90,0,0);
        }
    }
    void ModGUI()
    {
        GUILayout.Label("Mod Name:", EditorStyles.boldLabel);
        ModManager.mod.name = EditorGUILayout.TextField(ModManager.mod.name);
        GUILayout.Label("Mod Description:", EditorStyles.boldLabel);
        ModManager.mod.discription = EditorGUILayout.TextArea(ModManager.mod.discription);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Export XML"))
        {
            Exporter.SaveToXML(ModManager);
        }
        GUILayout.EndHorizontal();
        if (!enableEditing)
        {
            GUI.color = Color.green;
        }
        if (GUILayout.Button("Export MOD"))
        {
            Exporter.ExportMod(ModManager);
        }
        if (!enableEditing)
        {
            GUI.color = Color.white;
        }
    }

    void OnFocus()
    {
        if (FindObjectOfType<ParkitectModManager>())
        {
            ModManager = FindObjectOfType<ParkitectModManager>();
        }
        else
        {
            if (GameObject.Find("ParkitectModManager"))
            {
                ModManager = (GameObject.Find("ParkitectModManager")).AddComponent<ParkitectModManager>();
            }
            else
            {

                ModManager = (new GameObject("ParkitectModManager")).AddComponent<ParkitectModManager>();
            }
        }
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        // Add (or re-add) the delegate.
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }
    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        //Exporter.SaveToXML(ModManager);
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

    }
    //     ---      ____                         ____ _   _ ___      --- 
    //     ---     / ___|  ___ ___ _ __   ___   / ___| | | |_ _|     --- 
    //     ---     \___ \ / __/ _ \ '_ \ / _ \ | |  _| | | || |      --- 
    //     ---      ___) | (_|  __/ | | |  __/ | |_| | |_| || |      --- 
    //     ---     |____/ \___\___|_| |_|\___|  \____|\___/|___|     ---       
    public static void recursiveFindTransformsStartingWith(string name, Transform parentTransform, List<Transform> transforms)
    {
        Transform[] componentsInChildren = parentTransform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Transform transform = componentsInChildren[i];
            if (transform.name.StartsWith(name))
            {
                transforms.Add(transform);
            }
        }
    }
    public static void drawSittingPersonGizmo(Transform transform, int controlID)
    {
        Handles.SphereCap(controlID, transform.position, transform.rotation, 0.05f);
        Vector3 vector = transform.position - transform.up * 0.02f + transform.forward * 0.078f - transform.right * 0.045f;
        Handles.SphereCap(controlID, vector, transform.rotation, 0.03f);
        Vector3 vector2 = transform.position - transform.up * 0.02f + transform.forward * 0.078f + transform.right * 0.045f;
        Handles.SphereCap(controlID, vector2, transform.rotation, 0.03f);
        Vector3 center = transform.position + transform.up * 0.305f + transform.forward * 0.03f;
        Handles.SphereCap(controlID, center, transform.rotation, 0.1f);
        Vector3 center2 = vector + transform.forward * 0.015f - transform.up * 0.07f;
        Handles.SphereCap(controlID, center2, transform.rotation, 0.02f);
        Vector3 center3 = vector2 + transform.forward * 0.015f - transform.up * 0.07f;
        Handles.SphereCap(controlID, center3, transform.rotation, 0.02f);
    }
    void OnSceneGUI(SceneView sceneView)
    {
        if (ModManager && (ModManager.asset == null || !ModManager.asset.gameObject))
        {

            return;

        }
        if (BBW != null)
            BBW.OnSceneGUI(ModManager.asset, ModManager);
        else
        {



            Vector3 topLeft = new Vector3(-ModManager.asset.XSize / 2, helperPlaneY, ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Vector3 topRight = new Vector3(ModManager.asset.XSize / 2, helperPlaneY, ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Vector3 bottomLeft = new Vector3(-ModManager.asset.XSize / 2, helperPlaneY, -ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Vector3 bottomRight = new Vector3(ModManager.asset.XSize / 2, helperPlaneY, -ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            float currentX = 0f;
            foreach (ParkitectObject PObj in ModManager.ParkitectObjects)
            {

                PObj.gameObject.transform.position = new Vector3(currentX + PObj.XSize / 2, 0, .5f);


                currentX += PObj.XSize - 1 + 2f;
            }


            int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
            Handles.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, .3f);

            List<Transform> list = new List<Transform>();
            recursiveFindTransformsStartingWith("Seat", ModManager.asset.gameObject.transform, list);
            foreach (Transform current in list)
            {
                drawSittingPersonGizmo(current, controlID);
            }
            Handles.color = Color.white;
            Color fill = Color.white;
            fill.a = 0.1f;

            
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { topLeft, topRight, bottomRight, bottomLeft }, fill, Color.black);
            Handles.color = Color.red;
            Handles.DrawDottedLine(bottomRight, bottomLeft, 5f);
            SceneView.RepaintAll();
            if (!enableEditing)
            {
                return;
            }
            EditorUtility.SetDirty(ModManager);
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.black;


            Tools.current = Tool.None;


            switch (Event.current.type)
            {
                case EventType.layout:
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                case EventType.mouseUp:
                    if (Event.current.button == 0)
                    {
                        handleClick();
                    }
                    break;
                case EventType.keyDown:
                    if (Event.current.keyCode == KeyCode.S)
                    {
                        ModManager.asset.snap = true;
                    }
                    break;
                case EventType.keyUp:
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        if (state != State.CONNECT)
                        {
                            state = State.CONNECT;
                        }
                        else
                        {
                            state = State.NONE;
                        }
                    }
                    else if (Event.current.keyCode == KeyCode.R)
                    {
                        removeSelectedWaypoint();
                    }
                    else if (Event.current.keyCode == KeyCode.A)
                    {
                        addWaypoint();
                    }
                    else if (Event.current.keyCode == KeyCode.O && selectedWaypoint != null)
                    {
                        selectedWaypoint.isOuter = !selectedWaypoint.isOuter;
                    }
                    else if (Event.current.keyCode == KeyCode.I && selectedWaypoint != null)
                    {
                        selectedWaypoint.isRabbitHoleGoal = !selectedWaypoint.isRabbitHoleGoal;
                    }
                    else if (Event.current.keyCode == KeyCode.S)
                    {
                        ModManager.asset.snap = false;
                    }

                    SceneView.RepaintAll();
                    HandleUtility.Repaint();
                    break;
            }

            int i = 0;
            foreach (Waypoint waypoint in ModManager.asset.waypoints)
            {
                if (waypoint == selectedWaypoint)
                {
                    Handles.color = Color.red;
                }
                else if (waypoint.isOuter)
                {
                    Handles.color = Color.green;
                }
                else if (waypoint.isRabbitHoleGoal)
                {
                    Handles.color = Color.blue;
                }
                else
                {
                    Handles.color = Color.yellow;
                }
                Vector3 worldPos = waypoint.localPosition + ModManager.asset.gameObject.transform.position;
                Handles.SphereCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.2f);

                Handles.color = Color.blue;
                foreach (int connectedIndex in waypoint.connectedTo)
                {
                    Handles.DrawLine(worldPos, ModManager.asset.waypoints[connectedIndex].localPosition + ModManager.asset.gameObject.transform.position);
                }

                Handles.Label(worldPos, "#" + i, labelStyle);
                i++;
            }

            if (selectedWaypoint != null)
            {
                Vector3 worldPos = selectedWaypoint.localPosition + ModManager.asset.gameObject.transform.position;
                Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity) - ModManager.asset.gameObject.transform.position;
                selectedWaypoint.localPosition = handleSnap(newPos, selectedWaypoint);

                if (state == State.CONNECT)
                {
                    Handles.Label(worldPos, "\nConnecting...", labelStyle);
                }
                else
                {
                    Handles.Label(worldPos, "\n(C)onnect\n(R)emove\n(O)uter\nRabb(i)t Hole", labelStyle);
                }
            }
        }





    }
    private void handleClick()
    {

        Waypoint closestWaypoint = null;
        EditorGUI.BeginChangeCheck();
        foreach (Waypoint waypoint in ModManager.asset.waypoints)
        {
            Vector2 guiPosition = HandleUtility.WorldToGUIPoint(waypoint.localPosition + ModManager.asset.gameObject.transform.position);
            if ((guiPosition - Event.current.mousePosition).magnitude < 15)
            {
                closestWaypoint = waypoint;
                break;
            }
        }



        if (state == State.NONE && closestWaypoint != null)
        {
            selectedWaypoint = closestWaypoint;
        }
        else if (state == State.CONNECT && selectedWaypoint != null)
        {
            int closestWaypointIndex = ModManager.asset.waypoints.FindIndex(delegate (Waypoint wp)
            {
                return wp == closestWaypoint;
            });
            int selectedWaypointIndex = ModManager.asset.waypoints.FindIndex(delegate (Waypoint wp)
            {
                return wp == selectedWaypoint;
            });
            if (closestWaypointIndex >= 0 && selectedWaypointIndex >= 0)
            {
                if (!selectedWaypoint.connectedTo.Contains(closestWaypointIndex))
                {
                    selectedWaypoint.connectedTo.Add(closestWaypointIndex);
                    closestWaypoint.connectedTo.Add(selectedWaypointIndex);
                }
                else
                {
                    selectedWaypoint.connectedTo.Remove(closestWaypointIndex);
                    closestWaypoint.connectedTo.Remove(selectedWaypointIndex);
                }
            }
        }

    }

    private void addWaypoint()
    {
        Vector3 pos = ModManager.asset.gameObject.transform.position;
        selectedWaypoint = new Waypoint();

        if (Camera.current != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, helperPlaneY, 0));
            float enter = 0;
            plane.Raycast(ray, out enter);
            selectedWaypoint.localPosition = ray.GetPoint(enter) - pos;
        }
        else
        {
            selectedWaypoint.localPosition = new Vector3(0, helperPlaneY, 0);
        }

        ModManager.asset.waypoints.Add(selectedWaypoint);
    }

    private void rotateWaypoints()
    {
        Vector3 pos = ModManager.asset.gameObject.transform.position;

        foreach (Waypoint waypoint in ModManager.asset.waypoints)
        {
            Vector3 dir = waypoint.localPosition - pos;
            dir.y = 0;
            float phi = Mathf.Atan2(dir.z, dir.x);
            phi += Mathf.PI / 2;
            float x = pos.x + dir.magnitude * Mathf.Cos(phi);
            float z = pos.z + dir.magnitude * Mathf.Sin(phi);
            waypoint.localPosition = new Vector3(x, waypoint.localPosition.y, z);
        }

    }

    private void removeSelectedWaypoint()
    {

        int selectedWaypointIndex = ModManager.asset.waypoints.FindIndex(delegate (Waypoint wp)
        {
            return wp == selectedWaypoint;
        });
        foreach (Waypoint waypoint in ModManager.asset.waypoints)
        {
            waypoint.connectedTo.Remove(selectedWaypointIndex);
        }
        ModManager.asset.waypoints.Remove(selectedWaypoint);

        foreach (Waypoint waypoint in ModManager.asset.waypoints)
        {
            for (int i = 0; i < waypoint.connectedTo.Count; i++)
            {
                if (waypoint.connectedTo[i] > selectedWaypointIndex)
                {
                    waypoint.connectedTo[i]--;
                }
            }
        }

        selectedWaypoint = null;

    }

    private Vector3 handleSnap(Vector3 newPos, Waypoint waypoint)
    {
        Vector3 oldPos = waypoint.localPosition;

        if (ModManager.asset.snap && (newPos - oldPos).magnitude > Mathf.Epsilon)
        {
            if (Mathf.Abs(newPos.x - oldPos.x) > Mathf.Epsilon)
            {
                newPos = handleAxisSnap(newPos, waypoint, 0);
            }
            if (Mathf.Abs(newPos.y - oldPos.y) > Mathf.Epsilon)
            {
                newPos = handleAxisSnap(newPos, waypoint, 1);
            }
            if (Mathf.Abs(newPos.z - oldPos.z) > Mathf.Epsilon)
            {
                newPos = handleAxisSnap(newPos, waypoint, 2);
            }
        }

        return newPos;
    }

    private Vector3 handleAxisSnap(Vector3 newPos, Waypoint waypoint, int axisIndex)
    {

        foreach (int connectedIndex in waypoint.connectedTo)
        {
            Waypoint connectedWaypoint = ModManager.asset.waypoints[connectedIndex];
            if (Mathf.Abs(newPos[axisIndex] - connectedWaypoint.localPosition[axisIndex]) < 0.1f)
            {
                newPos[axisIndex] = connectedWaypoint.localPosition[axisIndex];
            }
        }

        return newPos;
    }

    // =========[ generateOuterGrid ]=========================================================
    private void generateOuterGrid()
    {
        float minX = -ModManager.asset.XSize / 2;
        float maxX = ModManager.asset.XSize / 2;
        float minZ = -ModManager.asset.ZSize / 2;
        float maxZ = ModManager.asset.ZSize / 2;
        for (int xi = 0; xi < Mathf.RoundToInt(maxX - minX); xi++)
        {
            for (int zi = 0; zi < Mathf.RoundToInt(maxZ - minZ); zi++)
            {
                float x = minX + xi;
                float z = minZ + zi;
                if (!(x == minX || x == maxX - 1) && !(z == minZ || z == maxZ - 1))
                {
                    continue;
                }
                Waypoint newWaypoint = new Waypoint();
                newWaypoint.localPosition = new Vector3(x + 0.5f, helperPlaneY, z + 0.5f);
                newWaypoint.isOuter = true;
                //if (waypoints.waypoints.Count > 0) {
                //newWaypoint.connectedTo.Add(waypoints.waypoints.Count - 1);
                //}
                ModManager.asset.waypoints.Add(newWaypoint);
            }
        }

    }
}





