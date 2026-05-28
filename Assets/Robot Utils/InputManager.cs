using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

#nullable enable

[System.Serializable]
public class Joystick {
    public const int NUM_JOYSTICKS = 4;
    public const int NUM_AXES = 12;
    public const int NUM_BUTTONS = 12;

    public float[] axis = new float[NUM_AXES];
    public bool[] button = new bool[NUM_BUTTONS];

    public void copyFrom(Joystick other) {
        Array.Copy(other.axis, axis, axis.Length);
        Array.Copy(other.button, button, button.Length);
    }
}

public class InputManager : NetworkBehaviour {
    [NonSerialized]
    [SyncVar]
    public string? authPublicKey = null;

    private string? server_authPrivateKey = null;

    [NonSerialized]
    public Joystick[] joysticks =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(_ => new Joystick()).ToArray();

    private readonly string[][] axisNames =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(j =>
            Enumerable.Range(0, Joystick.NUM_AXES).Select(a => "j" + j + "a" + a).ToArray()).ToArray();
    private readonly string[][] buttonCodes =
        Enumerable.Range(0, Joystick.NUM_JOYSTICKS).Select(j =>
            Enumerable.Range(0, Joystick.NUM_BUTTONS).Select(b => "j" + j + "b" + b).ToArray()).ToArray();

    public override void OnStartLocalPlayer() {
        CmdSetAccess(ApplicationArguments.AuthPublicKey, ApplicationArguments.AuthPrivateKey);
    }

    [Command]
    public void CmdSetAccess(string publicKey, string privateKey) {
        authPublicKey = publicKey;
        server_authPrivateKey = privateKey;
    }

    [Command(requiresAuthority = false)]
    private void CmdSetJoysticks(Joystick[] joysticks, string privateKeyChallenge) {
        //if (privateKeyChallenge != server_authPrivateKey) {
        //    return;
        //}
        this.joysticks = joysticks;
        //RpcSetJoysticks(joysticks);
    }

    [TargetRpc]
    private void RpcSetJoysticks(Joystick[] joysticks) {
        this.joysticks = joysticks;
    }

    [ClientCallback]
    void Update () {
        if (
            //authPublicKey == ApplicationArguments.AuthPublicKey &&
            ApplicationArguments.PlayerRole.IsInputPlayer()
        ) {
            for (var j = 0; j < Joystick.NUM_JOYSTICKS; ++j) {
                for (var a = 0; a < Joystick.NUM_AXES; ++a) {
                    joysticks[j].axis[a] = Input.GetAxis(axisNames[j][a]);
                }
                for (var b = 0; b < Joystick.NUM_BUTTONS; b++) {
                    joysticks[j].button[b] = Input.GetButton(buttonCodes[j][b]);
                }
            }
            CmdSetJoysticks(joysticks, ApplicationArguments.AuthPrivateKey);
        }
    }
}
