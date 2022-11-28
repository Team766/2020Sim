using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ElevatorKinematic))]
public sealed class ElevatorKinematicSensorr : StandardRobotSensor
{
    public float encoderScale;

    public override int SensorValue
    {
        get
        {
            return (int)(encoderScale * GetComponent<ElevatorKinematic>().position);
        }
    }
}