using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class RobotBuilder : NetworkBehaviour
{
    //private static readonly uint[] DRIVE_LEFT_MOTOR_IDS = new uint[] { 32, 33, 34 };
    //private static readonly uint[] DRIVE_RIGHT_MOTOR_IDS = new uint[] { 35, 36, 37 };
    private static readonly uint[] DRIVE_LEFT_MOTOR_IDS = new uint[] { 32, 34 };
    private static readonly uint[] DRIVE_RIGHT_MOTOR_IDS = new uint[] { 33, 35 };
    private static readonly uint[] STEER_MOTOR_IDS = new uint[] { 36, 37, 38, 39 };
    private static readonly SwerveControls.Motors DRIVE_SWERVE_MOTOR_IDS = new()
    {
        frontLeftDrive = 32,
        frontLeftSteer = 36,
        frontRightDrive = 33,
        frontRightSteer = 37,
        rearLeftDrive = 34,
        rearLeftSteer = 38,
        rearRightDrive = 35,
        rearRightSteer = 39,
    };

    private class WheelModuleSpec
    {
        public Vector2 location;
        public uint driveDeviceId;
        public uint steerDeviceId;
        public uint steerSensorDeviceId;
    }

    private const float DRIVE_WHEEL_RADIUS = 2 * 0.0254f; // 2 inches in meters
    private static readonly IReadOnlyList<WheelModuleSpec> wheelModuleLocations = new List<WheelModuleSpec>
    {
        new WheelModuleSpec() { location = new Vector2(-1, 1), driveDeviceId = DRIVE_SWERVE_MOTOR_IDS.frontLeftDrive, steerDeviceId = DRIVE_SWERVE_MOTOR_IDS.frontLeftSteer, steerSensorDeviceId = 40 },
        new WheelModuleSpec() { location = new Vector2(1, 1), driveDeviceId = DRIVE_SWERVE_MOTOR_IDS.frontRightDrive, steerDeviceId = DRIVE_SWERVE_MOTOR_IDS.frontRightSteer, steerSensorDeviceId = 41 },
        new WheelModuleSpec() { location = new Vector2(-1, -1), driveDeviceId = DRIVE_SWERVE_MOTOR_IDS.rearLeftDrive, steerDeviceId = DRIVE_SWERVE_MOTOR_IDS.rearLeftSteer, steerSensorDeviceId = 42 },
        new WheelModuleSpec() { location = new Vector2(1, -1), driveDeviceId = DRIVE_SWERVE_MOTOR_IDS.rearRightDrive, steerDeviceId = DRIVE_SWERVE_MOTOR_IDS.rearRightSteer, steerSensorDeviceId = 43 },
    };

    // Kraken x60 stats
    private const float ONE_MOTOR_MAX_TORQUE = 7.09f; // Newton-meters
    private const float ONE_MOTOR_MAX_SPEED = 628.3185f; // 6000 rpm in radians per second

    private const float DRIVETRAIN_STEER_GEAR_RATIO = 150f / 7f;

    private const float ROBOT_MASS = 68f; // kilograms // TODO: make this dynamic/configurable

    private class GamePieceTypeNodes
    {
        public List<Intake> intakes = new();
        public List<ContinuousLauncher> launchers = new();
        public List<BallStorage> stores = new();
    }

    public RobotDesignerData robotDesign;

    public GameObject wheelModulePrefab;
    public GameObject shapePrefab;
    public GameObject pivotPrefab;
    public GameObject extensionPrefab;
    public GameObject collectorPrefab;
    public GameObject storagePrefab;
    public GameObject ejectorPrefab;
    public GameObject grabberPrefab;

    private GameObject drivetrain = null;
    private readonly Dictionary<WheelModuleSpec, GameObject> wheelModules = new();
    private Dictionary<string, GameObject> nodes = new();
    private Dictionary<string, GameObject> previousNodes;
    private readonly DefaultDictionary<HashSet<string>, GamePieceTypeNodes> gamePieceNodes;

    public RobotBuilder()
    {
        gamePieceNodes = new(_ => new GamePieceTypeNodes());
    }

    void OnDestroy()
    {
        foreach (var module in wheelModules)
        {
            Destroy(module.Value);
        }
        foreach (var node in nodes)
        {
            Destroy(node.Value);
        }
    }

    void Start()
    {
        if (!robotDesign)
        {
            robotDesign = RobotDesignerData.LoadFromPlayerPrefs();
        }
        UpdateRobot();
    }

    public void UpdateRobot()
    {
        if (!NetworkServer.active && NetworkClient.active)
        {
            return;
        }
        if (!robotDesign)
        {
            return;
        }

        UpdateDrivetrain(robotDesign.drivetrain, this.transform);

        previousNodes = nodes;
        nodes = new();
        foreach (var child in robotDesign.children)
        {
            UpdateNode(child, this.transform);
        }
        foreach (var node in previousNodes)
        {
            Destroy(node.Value);
        }
        foreach (var entry in gamePieceNodes)
        {
            foreach (var intake in entry.Value.intakes)
            {
                intake.ballStorage = entry.Value.stores;
            }
            foreach (var launcher in entry.Value.launchers)
            {
                launcher.ballStorage = entry.Value.stores;
            }
        }
        gamePieceNodes.Clear();

        UpdateOperatorControls(robotDesign.operatorControls);
    }

    private void UpdateDrivetrain(RobotDesignerData.Drivetrain design, Transform parent)
    {
        drivetrain ??= new GameObject("Drivetrain");
        if (drivetrain.transform.parent != parent)
        {
            drivetrain.transform.SetParent(parent, false);
        }

        foreach (var location in wheelModuleLocations)
        {
            UpdateWheelModule(design, location, drivetrain.transform);
        }
    }

    private void UpdateWheelModule(RobotDesignerData.Drivetrain design, WheelModuleSpec spec, Transform parent)
    {
        var go = wheelModules.ComputeIfAbsent(spec, _ => InstantiateFromServer(wheelModulePrefab, parent));
        if (go.transform.parent != parent)
        {
            go.transform.SetParent(parent, false);
        }

        var moduleBounds = GameObjectUtils.CalculateBoundsRecursive(wheelModulePrefab);
        float moduleRadius = Mathf.Max(moduleBounds.extents.x, moduleBounds.extents.z);

        var modulePosition = new Vector3(
            spec.location.x * (design.length / 2 - moduleRadius),
            moduleBounds.center.y - wheelModulePrefab.transform.position.y + moduleBounds.extents.y,
            spec.location.y * (design.width / 2 - moduleRadius));
        go.transform.localPosition = modulePosition;

        var steer = go.transform.Find("steer").GetComponent<RotationalJoint>();
        steer.GetComponent<ArticulationBody>().parentAnchorPosition = modulePosition;
        steer.DeviceId = spec.steerDeviceId;
        steer.maxMotorTorque = ONE_MOTOR_MAX_TORQUE;
        steer.maxMotorSpeed = ONE_MOTOR_MAX_SPEED;
        steer.mechanicalScalar = DRIVETRAIN_STEER_GEAR_RATIO;
        if (design.type == RobotDesignerData.Drivetrain.Type.Differential)
        {
            steer.enabled = false;
            steer.GetComponent<ArticulationBody>().jointType = ArticulationJointType.FixedJoint;
        }
        else
        {
            steer.enabled = true;
        }
        var steerSensor = steer.GetComponent<RotaryEncoder>();
        steerSensor.DeviceId = spec.steerSensorDeviceId;
        var drive = go.transform.Find("steer/drive").GetComponent<RotationalJoint>();
        drive.DeviceId = spec.driveDeviceId;
        float motorTorque = DRIVE_WHEEL_RADIUS * design.acceleration * ROBOT_MASS; // TODO: 1.0 / wheelModuleLocations.Count
        float motorSpeed = design.maxSpeed / DRIVE_WHEEL_RADIUS;
        float mechanicalScalar = ONE_MOTOR_MAX_SPEED / motorSpeed;
        drive.maxMotorTorque = motorTorque / mechanicalScalar;
        drive.maxMotorSpeed = ONE_MOTOR_MAX_SPEED;
        drive.mechanicalScalar = mechanicalScalar;
    }

    private void UpdateNode(RobotDesignerData.Node design, Transform parent)
    {
        // TODO: Remove this?
        if (string.IsNullOrEmpty(design.guid))
        {
            design.guid = Guid.NewGuid().ToString();
        }

        if (previousNodes.TryGetValue(design.guid, out GameObject go) &&
            go.GetComponent<RobotDesignerNode>().type == design.type)
        {
            previousNodes.Remove(design.guid);
        }
        else
        {
            go = InstantiateFromServer(
                design.type switch {
                    RobotDesignerData.Node.Type.Collector => collectorPrefab,
                    RobotDesignerData.Node.Type.Ejector => ejectorPrefab,
                    RobotDesignerData.Node.Type.Extension => extensionPrefab,
                    RobotDesignerData.Node.Type.Grabber => grabberPrefab,
                    RobotDesignerData.Node.Type.Pivot => pivotPrefab,
                    RobotDesignerData.Node.Type.Shape => shapePrefab,
                    RobotDesignerData.Node.Type.Storage => storagePrefab,
                    _ => throw new ArgumentOutOfRangeException($"Unsupported node type {design.type}"),
                },
                parent);
            go.name = design.guid.ToString() + " :: " + design.name;
        }
        nodes[design.guid] = go;

        if (go.transform.parent != parent)
        {
            go.transform.SetParent(parent, false);
        }

        go.transform.localPosition = design.location;
        go.transform.localEulerAngles = design.orientation;

        var resizable = GetResizable(go.transform);
        resizable.localScale = design.size;

        switch (design.type)
        {
            case RobotDesignerData.Node.Type.Shape:
                {
                    // No-op
                }
                break;
            case RobotDesignerData.Node.Type.Pivot:
                {
                    var joint = go.GetComponent<RotationalJoint>();
                    joint.DeviceId = design.deviceId;
                    switch (design.pivot.constraintType)
                    {
                        //case RobotDesignerData.Joint.ConstraintType.Free:
                        //    break;
                        //case RobotDesignerData.Joint.ConstraintType.Locked:
                        //    break;
                        case RobotDesignerData.Joint.ConstraintType.Driven:
                            joint.maxMotorSpeed = ONE_MOTOR_MAX_SPEED;
                            joint.maxMotorTorque = design.pivot.constraintStrength * ONE_MOTOR_MAX_TORQUE;
                            joint.mechanicalScalar = ONE_MOTOR_MAX_SPEED / design.pivot.maxSpeed;
                            joint.inverted = design.pivot.inverted;
                            break;
                        //case RobotDesignerData.Joint.ConstraintType.Sprung:
                        //    break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported Joint.ConstraintType {design.pivot.constraintType}");
                    }
                    // TODO: design.pivot.minimumPosition
                    // TODO: design.pivot.maximumPosition
                }
                break;
            case RobotDesignerData.Node.Type.Extension:
                {
                    var elevator = go.transform.Find("container").GetComponent<ElevatorArticulation>();
                    elevator.DeviceId = design.deviceId;
                    switch (design.extension.constraintType)
                    {
                        //case RobotDesignerData.Joint.ConstraintType.Free:
                        //    break;
                        //case RobotDesignerData.Joint.ConstraintType.Locked:
                        //    break;
                        case RobotDesignerData.Joint.ConstraintType.Driven:
                            elevator.maxMotorSpeed = ONE_MOTOR_MAX_SPEED;
                            elevator.maxMotorTorque = design.extension.constraintStrength * ONE_MOTOR_MAX_TORQUE;
                            elevator.mechanicalScalar = ONE_MOTOR_MAX_SPEED / design.extension.maxSpeed;
                            elevator.inverted = design.extension.inverted;
                            break;
                        //case RobotDesignerData.Joint.ConstraintType.Sprung:
                        //    break;
                        default:
                            throw new ArgumentOutOfRangeException($"Unsupported Joint.ConstraintType {design.extension.constraintType}");
                    }
                    elevator.minPosition = design.extension.minimumPosition;
                    elevator.maxPosition = design.extension.maximumPosition;

                    Vector3 modelSize = design.size;
                    modelSize.y = design.extension.maximumPosition - design.extension.minimumPosition;
                    resizable.localScale = modelSize;
                    resizable.localPosition = new Vector3(0, design.extension.minimumPosition, 0);
                    var carriage = go.transform.Find("container");
                    carriage.localPosition = new Vector3(0, (design.extension.minimumPosition + design.extension.maximumPosition) / 2f, 0);
                }
                break;
            case RobotDesignerData.Node.Type.Collector:
                {
                    var intake = go.GetComponent<Intake>();
                    gamePieceNodes[new HashSet<string>(design.storage.compatiblePieceTypes)].intakes.Add(intake);
                    intake.flowRatePeriod = design.collector.flowRatePeriod;
                    intake.startupTime = design.collector.startupTime;
                    // TODO: intake.passive = design.collector.passive;
                    intake.compatiblePieceTypes = design.collector.compatiblePieceTypes;
                    var rollerSet = go.GetComponent<RollerSet>();
                    rollerSet.DeviceId = design.deviceId;
                }
                break;
            case RobotDesignerData.Node.Type.Ejector:
                {
                    var launcher = go.GetComponent<ContinuousLauncher>();
                    gamePieceNodes[new HashSet<string>(design.storage.compatiblePieceTypes)].launchers.Add(launcher);
                    launcher.flowRatePeriod = design.ejector.flowRatePeriod;
                    launcher.startupTime = design.ejector.startupTime;
                    launcher.maxForce = design.ejector.maxForce;
                    var rollerSet = go.GetComponent<RollerSet>();
                    rollerSet.DeviceId = design.deviceId;
                }
                break;
            case RobotDesignerData.Node.Type.Storage:
                {
                    var storage = go.GetComponent<BallStorage>();
                    gamePieceNodes[new HashSet<string>(design.storage.compatiblePieceTypes)].stores.Add(storage);
                    var sensor = go.GetComponent<BallStorageSensor>();
                    sensor.DeviceId = design.deviceId;
                    var holders = storage.heldObjects;
                    int designedCapacity = design.storage.capacity;
                    if (designedCapacity < holders.Count)
                    {
                        for (int i = designedCapacity; i < holders.Count; ++i)
                        {
                            Destroy(holders[i]);
                        }
                        holders.RemoveRange(designedCapacity, holders.Count - designedCapacity);
                    }
                    else
                    {
                        for (int i = holders.Count; i < designedCapacity; ++i)
                        {
                            var newHolderGo = new GameObject("Held ball");
                            newHolderGo.transform.SetParent(go.transform, false);
                            // TODO: Distribute positions of holders within design.size
                            newHolderGo.transform.localPosition = Vector3.zero;
                            var newHolder = newHolderGo.AddComponent<HoldObject>();
                            holders.Add(newHolder);
                        }
                    }
                }
                break;
            case RobotDesignerData.Node.Type.Grabber:
                {
                    var intake = go.GetComponent<Intake>();
                    intake.flowRatePeriod = design.collector.flowRatePeriod;
                    intake.startupTime = design.collector.startupTime;
                    // TODO: intake.passive = design.collector.passive;
                    intake.compatiblePieceTypes = design.collector.compatiblePieceTypes;
                    var launcher = go.GetComponent<ContinuousLauncher>();
                    launcher.flowRatePeriod = design.ejector.flowRatePeriod;
                    launcher.startupTime = design.ejector.startupTime;
                    launcher.maxForce = design.ejector.maxForce;
                    var rollerSet = go.GetComponent<RollerSet>();
                    rollerSet.DeviceId = design.deviceId;
                    // TODO: the BallStorageSensor needs a different DeviceId than the motor
                    // var sensor = go.GetComponent<BallStorageSensor>();
                    // sensor.DeviceId = design.deviceId;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported node type {design.type}");
        }

        var container = go.transform.Find("container") ?? go.transform;
        foreach (var child in design.children)
        {
            UpdateNode(child, container);
        }
    }

    private void UpdateOperatorControls(RobotDesignerData.OperatorControlsDesign design)
    {
        var real = new OperatorControls();

        real.driveControls = design.driveControlsLayout switch
        {
            RobotDesignerData.OperatorControlsDesign.DriveControlsLayout.SkidSteerArcadeControls => new SkidSteerArcadeControls(driveAxis: design.forwardAxis, steerAxis: design.steerAxis, DRIVE_LEFT_MOTOR_IDS, DRIVE_RIGHT_MOTOR_IDS),
            RobotDesignerData.OperatorControlsDesign.DriveControlsLayout.SwerveRobotOrientedControls => new SwerveRobotOrientedControls(forwardAxis: design.forwardAxis, lateralAxis: design.lateralAxis, steerAxis: design.steerAxis, DRIVE_SWERVE_MOTOR_IDS),
            RobotDesignerData.OperatorControlsDesign.DriveControlsLayout.SwerveFieldOrientedControls => new SwerveFieldOrientedControls(forwardAxis: design.forwardAxis, lateralAxis: design.lateralAxis, steerAxis: design.steerAxis, DRIVE_SWERVE_MOTOR_IDS),
            _ => throw new ArgumentOutOfRangeException($"Unknown DriveControlsLayout {design.driveControlsLayout}"),
        };

        real.buttons = design.buttons.Select(Realize).ToList();

        real.startup = Realize(design.startup);

        var codeConnector = this.GetComponent<CodeConnector>();
        codeConnector.operatorControls = real;

        OperatorControls.ApplySetpoint(real.startup, codeConnector.codelessCommands);
    }

    private OperatorControls.ButtonActions Realize(RobotDesignerData.OperatorControlsDesign.ButtonActions design)
    {
        return new()
        {
            button = design.button,
            pressed = Realize(design.pressed),
            released = Realize(design.released),
        };
    }

    private OperatorControls.RobotSetpoint Realize(RobotDesignerData.OperatorControlsDesign.RobotSetpoint design)
    {
        return new()
        {
            motors = design.motors.Select(Realize).Where(m => m != null).ToList(),
        };
    }

    private OperatorControls.MotorSetpoint Realize(RobotDesignerData.OperatorControlsDesign.MotorSetpoint design)
    {
        uint? deviceId = FindDeviceId(design.jointNodeGuid);
        if (!deviceId.HasValue)
        {
            return null;
        }
        return new()
        {
            deviceId = deviceId.Value,
            mode = design.mode,
            setpoint = design.setpoint,
        };
    }

    private uint? FindDeviceId(string deviceNodeGuid)
    {
        foreach (var child in robotDesign.children)
        {
            uint? result = FindDeviceId(child, deviceNodeGuid);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private static uint? FindDeviceId(RobotDesignerData.Node node, string deviceNodeGuid)
    {
        if (node.guid == deviceNodeGuid)
        {
            return node.deviceId;
        }
        foreach (var child in node.children)
        {
            uint? result = FindDeviceId(child, deviceNodeGuid);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private GameObject InstantiateFromServer(GameObject original, Transform parent)
    {
        var instance = GameObject.Instantiate(original, parent);
        if (NetworkServer.active)
        {
            NetworkServer.Spawn(instance);
        }
        return instance;
    }

    internal Transform GetNode(string nodeGuid)
    {
        return GetResizable(nodes[nodeGuid].transform);
    }

    static private Transform GetResizable(Transform xf)
    {
        return xf.Find("model") ?? xf;
    }
}
