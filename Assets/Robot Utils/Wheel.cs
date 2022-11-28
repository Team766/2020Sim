using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Wheel : StandardRobotJoint
{
    public float motorScaler;
    private Vector3 neutralPosition;
    private Quaternion neutralRotation;
    private Quaternion neutralRotationInv;
    public float maxSpeed;

    void Awake()
    {
        neutralPosition = transform.localPosition;
        neutralRotation = transform.localRotation;
        neutralRotationInv = Quaternion.Inverse(neutralRotation);
    }

    private static Quaternion ProjectRotation(Quaternion q, Vector3 axis)
    {
        var tangent = Vector3.Cross(axis, new Vector3(-axis.z, axis.x, axis.y));
        var q_tangent = q * tangent;
        var p_tangent = Vector3.ProjectOnPlane(q_tangent, axis);
        return Quaternion.FromToRotation(tangent, p_tangent);
    }

    void FixedUpdate()
    {
        transform.localPosition = neutralPosition;
        transform.localRotation = neutralRotation * ProjectRotation(neutralRotationInv * transform.localRotation, GetAxis());
        var rb = GetComponent<Rigidbody>();
        rb.position = transform.position;
        rb.rotation = transform.rotation;
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

    public override void RunJoint(float command)
    {
        float targetVel = command * motorScaler;

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

    public override void Disable() {
        RunJoint(0.0f);
    }

    public override void Destroy() {
        Destroy(this);
        var hinge = GetComponent<HingeJoint>();
        if (hinge) {
            Destroy(hinge);
        }
        var cjoint = GetComponent<ConfigurableJoint>();
        if (cjoint) {
            Destroy(cjoint);
        }
        Destroy(GetComponent<Rigidbody>());
        //GetComponent<Rigidbody>().isKinematic = true;
    }
}
