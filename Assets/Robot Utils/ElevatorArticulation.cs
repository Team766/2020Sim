using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team766.Simulator;

[RequireComponent(typeof(ArticulationBody))]
public sealed class ElevatorArticulation : StandardRobotJoint
{
    public float maxMotorSpeed; // radians per second
    public float maxMotorTorque; // Newtons-meters
    public bool inverted;

    private float neutralPosition;

    public float minPosition = 0.0f;
    public float maxPosition = 1.0f;

    public float mechanicalScalar = 1.0f;

    public float pGain, dGain, velocityPGain;

    private float positionRelativeToConnectedBody()
    {
        return GetComponent<ArticulationBody>().jointPosition[0];
    }

    private float velocityRelativeToConnectedBody()
    {
        return GetComponent<ArticulationBody>().jointVelocity[0];
    }

    void Start()
	{
        neutralPosition = positionRelativeToConnectedBody();

        var articBody = GetComponent<ArticulationBody>();
        if (articBody)
        {
            articBody.anchorPosition = Vector3.zero;
            articBody.anchorRotation = Quaternion.identity;
            var parentBody = transform.parent?.GetComponentInParent<ArticulationBody>();
            if (parentBody)
            {
                articBody.matchAnchors = false;
                articBody.parentAnchorPosition = parentBody.transform.InverseTransformPoint(transform.parent.position);
                articBody.parentAnchorRotation = Quaternion.Inverse(parentBody.transform.rotation) * transform.parent.rotation;
            }
        }
    }

    public override void RunJoint(MotorActuatorProto command)
    {
        float maxSpeed = maxMotorSpeed / mechanicalScalar;
        float maxForce = maxMotorTorque * mechanicalScalar;

        // Avoid NaNs in the following calculations.
        if (maxSpeed == 0) {
            throw new Exception("maxSpeed must be non-zero");
        }

        float inversionFactor = inverted ? -1.0f : 1.0f;
        //float appliedForce = Mathf.Abs(command.Command * maxForce);

        var articBody = GetComponent<ArticulationBody>();
        articBody.jointType = ArticulationJointType.PrismaticJoint;
        articBody.linearLockX = ArticulationDofLock.LockedMotion;
        articBody.linearLockY = ArticulationDofLock.LimitedMotion;
        articBody.linearLockZ = ArticulationDofLock.LockedMotion;
        ArticulationDrive drive = articBody.yDrive;

        switch (command.Mode)
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
                drive.damping = maxForce / maxSpeed;
                drive.targetVelocity = (float)command.Command * maxSpeed * inversionFactor;
                drive.driveType = ArticulationDriveType.Force;
                break;
            case MotorActuatorProto.Types.Mode.Position:
                drive.stiffness = pGain;
                drive.damping = dGain;
                drive.targetVelocity = 0;
                drive.target = (float)command.Command / mechanicalScalar * Mathf.Deg2Rad;
                drive.forceLimit = maxForce;
                break;
            case MotorActuatorProto.Types.Mode.Velocity:
                drive.stiffness = 0;
                drive.damping = velocityPGain;
                drive.targetVelocity = Mathf.Clamp(inversionFactor * (float)command.Command / mechanicalScalar * Mathf.Deg2Rad, -maxSpeed, -maxSpeed);
                drive.forceLimit = maxForce;
                break;
        }
        drive.lowerLimit = minPosition;
        drive.upperLimit = maxPosition;
        articBody.yDrive = drive;
    }

    public override void Disable() {
        RunJoint(new MotorActuatorProto {
            Mode = MotorActuatorProto.Types.Mode.PercentOutput,
            Command = 0.0,
        });
    }

    public override void Destroy() {
        Destroy(this);
        Destroy(GetComponent<ArticulationBody>());
    }

    public override double SensorPosition => (
        Mathf.Rad2Deg * mechanicalScalar * (positionRelativeToConnectedBody() - neutralPosition)
    );

    public override double SensorVelocity => (
        Mathf.Rad2Deg * mechanicalScalar * velocityRelativeToConnectedBody()
    );
}
