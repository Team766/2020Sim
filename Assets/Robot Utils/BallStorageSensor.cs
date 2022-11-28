using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

[RequireComponent(typeof(BallStorage))]
public sealed class BallStorageSensor : StandardRobotSensor {
    public override int SensorValue
    {
        get
        {
            return GetComponent<BallStorage>().holding > 0 ? 1 : 0;
        }
    }
}