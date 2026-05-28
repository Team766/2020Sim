using UnityEngine;
using Team766.Simulator;

[RequireComponent(typeof(BallStorage))]
public sealed class BallStorageSensor : RobotSensor {
    public override void UpdateSensorValue(SensorProto value) {
        var ballStorage = GetComponent<BallStorage>();
        value.Analog = new () {
            Value = ballStorage.NumHolding / (double)ballStorage.heldObjects.Count,
        };
    }
}