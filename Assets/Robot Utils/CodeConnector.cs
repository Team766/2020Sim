using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Google.Protobuf;
using Mirror;
using Team766.Simulator;

[RequireComponent(typeof(RobotController))]
[RequireComponent(typeof(InputManager))]
public class CodeConnector : NetworkBehaviour {
    const float EXCEPTION_LOG_PERIOD = 10f;

    private GameGUI gameGui;
    private RobotController robot;
    private InputManager oi;
    private UdpClient udpClient;
    private DateTime lastFeedback, lastCommand;
    private float lastConnectException = -1000;

    public OperatorControls operatorControls = new () {
        driveControls = new SkidSteerArcadeControls(
            driveAxis: 1,
            steerAxis: 0,
            leftMotors: new uint[] { 32, 33 },
            rightMotors: new uint[] { 34, 35 }),
        buttons = {
            // Intake
            new() {
                button = 0,
                pressed = new() { motors = {
                    new() { deviceId = 36, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = 1f },
                }},
                released = new() { motors = {
                    new() { deviceId = 36, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = 0f },
                }},
            },
            // Feeder
            new() {
                button = 1,
                pressed = new() { motors = {
                    new() { deviceId = 37, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = 1f },
                }},
                released = new() { motors = {
                    new() { deviceId = 37, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = 0f },
                }},
            },
            // Launcher
            new() {
                button = 2,
                pressed = new() { motors = {
                    new() { deviceId = 38, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = 1f },
                }},
                released = new() { motors = {
                    new() { deviceId = 38, mode = MotorActuatorProto.Types.Mode.PercentOutput, setpoint = 0f },
                }},
            },
        },
    };

    [NonSerialized]
    [SyncVar]
    public bool hasRobotCode = false;

    // resetCounter is initialized to a value that (should) be different each time the simulator is started.
    private static int resetCounter = (int)((DateTime.UtcNow - DateTime.MinValue).TotalSeconds % (Int32.MaxValue / 2));
    private static bool resetCallbackRegistered = false;

    public int commandsPort = 7661;
    public int feedbackPort = 7662;

    const int MaxButtonsPerJoystick = 31;

    void Start() {
        // Don't call FindAnyObjectByType in Awake because of script ordering issues.
        gameGui = FindAnyObjectByType<GameGUI>();
        robot = GetComponent<RobotController>();
        oi = GetComponent<InputManager>();

        if (Application.platform != RuntimePlatform.WebGLPlayer) {
            Application.targetFrameRate = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
        }

        if (!resetCallbackRegistered) {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => { ++resetCounter; };
            resetCallbackRegistered = true;
        }

        if (ApplicationArguments.PlayerRole.IsCodePlayer() && Application.platform != RuntimePlatform.WebGLPlayer) {
            Debug.Log("Starting UDP Code Connector");
            udpClient = new UdpClient(commandsPort);
            udpClient.Connect(IPAddress.Loopback, feedbackPort);
        
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // http://stackoverflow.com/a/7478498
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            }
        }
    }

    void OnDestroy() {
        if (udpClient != null) {
            udpClient.Close();
            udpClient = null;
        }
    }

    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    void FixedUpdate() {
        if (udpClient != null) {
            if (DateTime.Now - lastFeedback > TimeSpan.FromMilliseconds(1)) {
                lastFeedback = DateTime.Now;
                FeedbackPacket feedback = new();

                robot.RunSensors(feedback);

                feedback.Timestamp = Time.timeAsDouble;
                feedback.ResetCounter = resetCounter;

                var driverStation = new DriverStationProto();
                driverStation.RobotMode = robot.RobotMode switch {
                    RobotMode.Disabled => Team766.Simulator.RobotMode.DisabledMode,
                    RobotMode.Auton => Team766.Simulator.RobotMode.AutonMode,
                    RobotMode.Teleop => Team766.Simulator.RobotMode.TeleopMode,
                    _ => throw new ArgumentOutOfRangeException($"Unknown RobotMode value: {robot.RobotMode}"),
                };
                foreach (var joystick in oi.joysticks) {
                    var proto = new JoystickProto();
                    uint denseButtonState = 0;
                    for (var b = 0; b < Math.Min(MaxButtonsPerJoystick, joystick.button.Length); b++) {
                        denseButtonState |= (joystick.button[b] ? 1u : 0u) << b;
                    }
                    proto.DenseButtons = denseButtonState;
                    foreach (var axis in joystick.axis) {
                        proto.Axis.Add(axis);
                    }
                    driverStation.Joystick.Add(proto);
                }
                feedback.DriverStation = driverStation;

                try {
                    var sendBytes = feedback.ToByteArray();
                    udpClient.Send(sendBytes, sendBytes.Length);
                } catch (SocketException ex) {
                    if (hasRobotCode || (Time.realtimeSinceStartup - lastConnectException > EXCEPTION_LOG_PERIOD)) {
                        lastConnectException = Time.realtimeSinceStartup;
                        Debug.LogException(ex, this);
                    }
                }
            }

            byte[] receiveBytes = null;
            while (udpClient.Available > 0) {
                IPEndPoint e = new IPEndPoint(IPAddress.Any, commandsPort);
                try {
                    receiveBytes = udpClient.Receive(ref e);
                } catch (IOException ex) {
                    Debug.LogException(ex, this);
                }
            }
            if (receiveBytes != null) {
                var commands = CommandsPacket.Parser.ParseFrom(receiveBytes);

                //if (!commands.DeviceData<byte>(ReservedDeviceIds.RESET_SIM, CodeDeviceType.COMMAND_FLAG).IsEmpty) {
                //    TODO: replace this with GameGUI.LoadScene
                //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //    Debug.Log("Reset");
                //}

                if (commands.HasRobotMode) {
                    var newRobotMode = commands.RobotMode switch {
                        Team766.Simulator.RobotMode.DisabledMode => RobotMode.Disabled,
                        Team766.Simulator.RobotMode.AutonMode => RobotMode.Auton,
                        Team766.Simulator.RobotMode.TeleopMode => RobotMode.Teleop,
                        _ => throw new ArgumentOutOfRangeException($"Unknown RobotMode value: {robot.RobotMode}"),
                    };
                    if (newRobotMode != gameGui.RobotMode) {
                        gameGui.CmdSetRobotMode(newRobotMode.ToString());
                    }
                    gameGui.CmdSetRobotModeIsCodeControlled(true);
                } else {
                    gameGui.CmdSetRobotModeIsCodeControlled(false);
                }

                robot.RunJoints(commands);

                lastCommand = DateTime.Now;
            }
        }

        hasRobotCode = DateTime.Now - lastCommand < TimeSpan.FromSeconds(1);

        if (!hasRobotCode) {
            robot.RunJoints(GetCodelessCommands());
        }
    }

    internal readonly Dictionary<uint, ActuatorProto> codelessCommands = new();

    CommandsPacket GetCodelessCommands() {
        if (operatorControls != null)
        {
            operatorControls.Update(oi.joysticks[0], GetComponent<GyroSensor>(), codelessCommands);
        }

        CommandsPacket packet = new();
        foreach (var (deviceId, command) in codelessCommands)
        {
            command.Id = deviceId;
            packet.Actuator.Add(command);
        }
        return packet;
    }
}
