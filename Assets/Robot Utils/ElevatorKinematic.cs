using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ElevatorKinematic : StandardRobotJoint
{
    public float forceScale;
    public bool oneSided;
    public float velocity;

    public bool isStuck;

    public float stickForce;

    private Vector3 neutralPosition;

    public Vector3 axis = Vector3.right;

    public float position;
    public float limit;

    public float encoderScale;

    public TwoGripper gripper;

    void Awake()
	{
		neutralPosition = transform.localPosition;
        isStuck = false;
	}

    public override void RunJoint(float speed)
    {
        velocity = forceScale * speed;
        if (Mathf.Abs(velocity) < stickForce) {
            velocity = 0;
            isStuck = true;
        } else {
            isStuck = false;
            if (oneSided && speed < 0) {
                velocity = 0;
            }
            if (gripper.collidingCount > 0 && speed > 0) {
                velocity = 0;
            }
            position += velocity * Time.fixedDeltaTime;
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

    public override int SensorPosition => (int)(encoderScale * position);

    public override int SensorVelocity => (int)(encoderScale * velocity);
}
