using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Mirror;

public enum CodeDeviceType : byte {
    JOYSTICK = 1,
    MOTOR = 2,
    ENCODER_SENSOR = 3,
    GYRO_SENSOR = 4,
    BALL_STORAGE_SENSOR = 5,
    LINE_SENSOR = 6,
    ROBOT_POSITION_SENSOR = 7,
    BEACON_POSITION_SENSOR = 8,
    COMMAND_FLAG = 9,
    FEEDBACK_COUNTER_32 = 10,
    FEEDBACK_COUNTER_64 = 11,
    ROBOT_MODE = 12,
}

public static class ReservedDeviceIds {
    public const byte TIMESTAMP = 1;
    public const byte RESET_SIM = 2;
    public const byte RESET_COUNTER = 3;
    public const byte ROBOT_MODE = 4;
    public const byte JOYSTICK_1 = 5;
    public const byte JOYSTICK_2 = 6;
    public const byte JOYSTICK_3 = 7;
    public const byte JOYSTICK_4 = 8;

    public const byte BEGIN_ROBOT_DEVICE_ID = 32;
}

public class CodeBufferView {
    private readonly ArraySegment<byte> data;

    public CodeBufferView() {
        this.data = ArraySegment<byte>.Empty;
    }

    public CodeBufferView(ArraySegment<byte> data) {
        this.data = data;
    }

    public ArraySegment<byte> DeviceData(byte deviceId, CodeDeviceType deviceType) {
        for (int start = 0; start + 3 <= data.Count;) {
            byte spanId = data[start++];
            byte spanType = data[start++];
            byte length = data[start++];
            if (spanId == deviceId) {
                if (spanType == (byte)deviceType) {
                    return data.Slice(start, length);
                } else {
                    Debug.LogWarning($"Received Device ID {deviceId} has a different type {(CodeDeviceType)spanType} than expected {deviceType}");
                }
            }
            start += length;
        }
        return new ArraySegment<byte>();
    }

    public ArraySegment<T> DeviceData<T>(byte deviceId, CodeDeviceType deviceType) where T : unmanaged {
        ReadOnlySpan<byte> deviceBytes = DeviceData(deviceId, deviceType);
        T[] deviceData = new T[deviceBytes.Length / Marshal.SizeOf<T>()];
        deviceBytes.CopyTo(MemoryMarshal.AsBytes<T>(deviceData));
        return deviceData;
    }
}

public class CodeBufferBuilder {
    private readonly byte[] data = new byte[1024];
    private int size = 0;

    public void Clear() {
        size = 0;
    }

    public void DeviceData<T>(byte deviceId, CodeDeviceType deviceType, ReadOnlySpan<T> deviceData) where T : unmanaged {
        var deviceBytes = MemoryMarshal.AsBytes<T>(deviceData);
        if (deviceBytes.Length > byte.MaxValue) {
            throw new ArgumentException("Device data is larger than 255 bytes");
        }
        data[size++] = deviceId;
        data[size++] = (byte)deviceType;
        data[size++] = (byte)deviceBytes.Length;
        deviceBytes.CopyTo(new Span<byte>(data, size, deviceBytes.Length));
        size += deviceBytes.Length;
    }

    public ArraySegment<byte> Get() {
        return new ArraySegment<byte>(data, 0, size);
    }
}

[RequireComponent(typeof(RobotController))]
[RequireComponent(typeof(OperatorInterface))]
public class CodeConnector : NetworkBehaviour {
    const float EXCEPTION_LOG_PERIOD = 10f;

    private RobotController robot;
    private OperatorInterface oi;
    private UdpClient udpClient;
    private DateTime lastFeedback, lastCommand;
    private float lastConnectException = -1000;

    [NonSerialized]
    [SyncVar]
    public bool hasRobotCode = false;

    // resetCounter is initialized to a value that (should) be different each time the simulator is started.
    private static int resetCounter = (int)((DateTime.UtcNow - DateTime.MinValue).TotalSeconds % (Int32.MaxValue / 2));
    private static bool resetCallbackRegistered = false;

    public int commandsPort = 7661;
    public int feedbackPort = 7662;

    const byte DISABLED_MODE = 0;
    const byte AUTON_MODE = 1;
    const byte TELEOP_MODE = 2;

    static readonly byte[] JOYSTICK_DEVICE_IDS = new [] {
        ReservedDeviceIds.JOYSTICK_1,
        ReservedDeviceIds.JOYSTICK_2,
        ReservedDeviceIds.JOYSTICK_3,
        ReservedDeviceIds.JOYSTICK_4,
    };
    const int AxesPerJoystick = 10;
    const int ButtonsPerJoystick = 31;

    void Start() {
        robot = GetComponent<RobotController>();
        oi = GetComponent<OperatorInterface>();

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
                CodeBufferBuilder feedback = new CodeBufferBuilder();

                robot.RunSensors(feedback);

                long timestamp = (long)(Time.timeAsDouble * 1000);
                feedback.DeviceData<long>(ReservedDeviceIds.TIMESTAMP, CodeDeviceType.FEEDBACK_COUNTER_64, new[] { timestamp });
                feedback.DeviceData<int>(ReservedDeviceIds.RESET_COUNTER, CodeDeviceType.FEEDBACK_COUNTER_32, new[] { resetCounter });

                feedback.DeviceData<byte>(ReservedDeviceIds.ROBOT_MODE, CodeDeviceType.ROBOT_MODE, new [] {
                    robot.RobotMode switch {
                        RobotMode.Disabled => DISABLED_MODE,
                        RobotMode.Auton => AUTON_MODE,
                        RobotMode.Teleop => TELEOP_MODE,
                        _ => throw new ArgumentOutOfRangeException($"Unknown RobotMode value: {robot.RobotMode}"),
                    }
                });
                for (var j = 0; j < JOYSTICK_DEVICE_IDS.Length; ++j) {
                    int[] values = new int[1 + AxesPerJoystick];
                    int denseButtonState = 0;
                    for (var b = 0; b < Math.Min(ButtonsPerJoystick, oi.joysticks[j].button.Length); b++) {
                        denseButtonState |= (oi.joysticks[j].button[b] ? 1 : 0) << b;
                    }
                    values[0] = denseButtonState;
                    for (var a = 0; a < Math.Min(AxesPerJoystick, oi.joysticks[j].axis.Length); ++a) {
                        values[a + 1] = (int)(oi.joysticks[j].axis[a] * 100);
                    }
                    feedback.DeviceData<int>(JOYSTICK_DEVICE_IDS[j], CodeDeviceType.JOYSTICK, values);
                }

                try {
                    var sendBytes = feedback.Get();
                    System.Diagnostics.Trace.Assert(sendBytes.Offset == 0);
                    udpClient.Send(sendBytes.Array, sendBytes.Count);
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
                var commands = new CodeBufferView(receiveBytes);

                //if (!commands.DeviceData<byte>(ReservedDeviceIds.RESET_SIM, CodeDeviceType.COMMAND_FLAG).IsEmpty) {
                //    TODO: replace this with GameGUI.LoadScene
                //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //    Debug.Log("Reset");
                //}

                robot.RunJoints(commands);

                lastCommand = DateTime.Now;
            }
        }

        hasRobotCode = DateTime.Now - lastCommand < TimeSpan.FromSeconds(1);

        if (!hasRobotCode) {
            robot.RunJoints(GetFallbackCommands());
        }
    }

    private static int[] FloatToCommand(float value) {
        return new[] { (int)(value * 512.0f) };
    }

    private static int[] BoolToCommand(bool value) {
        return new[] { value ? 511 : -512 };
    }

    CodeBufferView GetFallbackCommands() {
        var commands = new CodeBufferBuilder();

        float drive = oi.joysticks[0].axis[1];
        float steer = oi.joysticks[0].axis[0];
        float leftPower = Mathf.Clamp(drive + steer, -1, 1);
        float rightPower = Mathf.Clamp(drive - steer, -1, 1);

        commands.DeviceData<int>(10, CodeDeviceType.MOTOR, FloatToCommand(-leftPower));
        commands.DeviceData<int>(11, CodeDeviceType.MOTOR, FloatToCommand(rightPower));

        float intake = oi.joysticks[0].button[0] ? 1.0f : 0.0f;
        commands.DeviceData<int>(12, CodeDeviceType.MOTOR, FloatToCommand(intake));

        float auxiliary = oi.joysticks[0].button[1] ? 1.0f : 0.0f;
        commands.DeviceData<int>(14, CodeDeviceType.MOTOR, FloatToCommand(auxiliary));

        float auxiliary2 = oi.joysticks[0].button[2] ? 0.5f : 0.0f;
        commands.DeviceData<int>(16, CodeDeviceType.MOTOR, FloatToCommand(auxiliary2));

        bool intakeArm = oi.joysticks[0].button[2];
        commands.DeviceData<int>(15, CodeDeviceType.MOTOR, BoolToCommand(intakeArm));

        bool launch = oi.joysticks[0].button[3];
        commands.DeviceData<int>(13, CodeDeviceType.MOTOR, BoolToCommand(launch));

        return new CodeBufferView(commands.Get());
    }
}
