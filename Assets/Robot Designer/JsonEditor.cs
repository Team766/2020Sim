using UnityEngine;
using UnityEngine.UIElements;

public class JsonEditor : MonoBehaviour
{
    public UIDocument document;
    public RobotDesignerData robotDesign;

    private VisualElement root;

    void OnEnable()
    {
        root = document.rootVisualElement;
        root.Q<TextField>("json-text").value = JsonUtility.ToJson(robotDesign, true);
        root.Q<Button>("button-apply").clicked += ApplyClicked;
        root.Q<Button>("button-cancel").clicked += CancelClicked;
    }

    void OnDisable()
    {
        root.Q<Button>("button-apply").clicked -= ApplyClicked;
        root.Q<Button>("button-cancel").clicked -= CancelClicked;
    }

    private void ApplyClicked()
    {
        JsonUtility.FromJsonOverwrite(root.Q<TextField>("json-text").value, robotDesign);
        gameObject.SetActive(false);
    }

    private void CancelClicked()
    {
        gameObject.SetActive(false);
    }
}
