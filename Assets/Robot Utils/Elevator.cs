using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team766.Simulator;

[RequireComponent(typeof(ConfigurableJoint))]
public sealed class Elevator : StandardRobotJoint
{
    public float forwardForceScale = 10;
    public float reverseForceScale = 10;
    public float maxSpeed = 10;
    
    public bool isStuck;

    public float stickSpeed;

    public float damperFromFriction;
    public float maxFrictionForce;

    private Vector3 neutralPosition;

    public float minPosition = -0.5f;
    public float maxPosition = 0.5f;

    public float encoderScale = 1000;

    private Vector3 positionRelativeToConnectedBody()
    {
        ConfigurableJoint joint = GetComponent<ConfigurableJoint>();
        return ((Component)joint.connectedBody ?? joint.connectedArticulationBody).transform.InverseTransformPoint(transform.position);
    }

    private Vector3 velocityRelativeToConnectedBody()
    {
        ConfigurableJoint joint = GetComponent<ConfigurableJoint>();
        return ((Component)joint.connectedBody ?? joint.connectedArticulationBody).transform.InverseTransformVector(
                joint.connectedBody ? joint.connectedBody.linearVelocity : joint.connectedArticulationBody.linearVelocity);
    }

    void Awake()
	{
        neutralPosition = positionRelativeToConnectedBody();
        isStuck = false;
	}

    private void setJointLimit() {
        ConfigurableJoint configurableJointComp = GetComponent<ConfigurableJoint>();
        SoftJointLimit softJointLimit = configurableJointComp.linearLimit;
        softJointLimit.limit = (maxPosition - minPosition) / 2;
        configurableJointComp.linearLimit = softJointLimit;
    }

    private static float Sign(float x) {
        if (x > 0) return 1;
        if (x < 0) return -1;
        return 0;
    }

    public override void RunJoint(MotorActuatorProto command) {
        // TODO: Support MotorCommandMode.POSITION (and VELOCITY?)

        float percentOutput = (float)command.Command;

        float appliedForce;
        if (Mathf.Abs(percentOutput) < stickSpeed) {
            if (!isStuck) {
                GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
                GetComponent<ConfigurableJoint>().connectedAnchor = positionRelativeToConnectedBody();
            }
            appliedForce = 0.0f;
            isStuck = true;
        } else {
            if (percentOutput >= 0) {
                appliedForce = forwardForceScale * percentOutput;
            } else {
                appliedForce = reverseForceScale * percentOutput;
            }
            if (isStuck)
            {
                GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Limited;
                setJointLimit();
                GetComponent<ConfigurableJoint>().connectedAnchor = neutralPosition + GetComponent<ConfigurableJoint>().axis * (maxPosition + minPosition) / 2;
            }
            isStuck = false;
        }

        // Avoid NaNs in the following calculations.
        if (maxSpeed == 0) {
            throw new Exception("maxSpeed must be non-zero");
        }

        JointDrive drive = GetComponent<ConfigurableJoint>().xDrive;
		drive.positionSpring = 0;
        drive.positionDamper = Mathf.Max(
            Mathf.Abs(appliedForce / maxSpeed),
            damperFromFriction);
        drive.maximumForce = Mathf.Max(
            Mathf.Abs(appliedForce),
            maxFrictionForce);
		GetComponent<ConfigurableJoint>().xDrive = drive;
        // For some reason, targetVelocity seems to be reversed relative to the
        // direction of `axis`, hence the extra negative sign.
        GetComponent<ConfigurableJoint>().targetVelocity =
            -Sign(appliedForce) * maxSpeed * Vector3.right;
    }

    public override void Disable() {
        RunJoint(new MotorActuatorProto {
            Mode = MotorActuatorProto.Types.Mode.PercentOutput,
            Command = 0.0,
        });
    }

    public override void Destroy() {
        Destroy(this);
        Destroy(GetComponent<ConfigurableJoint>());
        Destroy(GetComponent<Rigidbody>());
        //GetComponent<Rigidbody>().isKinematic = true;
    }

    public override double SensorPosition => (
        encoderScale *
        Vector3.Dot(positionRelativeToConnectedBody() - neutralPosition,
                    GetComponent<ConfigurableJoint>().axis)
    );

    public override double SensorVelocity => (
        encoderScale *
        Vector3.Dot(velocityRelativeToConnectedBody(),
                    GetComponent<ConfigurableJoint>().axis)
    );
}
