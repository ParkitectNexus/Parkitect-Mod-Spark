using System;
using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
[Serializable]
public class Mover : motor
{

    private enum State
    {
        RUNNING,
        STOPPED
    }
    [SerializeField]
    public Transform axis;
    [SerializeField]
    public Vector3 originalRotationValue;
    [SerializeField]
    private Vector3 fromPosition;
    [SerializeField]
    public Vector3 toPosition;
    [SerializeField]
    public float duration = 10f;

    [SerializeField]
    private Mover.State currentState = Mover.State.STOPPED;

    [SerializeField]
    private float currentPosition = 1f;

    [SerializeField]
    private int direction = -1;

    public override void Reset()
    {
        if (axis)
            axis.localPosition = originalRotationValue;
        currentPosition = 1f;
        direction = -1;
        base.Reset();
    }
    public override string EventName
    {
        get
        {
            return "Mover";
        }
    }
    public override void DrawGUI()
    {

        Identifier = EditorGUILayout.TextField("Name ", Identifier);
        axis = (Transform)EditorGUILayout.ObjectField("axis", axis, typeof(Transform), true);
        toPosition = EditorGUILayout.Vector3Field("Move To", toPosition);
        duration = EditorGUILayout.FloatField("Time", duration);
        base.DrawGUI();

    }
    public override void Enter()
    {
        if(axis)
        originalRotationValue = axis.localPosition;
        this.currentPosition = 1f;

        direction = -1;
        Initialize(axis, axis.localPosition, toPosition, duration);
        base.Enter();
    }
    public void Initialize(Transform axis, Vector3 fromPosition, Vector3 toPosition, float duration)
    {
        this.axis = axis;
        this.fromPosition = fromPosition;


        this.toPosition = toPosition;
        this.duration = duration;
        this.setPosition();
    }

    public bool startFromTo()
    {
        if (this.direction != 1)
        {
            this.direction = 1;
            this.currentPosition = 0f;
            this.currentState = Mover.State.RUNNING;
            return true;
        }
        return false;
    }

    public bool startToFrom()
    {
        if (this.direction != -1)
        {
            this.direction = -1;
            this.currentPosition = 0f;
            this.currentState = Mover.State.RUNNING;
            return true;
        }
        return false;
    }

    public bool reachedTarget()
    {
        return this.currentState == Mover.State.STOPPED && this.currentPosition >= 1f;
    }

    public void tick(float dt)
    {
        this.currentPosition += dt * 1f / this.duration;
        if (this.currentPosition >= 1f)
        {
            this.currentPosition = 1f;
            this.currentState = Mover.State.STOPPED;
        }
        this.setPosition();
    }

    private void setPosition()
    {
        Vector3 a;
        Vector3 b;
        if (this.direction == 1)
        {
            a = this.fromPosition;
            b = this.toPosition;
        }
        else
        {
            a = this.toPosition;
            b = this.fromPosition;
        }
        this.axis.localPosition = Vector3.Lerp(a, b, Mathfx.Hermite(0f, 1f, this.currentPosition));
    }
}
