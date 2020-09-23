using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class GameGUI : NetworkBehaviour {
    public Camera[] cameras;
    public Text messageText;
    public Text scoreText;
    [SyncVar]
    public int redScore;
    [SyncVar]
    public int blueScore;
    
    protected SyncListString messages = new SyncListString();
    
    void Start() {
        SelectCamera(5);
        // NOTE(ryan.cahoon, 2020-09-23): This seems to put the GUI in the right
        // place on WebGL. This shouldn't be necessary (and it works fine
        // without this on desktop), but it's cheap to do and there are more
        // important things to work on.
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
    
    void Update() {
        messageText.text = string.Join("\n", messages);
        scoreText.text = "Red score: " + redScore + "  Blue score: " + blueScore;
    }

    [Command(ignoreAuthority = true)]
    public void RestartScene() {
        NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
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
