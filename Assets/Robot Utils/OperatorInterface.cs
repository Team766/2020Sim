using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Coherence.Toolkit;

[System.Serializable]
public class Joystick {
    public const int NUM_JOYSTICKS = 4;
    public const int NUM_AXES = 12;
    public const int NUM_BUTTONS = 12;

    public float[] axis = new float[NUM_AXES];
    public bool[] button = new bool[NUM_BUTTONS];
}

public class OperatorInterface : MonoBehaviour {
    public Joystick[] joysticks =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(_ => new Joystick()).ToArray();

    private readonly string[][] axisNames =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(j =>
            Enumerable.Range(0, Joystick.NUM_AXES).Select(a => "j" + j + "a" + a).ToArray()).ToArray();
    private readonly string[][] buttonCodes =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(j =>
            Enumerable.Range(0, Joystick.NUM_BUTTONS).Select(b => "j" + j + "b" + b).ToArray()).ToArray();

    void Update () {
        var sync = GetComponent<CoherenceSync>();
        if (sync.HasInputAuthority && sync.HasStateAuthority) {
            for (var j = 0; j < Joystick.NUM_JOYSTICKS; ++j) {
                for (var a = 0; a < Joystick.NUM_AXES; ++a) {
                    joysticks[j].axis[a] = Input.GetAxis(axisNames[j][a]);
                }
                for (var b = 0; b < Joystick.NUM_BUTTONS; b++) {
                    joysticks[j].button[b] = Input.GetButton(buttonCodes[j][b]);
                }
            }
        } else if (sync.Input && sync.HasStateAuthority) {
            for (var j = 0; j < Joystick.NUM_JOYSTICKS; ++j) {
                for (var a = 0; a < Joystick.NUM_AXES; ++a) {
                    joysticks[j].axis[a] = sync.Input.GetAxis(axisNames[j][a]);
                }
                for (var b = 0; b < Joystick.NUM_BUTTONS; b++) {
                    joysticks[j].button[b] = sync.Input.GetButton(buttonCodes[j][b]);
                }
            }
        } else if (sync.Input && sync.HasInputAuthority) {
            for (var j = 0; j < Joystick.NUM_JOYSTICKS; ++j) {
                for (var a = 0; a < Joystick.NUM_AXES; ++a) {
                    var name = axisNames[j][a];
                    var value = Input.GetAxis(name);
                    sync.Input.SetAxis(name, value);
                }
                for (var b = 0; b < Joystick.NUM_BUTTONS; b++) {
                    var name = buttonCodes[j][b];
                    var value = Input.GetButton(name);
                    sync.Input.SetButton(name, value);
                }
            }
        }
    }
}
