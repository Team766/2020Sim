using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RobotController : NetworkBehaviour {
    public static string robotVariant = "";
    [SyncVar]
    private string clientRobotVariant = null;
    private string activeRobotVariant = null;

    public GameGUI gameGui;

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

    public bool IsDisabled {
        get {
            return gameGui.RobotMode == RobotMode.Disabled;
        }
    }

    void Awake()
    {
        UpdateVariant();
    }

    void Update()
    {
        UpdateVariant();

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
            foreach (RobotJoint j in Joints) {
                if (j) {
                    j.Destroy();
                }
            }
        }
    }

    void UpdateVariant() {
        if (isServer) {
            clientRobotVariant = robotVariant;
        } else if (clientRobotVariant != null) {
            robotVariant = clientRobotVariant;
        }
        if (activeRobotVariant != robotVariant) {
            if (activeRobotVariant != null) {
                Debug.Log("Deactivating robot variant: " + activeRobotVariant);
                var oldVariantObj = transform.Find(activeRobotVariant);
                if (oldVariantObj != null) {
                    Debug.Log("Found robot variant object");
                    oldVariantObj.gameObject.SetActive(false);
                }
            }
            Debug.Log("Activating robot variant: " + robotVariant);
            var variantObj = transform.Find(robotVariant);
            if (variantObj != null) {
                Debug.Log("Found robot variant object");
                variantObj.gameObject.SetActive(true);
            }
            activeRobotVariant = robotVariant;
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