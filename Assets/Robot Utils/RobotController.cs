using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;
using Team766.Simulator;

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

    void Start()
    {
        // Don't call FindAnyObjectByType in Awake because of script ordering issues.
        gameGui = FindAnyObjectByType<GameGUI>();
        if (!gameGui)
        {
            Debug.LogWarning("RobotController isn't connected to a GameGUI");
        }
        if (StartPositionDialog.GetStartPosition() is Pose2 startPose) {
            var startPosition = new Vector3(startPose.position.x, this.transform.position.y, startPose.position.y);
            var startRotation = Quaternion.Euler(0, startPose.rotation, 0);
            Debug.Log("Start position: " + startPosition + " @ " + startRotation);
            var articBody = GetComponent<ArticulationBody>();
            if (articBody)
            {
                articBody.TeleportRoot(startPosition, startRotation);
            }
            else
            {
                this.transform.position = startPosition;
                this.transform.rotation = startRotation;
            }
        }
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

    internal void ValidateDeviceIds(UnityEngine.Object origin) {
        var deviceIds = new Dictionary<DeviceIdKey, string>();
        foreach (RobotDevice d in GetComponentsInChildren<RobotDevice>(true)) {
            if (!d) {
                continue;
            }
            DeviceIdKey key = new(d.DeviceId, d.DeviceIdSpace);
            if (deviceIds.ContainsKey(key)) {
                Debug.LogError($"Multiple devices use DeviceId {d.DeviceIdSpace} {d.DeviceId}: {deviceIds[key]}, {d.name}.{d.GetType().Name}", d);
            } else {
                deviceIds.Add(key, $"{d.name}.{d.GetType().Name}");
            }
        }
    }

    new void OnValidate() {
        base.OnValidate();
        ValidateDeviceIds(null);
        Debug.Log("Robot property validation complete");
    }

    [Command]
    public void RunJoints(CommandsPacket commands) {
        if (IsDisabled) {
            return;
        }
        foreach (RobotDevice j in Devices) {
            if (j) {
                j.RunJoint(commands);
            }
        }
    }

    public void RunSensors(FeedbackPacket feedbackValues) {
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

    public GameObject ballPrefab;

    [Command(requiresAuthority = false)]
    public void cmdLoadBall()
    {
        LoadBall();
    }

    [Server]
    void LoadBall()
    {
        foreach (BallStorage stor in GetComponentsInChildren<BallStorage>(true)) {
            if (stor.NumHolding == stor.heldObjects.Count) {
                continue;
            }
            var obj = Instantiate(ballPrefab, this.transform.position, this.transform.rotation);
            NetworkServer.Spawn(obj);
            var rigidbody = obj.GetComponent<Rigidbody>();
            stor.StoreBall(rigidbody);
            return;
        }
        Debug.Log("No ball storage capacity remaining");
    }
}