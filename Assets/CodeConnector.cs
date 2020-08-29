using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

public class CodeConnector : MonoBehaviour {
    public RobotController robot;
    public InputController teleop;
    private UdpClient udpClient;
    private DateTime lastFeedback, lastCommand;
    
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

    // Feedback indexes
    const int TIMESTAMP = 5;

    const int LEFT_ENCODER = 10;
    const int RIGHT_ENCODER = 11;
    const int HEADING = 12;
    const int INTAKE_STATE = 13;
    const int BALL_PRESENCE = 14;
    const int ROBOT_MODE = 3;

    const int JoystickStart = 20;
    const int AxisPerJoystick = 4;
    const int JoystickButtonStart = 40;
    const int ButtonsPerJoystick = 8;
    const int Teleop = 1;
    const int Auton = 0;

    const int Joystick2_Axis4 = 27;
  
    void Awake() {
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
  
    void Update() {
        if (DateTime.Now - lastFeedback > TimeSpan.FromMilliseconds(20)) {
            lastFeedback = DateTime.Now;
            int[] values = new int[64];
            long timestamp = (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
            values[TIMESTAMP] = (int)timestamp;
            values[LEFT_ENCODER] = robot.LeftEncoder;
            values[RIGHT_ENCODER] = robot.RightEncoder;
            values[HEADING] = (int)robot.Gyro;
            values[BALL_PRESENCE] = robot.BallPresence ? 1 : 0;
            values[ROBOT_MODE] = Time.timeSinceLevelLoad < 15? Auton : Teleop;
            for (var j = 0; j < 2; ++j) {
                for (var a = 0; a < AxisPerJoystick; ++a) {
                    values[j * AxisPerJoystick + a + JoystickStart] = (int)(Input.GetAxis("j" + j + "a" + a) * 100);
                }
                for (var b = 0; b < ButtonsPerJoystick; b++)
                {
                    values[j * ButtonsPerJoystick + b + JoystickButtonStart] = (int)(Input.GetKey("joystick " + (j + 1) + " button " + b) ? 1 : 0);
                }
            }

            byte[] sendBytes = new byte[values.Length * sizeof(int)];
            Buffer.BlockCopy(values, 0, sendBytes, 0, sendBytes.Length);
            udpClient.Send(sendBytes, sendBytes.Length);
        }

        if (udpClient.Available > 0) {
            IPEndPoint e = new IPEndPoint(IPAddress.Any, COMMANDS_PORT);
            Byte[] receiveBytes = null;
            try {
                receiveBytes = udpClient.Receive(ref e);
            } catch (IOException ex) {
                Debug.LogException(ex);
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

                        teleop.enabled = false;
                        robot.SetMotors(commands[LEFT_MOTOR] / 512.0f, commands[RIGHT_MOTOR] / 512.0f, commands[CENTER_MOTOR] / 512.0f);
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
        
        if (DateTime.Now - lastCommand > TimeSpan.FromSeconds(1)) {
            teleop.enabled = true;
        }
    }
}
