using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public enum RobotMode {
    Disabled = 0,
    Auton = 1,
    Teleop = 2,
}

[RequireComponent(typeof(AudioSource))]
public class GameGUI : NetworkBehaviour {
    const float AUTON_DURATION = 30.0f;
    const float TELEOP_DURATION = 135.0f;

    const string SELECTED_CAMERA_PREF_KEY = "selectedCamera";

    public string[] sceneNames;
    public Camera[] cameras;
    public int initialCamera;
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
    private bool robotModeIsCodeControlled = false;
    [SyncVar]
    private float timeRemaining = 0.0f;

    public RobotMode RobotMode => robotMode;
    public bool RobotModeIsCodeControlled => robotModeIsCodeControlled;

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

        scoreText.text = "Red score: " + redScore + "  Blue score: " + blueScore;

        robotModeDropdown.value = (int)robotMode;
        robotModeDropdown.interactable = !robotModeIsCodeControlled;

        var identity = NetworkClient.connection?.identity;
        if (identity) {
            var robot = identity.GetComponent<CodeConnector>();
            if (robot) {
                codeStateText.enabled = true;
                codeStateText.text = robot.hasRobotCode ? "Code running" : "No robot code";
            } else {
                codeStateText.enabled = false;
            }
        }

        float stateDuration = robotMode switch {
            RobotMode.Disabled => 0.0f,
            RobotMode.Auton => AUTON_DURATION,
            RobotMode.Teleop => TELEOP_DURATION,
            _ => 0.0f,
        };
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
        CmdLoadScene(sceneNames[dropdownIndex - 1]);
    }

    [Command(requiresAuthority = false)]
    private void CmdLoadScene(string sceneName) {
        Debug.Log("Loading scene " + sceneName);
        NetworkManager.singleton.ServerChangeScene(sceneName);
    }
    
    [ClientCallback]
    public void RequestRobotMode(int mode) {
        if (!Enum.IsDefined(typeof(RobotMode), mode)) {
            throw new ArgumentOutOfRangeException();
        }
        if (robotMode != (RobotMode)mode) {
            CmdSetRobotMode(((RobotMode)mode).ToString());
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetRobotMode(string mode) {
        SetRobotMode((RobotMode)Enum.Parse(typeof(RobotMode), mode, true));
    }

    [Server]
    public void SetRobotMode(RobotMode mode) {
        Debug.Log("SetRobotMode " + mode);
        robotMode = mode;
        robotModeStartTime = Time.timeAsDouble;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetRobotModeIsCodeControlled(bool value) {
        robotModeIsCodeControlled = value;
    }

    public void PlaySound(AudioClip audioClip) {
        GetComponent<AudioSource>().PlayOneShot(audioClip);
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

    public void RestartButtonClicked()
    {
        CmdLoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitButtonClicked()
    {
        SceneManager.LoadScene("Menu Screen");
    }
}
