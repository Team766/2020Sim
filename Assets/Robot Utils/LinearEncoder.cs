using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public sealed class LinearEncoder : StandardRobotSensor
{
    public float encoderScale = 1000;
 
    private Vector3 neutralPosition;

    public override int SensorValue
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

    void Awake()
	{
		neutralPosition = transform.localPosition;
	}
}
