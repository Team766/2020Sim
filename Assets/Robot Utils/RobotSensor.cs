using System;
using Team766.Simulator;

public abstract class RobotSensor : RobotDevice {
    public sealed override void RunJoint(CommandsPacket commands) {}

    public sealed override void RunSensor(FeedbackPacket feedback) {
        SensorProto proto = new();
        UpdateSensorValue(proto);
        proto.Id = DeviceId;
        feedback.Sensor.Add(proto);
    }

    public abstract void UpdateSensorValue(SensorProto value);

    public override void Disable() { }

    public override void Destroy() { }
}