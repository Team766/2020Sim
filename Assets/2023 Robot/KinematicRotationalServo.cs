using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RotaryEncoder))]
public class KinematicRotationalServo : StandardRobotJoint
{
    public readonly Vector3 axis = Vector3.right;
    public float maxSpeed;
    public float minAngle;
    public float maxAngle;
    public float targetAngle;

    private Quaternion neutralRotation;
    private float neutralAngle;

    void Awake()
    {
        neutralRotation = transform.localRotation;
        neutralAngle = GetComponent<RotaryEncoder>().Angle;
    }

    void FixedUpdate()
    {
        float newAngle = Mathf.MoveTowards(GetComponent<RotaryEncoder>().Angle, targetAngle, maxSpeed * Time.deltaTime);
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        transform.localRotation = neutralRotation * Quaternion.AngleAxis(newAngle - neutralAngle, axis);
    }

    public override void RunJoint(float command)
    {
        targetAngle = command * 180.0f;
    }

    public override void Disable() {
        targetAngle = GetComponent<RotaryEncoder>().Angle;
    }

    public override void Destroy() {
        Destroy(this);
    }
}
