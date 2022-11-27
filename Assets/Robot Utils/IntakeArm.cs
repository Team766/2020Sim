using UnityEngine;
using System;
using System.Collections;

public class IntakeArm : MonoBehaviour
{
    private Vector3 neutralPosition;
    private Quaternion neutralRotation;
    private float previousAngle;
    public float maxSpeed;

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
        return Vector3.right;
    }

    private float GetAngle()
    {
        Vector3 offset = transform.localRotation * Vector3.forward;
        Vector3 normal = neutralRotation * GetAxis();
        offset = Vector3.ProjectOnPlane(offset, normal);
        return Angle360(offset, Vector3.forward, normal);
    }

    void Awake()
    {
        neutralPosition = transform.localPosition;
        neutralRotation = transform.localRotation;
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

    public float Angle;

    public int Encoder
    {
        get
        {
            return (int)Angle;
        }
    }

    public void RunJoint(float speed)
    {
        float targetVel = speed;

        // set joint motor parameters
        var hinge = GetComponent<HingeJoint>();
        if (hinge)
        {
            JointMotor myMotor = hinge.motor;
            myMotor.targetVelocity = targetVel;
            hinge.motor = myMotor;
        }
        var cjoint = GetComponent<ConfigurableJoint>();
        if (cjoint)
        {
            if (maxSpeed == 0)
            {
                throw new Exception("maxSpeed must be non-zero");
            }

            float appliedForce = targetVel;
            
            JointDrive drive = GetComponent<ConfigurableJoint>().angularXDrive;
            drive.positionSpring = 0;
            drive.positionDamper = Mathf.Abs(appliedForce / maxSpeed);
            drive.maximumForce = Mathf.Abs(appliedForce);
            GetComponent<ConfigurableJoint>().angularXDrive = drive;
            GetComponent<ConfigurableJoint>().targetAngularVelocity =
                Mathf.Sign(appliedForce) * maxSpeed * Vector3.right;
        }
    }
}
