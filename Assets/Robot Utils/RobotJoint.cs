using System;

public abstract class StandardRobotJoint : RobotDevice {
    public sealed override CodeDeviceType DeviceType => CodeDeviceType.MOTOR;

    public sealed override void RunJoint(CodeBufferView commands) {
        var deviceCommands = commands.DeviceData<int>(DeviceId, DeviceType);
        float command = 0.0f;
        if (deviceCommands.Count > 0) {
            command = deviceCommands[0] / 512.0f;
        }
        RunJoint(command);
    }

    public abstract void RunJoint(float command);

    public sealed override void RunSensor(CodeBufferBuilder feedbackValues) {
        feedbackValues.DeviceData<int>(DeviceId, DeviceType, new[] { SensorPosition, SensorVelocity });
    }

    public abstract int SensorPosition {
        get;
    }

    public abstract int SensorVelocity {
        get;
    }
}