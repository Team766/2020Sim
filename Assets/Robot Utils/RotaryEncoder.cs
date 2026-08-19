using UnityEngine;
using System;
using System.Collections;

public sealed class RotaryEncoder : RobotSensor
{
    private RotaryEncoderImpl impl;

    public float Angle;
    public int countsPerRevolution = 360;

    void Awake()
    {
        impl = new RotaryEncoderImpl(transform);
    }

    void FixedUpdate()
    {
        impl.FixedUpdate();
        Angle = impl.Angle;
    }

    public override void UpdateSensorValue(Team766.Simulator.SensorProto value)
    {
        value.Encoder = new() { Value = (long)(Angle * (countsPerRevolution / 360.0f)) };
    }
}
