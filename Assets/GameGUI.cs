using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameGUI : MonoBehaviour {
    public Camera[] cameras;
    public Text messageText;
    public Text scoreText;
    public int redScore;
    public int blueScore;
    
    LinkedList<string> messages = new LinkedList<string>();
    
    void Start() {
        SelectCamera(5);
    }
    
    void Update() {
        messageText.text = string.Join("\n", messages);
        scoreText.text = "Red score: " + redScore + "  Blue score: " + blueScore;
    }

    public void RestartScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public IEnumerator ShowMessage(string message) {
        messages.AddFirst(message);
        yield return new WaitForSeconds(2.0f);
        messages.RemoveLast();
    }
    
    public void SelectCamera(int cameraIndex) {
        foreach (var c in cameras) {
            c.enabled = false;
        }
        cameras[cameraIndex].enabled = true;
    }
}
