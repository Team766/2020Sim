using System;
using UnityEngine;
using Team766.Simulator;

public abstract class StandardRobotJoint : RobotDevice {
    private static readonly MotorActuatorProto defaultMotorCommand = new MotorActuatorProto {
        Mode = MotorActuatorProto.Types.Mode.PercentOutput,
        Command = 0.0,
    };

    public sealed override void RunJoint(CommandsPacket commands) {
        ActuatorProto actuator = System.Linq.Enumerable.FirstOrDefault(commands.Actuator, a => a.Id == DeviceId);
        MotorActuatorProto motorCommand = defaultMotorCommand;
        if (actuator == null) {
            // Debug.LogWarning(
            //     $"Simulation commands packet doesn't include data for device {DeviceId}");
        } else if (actuator.TypeCase != ActuatorProto.TypeOneofCase.Motor) {
            Debug.LogWarning(
                $"Simulation data for actuator {DeviceId} is the wrong type {actuator.TypeCase}. Expected Motor.");
        } else {
            motorCommand = actuator.Motor;
        }
        RunJoint(motorCommand);
    }

    public abstract void RunJoint(MotorActuatorProto command);

    public sealed override void RunSensor(FeedbackPacket feedback) {
        SensorProto proto = new();
        proto.Id = DeviceId;
        proto.Motor = new MotorSensorProto {
            Position = SensorPosition,
            Velocity = SensorVelocity,
        };
        feedback.Sensor.Add(proto);
    }

    public abstract double SensorPosition {
        get;
    }

    public abstract double SensorVelocity {
        get;
    }
}