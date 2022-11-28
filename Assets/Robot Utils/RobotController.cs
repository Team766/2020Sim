using UnityEngine;
using System.Collections.Generic;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
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

        GetComponent<Rigidbody>().isKinematic = IsDisabled;
        if (IsDisabled) {
            Disable();
        }

        if (!isServer) {
            GetComponent<Rigidbody>().useGravity = false;
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

    private const float minimumVelocityAllowed = 0.03f;
    private const float minimumAngularVelocityAllowed = 0.02f;
 
    private Vector3 lastPosition;
    private Quaternion lastRotation;
 
    void OnEnable() {
        var rigidbody = GetComponent<Rigidbody>();
        lastPosition = rigidbody.position;
        lastRotation = rigidbody.rotation;
    }
 
    void FixedUpdate() {
        // This is a hack to stop the robot from "sliding" when not moving.
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody.velocity.magnitude < minimumVelocityAllowed) {
            rigidbody.position = lastPosition;
        } else {
            lastPosition = rigidbody.position;
        }
        if (rigidbody.angularVelocity.magnitude < minimumAngularVelocityAllowed) {
            rigidbody.rotation = lastRotation;
        } else {
            lastRotation = rigidbody.rotation;
        }
    }
}