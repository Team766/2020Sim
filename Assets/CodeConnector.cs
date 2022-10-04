using UnityEngine;
using UnityEngine.SceneManagement;
using System;
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
    
    private bool launched = false;
    
    const int COMMANDS_PORT = 7661;
    const int FEEDBACK_PORT = 7662;

    // Command indexes
    const int RESET_SIM = 0;
  
	const int LEFT_MOTOR = 10;
	const int RIGHT_MOTOR = 11;
    const int CENTER_MOTOR = 14;
	const int INTAKE = 12;
	const int LAUNCH = 13;
	const int INTAKE_ARM = 15;
    const int AUX2_MOTOR = 16;

    // Feedback indexes
    const int TIMESTAMP_LSW = 5;
    const int TIMESTAMP_MSW = 4;

    const int ROBOT_X = 8;
    const int ROBOT_Y = 9;
    const int LEFT_ENCODER = 10;
    const int RIGHT_ENCODER = 11;
    const int HEADING = 12;
    const int MECHANISM_ENCODER = 13;
    const int BALL_PRESENCE = 14;
    const int HEADING_PRECISE = 15;
    const int HEADING_RATE = 16;
    const int LINE_SENSOR_1 = 17;
    const int LINE_SENSOR_2 = 18;
    const int LINE_SENSOR_3 = 19;

    const int ROBOT_MODE = 3;
    const int DISABLED_MODE = 0;
    const int AUTON_MODE = 1;
    const int TELEOP_MODE = 2;

    const int NumJoysticks = 4;
    const int JoystickAxisStart = 20;
    const int AxesPerJoystick = 4;
    const int JoystickButtonStart = 40;
    const int ButtonsPerJoystick = 8;

    const int GYRO_PITCH = 80;
    const int GYRO_ROLL = 81;
  
    void Start() {
        if (Application.platform != RuntimePlatform.WebGLPlayer) {
            Application.targetFrameRate = Mathf.RoundToInt(1f / Time.fixedDeltaTime);
        }
        Debug.Log("Starting UDP Code Connector");
        udpClient = new UdpClient(COMMANDS_PORT);
        udpClient.Connect(IPAddress.Loopback, FEEDBACK_PORT);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // http://stackoverflow.com/a/7478498
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
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
        if (DateTime.Now - lastFeedback > TimeSpan.FromMilliseconds(1)) {
            lastFeedback = DateTime.Now;
            int[] values = new int[100];
            long timestamp = (long)(Time.timeAsDouble * 1000);
            values[TIMESTAMP_LSW] = (int)timestamp;
            values[TIMESTAMP_MSW] = (int)(timestamp >> 32);
            values[ROBOT_X] = (int)(robot.transform.position.x * 1000);
            values[ROBOT_Y] = (int)(robot.transform.position.z * 1000);
            values[LEFT_ENCODER] = robot.LeftEncoder;
            values[RIGHT_ENCODER] = robot.RightEncoder;
            values[MECHANISM_ENCODER] = robot.MechanismEncoder;
            values[HEADING] = (int)robot.GyroAngle;
            values[HEADING_PRECISE] = (int)(robot.GyroAngle * 10);
            values[HEADING_RATE] = (int)(robot.GyroRate * 100);
            values[GYRO_PITCH] = (int)(robot.GyroPitch * 10);
            values[GYRO_ROLL] = (int)(robot.GyroRoll * 10);
            values[BALL_PRESENCE] = robot.BallPresence ? 1 : 0;
            if (robot.lineSensor1 != null) {
                values[LINE_SENSOR_1] = robot.lineSensor1.IsDetecting ? 1 : 0;
            }
            if (robot.lineSensor2 != null) {
                values[LINE_SENSOR_2] = robot.lineSensor2.IsDetecting ? 1 : 0;
            }
            if (robot.lineSensor3 != null) {
                values[LINE_SENSOR_3] = robot.lineSensor3.IsDetecting ? 1 : 0;
            }
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
                for (var a = 0; a < AxesPerJoystick; ++a) {
                    values[j * AxesPerJoystick + a + JoystickAxisStart] = (int)(oi.joysticks[j].axis[a] * 100);
                }
                for (var b = 0; b < ButtonsPerJoystick; b++)
                {
                    values[j * ButtonsPerJoystick + b + JoystickButtonStart] = oi.joysticks[j].button[b] ? 1 : 0;
                }
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

        if (udpClient.Available > 0) {
            IPEndPoint e = new IPEndPoint(IPAddress.Any, COMMANDS_PORT);
            Byte[] receiveBytes = null;
            try {
                receiveBytes = udpClient.Receive(ref e);
            } catch (IOException ex) {
                Debug.LogException(ex, this);
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

                        robot.SetMotors(commands[LEFT_MOTOR] / 512.0f, commands[RIGHT_MOTOR] / 512.0f, commands[CENTER_MOTOR] / 512.0f);
                        robot.SetAuxiliaryMotor(commands[CENTER_MOTOR] / 512.0f);
                        robot.SetAuxiliary2Motor(commands[AUX2_MOTOR] / 512.0f);
                        robot.SetIntake(commands[INTAKE] / 512.0f);
                        robot.SetIntakeArm(commands[INTAKE_ARM] > 0);
                        if (commands[LAUNCH] >= 256) {
                            if (!launched) {
                                robot.Launch();
                            }
                            launched = true;
                        } else {
                            launched = false;
                        }
                    }
                }

                lastCommand = DateTime.Now;
            }
        }

        gameGui.haveRobotCode = DateTime.Now - lastCommand < TimeSpan.FromSeconds(1);
    }
}
