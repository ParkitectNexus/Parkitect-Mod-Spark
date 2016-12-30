

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[Serializable]
public class ApplyRotation : RideAnimationEvent 
{
    [SerializeField]
    public MultipleRotations rotator;
    float lastTime;
    public ParkitectObject obj;


    public override string EventName
    {
        get
        {
            return "ApplyRotations";
        }
    }
    
    public override void DrawGUI()
    {
#if UNITY_EDITOR

        if (rotator)
        {
            ColorIdentifier = rotator.ColorIdentifier;
        }
        foreach (MultipleRotations R in obj.Animation.motors.OfType<MultipleRotations>().ToList())
        {
            if (R == rotator)
                GUI.color = Color.red / 1.3f;
            if (GUILayout.Button(R.Identifier))
            {
                rotator = R;

            }
            GUI.color = Color.white;
        }
        base.DrawGUI();
#endif
    }

    public override void Enter()
    {
        
    }
    public override void Run()
    {
        if (rotator)
        {


            rotator.tick(Time.realtimeSinceStartup - lastTime);
            lastTime = Time.realtimeSinceStartup;
            done = true;
            base.Run();
        }

    }
}
