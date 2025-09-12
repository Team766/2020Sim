using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

public class CodeConnector : MonoBehaviour {
    const float EXCEPTION_LOG_PERIOD = 10f;

    public RobotController robot;
    public OperatorInterface oi;
    public GameGUI gameGui;
    private UdpClient udpClient;
    private DateTime lastFeedback, lastCommand;
    private float lastConnectException = -1000;

    // resetCounter is initialized to a value that (should) be different each time the simulator is started.
    private static int resetCounter = (int)((DateTime.UtcNow - DateTime.MinValue).TotalSeconds % (Int32.MaxValue / 2));
    private static bool resetCallbackRegistered = false;

    public int commandsPort = 7661;
    public int feedbackPort = 7662;

    // Command indexes
    const int RESET_SIM = 0;

    // Feedback indexes
    const int MAX_FEEDBACKS = 200;

    const int TIMESTAMP_LSW = 5;
    const int TIMESTAMP_MSW = 4;

    const int RESET_COUNTER = 6;

    const int ROBOT_MODE = 3;
    const int DISABLED_MODE = 0;
    const int AUTON_MODE = 1;
    const int TELEOP_MODE = 2;

    const int NumJoysticks = 4;
    const int BaseAxisStart = 20;
    const int BaseAxesPerJoystick = 4;
    const int AdditionalAxisStart = 100;
    const int AdditionalAxesPerJoystick = 4;
    const int LegacyButtonsStart = 40;
    const int LegacyButtonsPerJoystick = 8;
    const int DenseButtonsStart = 72;
    const int DenseButtonsPerJoystick = 20;

    public static Dictionary<int, string> BaseFeedbackValueIndices {
        get {
            var feedbackIndices = new Dictionary<int, string>();
            for (int i = 0; i < 8; ++i) {
                feedbackIndices[i] = "Reserved";
            }
            feedbackIndices[TIMESTAMP_LSW] = "Timestamp";
            feedbackIndices[TIMESTAMP_MSW] = "Timestamp";
            feedbackIndices[RESET_COUNTER] = "Reset Counter";
            feedbackIndices[ROBOT_MODE] = "Robot Mode";
            for (var j = 0; j < NumJoysticks; ++j) {
                for (var a = 0; a < BaseAxesPerJoystick; ++a) {
                    feedbackIndices.Add(j * BaseAxesPerJoystick + a + BaseAxisStart, "Joystick");
                }
                for (var a = 0; a < AdditionalAxesPerJoystick; ++a) {
                    feedbackIndices.Add(j * AdditionalAxesPerJoystick + a + AdditionalAxisStart, "Joystick");
                }
                for (var b = 0; b < LegacyButtonsPerJoystick; b++) {
                    feedbackIndices.Add(j * LegacyButtonsPerJoystick + b + LegacyButtonsStart, "Joystick");
                }
                feedbackIndices.Add(j + DenseButtonsStart, "Joystick");
            }
            return feedbackIndices;
        }
    }

    void Start() {
        if (Application.platform != RuntimePlatform.WebGLPlayer) {
            Application.targetFrameRate = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
        }

        if (!resetCallbackRegistered) {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => { ++resetCounter; };
            resetCallbackRegistered = true;
        }

        if (Application.platform != RuntimePlatform.WebGLPlayer) {
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
                int[] values = new int[MAX_FEEDBACKS];

                robot.RunSensors(values);

                long timestamp = (long)(Time.timeAsDouble * 1000);
                values[TIMESTAMP_LSW] = (int)timestamp;
                values[TIMESTAMP_MSW] = (int)(timestamp >> 32);
                values[RESET_COUNTER] = resetCounter;

                switch (gameGui.RobotMode) {
                    case RobotMode.Disabled:
                        values[ROBOT_MODE] = DISABLED_MODE;
                        break;
                    case RobotMode.Auton:
                        values[ROBOT_MODE] = AUTON_MODE;
                        break;
                    case RobotMode.Teleop:
                        values[ROBOT_MODE] = TELEOP_MODE;
                        break;
                }
                for (var j = 0; j < NumJoysticks; ++j) {
                    for (var a = 0; a < BaseAxesPerJoystick; ++a) {
                        values[j * BaseAxesPerJoystick + a + BaseAxisStart] = (int)(oi.joysticks[j].axis[a] * 100);
                    }
                    for (var a = 0; a < AdditionalAxesPerJoystick; ++a) {
                        values[j * AdditionalAxesPerJoystick + a + AdditionalAxisStart] = (int)(oi.joysticks[j].axis[a + BaseAxesPerJoystick] * 100);
                    }
                    for (var b = 0; b < LegacyButtonsPerJoystick; b++) {
                        values[j * LegacyButtonsPerJoystick + b + LegacyButtonsStart] = oi.joysticks[j].button[b] ? 1 : 0;
                    }
                    int denseButtonState = 0;
                    for (var b = 0; b < DenseButtonsPerJoystick; b++) {
                        denseButtonState |= (oi.joysticks[j].button[b] ? 1 : 0) << b;
                    }
                    values[j + DenseButtonsStart] = denseButtonState;
                }

                byte[] sendBytes = new byte[values.Length * sizeof(int)];
                Buffer.BlockCopy(values, 0, sendBytes, 0, sendBytes.Length);
                try {
                    udpClient.Send(sendBytes, sendBytes.Length);
                } catch (SocketException ex) {
                    if (gameGui.haveRobotCode || (Time.realtimeSinceStartup - lastConnectException > EXCEPTION_LOG_PERIOD)) {
                        lastConnectException = Time.realtimeSinceStartup;
                        Debug.LogException(ex, this);
                    }
                }
            }

            Byte[] receiveBytes = null;
            while (udpClient.Available > 0) {
                IPEndPoint e = new IPEndPoint(IPAddress.Any, commandsPort);
                try {
                    receiveBytes = udpClient.Receive(ref e);
                } catch (IOException ex) {
                    Debug.LogException(ex, this);
                }
            }
            if (receiveBytes != null) {
                if (receiveBytes.Length % sizeof(int) == 0) {
                    int[] commands = new int[receiveBytes.Length / sizeof(int)];
                    Buffer.BlockCopy(receiveBytes, 0, commands, 0, receiveBytes.Length);

                    if (commands.Length >= 14) {
                        /*if (commands[RESET_SIM] > 0) {
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                            Debug.Log("Reset");
                            commands[RESET_SIM] = 0;
                        }*/

                        robot.RunJoints(commands);
                    }
                }

                lastCommand = DateTime.Now;
            }
        }

        gameGui.haveRobotCode = DateTime.Now - lastCommand < TimeSpan.FromSeconds(1);

        if (!gameGui.haveRobotCode) {
            robot.RunJoints(GetFallbackCommands());
        }
    }

    private int FloatToCommand(float value) {
        return (int)(value * 512.0f);
    }

    private int BoolToCommand(bool value) {
        return value ? 511 : -512;
    }

    int[] GetFallbackCommands() {
        int[] commands = new int[128];

        float drive = oi.joysticks[0].axis[1];
        float steer = oi.joysticks[0].axis[0];
        float leftPower = Mathf.Clamp(drive + steer, -1, 1);
        float rightPower = Mathf.Clamp(drive - steer, -1, 1);

        commands[10] = FloatToCommand(-leftPower);
        commands[11] = FloatToCommand(rightPower);

        float intake = oi.joysticks[0].button[0] ? 1.0f : 0.0f;
        commands[12] = FloatToCommand(intake);

        float auxiliary = oi.joysticks[0].button[1] ? 1.0f : 0.0f;
        commands[14] = FloatToCommand(auxiliary);

        float auxiliary2 = oi.joysticks[0].button[2] ? 0.5f : 0.0f;
        commands[16] = FloatToCommand(auxiliary2);

        bool intakeArm = oi.joysticks[0].button[2];
        commands[15] = BoolToCommand(intakeArm);

        bool launch = oi.joysticks[0].button[3];
        commands[13] = BoolToCommand(launch);

        return commands;
    }
}
