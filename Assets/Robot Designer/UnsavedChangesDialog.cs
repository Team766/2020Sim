using UnityEngine;
using UnityEngine.UIElements;

public class UnsavedChangesDialog : MonoBehaviour
{
    public UIDocument document;
    public RobotEditor robotEditor;

    private VisualElement root;

    void OnEnable()
    {
        root = document.rootVisualElement;
        root.Q<Button>("button-save").clicked += SaveClicked;
        root.Q<Button>("button-discard").clicked += DiscardClicked;
        root.Q<Button>("button-cancel").clicked += CancelClicked;
    }

    void OnDisable()
    {
        root.Q<Button>("button-save").clicked -= SaveClicked;
        root.Q<Button>("button-discard").clicked -= DiscardClicked;
        root.Q<Button>("button-cancel").clicked -= CancelClicked;
    }

    private void SaveClicked()
    {
        robotEditor.SaveButtonClicked();
        robotEditor.ExitButtonClicked();
    }

    private void DiscardClicked()
    {
        RobotDesignerData.ClearBackup();
        robotEditor.ExitButtonClicked();
    }

    private void CancelClicked()
    {
        gameObject.SetActive(false);
    }
}
