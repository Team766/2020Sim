using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class Elevator : MonoBehaviour
{
    public float forwardForceScale = 10;
    public float reverseForceScale = 10;
    public float maxSpeed = 10;
    public float encoderScale;
    
    public bool isStuck;

    public float stickSpeed;

    public float damperFromFriction;
    public float maxFrictionForce;

    private Vector3 neutralPosition;

    public float minPosition = -0.5f;
    public float maxPosition = 0.5f;

    public int Encoder
    {
        get
        {
            return (int)(
                encoderScale *
                Vector3.Dot(transform.localPosition - neutralPosition,
                            GetComponent<ConfigurableJoint>().axis)
            );
        }
    }

    void Awake ()
	{
		neutralPosition = transform.localPosition;
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

    public void RunJoint (float speed)
    {
        float appliedForce;
        if (Mathf.Abs(speed) < stickSpeed) {
            if (!isStuck) {
                GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
                GetComponent<ConfigurableJoint>().connectedAnchor = transform.localPosition;
            }
            appliedForce = 0.0f;
            isStuck = true;
        } else {
            isStuck = false;
            if (speed >= 0) {
                appliedForce = forwardForceScale * speed;
            } else {
                appliedForce = reverseForceScale * speed;
            }
            GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Limited;
            setJointLimit();
            GetComponent<ConfigurableJoint>().connectedAnchor = neutralPosition + GetComponent<ConfigurableJoint>().axis * (maxPosition + minPosition) / 2;
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
}
