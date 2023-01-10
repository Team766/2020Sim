using UnityEngine;
using System;
using System.Collections;

public class Wheel : StandardRobotJoint
{
    public float motorScaler;
    private Vector3 neutralPosition;
    private Quaternion neutralRotation;
    private Quaternion neutralRotationInv;
    public float maxSpeed;
    public float minForce;

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
        var rb = GetComponent<Rigidbody>();
        if (rb) {
            transform.localPosition = neutralPosition;
            transform.localRotation = neutralRotation * ProjectRotation(neutralRotationInv * transform.localRotation, GetAxis());
            rb.position = transform.position;
            rb.rotation = transform.rotation;
        }
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
        var articBody = GetComponent<ConfigurableJoint>();
        if (articBody)
        {
            // TODO: return articBody.anchorRotation * Vector3.right;
        }
        return Vector3.right; // X axis
    }

    public override void RunJoint(float command)
    {
        // set joint motor parameters
        var hinge = GetComponent<HingeJoint>();
        if (hinge)
        {
            float targetVel = command * motorScaler;

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

            float appliedForce = command * motorScaler;;

            JointDrive drive = cjoint.angularXDrive;
            drive.positionSpring = 0;
            drive.positionDamper = Mathf.Abs(appliedForce / maxSpeed);
            drive.maximumForce = Mathf.Abs(appliedForce);
            cjoint.angularXDrive = drive;
            cjoint.targetAngularVelocity = Mathf.Sign(appliedForce) * maxSpeed * Vector3.right;
        }
        var articBody = GetComponent<ArticulationBody>();
        if (articBody)
        {
            if (maxSpeed == 0)
            {
                throw new Exception("maxSpeed must be non-zero");
            }

            float targetVel = command * maxSpeed;
            float appliedForce = command * motorScaler;;

            ArticulationDrive drive = articBody.xDrive;
            drive.stiffness = 0;
            drive.damping = Mathf.Abs(maxSpeed / maxSpeed);
            drive.forceLimit = Mathf.Max(minForce, Mathf.Abs(appliedForce));
            drive.targetVelocity = targetVel;
            articBody.xDrive = drive;
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
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody) {
            Destroy(rigidbody);
            //rigidbody.isKinematic = true;
        }
        var articBody = GetComponent<ArticulationBody>();
        if (articBody) {
            Destroy(articBody);
        }
    }
}
