using UnityEngine;

public class RotaryEncoderImpl {
    static float Angle360(Vector3 v1, Vector3 v2, Vector3 n)
    {
        //  Acute angle [0,180]
        float angle = Vector3.Angle(v1, v2);

        //  -Acute angle [180,-179]
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(v1, v2)));
        return angle * sign;
    }

    private readonly Transform transform;
    private float previousAngle;
    private Quaternion neutralRotationInv;

    public float Angle { get; private set; }
    public float Velocity { get; private set; }

    public RotaryEncoderImpl(Transform body)
    {
        transform = body;
        neutralRotationInv = Quaternion.Inverse(transform.localRotation);
        previousAngle = GetAngle();
    }

    private Vector3 GetAxis()
    {
        var hinge = transform.GetComponent<HingeJoint>();
        if (hinge)
        {
            return hinge.axis;
        }
        var cjoint = transform.GetComponent<ConfigurableJoint>();
        if (cjoint)
        {
            return cjoint.axis;
        }
        var articBody = transform.GetComponent<ArticulationBody>();
        if (articBody)
        {
            // TODO: return articBody.anchorRotation * Vector3.right;
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

    public void FixedUpdate()
    {
        float angle = GetAngle();
        float delta = angle - previousAngle;
        previousAngle = angle;
        while (delta >= 180) delta -= 360;
        while (delta < -180) delta += 360;
        Angle += delta;
        Velocity = delta / Time.fixedDeltaTime;
    }
}
