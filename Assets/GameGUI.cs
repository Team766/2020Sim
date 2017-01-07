using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameGUI : MonoBehaviour {

    public Rect resetRect;
    public Rect resetRect2;
    public Rect resetRect3;
    public Rect resetRect4;
    public DefenseSelection defenseSelection;
    public Rect defenseButtonRect;
    public Rect defenseWindowRect;
    public bool showDefenseSelection;
    public MonoBehaviour[] guis;

    void OnGUI()
    {
        if (!showDefenseSelection)
        {
            if (GUI.Button(resetRect2, "Restart"))
            {
                Application.LoadLevel(Application.loadedLevel);
            }

            if (GUI.Button(resetRect, "Load racetrack"))
            {
                Application.LoadLevel("Racetrack");
            }

            if (GUI.Button(resetRect3, "Load stronghold"))
            {
                Application.LoadLevel("Original Robotics Simulator");
            }

            if (GUI.Button(resetRect4, "Load Skatepark"))
            {
                Application.LoadLevel("Skatepark");
            }

            if (GUI.Button(defenseButtonRect, "Defenses"))
            {
                showDefenseSelection = true;
            }
            foreach (var g in guis)
            {
                g.enabled = true;
            }
        }
        else
        {
            GUILayout.Window(1000, defenseWindowRect, id => {
                if (defenseSelection.DrawGUI())
                {
                    showDefenseSelection = false;
                }
            }, "Defenses");
            foreach (var g in guis)
            {
                g.enabled = false;
            }
        }
    }
}
