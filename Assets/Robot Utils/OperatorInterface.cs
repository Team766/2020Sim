using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class Joystick {
    public const int NUM_JOYSTICKS = 4;
    public const int NUM_AXES = 4;
    public const int NUM_BUTTONS = 8;

    public double[] axis = new double[NUM_AXES];
    public bool[] button = new bool[NUM_BUTTONS];
}

public class OperatorInterface : NetworkBehaviour {
    public GameGUI gameGui;

    public Joystick[] joysticks =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(_ => new Joystick()).ToArray();

    [Command(requiresAuthority = false)]
    private void CmdSetJoysticks(Joystick[] joysticks) {
        this.joysticks = joysticks;
    }

    [ClientCallback]
    void Update () {
        if (gameGui.IsOwner) {
            for (var j = 0; j < Joystick.NUM_JOYSTICKS; ++j) {
                for (var a = 0; a < Joystick.NUM_AXES; ++a) {
                    joysticks[j].axis[a] = Input.GetAxis("j" + j + "a" + a);
                }
                for (var b = 0; b < Joystick.NUM_BUTTONS; b++) {
                    joysticks[j].button[b] = Input.GetKey("joystick " + (j + 1) + " button " + b);
                }
            }
            joysticks[0].button[0] |= Input.GetKey(KeyCode.LeftControl);
            joysticks[0].button[1] |= Input.GetKey(KeyCode.LeftShift);
            joysticks[0].button[2] |= Input.GetKey(KeyCode.LeftAlt);
            joysticks[1].button[0] |= Input.GetKey(KeyCode.RightControl);
            joysticks[1].button[1] |= Input.GetKey(KeyCode.RightShift);
            joysticks[1].button[2] |= Input.GetKey(KeyCode.RightAlt);
            joysticks[2].button[0] |= Input.GetKey(KeyCode.Alpha1);
            joysticks[2].button[1] |= Input.GetKey(KeyCode.Alpha2);
            joysticks[2].button[2] |= Input.GetKey(KeyCode.Alpha3);
            joysticks[2].button[3] |= Input.GetKey(KeyCode.Alpha4);
            joysticks[2].button[4] |= Input.GetKey(KeyCode.Alpha5);
            joysticks[2].button[5] |= Input.GetKey(KeyCode.Alpha6);
            joysticks[2].button[6] |= Input.GetKey(KeyCode.Alpha7);
            joysticks[2].button[7] |= Input.GetKey(KeyCode.Alpha8);
            CmdSetJoysticks(joysticks);
        }
    }
}
