using UnityEngine;
using System;
using Team766.Simulator;
using System.Collections.Generic;

public class RotationalJoint : StandardRobotJoint
{
    private Vector3 neutralPosition;
    private Quaternion neutralRotation;
    private Quaternion neutralRotationInv;
    public float maxMotorSpeed = 100 * 360; // degrees per second
    public float maxMotorTorque = 7.09f;  // Newton-meters (at least when using ArticulationBody)
    public float minTorque;
    public bool inverted;
    public float mechanicalScalar = 1.0f;
    public float pGain, dGain, velocityPGain;
    private RotaryEncoderImpl encoderImpl;

    public double sensorVelocity;

    void Awake()
    {
        neutralPosition = transform.localPosition;
        neutralRotation = transform.localRotation;
        neutralRotationInv = Quaternion.Inverse(neutralRotation);
        encoderImpl = new RotaryEncoderImpl(transform);

        var articBody = GetComponent<ArticulationBody>();
        if (articBody)
        {
            articBody.anchorPosition = Vector3.zero;
            articBody.anchorRotation = Quaternion.identity;
            var parentBody = transform.parent?.GetComponentInParent<ArticulationBody>();
            if (parentBody)
            {
                articBody.matchAnchors = false;
                articBody.parentAnchorRotation = Quaternion.Inverse(parentBody.transform.rotation) * transform.rotation;
                articBody.parentAnchorPosition = parentBody.transform.InverseTransformPoint(transform.position);
            }
        }
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
        encoderImpl.FixedUpdate();
        sensorVelocity = SensorVelocity;
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
        var articBody = GetComponent<ArticulationBody>();
        if (articBody)
        {
            // TODO: return articBody.anchorRotation * Vector3.right;
        }
        return Vector3.right; // X axis
    }

    public MotorActuatorProto.Types.Mode commandMode;
    public float commandSetpoint;

    public override void RunJoint(MotorActuatorProto command)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        this.commandMode = command.Mode;
        this.commandSetpoint = (float)command.Command;

        if (commandMode == MotorActuatorProto.Types.Mode.PercentOutput)
        {
            commandSetpoint = Mathf.Clamp(commandSetpoint, -1.0f, 1.0f);
        }

        //Debug.Log($"{gameObject.name} {mode} {command}");
        float maxSpeed = maxMotorSpeed / mechanicalScalar;
        float maxTorque = maxMotorTorque * mechanicalScalar;

        // Avoid NaNs in the following calculations.
        if (maxSpeed == 0)
        {
            throw new Exception("maxSpeed must be non-zero");
        }

        float inversionFactor = inverted ? -1.0f : 1.0f;
        float targetVel = commandSetpoint * maxSpeed * inversionFactor;
        float appliedForce = Mathf.Max(minTorque, Mathf.Abs(commandSetpoint * maxTorque));

        // set joint motor parameters
        var hinge = GetComponent<HingeJoint>();
        if (hinge)
        {
            JointMotor myMotor = hinge.motor;
            myMotor.targetVelocity = targetVel * Mathf.Rad2Deg;
            myMotor.force = appliedForce;
            hinge.motor = myMotor;
        }
        var cjoint = GetComponent<ConfigurableJoint>();
        if (cjoint)
        {
            JointDrive drive = cjoint.angularXDrive;
            drive.positionSpring = 0;
            drive.positionDamper = 1.0f;
            drive.maximumForce = appliedForce;
            cjoint.angularXDrive = drive;
            cjoint.targetAngularVelocity = targetVel * Vector3.right;
        }
        var articBody = GetComponent<ArticulationBody>();
        if (articBody)
        {
            maxTorque *= 25; // TODO: hacks
            articBody.jointType = ArticulationJointType.RevoluteJoint;
            ArticulationDrive drive = articBody.xDrive;
            switch (commandMode)
            {
                case MotorActuatorProto.Types.Mode.PercentOutput:
                    // From https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ArticulationDrive.html:
                    // > The drive will apply force to the body that is calculated from
                    // > the current value of the drive, using this formula:
                    // > F = stiffness * (currentPosition - target) - damping * (currentVelocity - targetVelocity)
                    // For rotational joints, "force" is torque.
                    //
                    // Comparatively, the equations for the ideal physics model of a
                    // DC motor can be arranged so that:
                    // Torque = - maxTorque / maxSpeed * (currentVelocity - maxSpeed * appliedVoltage / maxVoltage)
                    // So to have the joint drive simulate a DC motor with a certain
                    // applied voltage, we can set:
                    // targetVelocity = maxSpeed * appliedVoltage / maxVoltage = maxSpeed * command
                    // damping = maxTorque / maxSpeed
                    drive.stiffness = 0;
                    drive.damping = maxTorque / maxSpeed; // TODO: need to convert from radians to degrees?
                    drive.targetVelocity = targetVel * Mathf.Rad2Deg;
                    break;
                case MotorActuatorProto.Types.Mode.Position:
                    // TODO: Compensate for Unity's different units for `target` and `targetVelocity` when setting PID gains
                    drive.stiffness = pGain;
                    drive.damping = dGain;
                    drive.targetVelocity = 0;
                    // NB: `drive.target` is in degrees
                    drive.target = commandSetpoint / mechanicalScalar;
                    drive.forceLimit = maxTorque;
                    break;
                case MotorActuatorProto.Types.Mode.Velocity:
                    drive.stiffness = 0;
                    drive.damping = velocityPGain;
                    // NB: `drive.targetVelocity` is in radians/second
                    drive.targetVelocity = Mathf.Clamp(inversionFactor * commandSetpoint / mechanicalScalar, -maxSpeed, -maxSpeed) * Mathf.Rad2Deg;
                    drive.forceLimit = maxTorque;
                    break;
            }
            drive.driveType = ArticulationDriveType.Force;
            articBody.xDrive = drive;

            //if (this.name == "drive")
            //{
            //    var targets = new List<float>();
            //    articBody.GetDriveTargetVelocities(targets);
            //    Debug.Log(this.name + " rtargets " + string.Join(", ", targets));

            //    var forces = new List<float>();
            //    articBody.GetDriveForces(forces);
            //    Debug.Log(this.name + " rforces " + string.Join(", ", forces));

            //    Debug.Log(this.name + " forces " + Dump(articBody.jointForce));
            //    Debug.Log(this.name + " velocity " + Dump(articBody.jointVelocity));
            //}
        }
    }

    private static string Dump(ArticulationReducedSpace v)
    {
        var l = v.dofCount;
        var s = "";
        for (int i = 0; i < l; ++i)
        {
            s += v[i];
            s += " ";
        }
        return s;
    }

    public override void Disable() {
        RunJoint(new MotorActuatorProto {
            Mode = MotorActuatorProto.Types.Mode.PercentOutput,
            Command = 0.0,
        });
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

    public override double SensorPosition => encoderImpl.Angle * mechanicalScalar;

    public override double SensorVelocity => encoderImpl.Velocity * mechanicalScalar;
}
