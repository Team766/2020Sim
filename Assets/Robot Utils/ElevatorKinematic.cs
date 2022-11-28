using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ElevatorKinematic : StandardRobotJoint
{
    public float forceScale;
    public bool oneSided;
    public Vector3 appliedForce;

    public bool isStuck;

    public float stickForce;

    private Vector3 neutralPosition;

    public Vector3 axis = Vector3.right;

    public float position;
    public float limit;

    public TwoGripper gripper;

    void Awake()
	{
		neutralPosition = transform.localPosition;
        isStuck = false;
	}

    public override void RunJoint(float speed)
    {
        float force = forceScale * speed;
        if (Mathf.Abs(force) < stickForce) {
            appliedForce = Vector3.zero;
            isStuck = true;
        } else {
            isStuck = false;
            if (oneSided && speed < 0) {
                force = 0;
            }
            if (gripper.collidingCount > 0 && speed > 0) {
                force = 0;
            }
            appliedForce = axis * force;
            position += force * Time.fixedDeltaTime;
        }
        if (position < -limit) {
            position = -limit;
        }
        if (position > limit) {
            position = limit;
        }
        transform.localPosition = neutralPosition + axis * position;
    }

    public override void Disable() {
        RunJoint(0.0f);
    }

    public override void Destroy() {
        Destroy(this);
    }
}
