using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public sealed class LinearEncoder : RobotSensor
{
    public float encoderScale = 1000;
 
    private Vector3 neutralPosition;

    public override void UpdateSensorValue(Team766.Simulator.SensorProto value)
    {
        value.Encoder = new() {
            Value = (long)(
                encoderScale *
                Vector3.Dot(transform.localPosition - neutralPosition,
                            GetComponent<ConfigurableJoint>().axis)
            ),
        };
    }

    void Awake()
	{
		neutralPosition = transform.localPosition;
	}
}
