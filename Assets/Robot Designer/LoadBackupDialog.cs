using UnityEngine;
using UnityEngine.UIElements;

public class LoadBackupDialog : MonoBehaviour
{
    public UIDocument document;
    public RobotEditor robotEditor;

    private VisualElement root;

    void OnEnable()
    {
        root = document.rootVisualElement;
        root.Q<Button>("button-restore").clicked += RestoreClicked;
        root.Q<Button>("button-discard").clicked += DiscardClicked;
    }

    void OnDisable()
    {
        root.Q<Button>("button-restore").clicked -= RestoreClicked;
        root.Q<Button>("button-discard").clicked -= DiscardClicked;
    }

    private void RestoreClicked()
    {
        robotEditor.LoadFromBackup();
        gameObject.SetActive(false);
    }

    private void DiscardClicked()
    {
        RobotDesignerData.ClearBackup();
        gameObject.SetActive(false);
    }
}
