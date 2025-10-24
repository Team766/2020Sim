using UnityEngine;
using System.Collections.Generic;
using Coherence;
using Coherence.Toolkit;

public class RobotController : MonoBehaviour {
    public readonly string AuthPublicKey = ApplicationArguments.GetArgument("robotPublicKey");
    public readonly string AuthPrivateKey = ApplicationArguments.GetArgument("robotPrivateKey");

    [Sync]
    public string authPublicKey;
    private GameGUI gameGui;

    RobotJoint[] Joints {
        get {
            return GetComponentsInChildren<RobotJoint>();
        }
    }
    RobotSensor[] Sensors {
        get {
            return GetComponentsInChildren<RobotSensor>();
        }
    }

    public RobotMode RobotMode {
        get {
            return gameGui.robotMode;
        }
    }

    public bool IsDisabled {
        get {
            return RobotMode == RobotMode.Disabled;
        }
    }

    void Awake() {
        gameGui = FindAnyObjectByType<GameGUI>();
    }

    private void Start() {
        if (!string.IsNullOrWhiteSpace(AuthPublicKey)) {
            if (GetComponent<CoherenceSync>().HasStateAuthority) {
                Debug.Log("Robot Has Authority");
                authPublicKey = AuthPublicKey;
            }
            else if (authPublicKey == AuthPublicKey) {
                Debug.Log("SendCommand CmdSetInputClient");
                GetComponent<CoherenceSync>().SendCommand<RobotController>(
                    nameof(CmdSetInputClient),
                    MessageTarget.StateAuthorityOnly,
                    AuthPrivateKey);
            }
        }
    }

    [Command(UseMeta = true)]
    public void CmdSetInputClient(string privateKeyChallenge) {
        GetComponent<CoherenceSync>().TransferAuthority(
            CoherenceSync.CurrentCommandMeta.Sender,
            Coherence.AuthorityType.Input);
    }

    void Update() {
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

        if (!GetComponent<CoherenceSync>().HasStateAuthority) {
            if (rigidbody) {
                rigidbody.useGravity = false;
            }
            if (articBody) {
                articBody.useGravity = false;
                articBody.enabled = false;
            }
            foreach (RobotJoint j in Joints) {
                if (j) {
                    j.Destroy();
                }
            }
        }
    }

    internal void ValidateSensorIndices(Object origin) {
        var sensorIndices = new Dictionary<int, string>(CodeConnector.BaseFeedbackValueIndices);
        foreach (RobotSensor s in GetComponentsInChildren<RobotSensor>(true)) {
            if (!s) {
                continue;
            }
            foreach (int index in s.FeedbackValueIndices) {
                if (sensorIndices.ContainsKey(index)) {
                    Debug.LogError($"Multiple sensors use feedback index {index}: {sensorIndices[index]}, {s.name}", origin ?? s);
                } else {
                    sensorIndices.Add(index, s.name);
                }
            }
        }
    }

    void OnValidate() {
        ValidateSensorIndices(null);
        Debug.Log("Robot property validation complete");
    }

    public void RunJoints(int[] commands) {
        if (IsDisabled) {
            return;
        }
        foreach (RobotJoint j in Joints) {
            if (j) {
                j.RunJoint(commands);
            }
        }
    }

    public void RunSensors(int[] feedbackValues) {
        foreach (RobotSensor s in Sensors) {
            if (s) {
                s.RunSensor(feedbackValues);
            }
        }
    }

    void Disable() {
        foreach (RobotJoint j in Joints) {
            if (j) {
                j.Disable();
            }
        }
    }
}