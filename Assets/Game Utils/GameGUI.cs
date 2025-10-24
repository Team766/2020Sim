using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Coherence;
using Coherence.Toolkit;

public enum RobotMode : int {
    Disabled = 0,
    Auton = 1,
    Teleop = 2,
}

[RequireComponent(typeof(AudioSource))]
public class GameGUI : MonoBehaviour {
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
    [Sync]
    public int redScore;
    [Sync]
    public int blueScore;
    [Sync]
    public RobotMode robotMode = RobotMode.Disabled;
    private double robotModeStartTime = 0.0;
    [Sync]
    public float timeRemaining = 0.0f;

    void Start() {
        int cameraIndex = PlayerPrefs.GetInt(SELECTED_CAMERA_PREF_KEY, initialCamera);
        if (cameraIndex >= cameras.Length || !cameras[cameraIndex]) {
            cameraIndex = initialCamera;
        }
        SelectCamera(cameraIndex);
    }

    RobotController findOurRobot() {
        foreach (var robot in FindObjectsByType<RobotController>(FindObjectsSortMode.None)) {
            if (robot.GetComponent<CoherenceSync>().HasInputAuthority) {
                return robot;
            }
        }
        return null;
    }

    void Update() {
        // NOTE(ryan.cahoon, 2020-09-23): This seems to put the GUI in the right
        // place on WebGL. This shouldn't be necessary (and it works fine
        // without this on desktop), but it's cheap to do and there are more
        // important things to work on.
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        scoreText.text = "Red score: " + redScore + "  Blue score: " + blueScore;

        var robot = findOurRobot();
        if (robot) {
            codeStateText.enabled = true;
            codeStateText.text = robot.GetComponent<CodeConnector>().hasRobotCode ? "Code running" : "No robot code";
        } else {
            codeStateText.enabled = false;
        }

        robotModeDropdown.value = (int)robotMode;

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
        if (GetComponent<CoherenceSync>().HasStateAuthority) {
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
        if (GetComponent<CoherenceSync>().HasStateAuthority) {
            redScore += delta;
        }
    }

    public void addBlueScore(int delta) {
        if (GetComponent<CoherenceSync>().HasStateAuthority) {
            blueScore += delta;
        }
    }

    public void LoadScene(int dropdownIndex) {
        if (dropdownIndex == 0)
        {
            return;
        }
        string sceneName = sceneNames[dropdownIndex - 1];
        IEnumerator DoLoadScene() {
            Debug.Log("Loading scene " + sceneName);

            var authoritativeObjects = new List<CoherenceSync>();
            foreach (var sync in FindObjectsByType<CoherenceSync>(FindObjectsSortMode.None))
            {
                // Check if the current client has state authority over this entity
                if (sync.HasStateAuthority)
                {
                    sync.AbandonAuthority();
                    authoritativeObjects.Add(sync);
                }
            }

            yield return new WaitUntil(
                () => authoritativeObjects.TrueForAll(sync => !sync.HasStateAuthority));

            SceneManager.LoadScene(sceneName);
        }
        StartCoroutine(DoLoadScene());
    }


    public void RequestRobotMode(int mode) {
        if (!Enum.IsDefined(typeof(RobotMode), mode)) {
            throw new ArgumentOutOfRangeException();
        }
        if (robotMode != (RobotMode)mode) {
            GetComponent<CoherenceSync>().SendCommand<GameGUI>(
                nameof(CmdSetRobotMode),
                MessageTarget.StateAuthorityOnly,
                (RobotMode)mode);
        }
    }

    [Command]
    public void CmdSetRobotMode(RobotMode mode)
    {
        robotMode = mode;
        robotModeStartTime = Time.timeAsDouble;
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
}
