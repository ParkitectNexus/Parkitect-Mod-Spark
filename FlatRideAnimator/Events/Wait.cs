using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
[Serializable]
[ExecuteInEditMode]
public class Wait : RideAnimationEvent
{
    [SerializeField]
    public float seconds;
    float timeLimit;
    public override string EventName
    {
        get
        {
            return "Wait";
        }
    }
    public override void DrawGUI()
    {

#if UNITY_EDITOR
        seconds = EditorGUILayout.FloatField("Seconds", seconds);
        if (isPlaying)
        {
            GUILayout.Label("Time" + (timeLimit - Time.realtimeSinceStartup));
        }
        base.DrawGUI();
#endif
    }

    public override void Enter()
    {
        
        timeLimit = Time.realtimeSinceStartup + seconds;
        base.Enter();
    }
    public override void Run()
    {
        if (Time.realtimeSinceStartup > timeLimit)
        {

            done = true;
        }
        else
        {

        }
        base.Run();
    }

}
