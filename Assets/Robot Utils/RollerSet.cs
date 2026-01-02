using System;
using UnityEngine;

public sealed class RollerSet : StandardRobotJoint
{
    public float maxDegreesPerSecond = 800;

    public Transform[] rollers;

    [NonSerialized]
    public float command;

    private float angle;
    private float velocity;

    void Update()
    {
        velocity = maxDegreesPerSecond * command;
        float delta = velocity * Time.deltaTime;
        angle += delta;
        foreach (var roller in rollers)
        {
            roller.Rotate(0, delta, 0);
        }
    }

    public override void RunJoint(float command)
    {
        this.command = command;
    }

    public override void Disable()
    {
        RunJoint(0.0f);
    }

    public override void Destroy()
    {
        //Destroy(this);
    }

    public override int SensorPosition => (int)angle;

    public override int SensorVelocity => (int)velocity;
}
