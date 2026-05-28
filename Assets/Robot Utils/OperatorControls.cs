using System;
using System.Collections.Generic;
using Team766.Simulator;
using UnityEngine;

public interface IDriveControls
{
    void Update(Joystick joystick, GyroSensor gyro, Dictionary<uint, ActuatorProto> commands);
}

public class SkidSteerArcadeControls : IDriveControls
{
    private readonly int driveAxis;
    private readonly int steerAxis;
    private readonly uint[] leftMotors;
    private readonly uint[] rightMotors;

    public SkidSteerArcadeControls(uint[] leftMotors, uint[] rightMotors) : this(1, 0, leftMotors, rightMotors) { }

    public SkidSteerArcadeControls(int driveAxis, int steerAxis, uint[] leftMotors, uint[] rightMotors)
    {
        this.driveAxis = driveAxis;
        this.steerAxis = steerAxis;
        this.leftMotors = leftMotors;
        this.rightMotors = rightMotors;
    }

    public void Update(Joystick joystick, GyroSensor gyro, Dictionary<uint, ActuatorProto> commands)
    {
        float drive = joystick.axis[driveAxis];
        float steer = joystick.axis[steerAxis];
        float leftPower = Mathf.Clamp(drive + steer, -1, 1);
        float rightPower = Mathf.Clamp(drive - steer, -1, 1);

        foreach (uint id in leftMotors)
        {
            OperatorControls.ApplyMotorSetpoint(new() { deviceId = id, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = -leftPower }, commands);
        }
        foreach (uint id in rightMotors)
        {
            OperatorControls.ApplyMotorSetpoint(new() { deviceId = id, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = rightPower }, commands);
        }
    }
}

// This class uses the robot-code coordinate convention of +X=forward, +Y=left, +Rotation=CCW, which is different than Unity's.
public abstract class SwerveControls : IDriveControls
{
    public const int DEFAULT_FORWARD_AXIS = 1;
    public const int DEFAULT_LATERAL_AXIS = 0;
    public const int DEFAULT_STEER_AXIS = 3;

    public struct Motors
    {
        public uint frontLeftDrive;
        public uint frontLeftSteer;
        public uint frontRightDrive;
        public uint frontRightSteer;
        public uint rearLeftDrive;
        public uint rearLeftSteer;
        public uint rearRightDrive;
        public uint rearRightSteer;
    }
    private static readonly Vector2
        frontLeftTangent = new(-1, 1),
        frontRightTangent = new(1, 1),
        rearLeftTangent = new(-1, -1),
        rearRightTangent = new(1, -1);

    private const float DRIVETRAIN_STEER_GEAR_RATIO = 150f / 7f;

    private readonly Motors motors;

    public SwerveControls(Motors motors)
    {
        this.motors = motors;
    }

    public void Update(Vector2 driveCommand, float steerCommand, Dictionary<uint, ActuatorProto> commands)
    {
        float frontLeftDrive;
        float frontLeftSteer;
        float frontRightDrive;
        float frontRightSteer;
        float rearLeftDrive;
        float rearLeftSteer;
        float rearRightDrive;
        float rearRightSteer;
        if (driveCommand.sqrMagnitude < 0.05f * 0.05f && Mathf.Abs(steerCommand) < 0.05f)
        {
            // Cross wheels
            frontLeftDrive = 0f;
            frontRightDrive = 0f;
            rearLeftDrive = 0f;
            rearRightDrive = 0f;
            frontLeftSteer = Mathf.PI / 4f;
            frontRightSteer = -Mathf.PI / 4f;
            rearLeftSteer = 3 * Mathf.PI / 4f;
            rearRightSteer = -3 * Mathf.PI / 4f;
        }
        else
        {
            Vector2 frontLeftCommand  = driveCommand + steerCommand * frontLeftTangent;
            Vector2 frontRightCommand = driveCommand + steerCommand * frontRightTangent;
            Vector2 rearLeftCommand   = driveCommand + steerCommand * rearLeftTangent;
            Vector2 rearRightCommand  = driveCommand + steerCommand * rearRightTangent;
            float norm = Mathf.Sqrt(Mathf.Max(
                frontLeftCommand.sqrMagnitude,
                frontRightCommand.sqrMagnitude,
                rearLeftCommand.sqrMagnitude,
                rearRightCommand.sqrMagnitude));
            frontLeftCommand  /= norm;
            frontRightCommand /= norm;
            rearLeftCommand   /= norm;
            rearRightCommand  /= norm;

            frontLeftDrive = frontLeftCommand.magnitude;
            frontLeftSteer = frontLeftCommand.Angle();
            frontRightDrive = frontRightCommand.magnitude;
            frontRightSteer = frontRightCommand.Angle();
            rearLeftDrive = rearLeftCommand.magnitude;
            rearLeftSteer = rearLeftCommand.Angle();
            rearRightDrive = rearRightCommand.magnitude;
            rearRightSteer = rearRightCommand.Angle();
        }
        frontLeftSteer  *= -DRIVETRAIN_STEER_GEAR_RATIO * Mathf.Rad2Deg;
        frontRightSteer *= -DRIVETRAIN_STEER_GEAR_RATIO * Mathf.Rad2Deg;
        rearLeftSteer   *= -DRIVETRAIN_STEER_GEAR_RATIO * Mathf.Rad2Deg;
        rearRightSteer  *= -DRIVETRAIN_STEER_GEAR_RATIO * Mathf.Rad2Deg;

        //Debug.Log($"FL {frontLeftSteer}  FR {frontRightSteer}  BL {rearLeftSteer}  BR {rearRightSteer}");
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.frontLeftDrive,  mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = frontLeftDrive  }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.frontLeftSteer,  mode = MotorActuatorProto.Types.Mode.Position,      setpoint = frontLeftSteer  }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.frontRightDrive, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = frontRightDrive }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.frontRightSteer, mode = MotorActuatorProto.Types.Mode.Position,      setpoint = frontRightSteer }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.rearLeftDrive,   mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = rearLeftDrive   }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.rearLeftSteer,   mode = MotorActuatorProto.Types.Mode.Position,      setpoint = rearLeftSteer   }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.rearRightDrive,  mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = rearRightDrive  }, commands);
        OperatorControls.ApplyMotorSetpoint(new() { deviceId = motors.rearRightSteer,  mode = MotorActuatorProto.Types.Mode.Position,      setpoint = rearRightSteer  }, commands);
    }

    public abstract void Update(Joystick joystick, GyroSensor gyro, Dictionary<uint, ActuatorProto> commands);
}

public class SwerveRobotOrientedControls : SwerveControls
{
    private readonly int forwardAxis;
    private readonly int lateralAxis;
    private readonly int steerAxis;

    public SwerveRobotOrientedControls(SwerveControls.Motors motors) : this(DEFAULT_FORWARD_AXIS, DEFAULT_LATERAL_AXIS, DEFAULT_STEER_AXIS, motors) { }

    public SwerveRobotOrientedControls(int forwardAxis, int lateralAxis, int steerAxis, SwerveControls.Motors motors) : base(motors)
    {
        this.forwardAxis = forwardAxis;
        this.lateralAxis = lateralAxis;
        this.steerAxis = steerAxis;
    }

    public override void Update(Joystick joystick, GyroSensor gyro, Dictionary<uint, ActuatorProto> commands)
    {
        Update(new (-joystick.axis[forwardAxis], -joystick.axis[lateralAxis]), -joystick.axis[steerAxis], commands);
    }
}

public class SwerveFieldOrientedControls : SwerveControls
{
    private readonly int forwardAxis;
    private readonly int lateralAxis;
    private readonly int steerAxis;

    public SwerveFieldOrientedControls(SwerveControls.Motors motors) : this(DEFAULT_FORWARD_AXIS, DEFAULT_LATERAL_AXIS, DEFAULT_STEER_AXIS, motors) { }

    public SwerveFieldOrientedControls(int forwardAxis, int lateralAxis, int steerAxis, SwerveControls.Motors motors) : base(motors)
    {
        this.forwardAxis = forwardAxis;
        this.lateralAxis = lateralAxis;
        this.steerAxis = steerAxis;
    }

    public override void Update(Joystick joystick, GyroSensor gyro, Dictionary<uint, ActuatorProto> commands)
    {
        Vector2 fieldCommand = new (-joystick.axis[forwardAxis], -joystick.axis[lateralAxis]);
        float steerCommand = -joystick.axis[steerAxis];

        Vector2 robotCommand = fieldCommand.Rotate(gyro.Heading);

        Update(robotCommand, steerCommand, commands);
    }
}

public class OperatorControls
{
    [Serializable]
    public class MotorSetpoint
    {
        public uint deviceId;
        public MotorActuatorProto.Types.Mode mode = MotorActuatorProto.Types.Mode.PercentOutput;
        public float setpoint;
    }

    [Serializable]
    public class RobotSetpoint
    {
        public List<MotorSetpoint> motors = new();
    }

    [Serializable]
    public class ButtonActions
    {
        public int button;
        public RobotSetpoint pressed;
        public RobotSetpoint released;
    }

    public IDriveControls driveControls;

    public RobotSetpoint startup;

    public List<ButtonActions> buttons = new();


    public void Update(Joystick joystick, GyroSensor gyro, Dictionary<uint, ActuatorProto> commands)
    {
        driveControls?.Update(joystick, gyro, commands);
        foreach (var bmap in buttons)
        {
            var appliedSetpoint = joystick.button[bmap.button] ? bmap.pressed : bmap.released;
            ApplySetpoint(appliedSetpoint, commands);
        }
    }

    public static void ApplySetpoint(OperatorControls.RobotSetpoint setpoint, Dictionary<uint, ActuatorProto> commands)
    {
        if (setpoint == null)
        {
            return;
        }
        foreach (var mmap in setpoint.motors)
        {
            ApplyMotorSetpoint(mmap, commands);
        }
    }

    public static void ApplyMotorSetpoint(OperatorControls.MotorSetpoint setpoint, Dictionary<uint, ActuatorProto> commands)
    {
        if (setpoint == null)
        {
            return;
        }

        var command = new ActuatorProto();
        command.Id = setpoint.deviceId;
        command.Motor = new() {
            Mode = setpoint.mode,
            Command = setpoint.setpoint,
        };
        commands[setpoint.deviceId] = command;
    }
}