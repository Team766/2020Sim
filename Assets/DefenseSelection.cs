using UnityEngine;
using System.Linq;

public class DefenseSelection : MonoBehaviour
{
    public GameObject[] prefabs;
    public GUIContent[] prefabNames;
    public GUIStyle labelStyle;
    public int[] selections;
    public GUIStyle selectionStyle;
    public Transform[] locations;
    public string[] locationNames;
    public GameObject[] defenses;

    Vector2 scrollPosition;

    void Start()
    {
        for (int i = 0; i < selections.Length; ++i)
        {
            selections[i] = PlayerPrefs.GetInt("Defense " + i, i);
        }
        Instantiate();
    }

    public void Instantiate()
    {
        foreach (var go in defenses)
        {
            Destroy(go);
        }
        defenses = new GameObject[selections.Length];
        for (int i = 0; i < selections.Length; ++i)
        {
            defenses[i] = Instantiate(prefabs[selections[i]], locations[i].position, locations[i].rotation) as GameObject;
            PlayerPrefs.SetInt("Defense " + i, selections[i]);
        }
    }

    public bool DrawGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < locations.Length; ++i)
        {
            GUILayout.BeginVertical(locationNames[i], labelStyle);
            selections[i] = GUILayout.SelectionGrid(selections[i], prefabNames, 1, selectionStyle);
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        if (GUILayout.Button("OK"))
        {
            Instantiate();
            return true;
        }
        else
        {
            return false;
        }
    }
}
