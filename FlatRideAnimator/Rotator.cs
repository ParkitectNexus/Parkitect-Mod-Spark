using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[Serializable]
public class Rotator : motor
{
    public enum State
    {
        STARTING,
        RUNNING,
        PAUSING,
        REQUEST_STOP,
        STOPPING
    }
    [SerializeField]
    public Quaternion originalRotationValue;
    [SerializeField]
    public float accelerationSpeed = 12f;
    [SerializeField]
    public float maxSpeed = 180f;
    [SerializeField]
    public Vector3 rotationAxis = Vector3.up;
    [SerializeField]
    public int rotationAxisIndex = 1;
    [SerializeField]
    public float minRotationSpeedPercent = 0.3f;
    [SerializeField]
    public Quaternion initialRotation;

    [SerializeField]
    public Rotator.State currentState = Rotator.State.STOPPING;

    [SerializeField]
    public float currentSpeed;

    [SerializeField]
    public float currentRotation;

    [SerializeField]
    public int direction = 1;

    [SerializeField]
    public Transform axis;

    public override void Reset()
    {
        if(axis)
        axis.localRotation = originalRotationValue;
        base.Reset();
    }
    public override string EventName
    {
        get
        {
            return "Rotator";
        }
    }
    public override void DrawGUI()
    {

        Identifier = EditorGUILayout.TextField("Name ", Identifier);
        axis = (Transform)EditorGUILayout.ObjectField("axis", axis, typeof(Transform),true);
        maxSpeed = EditorGUILayout.FloatField("maxSpeed", maxSpeed);
        accelerationSpeed = EditorGUILayout.FloatField("accelerationSpeed", accelerationSpeed);
        rotationAxis = EditorGUILayout.Vector3Field("rotationAxis", rotationAxis);
        base.DrawGUI();
    }
    public override void Enter()
    {
        originalRotationValue = axis.localRotation;
        resetRotations();
        this.currentRotation = 0;
        currentSpeed = 0;
        changeState(State.STARTING);
        Initialize(axis, accelerationSpeed, maxSpeed, rotationAxis);
        base.Enter();
    }
        
    public void Initialize(Transform axis, float accelerationSpeed, float maxSpeed)
    {
        this.Initialize(axis, accelerationSpeed, maxSpeed, Vector3.up);
    }

    public void Initialize(Transform axis, float accelerationSpeed, float maxSpeed, Vector3 rotationAxis)
    {
        this.axis = axis;
        this.accelerationSpeed = accelerationSpeed;
        this.maxSpeed = maxSpeed;
        this.setRotationAxis(rotationAxis);
        this.setInitialRotation(axis.localRotation);
        axis.Rotate(rotationAxis, this.currentRotation);
    }

    public void setInitialRotation(Quaternion initialLocalRotation)
    {
        this.initialRotation = initialLocalRotation;
    }

    public void setMinRotationSpeedPercent(float minRotationSpeedPercent)
    {
        this.minRotationSpeedPercent = minRotationSpeedPercent;
    }

    private void setRotationAxis(Vector3 rotationAxis)
    {
        this.rotationAxis = rotationAxis;
        if (rotationAxis.x != 0f)
        {
            this.rotationAxisIndex = 0;
        }
        else if (rotationAxis.y != 0f)
        {
            this.rotationAxisIndex = 1;
        }
        else if (rotationAxis.z != 0f)
        {
            this.rotationAxisIndex = 2;
        }
    }

    public bool start()
    {
        if (this.currentState != Rotator.State.STARTING && this.currentState != Rotator.State.RUNNING)
        {
            this.changeState(Rotator.State.STARTING);
            this.currentSpeed = 0f;
            this.currentRotation = 0f;
            return true;
        }
        return false;
    }

    public void stop()
    {
        this.changeState(Rotator.State.REQUEST_STOP);
    }

    public void pause()
    {
        this.changeState(Rotator.State.PAUSING);
    }

    public bool isStopped()
    {
        return this.currentState == Rotator.State.STOPPING && Mathf.Approximately(this.currentSpeed, 0f);
    }

    public Rotator.State getState()
    {
        return this.currentState;
    }

    public void resetRotations()
    {
        this.currentRotation = 0f;
    }

    public float getRotationsCount()
    {
        return Mathf.Abs(this.currentRotation) / 360f;
    }

    public int getCompletedRotationsCount()
    {
        return Mathf.FloorToInt(this.getRotationsCount());
    }

    public bool isInAngleRange(float fromAngle, float toAngle)
    {
        fromAngle %= 360f;
        toAngle %= 360f;
        float num = this.axis.localEulerAngles[this.rotationAxisIndex];
        if (fromAngle >= toAngle)
        {
            return num >= fromAngle || num <= toAngle;
        }
        return num < toAngle && num > fromAngle;
    }

    public bool reachedFullSpeed()
    {
        return this.currentState != Rotator.State.STARTING;
    }

    public float getCurrentSpeed()
    {
        return this.currentSpeed;
    }

    public float getMaxSpeed()
    {
        return this.maxSpeed;
    }

    public void setDirection(int direction)
    {
        this.direction = direction;
    }

    public int getDirection()
    {
        return this.direction;
    }

    public void changeState(Rotator.State newState)
    {
        this.currentState = newState;
        
    }

    public virtual void tick(float dt)
    {
        float num = this.currentSpeed * dt;
        this.currentRotation += num;
        if (this.currentState == Rotator.State.STARTING || this.currentState == Rotator.State.RUNNING || this.currentState == Rotator.State.PAUSING)
        {
            this.axis.Rotate(this.rotationAxis, num * (float)this.direction);
        }
        if (this.currentState == Rotator.State.STARTING)
        {
            if (this.currentSpeed < this.maxSpeed)
            {
                this.currentSpeed += dt * this.accelerationSpeed;
            }
            else
            {
                this.changeState(Rotator.State.RUNNING);
            }
        }
        else if (this.currentState == Rotator.State.PAUSING)
        {
            this.currentSpeed -= dt * this.accelerationSpeed;
            if (this.currentSpeed < 0f)
            {
                this.currentSpeed = 0f;
            }
        }
        else if (this.currentState == Rotator.State.REQUEST_STOP)
        {
            this.currentSpeed -= dt * this.accelerationSpeed;
            this.currentSpeed = Mathf.Max(this.maxSpeed * this.minRotationSpeedPercent - 0.01f, this.currentSpeed);
            if (this.currentSpeed < this.maxSpeed * this.minRotationSpeedPercent)
            {
                float num2 = this.axis.localEulerAngles[this.rotationAxisIndex] - this.initialRotation.eulerAngles[this.rotationAxisIndex] + 180f;
                float num3 = num2 - 360f * Mathf.Round(num2 / 360f);
                if ((num3 > 0f && this.direction > 0) || (num3 < 0f && this.direction < 0))
                {
                    this.changeState(Rotator.State.STOPPING);
                }
            }
            this.axis.Rotate(this.rotationAxis, num * (float)this.direction);
        }
        else if (this.currentState == Rotator.State.STOPPING && this.currentSpeed != 0f)
        {
            float b = Quaternion.Angle(this.axis.localRotation, this.initialRotation);
            this.currentSpeed = Mathf.Min(this.currentSpeed, b);
            this.axis.localRotation = Quaternion.RotateTowards(this.axis.localRotation, this.initialRotation, Mathf.Max(1f, this.currentSpeed) * dt);
            float num4 = this.axis.localEulerAngles[this.rotationAxisIndex] - this.initialRotation.eulerAngles[this.rotationAxisIndex];
            float num5 = num4 - 360f * Mathf.Round(num4 / 360f);
            if ((num5 > 0f && this.direction > 0) || (num5 < 0f && this.direction < 0))
            {
                this.axis.localRotation = this.initialRotation;
                this.currentSpeed = 0f;
            }
        }
    }
}
