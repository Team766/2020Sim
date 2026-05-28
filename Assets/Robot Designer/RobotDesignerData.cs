using System;
using System.Collections.Generic;
using UnityEngine;
using Team766.Simulator;

[CreateAssetMenu(fileName = "NewRobotDesign", menuName = "Robot Designer/Robot Design", order = 1)]
public class RobotDesignerData : ScriptableObject
{
    public const float INCHES_TO_METERS = 0.0254f;
    public const float FEET_TO_METERS = 0.3048f;

    [Serializable]
    public class Drivetrain
    {
        public float length = 30 * INCHES_TO_METERS;
        public float width = 30 * INCHES_TO_METERS;
        // Defaults based on SDS Mk4i L2 modules
        public float maxSpeed = 14 * FEET_TO_METERS;
        public float acceleration = 25 * FEET_TO_METERS;
    }

    [Serializable]
    public class Joint
    {
        public enum ConstraintType
        {
            // TODO: Free,
            // TODO: Locked,
            Driven,
            // TODO: Sprung,
        }

        public ConstraintType constraintType = ConstraintType.Driven;
        // TODO: [SerializeReference] public Node constraintReference;
        public float constraintStrength = 1.0f;
        public float maxSpeed = 30.0f; // degrees per second or meters per second
        public bool inverted = false;
        public float minimumPosition; // degrees or meters
        public float maximumPosition; // degrees or meters
    }

    [Serializable]
    public class Collector
    {
        public float flowRatePeriod = 0.1f;
        public float startupTime = 1.0f;
        public List<string> compatiblePieceTypes;

        // TODO: public bool passive = false;
    }

    [Serializable]
    public class Ejector
    {
        public float flowRatePeriod = 0.1f;
        public float startupTime = 1.0f;
        public List<string> compatiblePieceTypes;

        public float maxForce = 1.0f; // TODO: Set default
    }

    [Serializable]
    public class Storage
    {
        public int capacity = 1;
        public List<string> compatiblePieceTypes;
    }

    [Serializable]
    public class Node
    {
        public enum Type
        {
            Shape,
            Pivot,
            Extension,
            Collector,
            Ejector,
            Storage,
            Grabber,
        }

        public string guid = Guid.NewGuid().ToString();
        public string name;
        public uint deviceId;

        public Vector3 location = Vector3.zero;
        public Vector3 orientation = Vector3.zero;
        public Vector3 size = new(12 * INCHES_TO_METERS, 24 * INCHES_TO_METERS, 4 * INCHES_TO_METERS);

        // TODO: public float mass;

        public Type type = Type.Shape;
        public Joint pivot;
        public Joint extension;
        public Collector collector;
        public Ejector ejector;
        public Storage storage;

        public List<Node> children = new();
    }

    [Serializable]
    public class OperatorControlsDesign
    {
        public enum DriveControlsLayout
        {
            SkidSteerArcadeControls,
            SwerveRobotOrientedControls,
            SwerveFieldOrientedControls,
        }

        [Serializable]
        public class MotorSetpoint
        {
            public string jointNodeGuid;
            public MotorActuatorProto.Types.Mode mode = MotorActuatorProto.Types.Mode.PercentOutput;
            public float setpoint;
        }

        [Serializable]
        public class RobotSetpoint
        {
            public List<MotorSetpoint> motors = new();
        }

        [Serializable]
        public class ButtonActions
        {
            public int button;
            public RobotSetpoint pressed = new();
            public RobotSetpoint released = new();
        }

        public DriveControlsLayout driveControlsLayout = DriveControlsLayout.SwerveRobotOrientedControls;
        public int forwardAxis = SwerveControls.DEFAULT_FORWARD_AXIS;
        public int lateralAxis = SwerveControls.DEFAULT_LATERAL_AXIS;
        public int steerAxis = SwerveControls.DEFAULT_STEER_AXIS;

        public RobotSetpoint startup = new();
        public List<ButtonActions> buttons = new();
    }

    public int version = 1;
    public string robotName;
    public Drivetrain drivetrain = new();
    public List<Node> children = new();
    public OperatorControlsDesign operatorControls = new();

    private const string PLAYER_PREFS_KEY = "robot-design";
    private const string BACKUP_PLAYER_PREFS_KEY = "robot-design-unsaved";

    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFrom(string serializedRobotDesign)
    {
        JsonUtility.FromJsonOverwrite(serializedRobotDesign, this);
    }

    public static RobotDesignerData Load(string serializedRobotDesign)
    {
        var robotDesign = ScriptableObject.CreateInstance<RobotDesignerData>();
        robotDesign.LoadFrom(serializedRobotDesign);
        return robotDesign;
    }

    public static RobotDesignerData LoadFromPlayerPrefs()
    {
        return Load(PlayerPrefs.GetString(PLAYER_PREFS_KEY));
    }

    public static void SaveToPlayerPrefs(RobotDesignerData robotDesign)
    {
        PlayerPrefs.SetString(PLAYER_PREFS_KEY, robotDesign.Serialize());
    }

    public static void LoadBackupFromPlayerPrefs(RobotDesignerData robotDesign)
    {
        robotDesign.LoadFrom(PlayerPrefs.GetString(BACKUP_PLAYER_PREFS_KEY));
    }

    public static void BackupToPlayerPrefs(string serializedRobotDesign)
    {
        PlayerPrefs.SetString(BACKUP_PLAYER_PREFS_KEY, serializedRobotDesign);
    }

    public static bool HasUnsavedBackup()
    {
        var backup = PlayerPrefs.GetString(BACKUP_PLAYER_PREFS_KEY);
        if (string.IsNullOrEmpty(backup))
        {
            return false;
        }
        return backup != PlayerPrefs.GetString(PLAYER_PREFS_KEY);
    }

    public static void ClearBackup()
    {
        PlayerPrefs.DeleteKey(BACKUP_PLAYER_PREFS_KEY);
    }
}