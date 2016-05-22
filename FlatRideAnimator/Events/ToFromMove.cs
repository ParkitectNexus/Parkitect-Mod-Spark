
using UnityEditor;
using System;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[Serializable]
public class ToFromMove : RideAnimationEvent
{
    public Mover rotator;
    public ParkitectObject obj;

    float lastTime;
    public override string EventName
    {
        get
        {
            return "To-From Move";
        }
    }
    public override void DrawGUI()
    {
        if (rotator)
        {
            ColorIdentifier = rotator.ColorIdentifier;
        }
        foreach (Mover R in obj.Animation.motors.OfType<Mover>().ToList())
        {
            if (R == rotator)
                GUI.color = Color.red/ 1.3f;
            if(GUILayout.Button(R.Identifier))
            {
                rotator = R;
            }
            GUI.color = Color.white;
        }
        base.DrawGUI();
    }

    public override void Enter()
    {
        lastTime = Time.realtimeSinceStartup;

        rotator.startToFrom();
        base.Enter();
    }
    public override void Run()
    {

        if (rotator)
        {
            
            
            rotator.tick(Time.realtimeSinceStartup - lastTime);
            lastTime = Time.realtimeSinceStartup;
            if (rotator.reachedTarget())
            {
                done = true;
            }
            base.Run();
        }
        
    }
}
