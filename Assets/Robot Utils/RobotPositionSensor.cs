using UnityEngine;

public sealed class RobotPositionSensor : RobotSensor {
    public override void UpdateSensorValue(Team766.Simulator.SensorProto value) {
        Vector3 rotation = transform.rotation.eulerAngles;
        // In robot code, X axis is forward, Y axis is left, Z axis is up
        value.RobotPosition = new() {
            Pose = new() {
                X = transform.position.z,
                Y = -transform.position.x,
                Z = transform.position.y,
                Yaw = (rotation.y + 270) % 360,
                Pitch = rotation.x,
                Roll = rotation.z,
            },
        };
    }
}