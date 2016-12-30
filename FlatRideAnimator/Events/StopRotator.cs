using System;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[Serializable]
public class StopRotator : RideAnimationEvent
{
    public Rotator rotator;
    float lastTime;
    public ParkitectObject obj;
    public override string EventName
    {
        get
        {
            return "StopRotator";
        }
    }
    public override void DrawGUI()
    {
        if (rotator)
        {
            ColorIdentifier = rotator.ColorIdentifier;
        }
        foreach (Rotator R in obj.Animation.motors.OfType<Rotator>().ToList())
        {
            if (R == rotator)
                GUI.color = Color.red / 1.3f;
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

        rotator.stop();
        base.Enter();
    }
    public override void Run()
    {

        if (rotator)
        {
            
            
            rotator.tick(Time.realtimeSinceStartup - lastTime);
            lastTime = Time.realtimeSinceStartup;
            if (rotator.isStopped())
            {
                done = true;
            }
            base.Run();
        }
        
    }
}
