using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RobotEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    public static void RegisterMyConverters()
    {
        var group = new ConverterGroup("ShowWhenDrivetrainTypeIsSwerve");
        group.AddConverter((ref RobotDesignerData.Drivetrain.Type value) => new StyleEnum<DisplayStyle>(
            value == RobotDesignerData.Drivetrain.Type.Swerve
            ? DisplayStyle.Flex
            : DisplayStyle.None));
        ConverterGroups.RegisterConverterGroup(group);
    }

    public UIDocument document;
    public JsonEditor jsonEditor;
    public UnsavedChangesDialog unsavedChangesDialog;
    public LoadBackupDialog loadBackupDialog;
    public WireframeCube selectionBox;
    public RobotBuilder robotBuilder;
    public RobotDesignerData robotDesign;

    private VisualElement root;
    private TreeView partTree;
    private VisualElement partEditor;
    private Button addPartButton;
    private Button jsonButton;
    private Button saveButton;
    private Button undoButton;
    private Button redoButton;
    private Button exitButton;
    private TextField partName;
    private EnumField partType;
    private DropdownField joystickButtonSelect;
    private VisualElement buttonActionsContainer;
    private ListView pressedActionsList;
    private ListView releasedActionsList;
    private VisualElement robotControls;
    private VisualElement drivetrainControls;
    private VisualElement jointControls;
    private VisualElement collectorControls;
    private VisualElement ejectorControls;
    private VisualElement storageControls;
    private VisualElement compatibleGamePieceControls;

    const int ROBOT_TREE_ID = -1;
    const int DRIVETRAIN_TREE_ID = 0;
    const int FIRST_NODE_TREE_ID = 1;
    private static readonly RobotDesignerData.Node robotNode = new() { name = "Robot" };
    private static readonly RobotDesignerData.Node drivetrainNode = new() { name = "Drivetrain" };

    const uint NODE_DEVICE_ID_START = 50;

    private uint nextDeviceId = NODE_DEVICE_ID_START;

    private Dictionary<string, int> nodeGuidToTreeId = new()
    {
        { robotNode.guid, ROBOT_TREE_ID },
        { drivetrainNode.guid, DRIVETRAIN_TREE_ID },
    };

    private readonly Dictionary<string, string> controllableNodesDict = new();
    private readonly List<string> controllableNodesGuids = new();

    private readonly List<string> undoStack = new();
    private int undoStackPosition = -1;

    void Awake()
    {
        robotDesign = RobotDesignerData.LoadFromPlayerPrefs();
        robotBuilder.robotDesign = robotDesign;
        jsonEditor.robotDesign = robotDesign;

        if (RobotDesignerData.HasUnsavedBackup())
        {
            loadBackupDialog.gameObject.SetActive(true);
        }
    }

    void OnEnable()
    {
        root = document.rootVisualElement;
        partName = root.Q<TextField>("part-name");
        partTree = root.Q<TreeView>("part-tree");
        addPartButton = root.Q<Button>("button-add-part");
        jsonButton = root.Q<Button>("button-json");
        saveButton = root.Q<Button>("button-save");
        undoButton = root.Q<Button>("button-undo");
        redoButton = root.Q<Button>("button-redo");
        exitButton = root.Q<Button>("button-exit");

        partEditor = root.Q("part-editor");
        partType = root.Q<EnumField>("part-type");
        joystickButtonSelect = root.Q<DropdownField>("joystick-button");
        buttonActionsContainer = root.Q<VisualElement>("button-actions-container");
        pressedActionsList = root.Q<ListView>("list-pressed-actions");
        releasedActionsList = root.Q<ListView>("list-released-actions");
        robotControls = root.Q<VisualElement>("robot-controls");
        drivetrainControls = root.Q<VisualElement>("drivetrain-controls");
        jointControls = root.Q<VisualElement>("joint-controls");
        collectorControls = root.Q<VisualElement>("collector-controls");
        ejectorControls = root.Q<VisualElement>("ejector-controls");
        storageControls = root.Q<VisualElement>("storage-controls");
        compatibleGamePieceControls = root.Q<VisualElement>("compatible-game-pieces-controls");

        root.dataSource = robotDesign;

        partTree.makeItem = () =>
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;

            var nameLabel = new Label();
            nameLabel.name = "name";
            nameLabel.style.flexGrow = 1;
            container.Add(nameLabel);

            var deleteButton = new Button();
            deleteButton.name = "button-delete";
            deleteButton.text = "X";
            deleteButton.tooltip = "Delete part";
            container.Add(deleteButton);

            return container;
        };
        partTree.bindItem = (element, index) =>
        {
            var nodeData = partTree.GetItemDataForIndex<RobotDesignerData.Node>(index);
            var parentData = partTree.GetItemDataForId<RobotDesignerData.Node>(partTree.GetParentIdForIndex(index));
            element.Q<Label>("name").text =
                nodeData == robotNode ? "Robot" :
                nodeData == drivetrainNode ? "Drivetrain" :
                $"{nodeData.name} - {nodeData.type}";
            System.Action clickedHandler = () =>
            {
                if (parentData == robotNode)
                {
                    robotDesign.children.Remove(nodeData);
                }
                else
                {
                    parentData.children.Remove(nodeData);
                }
                robotDesign.operatorControls.startup.motors.RemoveAll(m => m.jointNodeGuid == nodeData.guid);
                foreach (var button in robotDesign.operatorControls.buttons)
                {
                    button.pressed.motors.RemoveAll(m => m.jointNodeGuid == nodeData.guid);
                    button.released.motors.RemoveAll(m => m.jointNodeGuid == nodeData.guid);
                }
                UpdatePartTree();
            };
            var deleteButton = element.Q<Button>("button-delete");
            deleteButton.userData = clickedHandler;
            deleteButton.clicked += clickedHandler;
        };
        partTree.unbindItem = (element, index) =>
        {
            var deleteButton = element.Q<Button>("button-delete");
            var clickedHandler = deleteButton.userData as System.Action;
            deleteButton.clicked -= clickedHandler;
        };
        partTree.autoExpand = true;

        UpdatePartTree();
        partTree.SetSelectionById(ROBOT_TREE_ID);
        PartSelected(new int[0]);

        partTree.selectedIndicesChanged += PartSelected;

        addPartButton.clicked += AddPartButtonClicked;

        jsonButton.clicked += JsonButtonClicked;
        saveButton.clicked += SaveButtonClicked;
        undoButton.clicked += UndoButtonClicked;
        redoButton.clicked += RedoButtonClicked;
        exitButton.clicked += ExitButtonClicked;

        partName.RegisterValueChangedCallback(PartNameChanged);

        partType.RegisterValueChangedCallback(PartTypeChanged);

        JoystickButtonChanged(null);
        joystickButtonSelect.RegisterValueChangedCallback(JoystickButtonChanged);

        pressedActionsList.bindItem = (element, index) =>
        {
            if (pressedActionsList.itemsSource[index] == null)
            {
                pressedActionsList.itemsSource[index] = new RobotDesignerData.OperatorControlsDesign.MotorSetpoint();
            }
            element.dataSource = pressedActionsList.itemsSource[index];
            var targetNodeSelect = element.Q<DropdownField>("target-node");
            targetNodeSelect.choices = controllableNodesGuids;
            targetNodeSelect.formatSelectedValueCallback = FormatNodeGuidForDisplay;
            targetNodeSelect.formatListItemCallback = FormatNodeGuidForDisplay;
            targetNodeSelect.userData = pressedActionsList.itemsSource[index];
            targetNodeSelect.value = ((RobotDesignerData.OperatorControlsDesign.MotorSetpoint)targetNodeSelect.userData).jointNodeGuid;
            targetNodeSelect.RegisterValueChangedCallback(ButtonActionTargetChanged);
        };
        pressedActionsList.unbindItem = (element, index) =>
        {
            var targetNodeSelect = element.Q<DropdownField>("target-node");
            targetNodeSelect.UnregisterValueChangedCallback(ButtonActionTargetChanged);
            targetNodeSelect.value = null;
        };
        releasedActionsList.bindItem = (element, index) =>
        {
            if (releasedActionsList.itemsSource[index] == null)
            {
                releasedActionsList.itemsSource[index] = new RobotDesignerData.OperatorControlsDesign.MotorSetpoint();
            }
            element.dataSource = releasedActionsList.itemsSource[index];
            var targetNodeSelect = element.Q<DropdownField>("target-node");
            targetNodeSelect.choices = controllableNodesGuids;
            targetNodeSelect.formatSelectedValueCallback = FormatNodeGuidForDisplay;
            targetNodeSelect.formatListItemCallback = FormatNodeGuidForDisplay;
            targetNodeSelect.userData = releasedActionsList.itemsSource[index];
            targetNodeSelect.value = ((RobotDesignerData.OperatorControlsDesign.MotorSetpoint)targetNodeSelect.userData).jointNodeGuid;
            targetNodeSelect.RegisterValueChangedCallback(ButtonActionTargetChanged);
        };
        releasedActionsList.unbindItem = (element, index) =>
        {
            var targetNodeSelect = element.Q<DropdownField>("target-node");
            targetNodeSelect.UnregisterValueChangedCallback(ButtonActionTargetChanged);
            targetNodeSelect.value = null;
        };
    }

    void OnDisable()
    {
        partTree.makeItem = null;
        partTree.bindItem = null;
        partTree.unbindItem = null;

        partTree.selectedIndicesChanged -= PartSelected;

        addPartButton.clicked -= AddPartButtonClicked;

        jsonButton.clicked -= JsonButtonClicked;
        saveButton.clicked -= SaveButtonClicked;
        undoButton.clicked -= UndoButtonClicked;
        redoButton.clicked -= RedoButtonClicked;

        partName.UnregisterValueChangedCallback(PartNameChanged);

        partType.UnregisterValueChangedCallback(PartTypeChanged);

        joystickButtonSelect.UnregisterValueChangedCallback(JoystickButtonChanged);
    }

    private TreeViewItemData<RobotDesignerData.Node> TreeDataForNode(RobotDesignerData.Node node)
    {
        var treeId = nodeGuidToTreeId.ComputeIfAbsent(node.guid, _ => nodeGuidToTreeId.Count + FIRST_NODE_TREE_ID);
        return new(treeId, node, node.children.Select(TreeDataForNode).ToList());
    }

    private IEnumerator DelayedUpdatePartTree()
    {
        yield return null; // Wait until next frame

        UpdatePartTree();
    }

    private void UpdatePartTree()
    {
        partTree.SetRootItems(new List<TreeViewItemData<RobotDesignerData.Node>> {
            new TreeViewItemData<RobotDesignerData.Node>(ROBOT_TREE_ID, robotNode,
            robotDesign.children
                .Select(TreeDataForNode)
                .Prepend(new TreeViewItemData<RobotDesignerData.Node>(DRIVETRAIN_TREE_ID, drivetrainNode))
                .ToList())
        });
        partTree.RefreshItems();

        controllableNodesDict.Clear();
        controllableNodesGuids.Clear();
        nextDeviceId = NODE_DEVICE_ID_START - 1;
        UpdateControllableNodesList(robotDesign.children);
        ++nextDeviceId;

        pressedActionsList.RefreshItems();
        releasedActionsList.RefreshItems();
    }

    private void UpdateControllableNodesList(List<RobotDesignerData.Node> source)
    {
        foreach (var n in source)
        {
            if (IsNodeControllable(n))
            {
                controllableNodesDict.Add(n.guid, n.name);
                controllableNodesGuids.Add(n.guid);
            }
            nextDeviceId = Math.Max(nextDeviceId, n.deviceId);
            UpdateControllableNodesList(n.children);
        }
    }

    private bool IsNodeControllable(RobotDesignerData.Node n)
    {
        return n.type switch
        {
            RobotDesignerData.Node.Type.Shape => false,
            RobotDesignerData.Node.Type.Pivot => true,
            RobotDesignerData.Node.Type.Extension => true,
            RobotDesignerData.Node.Type.Collector => true,
            RobotDesignerData.Node.Type.Ejector => true,
            RobotDesignerData.Node.Type.Storage => false,
            RobotDesignerData.Node.Type.Grabber => true,
            _ => throw new ArgumentOutOfRangeException($"Unknown RobotDesignerData.Node.Type {n.type}"),
        };
    }

    private RobotDesignerData.Node getSelectedNode()
    {
        return partTree.GetSelectedItems<RobotDesignerData.Node>().Select(n => n.data).SingleOrDefault();
    }

    private void AddPartButtonClicked()
    {
        var selectedNode = getSelectedNode();
        if (selectedNode == null)
        {
            return;
        }
        var newNode = new RobotDesignerData.Node() { name = "New part", deviceId = nextDeviceId };
        if (selectedNode == robotNode)
        {
            robotDesign.children.Add(newNode);
        }
        else
        {
            selectedNode.children.Add(newNode);
        }
        Debug.Log("Add Part");
        UpdatePartTree();
        partTree.SetSelectionById(nodeGuidToTreeId[newNode.guid]);
    }

    private void PartSelected(IEnumerable<int> selectedIndices)
    {
        var selectedNode = getSelectedNode();

        addPartButton.SetEnabled(selectedNode != drivetrainNode && selectedNode != null);

        robotControls.style.display = selectedNode == robotNode ? DisplayStyle.Flex : DisplayStyle.None;

        drivetrainControls.style.display = selectedNode == drivetrainNode ? DisplayStyle.Flex : DisplayStyle.None;

        if (selectedNode == robotNode || selectedNode == drivetrainNode)
        {
            selectedNode = null;
        }

        if (selectedNode == null)
        {
            selectionBox.gameObject.SetActive(false);
            partEditor.style.display = DisplayStyle.None;
        }
        else
        {
            partEditor.style.display = DisplayStyle.Flex;
            partEditor.dataSource = selectedNode;

            PartTypeChanged(null);

            var xf = robotBuilder.GetNode(selectedNode.guid);
            selectionBox.gameObject.SetActive(true);
            selectionBox.transform.position = xf.position;
            selectionBox.transform.rotation = xf.rotation;
            selectionBox.size = xf.localScale;
        }
    }

    private void JoystickButtonChanged(ChangeEvent<string> evt)
    {
        if (joystickButtonSelect.index == 0)
        {
            releasedActionsList.itemsSource = robotDesign.operatorControls.startup.motors;
            buttonActionsContainer.style.display = DisplayStyle.None;
        }
        else
        {
            var buttonNum = joystickButtonSelect.index - 1;
            var button = robotDesign.operatorControls.buttons.FirstOrDefault(b => b.button == buttonNum);
            if (button == null)
            {
                button = new() { button = buttonNum };
                robotDesign.operatorControls.buttons.Add(button);
            }
            pressedActionsList.itemsSource = button.pressed.motors;
            releasedActionsList.itemsSource = button.released.motors;
            buttonActionsContainer.style.display = DisplayStyle.Flex;
        }
    }

    private void ButtonActionTargetChanged(ChangeEvent<string> evt)
    {
        var field = (DropdownField)evt.currentTarget;
        var data = (RobotDesignerData.OperatorControlsDesign.MotorSetpoint)field.userData;
        data.jointNodeGuid = evt.newValue;
    }

    private string FormatNodeGuidForDisplay(string nodeGuid)
    {
        if (nodeGuid == null) return null;
        return controllableNodesDict[nodeGuid];
    }

    private void PartNameChanged(ChangeEvent<string> evt)
    {
        StartCoroutine(DelayedUpdatePartTree());
    }

    private void PartTypeChanged(ChangeEvent<Enum> evt)
    {
        var selectedType = (RobotDesignerData.Node.Type)partType.value;
        switch (selectedType)
        {
            case RobotDesignerData.Node.Type.Shape:
                jointControls.style.display = DisplayStyle.None;
                collectorControls.style.display = DisplayStyle.None;
                ejectorControls.style.display = DisplayStyle.None;
                storageControls.style.display = DisplayStyle.None;
                compatibleGamePieceControls.style.display = DisplayStyle.None;
                break;
            case RobotDesignerData.Node.Type.Pivot:
                jointControls.dataSource = getSelectedNode().pivot;
                jointControls.style.display = DisplayStyle.Flex;
                collectorControls.style.display = DisplayStyle.None;
                ejectorControls.style.display = DisplayStyle.None;
                storageControls.style.display = DisplayStyle.None;
                compatibleGamePieceControls.style.display = DisplayStyle.None;
                break;
            case RobotDesignerData.Node.Type.Extension:
                jointControls.dataSource = getSelectedNode().extension;
                jointControls.style.display = DisplayStyle.Flex;
                collectorControls.style.display = DisplayStyle.None;
                ejectorControls.style.display = DisplayStyle.None;
                storageControls.style.display = DisplayStyle.None;
                compatibleGamePieceControls.style.display = DisplayStyle.None;
                break;
            case RobotDesignerData.Node.Type.Collector:
                jointControls.style.display = DisplayStyle.None;
                collectorControls.style.display = DisplayStyle.Flex;
                ejectorControls.style.display = DisplayStyle.None;
                storageControls.style.display = DisplayStyle.None;
                compatibleGamePieceControls.style.display = DisplayStyle.Flex;
                break;
            case RobotDesignerData.Node.Type.Ejector:
                jointControls.style.display = DisplayStyle.None;
                collectorControls.style.display = DisplayStyle.None;
                ejectorControls.style.display = DisplayStyle.Flex;
                storageControls.style.display = DisplayStyle.None;
                compatibleGamePieceControls.style.display = DisplayStyle.Flex;
                break;
            case RobotDesignerData.Node.Type.Storage:
                jointControls.style.display = DisplayStyle.None;
                collectorControls.style.display = DisplayStyle.None;
                ejectorControls.style.display = DisplayStyle.None;
                storageControls.style.display = DisplayStyle.Flex;
                compatibleGamePieceControls.style.display = DisplayStyle.Flex;
                break;
            case RobotDesignerData.Node.Type.Grabber:
                jointControls.style.display = DisplayStyle.None;
                collectorControls.style.display = DisplayStyle.Flex;
                ejectorControls.style.display = DisplayStyle.Flex;
                storageControls.style.display = DisplayStyle.None;
                compatibleGamePieceControls.style.display = DisplayStyle.Flex;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported node type {selectedType}");
        }
    }

    private void JsonButtonClicked()
    {
        StartCoroutine(ShowJsonEditor());
    }

    private IEnumerator ShowJsonEditor()
    {
        jsonEditor.gameObject.SetActive(true);
        yield return new WaitWhile(() => jsonEditor.gameObject.activeSelf);
        UpdatePartTree();
    }

    internal void SaveButtonClicked()
    {
        RobotDesignerData.SaveToPlayerPrefs(robotDesign);
    }

    private void UndoButtonClicked()
    {
        if (undoStackPosition <= 0)
        {
            return;
        }
        --undoStackPosition;
        robotDesign.LoadFrom(undoStack[undoStackPosition]);
    }

    private void RedoButtonClicked()
    {
        if (undoStackPosition >= undoStack.Count - 1)
        {
            return;
        }
        ++undoStackPosition;
        robotDesign.LoadFrom(undoStack[undoStackPosition]);
    }

    internal void ExitButtonClicked()
    {
        if (RobotDesignerData.HasUnsavedBackup())
        {
            unsavedChangesDialog.gameObject.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("Menu Screen");
        }
    }

    internal void LoadFromBackup()
    {
        RobotDesignerData.LoadBackupFromPlayerPrefs(robotDesign);
    }

    void Update()
    {
        robotBuilder.UpdateRobot();

        // Mirror.NetworkIdentity disables this object on while it's waiting for
        // a server connection, but we don't use networking in the
        // Robot Designer scene.
        robotBuilder.gameObject.SetActive(true);

        // It seems like the robot slides around sometimes when changing the position
        // of large parts of the robot. Lock it at the origin so that the camera
        // stays focused on the robot.
        robotBuilder.transform.position = Vector3.zero;

        var serializedState = robotDesign.Serialize();
        if (undoStack.Count == 0 || undoStack[undoStackPosition] != serializedState)
        {
            undoStack.RemoveRange(undoStackPosition + 1, undoStack.Count - (undoStackPosition + 1));
            undoStack.Add(serializedState);
            undoStackPosition = undoStack.Count - 1;
        }
        if (!loadBackupDialog.gameObject.activeSelf)
        {
            RobotDesignerData.BackupToPlayerPrefs(serializedState);
        }

        var selectedNode = getSelectedNode();
        if (selectedNode == null || selectedNode == robotNode || selectedNode == drivetrainNode)
        {
            selectionBox.gameObject.SetActive(false);
        }
        else
        {
            var xf = robotBuilder.GetNode(selectedNode.guid);
            var bounds = GameObjectUtils.CalculateOrientedRendererBoundsRecursive(xf.gameObject);
            selectionBox.gameObject.SetActive(true);
            selectionBox.transform.position = bounds.center;
            selectionBox.transform.rotation = bounds.rotation;
            selectionBox.size = bounds.size;
        }
    }
}
