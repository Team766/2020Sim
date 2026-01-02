using UnityEngine;
using System;
using System.Collections;

public sealed class RotaryEncoder : StandardRobotSensor
{
    private RotaryEncoderImpl impl;

    public float Angle;

    void Awake()
    {
        impl = new RotaryEncoderImpl(transform);
    }

    void FixedUpdate()
    {
        impl.FixedUpdate();
        Angle = impl.Angle;
    }

    public override CodeDeviceType DeviceType => CodeDeviceType.ENCODER_SENSOR;

    public override int SensorValue => (int)Angle;
}
