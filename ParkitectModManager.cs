using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using System.Linq;

public class ParkitectModManager : MonoBehaviour
{

    [SerializeField]
    public Mod mod = new Mod();

    [SerializeField]
    public List<ParkitectObject> ParkitectObjects = new List<ParkitectObject>();

    public ParkitectObject asset = null;
    // Use this for initialization

    public void ValidateSelected()
    {
        if (!ParkitectObjects.Contains(asset))
        {
            asset = null;
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
[Serializable]
public class Mod
{
    public string name = "New Parkitect Mod";
    public string discription = "New Parkitect Mod";
}
[Serializable]
public class ParkitectObject
{
    //Basic
    public enum ObjType
    {
        none,
        _,
        deco,
        wall,
        trashbin,
        seating,
        lamp,
        fence,
        FlatRide,
        Shop,
        PathStyle,
        CoasterCar
    }
    public GameObject gameObject;
    public string inGameName;
    public ObjType type = ObjType.none;
    public float price;

    //Recolorable
    public bool recolorable;
    public Color color1 = new Color(0.95f, 0, 0);
    public Color color2 = new Color(0.32f, 1, 0);
    public Color color3 = new Color(0.110f, 0.059f, 1f);
    public Color color4 = new Color(1, 0, 1);
    public enum Shaders
    {
        Diffuse,
        Specular,
        None
    }
    public Shaders Shader;

    //Deco
    public bool snapCenter = true;
    public bool snap;
    public bool grid;
    public float heightDelta;
    public string category;
    public float gridSubdivision = 1;

    //Flat-Rides
    public float Excitement;
    public float Intensity;
    public float Nausea;
    public float XSize = 1;
    public float ZSize = 1;
    public Vector3 closedAngleRetraints;
    public List<Waypoint> waypoints = new List<Waypoint>();
    //--Animation
    public RideAnimation Animation = new RideAnimation();
    private Phase phase;

    //Boundingbox
    public List<BoundingBox> BoundingBoxes = new List<BoundingBox>();

    //Shop
    [SerializeField]
    public Shop shop = new Shop();

    //Path
    public enum PathType { Normal, Queue, Employee }
    public PathType pathType;
    public Texture2D PathTexture;
    //Coaster Car
    public string CoasterName = "Steel Coaster";
    public GameObject frontCar;

}




[Serializable]
[ExecuteInEditMode]
public class RideAnimation
{

    [SerializeField]
    public List<motor> motors = new List<motor>();

    [SerializeField]
    public List<Phase> phases = new List<Phase>();

    [SerializeField]
    public Phase currentPhase;
    int phaseNum;

    [SerializeField]
    public bool animating;
    public void Animate()
    {
        foreach (motor m in motors)
        {
            m.Enter();
        }
        if (phases.Count <= 0)
        {
            animating = false;
            foreach (motor m in motors)
            {
                m.Reset();
            }
            foreach (MultipleRotations R in motors.OfType<MultipleRotations>().ToList())
            {
                R.Reset();
            }
            return;
        }
        foreach (motor m in motors)
        {
            m.Enter();
        }

        animating = true;
        phaseNum = 0;
        currentPhase = phases[phaseNum];
        currentPhase.running = true;
        currentPhase.Enter();
        currentPhase.Run();
    }
    void NextPhase()
    {

        currentPhase.Exit();
        currentPhase.running = false;
        phaseNum++;
        if (phases.Count > phaseNum)
        {
            currentPhase = phases[phaseNum];
            currentPhase.running = true;
            currentPhase.Enter();
            currentPhase.Run();
            return;
        }
        animating = false;
        foreach (motor m in motors.OfType<Rotator>().ToList())
        {
            m.Enter();

        }
        foreach (Rotator m in motors.OfType<Rotator>().ToList())
        {
            m.axis.localRotation = m.originalRotationValue;

        }
        foreach (RotateBetween m in motors.OfType<RotateBetween>().ToList())
        {
            m.axis.localRotation = m.originalRotationValue;

        }
        foreach (Mover m in motors.OfType<Mover>().ToList())
        {
            m.axis.localPosition = m.originalRotationValue;

        }

        currentPhase = null;
    }
    public void Run()
    {
        if (currentPhase != null)
        {
            currentPhase.Run();
            if (!currentPhase.running)
            {
                NextPhase();
            }
        }

    }
}


[ExecuteInEditMode]
[Serializable]
public class Phase
{
    [SerializeField]
    public List<RideAnimationEvent> Events = new List<RideAnimationEvent>();
    public bool running = false;
    bool done = false;
    public void Enter()
    {
        foreach (RideAnimationEvent RAE in Events)
        {
            RAE.Enter();
        }
    }
    public Phase ShallowCopy()
    {
        return (Phase)this.MemberwiseClone();
    }
    public void Run()
    {
        foreach (RideAnimationEvent RAE in Events)
        {
            RAE.Run();
        }
        done = true;
        foreach (RideAnimationEvent RAE in Events)
        {
            if (!RAE.done)
            {
                running = true;
                done = false;
                break;
            }
        }
        if (done)
        {
            running = false;
        }

    }
    public void Exit()
    {
        foreach (RideAnimationEvent RAE in Events)
        {
            RAE.Exit();
        }
    }

}

[ExecuteInEditMode]
[Serializable]
public class RideAnimationEvent : ScriptableObject
{
    public bool done = false;
    public bool showSettings;
    public bool isPlaying;
    public Color ColorIdentifier;
    public virtual string EventName { set; get; }

    public virtual void DrawGUI()
    {

    }
    public virtual void Enter()
    {
        isPlaying = true;
    }
    public virtual void Run()
    {

    }
    public virtual void Exit()
    {

        isPlaying = false;
        done = false;
    }

}
[Serializable]
public class motor : ScriptableObject
{
    public bool showSettings;
    public string Identifier = "";
    public Color ColorIdentifier;
    public virtual string EventName { set; get; }
    public void Awake()
    {
        ColorIdentifier = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
    }
    public virtual void DrawGUI()
    {
        ColorIdentifier = EditorGUILayout.ColorField("Color ", ColorIdentifier);
    }
    public virtual void Enter()
    {

    }
    public virtual void Reset()
    {

    }
}