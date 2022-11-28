using UnityEngine;
using System;
using System.Collections;

public sealed class RotaryEncoder : StandardRobotSensor
{
    private float previousAngle;
    private Quaternion neutralRotationInv;

    public float Angle;

    static float Angle360(Vector3 v1, Vector3 v2, Vector3 n)
    {
        //  Acute angle [0,180]
        float angle = Vector3.Angle(v1, v2);

        //  -Acute angle [180,-179]
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(v1, v2)));
        return angle * sign;
    }

    private Vector3 GetAxis()
    {
        var hinge = GetComponent<HingeJoint>();
        if (hinge)
        {
            return hinge.axis;
        }
        var cjoint = GetComponent<ConfigurableJoint>();
        if (cjoint)
        {
            return cjoint.axis;
        }
        return Vector3.right; // X axis
    }

    private float GetAngle()
    {
        var q = neutralRotationInv * transform.localRotation;
        var axis = GetAxis();
        var tangent = Vector3.Cross(axis, new Vector3(-axis.z, axis.x, axis.y));
        var q_tangent = q * tangent;
        var p_tangent = Vector3.ProjectOnPlane(q_tangent, axis);
        return Angle360(tangent, p_tangent, axis);
    }

    void Awake()
    {
        neutralRotationInv = Quaternion.Inverse(transform.localRotation);
        previousAngle = GetAngle();
    }

    void FixedUpdate()
    {
        float angle = GetAngle();
        float delta = angle - previousAngle;
        while (delta >= 180) delta -= 360;
        while (delta < -180) delta += 360;
        Angle += delta;
        previousAngle = angle;
    }

    public override int SensorValue
    {
        get
        {
            return (int)Angle;
        }
    }
}
