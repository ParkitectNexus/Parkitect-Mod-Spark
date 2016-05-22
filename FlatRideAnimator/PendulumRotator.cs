using System;
using UnityEditor;
using UnityEngine;

public class PendulumRotator : Rotator
{
    [SerializeField]
    public float armLength;
    [SerializeField]
    public float gravity;
    [SerializeField]
    public float angularFriction;
    [SerializeField]
    public bool pendulum;

    public override void DrawGUI()
    {
        armLength = EditorGUILayout.FloatField("armLength ", armLength);
        gravity = EditorGUILayout.FloatField("gravity", gravity);
        angularFriction = EditorGUILayout.FloatField("angularFriction", angularFriction);
        pendulum = EditorGUILayout.Toggle("pendulum", pendulum);
        base.DrawGUI();
    }
    public override string EventName
    {
        get
        {
            return "";
        }
    }
    public void setActAsPendulum(bool pendulum)
    {
        this.pendulum = pendulum;
    }
    
    public override void tick(float dt)
    {

        if (!this.pendulum)
        {
            base.tick(dt);
            return;
        }
        float num = -1f * this.gravity * Mathf.Sin(base.axis.localEulerAngles[this.rotationAxisIndex] * 0.0174532924f) / this.armLength * 157.29578f;
        num = Mathf.Clamp(num, -this.accelerationSpeed, this.accelerationSpeed);
        this.currentSpeed += num * dt;
        this.currentRotation += num * dt;
        this.currentSpeed -= this.currentSpeed * this.angularFriction * dt;
        this.currentSpeed = Mathf.Clamp(this.currentSpeed, -this.maxSpeed, this.maxSpeed);
        Vector3 localEulerAngles = base.axis.localEulerAngles;
        int rotationAxisIndex;
        int expr_C6 = rotationAxisIndex = this.rotationAxisIndex;
        float num2 = localEulerAngles[rotationAxisIndex];
        localEulerAngles[expr_C6] = num2 + this.currentSpeed * dt;
        base.axis.localEulerAngles = localEulerAngles;
        if (this.currentState == Rotator.State.REQUEST_STOP && Mathf.Abs(this.currentSpeed) <= 0.5f && Mathf.Abs(num) <= 0.3f)
        {
            base.changeState(Rotator.State.STOPPING);
            base.axis.localRotation = this.initialRotation;
            this.currentSpeed = 0f;
        }
    }
}