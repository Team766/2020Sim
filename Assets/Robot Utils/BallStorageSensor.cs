using UnityEngine;

[RequireComponent(typeof(BallStorage))]
public sealed class BallStorageSensor : StandardRobotSensor {
    public override int SensorValue => GetComponent<BallStorage>().holding;

    public override CodeDeviceType DeviceType => CodeDeviceType.BALL_STORAGE_SENSOR;
}