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
    public bool enableEditing = false;
    public BoundingBoxWindow BBW = new BoundingBoxWindow();
    Texture2D logo;
    public enum State
    {
        NONE, CONNECT
    }
    public Waypoint selectedWaypoint;
    public Tool currentTool = Tool.None;
    public State state = State.NONE;
    public float helperPlaneY = 0;

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
 
    void OnGUI()
    {
        ModManager.ValidateSelected();
        GUI.changed = false;
        if (!ModManager)
        {
            ModManager = ModWindowUtils.GetModManager();
        }

        //Make object withouth go unselectable
        if (ModManager.asset != null && !ModManager.asset.gameObject)
                ModManager.asset = null;

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

        //Basic mod settings
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
                GUI.color = Color.red;

            //No game object found
            if (!PO.gameObject && GUILayout.Button("##No game object found##", "minibutton") && EditorUtility.DisplayDialog("Are you sure to delete this item?", "Are you sure to delete this missing GameObject item?  ", "Ok", "Cancel"))
                ModWindowUtils.DeleteObject(PO, this);
            else
            {
                //Select game
                if (GUILayout.Button(PO.inGameName, "minibutton"))
                {
                    if (e.button == 1 && EditorUtility.DisplayDialog("Are you sure to delete this item?", "Are you sure to delete this item? Name: " + PO.gameObject.name, "Ok", "Cancel"))
                        ModWindowUtils.DeleteObject(PO, this);
                    else
                        ModWindowUtils.SelectObject(PO, this);
                }
                GUI.color = Color.white;
            }
        }

        EditorGUILayout.EndScrollView();
        if (enableEditing)
            GUI.color = Color.grey;

        //Add object 
        GUILayout.BeginHorizontal();
        GUILayout.Label("Add Gameobject:", "GUIEditor.BreadcrumbLeft");

        GUILayout.Space(5);
        GameObject GO = (GameObject)EditorGUILayout.ObjectField(null, typeof(GameObject), true);
        if (GO)
            ModWindowUtils.AddObject(GO, this);

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        //Add selection
        if (Selection.objects.Count() > 0 && GUILayout.Button("Add selection"))
        {
            ModWindowUtils.AddObjects(Selection.objects.ToList(), this);
        }

        GUILayout.Space(5);

        //Delete list
        if (GUILayout.Button("Delete the list") && EditorUtility.DisplayDialog("Are you sure to delete this list?", "Are you sure to delete this whole list ", "Ok", "Cancel"))
            ModWindowUtils.DeleteAll(this);

        //Delete object
        if (ModManager.asset != null && ModManager.asset.gameObject && GUILayout.Button("Delete this object") && EditorUtility.DisplayDialog("Are you sure to delete this item?", "Are you sure to delete this item? Name: " + ModManager.asset.gameObject.name, "Ok", "Cancel"))
                 ModWindowUtils.DeleteObject(ModManager.asset, this);

        if (enableEditing)
            GUI.color = Color.grey;

        //Display objects settings
        if (ModManager.asset != null)
        {
            ObjectGUI.objectGUI(this);   
        }
                EditorUtility.SetDirty(ModManager);
        GUILayout.Space(50);
        EditorGUILayout.EndScrollView();

        GUILayout.Label("© H-POPS - " + UpdateInfo.VersionName, "PreToolbar");

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
        ModManager = ModWindowUtils.GetModManager();
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }
    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }
        
    void OnSceneGUI(SceneView sceneView)
    {
        if (ModManager && (ModManager.asset == null || !ModManager.asset.gameObject))
            return;
        
       
        if (BBW != null)
            BBW.OnSceneGUI(ModManager.asset, ModManager);
        
            int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

            //Place objects next to eachother
            float currentX = 0f;
            foreach (ParkitectObject PObj in ModManager.ParkitectObjects)
            {
                PObj.gameObject.transform.position = new Vector3(currentX + PObj.XSize / 2, 0, .5f);
                currentX += PObj.XSize - 1 + 2f;
            }

            //Draw quad
            Vector3 topLeft = new Vector3(-ModManager.asset.XSize / 2, helperPlaneY, ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Vector3 topRight = new Vector3(ModManager.asset.XSize / 2, helperPlaneY, ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Vector3 bottomLeft = new Vector3(-ModManager.asset.XSize / 2, helperPlaneY, -ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Vector3 bottomRight = new Vector3(ModManager.asset.XSize / 2, helperPlaneY, -ModManager.asset.ZSize / 2) + ModManager.asset.gameObject.transform.position;
            Handles.color = Color.white;
            Color fill = Color.white;
            fill.a = 0.1f;

            Handles.DrawSolidRectangleWithOutline(new Vector3[] { topLeft, topRight, bottomRight, bottomLeft }, fill, Color.black);
            Handles.color = Color.red;
            Handles.DrawDottedLine(bottomRight, bottomLeft, 5f);


            //Draw seating peaps
            Handles.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, .3f);
            List<Transform> list = new List<Transform>();
            ModWindowUtils.recursiveFindTransformsStartingWith("Seat", ModManager.asset.gameObject.transform, list);
            foreach (Transform current in list)
                UtilsGUI.drawSittingPersonGizmo(current, controlID);

            SceneView.RepaintAll();
            //Waypoints
            if (enableEditing)
                WaypointGUI.OnSceneGUI(this);
    }



}





