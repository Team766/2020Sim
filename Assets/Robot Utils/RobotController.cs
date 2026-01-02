using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;

public class RobotController : NetworkBehaviour {
    private GameGUI gameGui;

    RobotDevice[] Devices {
        get {
            return GetComponentsInChildren<RobotDevice>();
        }
    }

    public RobotMode RobotMode {
        get {
            return gameGui ? gameGui.RobotMode : RobotMode.Disabled;
        }
    }

    public bool IsDisabled {
        get {
            return RobotMode == RobotMode.Disabled;
        }
    }

    void Awake()
    {
        gameGui = FindAnyObjectByType<GameGUI>();
    }

    void Update()
    {
        var rigidbody = GetComponent<Rigidbody>();
        var articBody = GetComponent<ArticulationBody>();

        if (rigidbody) {
            rigidbody.isKinematic = IsDisabled;
        }
        if (articBody) {
            articBody.immovable = IsDisabled;
        }
        if (IsDisabled) {
            Disable();
        }

        if (!isServer) {
            if (rigidbody) {
                rigidbody.useGravity = false;
            }
            if (articBody) {
                articBody.useGravity = false;
                articBody.enabled = false;
            }
            foreach (RobotDevice j in Devices) {
                if (j) {
                    j.Destroy();
                }
            }
        }
    }

    internal byte ValidateDeviceIds(UnityEngine.Object origin) {
        var deviceIds = new Dictionary<byte, string>();
        foreach (RobotDevice d in GetComponentsInChildren<RobotDevice>(true)) {
            if (!d) {
                continue;
            }
            if (d.DeviceId < ReservedDeviceIds.BEGIN_ROBOT_DEVICE_ID) {
                Debug.LogError($"Robot device {d.name} has a reserved DeviceId {d.DeviceId}", d);
            }
            if (deviceIds.ContainsKey(d.DeviceId)) {
                Debug.LogError($"Multiple devices use DeviceId {d.DeviceId}: {deviceIds[d.DeviceId]}, {d.name}", d);
            } else {
                deviceIds.Add(d.DeviceId, d.name);
            }
        }
        for (byte id = ReservedDeviceIds.BEGIN_ROBOT_DEVICE_ID; id < byte.MaxValue; ++id) {
            if (!deviceIds.ContainsKey(id)) {
                return id;
            }
        }
        Debug.LogWarning("Robot DeviceIds have been exhausted", origin);
        return byte.MaxValue;
    }

    new void OnValidate() {
        base.OnValidate();
        ValidateDeviceIds(null);
        Debug.Log("Robot property validation complete");
    }

    [Command]
    public void RunJoints(CodeBufferView commands) {
        if (IsDisabled) {
            return;
        }
        foreach (RobotDevice j in Devices) {
            if (j) {
                j.RunJoint(commands);
            }
        }
    }

    public void RunSensors(CodeBufferBuilder feedbackValues) {
        foreach (RobotDevice s in Devices) {
            if (s) {
                s.RunSensor(feedbackValues);
            }
        }
    }

    void Disable() {
        foreach (RobotDevice j in Devices) {
            if (j) {
                j.Disable();
            }
        }
    }
}