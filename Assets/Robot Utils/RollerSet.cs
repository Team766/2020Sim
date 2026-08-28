using System;
using UnityEngine;
using Team766.Simulator;

public sealed class RollerSet : StandardRobotJoint
{
    public float maxMotorSpeed = 628.32f; // radians per second
    public float minTorque;
    public bool inverted;
    public float mechanicalScalar = 45.0f;

    public Transform[] rollers;

    private float angle;
    //[NonSerialized]
    public float velocity;
    //[NonSerialized]
    public float percentVelocity;

    void Update()
    {
        float delta = velocity * Time.deltaTime;
        angle += delta;
        foreach (var roller in rollers)
        {
            roller.Rotate(0, delta, 0);
        }
    }

    public override void RunJoint(MotorActuatorProto command)
    {
        float maxDegreesPerSecond = maxMotorSpeed / mechanicalScalar * Mathf.Rad2Deg;

        switch (command.Mode)
        {
            case MotorActuatorProto.Types.Mode.PercentOutput:
                velocity = maxDegreesPerSecond * (float)command.Command;
                break;
            case MotorActuatorProto.Types.Mode.Position:
                // Unsupported
                break;
            case MotorActuatorProto.Types.Mode.Velocity:
                // Convert from revolutions/second to degrees/second
                velocity = 360f * (float)command.Command;
                break;
        }
        velocity = Mathf.Clamp(velocity, -maxDegreesPerSecond, maxDegreesPerSecond);
        percentVelocity = velocity / maxDegreesPerSecond;
    }

    public override void Disable()
    {
        RunJoint(new MotorActuatorProto {
            Mode = MotorActuatorProto.Types.Mode.PercentOutput,
            Command = 0.0,
        });
    }

    public override void Destroy()
    {
        //Destroy(this);
    }

    public override double SensorPosition => angle * mechanicalScalar;

    public override double SensorVelocity => velocity * mechanicalScalar;
}
