using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public enum RobotMode : int {
    Disabled = 0,
    Auton = 1,
    Teleop = 2,
}

public class GameGUI : NetworkBehaviour {
    const float AUTON_DURATION = 30.0f;
    const float TELEOP_DURATION = 135.0f;

    const string SELECTED_CAMERA_PREF_KEY = "selectedCamera";

    public string[] sceneNames;
    public string[] robotVariantNames;
    public Camera[] cameras;
    public int initialCamera;
    public Text messageText;
    public Text scoreText;
    public Text timeText;
    public Text codeStateText;
    public Dropdown robotModeDropdown;
    public Dropdown cameraDropdown;
    [SerializeField]
    [SyncVar]
    private int redScore;
    [SerializeField]
    [SyncVar]
    private int blueScore;
    [SyncVar]
    private RobotMode robotMode = RobotMode.Disabled;
    private double robotModeStartTime = 0.0;
    [SyncVar]
    private float timeRemaining = 0.0f;
    [SyncVar]
    public bool haveRobotCode = false;
    private System.Guid thisId = System.Guid.NewGuid();
    [SyncVar]
    private System.Guid ownerId = System.Guid.Empty;
    
    protected readonly SyncList<string> messages = new SyncList<string>();
    
    public RobotMode RobotMode {
        get {
            return robotMode;
        }
    }

    public bool IsOwner {
        get {
            // TODO: Use Mirror's authority mechanism instead
            return ownerId == thisId;
        }
    }

    void Start() {
        int cameraIndex = PlayerPrefs.GetInt(SELECTED_CAMERA_PREF_KEY, initialCamera);
        if (cameraIndex >= cameras.Length || !cameras[cameraIndex]) {
            cameraIndex = initialCamera;
        }
        SelectCamera(cameraIndex);
    }

    void Update() {
        // NOTE(ryan.cahoon, 2020-09-23): This seems to put the GUI in the right
        // place on WebGL. This shouldn't be necessary (and it works fine
        // without this on desktop), but it's cheap to do and there are more
        // important things to work on.
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    
        messageText.text = string.Join("\n", messages);
        scoreText.text = "Red score: " + redScore + "  Blue score: " + blueScore;

        robotModeDropdown.value = (int)robotMode;

        codeStateText.text = haveRobotCode ? "Code running" : "No robot code";

        float stateDuration = 0.0f;
        switch (robotMode) {
            case RobotMode.Disabled:
                stateDuration = 0.0f;
                break;
            case RobotMode.Auton:
                stateDuration = AUTON_DURATION;
                break;
            case RobotMode.Teleop:
                stateDuration = TELEOP_DURATION;
                break;
        }
        if (isServer) {
            timeRemaining = Mathf.Ceil(
                (float)Math.Max(0.0, robotModeStartTime + stateDuration - Time.timeAsDouble));
            /*if (timeRemaining <= 0.0) {
                SetRobotMode(System.Guid.Empty, RobotMode.Disabled);
            }*/
        }
        timeText.text = String.Format(
            "Time left: {0:D}:{1:D2}",
            (int)(timeRemaining / 60), (int)(timeRemaining % 60));
    }

    public void addRedScore(int delta) {
        if (isServer) {
            redScore += delta;
        }
    }

    public void addBlueScore(int delta) {
        if (isServer) {
            blueScore += delta;
        }
    }

    [ClientCallback]
    public void LoadScene(int dropdownIndex) {
        if (dropdownIndex == 0) {
            return;
        }
        CmdLoadScene(sceneNames[dropdownIndex - 1],
                     robotVariantNames[dropdownIndex - 1]);
    }

    [Command(requiresAuthority = false)]
    private void CmdLoadScene(string sceneName, string robotVariantName) {
        Debug.Log("Loading scene " + sceneName + " " + robotVariantName);
        RobotController.robotVariant = robotVariantName;
        NetworkManager.singleton.ServerChangeScene(sceneName);
    }
    
    [ClientCallback]
    public void RequestRobotMode(int mode) {
        if (!Enum.IsDefined(typeof(RobotMode), mode)) {
            throw new ArgumentOutOfRangeException();
        }
        if (robotMode != (RobotMode)mode) {
            CmdSetRobotMode(thisId, ((RobotMode)mode).ToString());
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSetRobotMode(System.Guid newOwnerId, string mode) {
        SetRobotMode(newOwnerId, (RobotMode)Enum.Parse(typeof(RobotMode), mode, true));
    }

    [Server]
    public void SetRobotMode(System.Guid newOwnerId, RobotMode mode) {
        robotMode = mode;
        robotModeStartTime = Time.timeAsDouble;
        ownerId = newOwnerId;
    }

    public IEnumerator ShowMessage(string message) {
        messages.Insert(0, message);
        yield return new WaitForSeconds(2.0f);
        messages.RemoveAt(messages.Count - 1);
    }
    
    public void SelectCamera(int cameraIndex) {
        foreach (var c in cameras) {
            if (c) {
                c.enabled = false;
            }
        }
        if (cameras[cameraIndex]) {
            cameras[cameraIndex].enabled = true;
        }
        cameraDropdown.value = cameraIndex;
        PlayerPrefs.SetInt(SELECTED_CAMERA_PREF_KEY, cameraIndex);
    }
}
