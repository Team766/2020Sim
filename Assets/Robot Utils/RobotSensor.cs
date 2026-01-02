using System;

public abstract class RobotSensor : RobotDevice {
    public sealed override void RunJoint(CodeBufferView commands) {}

    public override void Disable() { }

    public override void Destroy() { }
}

public abstract class StandardRobotSensor : RobotSensor {
    public sealed override void RunSensor(CodeBufferBuilder feedbackValues) {
        feedbackValues.DeviceData<int>(DeviceId, DeviceType, new[] { SensorValue });
    }

    public abstract int SensorValue {
        get;
    }
}