using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class FlatRideAnimator : EditorWindow
{
    
    Vector2 scrollPosPhases;
    Vector2 scrollPosMotors;
    public ParkitectModManager ModManager;
    bool isPlaying;
    //For Adding events
    Phase curentPhase;
    List<ReflectionProbe> ReflectionProbes = new List<ReflectionProbe>();
    // Add menu named "My Window" to the Window menu
    [MenuItem("Parkitect/Flat-Ride Animator", false, 2)]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        FlatRideAnimator window = (FlatRideAnimator)EditorWindow.GetWindow(typeof(FlatRideAnimator));
        window.Show();
        window.titleContent.text = "RideAnimator";
    }
    void OnEnable()
    {
        titleContent.image = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Parkitect_ModSpark/Textures/AnimateIcon.png", typeof(Texture2D));
        EditorApplication.update += CallbackFunction;
    }
    public void repaintWindow()
    {
        Repaint();
    }
    void AddEvent(object obj)
    {
        switch (obj.ToString())
        {
            case "Wait":
                curentPhase.Events.Add(CreateInstance < Wait>());
                break;
            case "StartRotator":
                StartRotator SR = CreateInstance < StartRotator>();
                curentPhase.Events.Add(SR);
                SR.obj = ModManager.asset;
                break;
            case "SpinRotator":
                SpinRotater SpR = CreateInstance<SpinRotater>();
                curentPhase.Events.Add(SpR);
                SpR.obj = ModManager.asset;
                break;
            case "StopRotator":
                StopRotator StR = CreateInstance < StopRotator>();
                curentPhase.Events.Add(StR);
                StR.obj = ModManager.asset;
                break;
            case "From-ToRot":
                FromToRot FT = CreateInstance<FromToRot>();
                curentPhase.Events.Add(FT);
                FT.obj = ModManager.asset;
                break;
            case "To-FromRot":
                ToFromRot TF = CreateInstance<ToFromRot>();
                curentPhase.Events.Add(TF);
                TF.obj = ModManager.asset;
                break;
            case "From-ToMove":
                FromToMove FTM = CreateInstance<FromToMove>();
                curentPhase.Events.Add(FTM);
                FTM.obj = ModManager.asset;
                break;
            case "To-FromMove":
                ToFromMove TFM = CreateInstance<ToFromMove>();
                curentPhase.Events.Add(TFM);
                TFM.obj = ModManager.asset;
                break;
            case "ApplyRotations":
                ApplyRotation AP = CreateInstance<ApplyRotation>();
                curentPhase.Events.Add(AP);
                AP.obj = ModManager.asset;
                break;
            case "ChangePendulum":
                ChangePendulum WU = CreateInstance<ChangePendulum>();
                curentPhase.Events.Add(WU);
                WU.obj = ModManager.asset;
                break;
            case "Rotator":
                Rotator R = CreateInstance<Rotator>();
                ModManager.asset.Animation.motors.Add(R);
                R.Identifier = "Rotator_" + ModManager.asset.Animation.motors.Count();

                break;
            case "PendulumRotator":
                PendulumRotator PR = CreateInstance<PendulumRotator>();
                ModManager.asset.Animation.motors.Add(PR);
                PR.Identifier = "PendulumRotator_" + ModManager.asset.Animation.motors.Count();

                break;
            case "RotateBetween":
                RotateBetween RB = CreateInstance<RotateBetween>();
                ModManager.asset.Animation.motors.Add(RB);
                RB.Identifier = "RotateBetween_" + ModManager.asset.Animation.motors.Count();

                break;
            case "Mover":
                Mover M = CreateInstance<Mover>();
                ModManager.asset.Animation.motors.Add(M);
                M.Identifier = "Mover_" + ModManager.asset.Animation.motors.Count();

                break;
            case "MultiplyRotations":
                MultipleRotations MR = CreateInstance<MultipleRotations>();
                ModManager.asset.Animation.motors.Add(MR);
                MR.Identifier = "MultiplyRotations_" + ModManager.asset.Animation.motors.Count();

                break;
            default:
                
                break;
        }
    }
    void OnSelectionChange()
    {
        Repaint();
    }
    void OnGUI()
    {
        if (ModManager && ModManager.asset != null && ModManager.asset.type == ParkitectObject.ObjType.FlatRide)
        {

            EditorUtility.SetDirty(ModManager);
            foreach(Phase p in ModManager.asset.Animation.phases)
            {
                foreach (RideAnimationEvent RAE in p.Events)
                {
                    EditorUtility.SetDirty(RAE);
                }

            }
            foreach(motor m in ModManager.asset.Animation.motors)
                EditorUtility.SetDirty(m);
            Event e = Event.current;
            if (ModManager.asset.Animation == null)
            {
                ModManager.asset.Animation = new RideAnimation();
            }
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("grey_border", GUILayout.Width(300));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawToolStripEvents();
            GUILayout.EndHorizontal();
            scrollPosMotors = EditorGUILayout.BeginScrollView(scrollPosMotors, "grey_border");
            foreach (motor m in ModManager.asset.Animation.motors)
            {

               
                EditorGUILayout.BeginVertical("ShurikenEffectBg");
                GUI.color = Color.Lerp(new Color(m.ColorIdentifier.r, m.ColorIdentifier.g, m.ColorIdentifier.b), new Color(1, 1, 1), .7f);
                if (GUILayout.Button(m.Identifier, "ShurikenModuleTitle"))
                {

                    GUI.FocusControl("");
                    m.showSettings = !m.showSettings;
                    if (e.button == 1)
                    {
                        ModManager.asset.Animation.motors.Remove(m);
                        DestroyImmediate(m);
                        return;
                    }


                }

                GUI.color = Color.white;
                if (m.showSettings)
                    m.DrawGUI();
                EditorGUILayout.EndVertical();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add Motor"))
            {
                GenericMenu menu2 = new GenericMenu();
                menu2.AddItem(new GUIContent("Rotator"), false, AddEvent, "Rotator");
                menu2.AddItem(new GUIContent("RotateBetween"), false, AddEvent, "RotateBetween");
                menu2.AddItem(new GUIContent("Mover"), false, AddEvent, "Mover");
                menu2.AddItem(new GUIContent("MultiplyRotations"), false, AddEvent, "MultiplyRotations");
                menu2.AddItem(new GUIContent("PendulumRotator"), false, AddEvent, "PendulumRotator");
                menu2.ShowAsContext();
                e.Use();
                
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Add Phase"))
            {
                // Now create the menu, add items and show it
                
                ModManager.asset.Animation.phases.Add(new Phase());
            }
            EditorGUILayout.EndVertical();
            scrollPosPhases = EditorGUILayout.BeginScrollView(scrollPosPhases, "grey_border", GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();
            if (ModManager.asset.Animation != null)
            {
                for (int i = 0; i < ModManager.asset.Animation.phases.Count; i++) // Loop with for.
                {
                    if (ModManager.asset.Animation.phases[i] == ModManager.asset.Animation.currentPhase)
                    {
                        GUI.color = Color.Lerp(Color.green, new Color(1, 1, 1), .7f);
                    }
                    EditorGUILayout.BeginVertical("ProgressBarBack", GUILayout.MaxWidth(120), GUILayout.MinWidth(120), GUILayout.ExpandHeight(true));
                    GUILayout.Button("Phase " + (i + 1), "ShurikenModuleTitle");

                    if (GUILayout.Button(" Add Event", "OL Plus"))
                    {
                        GUI.FocusControl("");
                        curentPhase = ModManager.asset.Animation.phases[i];
                        // Now create the menu, add items and show it
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("WaitForSeconds"), false, AddEvent, "Wait");
                        menu.AddItem(new GUIContent("Rotator/StartRotator"), false, AddEvent, "StartRotator");
                        menu.AddItem(new GUIContent("Rotator/SpinRotator"), false, AddEvent, "SpinRotator");
                        menu.AddItem(new GUIContent("Rotator/StopRotator"), false, AddEvent, "StopRotator");
                        menu.AddItem(new GUIContent("RotateBetween/FromTo"), false, AddEvent, "From-ToRot");
                        menu.AddItem(new GUIContent("RotateBetween/ToFrom"), false, AddEvent, "To-FromRot");
                        menu.AddItem(new GUIContent("Mover/FromTo"), false, AddEvent, "From-ToMove");
                        menu.AddItem(new GUIContent("Mover/ToFrom"), false, AddEvent, "To-FromMove");
                        menu.AddItem(new GUIContent("MultiplyRotations/ApplyRotations"), false, AddEvent, "ApplyRotations");
                        menu.AddItem(new GUIContent("PendulumRotator/ChangePendulum"), false, AddEvent, "ChangePendulum");
                        menu.ShowAsContext();
                        e.Use();
                    }
                    foreach (RideAnimationEvent RAE in ModManager.asset.Animation.phases[i].Events)
                    {

                        EditorGUILayout.BeginVertical("ShurikenEffectBg");
                        GUI.color = Color.Lerp(new Color(RAE.ColorIdentifier.r, RAE.ColorIdentifier.g, RAE.ColorIdentifier.b), new Color(1, 1, 1), .7f);
                        string Done = "";
                        if(ModManager.asset.Animation.animating && RAE.done)
                            Done = " ✓";
                        if (GUILayout.Button(RAE.EventName + Done, "ShurikenModuleTitle"))
                        { GUI.FocusControl("");
                            RAE.showSettings = !RAE.showSettings;
                            if (e.button == 1)
                            {
                                ModManager.asset.Animation.phases[i].Events.Remove(RAE);
                                return;
                            }

                        }
                        GUI.color = Color.white;
                        if (RAE.showSettings)
                            RAE.DrawGUI();
                        EditorGUILayout.EndVertical();
                    }
                    GUILayout.FlexibleSpace();

                  

                    EditorGUILayout.BeginHorizontal();
                    if (i != 0)
                    {

                        if (GUILayout.Button("<<", "minibuttonleft"))
                        {
                            ModManager.asset.Animation.phases.Move(i, ListExtensions.MoveDirection.Up);
                        }
                    }
                    else
                    {
                        GUILayout.Button("  ", "minibuttonleft");
                    }
                    if (GUILayout.Button("Clone", "minibuttonmid"))
                    {

                        ModManager.asset.Animation.phases.Add(ModManager.asset.Animation.phases[i].ShallowCopy());
                    }
                    if (i != ModManager.asset.Animation.phases.Count -1 )
                    { 
                        if (GUILayout.Button(">>", "minibuttonright"))
                        {
                            ModManager.asset.Animation.phases.Move(i, ListExtensions.MoveDirection.Down);
                        }
                    }
                    else
                    {
                        GUILayout.Button("  ", "minibuttonright");
                    }
                    EditorGUILayout.EndHorizontal();
                    if (GUILayout.Button(" Delete Phase", "OL Minus"))
                    {
                        ModManager.asset.Animation.phases.Remove(ModManager.asset.Animation.phases[i]);
                    }
                    EditorGUILayout.EndVertical();
                    GUI.color = Color.white;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("© H-POPS - " + ModWindow.version, "PreToolbar");
        }
    }
    public enum MoveDirection
    {
        Up,
        Down
    }
   

    void OnFocus()
    {
        if (FindObjectOfType<ParkitectModManager>())
        {
            ModManager = FindObjectOfType<ParkitectModManager>();
        }
        else
        {
            ModManager = null;
        }

        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        // Add (or re-add) the delegate.
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;

    }
    void ResetMotors()
    {
        foreach (motor m in ModManager.asset.Animation.motors)
        {
            m.Reset();
        }
        foreach(MultipleRotations R in ModManager.asset.Animation.motors.OfType<MultipleRotations>().ToList())
        {
            R.Reset();
        }
    }
    void DrawToolStripEvents()
    {
        string button;
        if (isPlaying)
        {
            button = "Stop";
            GUI.color = Color.red / 1.3f;
        }
        else
        {
            button = "Play";
            GUI.color = Color.green / 1.3f;
        }
        
        if (isPlaying)
        {
           if(!ModManager.asset.Animation.animating)
            {
                ResetMotors();
                ModManager.asset.Animation.currentPhase = null;
            }
            isPlaying = ModManager.asset.Animation.animating;
            
            
        }
        if (GUILayout.Button(button, EditorStyles.toolbarButton))
        {
           ReflectionProbes = FindObjectsOfType<ReflectionProbe>().ToList();
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                ModManager.asset.Animation.Animate();
                isPlaying = ModManager.asset.Animation.animating;

            }
            else
            {
                ResetMotors();

                ModManager.asset.Animation.currentPhase = null;
            }
        }
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();

    }
    
    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }
    void OnSceneGUI(SceneView sceneView)
    {

        if (ModManager && ModManager.asset != null && ModManager.asset.type == ParkitectObject.ObjType.FlatRide)
        {
            
            foreach (Rotator R in ModManager.asset.Animation.motors.OfType<Rotator>().ToList())
            {

                if (R && R.axis)
                {
                    if (R.rotationAxis != Vector3.zero)
                    {
                        Handles.color = new Color(R.ColorIdentifier.r, R.ColorIdentifier.g, R.ColorIdentifier.b, 1);
                        Handles.CircleCap(0, R.axis.position, R.axis.rotation * Quaternion.LookRotation(R.rotationAxis), .3f);
                        Handles.color = new Color(R.ColorIdentifier.r, R.ColorIdentifier.g, R.ColorIdentifier.b, 0.1f);
                        Handles.DrawSolidDisc(R.axis.position, R.axis.rotation * R.rotationAxis, .3f);
                    }
                }
            }
            foreach (RotateBetween R in ModManager.asset.Animation.motors.OfType<RotateBetween>().ToList())
            {

                if (R && R.axis)
                {

                    
                    Color color = new Color(R.ColorIdentifier.r, R.ColorIdentifier.g, R.ColorIdentifier.b, .8f);
                    float rayRange = 2.0f;
                    Quaternion leftRayRotation = R.axis.localRotation;
                    Quaternion rightRayRotation =  Quaternion.Euler(R.axis.localEulerAngles + R.rotationAxis);
                    if (!isPlaying)
                    {
                        Vector3 leftRayDirection = leftRayRotation * R.axis.up;
                        Vector3 rightRayDirection = rightRayRotation * R.axis.up;
                        Debug.DrawRay(R.axis.position, leftRayDirection * rayRange, color);
                        Debug.DrawRay(R.axis.position, rightRayDirection * rayRange, color);
                        
                    }
                   
                }
            }
            foreach (Mover R in ModManager.asset.Animation.motors.OfType<Mover>().ToList())
            {

                if (R && R.axis)
                {

                    Handles.color = new Color(R.ColorIdentifier.r, R.ColorIdentifier.g, R.ColorIdentifier.b, 1);
                    Handles.DrawLine(R.axis.position, R.axis.TransformPoint(R.toPosition) - R.axis.localPosition);
                    Handles.color = new Color(R.ColorIdentifier.r, R.ColorIdentifier.g, R.ColorIdentifier.b, 0.5f);
                    Handles.SphereCap(0, R.axis.position, R.axis.rotation, .1f);
                    Handles.SphereCap(0, R.axis.TransformPoint(R.toPosition) - R.axis.localPosition, R.axis.rotation, .1f);
                }
            }
            Handles.BeginGUI();

            
            Handles.EndGUI();
        }
    }
    private void CallbackFunction()
    {
        if(isPlaying)
        {
            foreach(ReflectionProbe RP in ReflectionProbes)
            {
                RP.RenderProbe();
            }
            repaintWindow();

            ModManager.asset.Animation.Run();
        }
    }
    void OnDisable()
    {
        EditorApplication.update -= CallbackFunction;
    }

}
public static class ListExtensions
{
    public static void Move<T>(this IList<T> list, int iIndexToMove,
        MoveDirection direction)
    {

        if (direction == MoveDirection.Up)
        {
            var old = list[iIndexToMove - 1];
            list[iIndexToMove - 1] = list[iIndexToMove];
            list[iIndexToMove] = old;
        }
        else
        {
            var old = list[iIndexToMove + 1];
            list[iIndexToMove + 1] = list[iIndexToMove];
            list[iIndexToMove] = old;
        }
    }
    public enum MoveDirection
    {
        Up,
        Down
    }
}

