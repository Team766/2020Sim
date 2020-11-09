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

    public string[] sceneNames;
    public Camera[] cameras;
    public int initialCamera;
    public Text messageText;
    public Text scoreText;
    public Text timeText;
    public Text codeStateText;
    public Dropdown robotModeDropdown;
    [SyncVar]
    public int redScore;
    [SyncVar]
    public int blueScore;
    [SyncVar]
    private RobotMode robotMode = RobotMode.Disabled;
    private float robotModeStartTime = 0.0f;
    [SyncVar]
    private float timeRemaining = 0.0f;
    [SyncVar]
    public bool haveRobotCode = false;
    private System.Guid thisId = System.Guid.NewGuid();
    [SyncVar]
    private System.Guid ownerId = System.Guid.Empty;
    
    protected SyncListString messages = new SyncListString();
    
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
        SelectCamera(initialCamera);
        // NOTE(ryan.cahoon, 2020-09-23): This seems to put the GUI in the right
        // place on WebGL. This shouldn't be necessary (and it works fine
        // without this on desktop), but it's cheap to do and there are more
        // important things to work on.
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
    
    void Update() {
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
                Mathf.Max(0.0f, robotModeStartTime + stateDuration - Time.time));
            /*if (timeRemaining <= 0.0) {
                SetRobotMode(System.Guid.Empty, RobotMode.Disabled);
            }*/
        }
        timeText.text = String.Format(
            "Time left: {0:D}:{1:D2}",
            (int)(timeRemaining / 60), (int)(timeRemaining % 60));
    }

    [ClientCallback]
    public void LoadScene(int dropdownIndex) {
        if (dropdownIndex == 0) {
            return;
        }
        CmdLoadScene(sceneNames[dropdownIndex - 1]);
    }

    [Command(ignoreAuthority = true)]
    private void CmdLoadScene(string sceneName) {
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

    [Command(ignoreAuthority = true)]
    private void CmdSetRobotMode(System.Guid newOwnerId, string mode) {
        SetRobotMode(newOwnerId, (RobotMode)Enum.Parse(typeof(RobotMode), mode, true));
    }

    [Server]
    public void SetRobotMode(System.Guid newOwnerId, RobotMode mode) {
        robotMode = mode;
        robotModeStartTime = Time.time;
        ownerId = newOwnerId;
    }

    public IEnumerator ShowMessage(string message) {
        messages.Insert(0, message);
        yield return new WaitForSeconds(2.0f);
        messages.RemoveAt(messages.Count - 1);
    }
    
    public void SelectCamera(int cameraIndex) {
        foreach (var c in cameras) {
            c.enabled = false;
        }
        cameras[cameraIndex].enabled = true;
    }
}
