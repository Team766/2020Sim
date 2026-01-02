using System.Collections.Generic;

public sealed class RobotPositionSensor : RobotSensor {
    public override CodeDeviceType DeviceType => CodeDeviceType.ROBOT_POSITION_SENSOR;

    public override void RunSensor(CodeBufferBuilder feedbackValues) {
        feedbackValues.DeviceData<int>(DeviceId, DeviceType, new[] {
            (int)(transform.position.x * 1000),
            (int)(transform.position.z * 1000),
        });
    }
}