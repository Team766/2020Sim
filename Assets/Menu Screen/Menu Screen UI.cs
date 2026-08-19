using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuScreenUI : MonoBehaviour
{
    public UIDocument document;
    public RobotBuilder robotBuilder;

    private VisualElement root;
    private Button level2020;
    private Button level2024;
    private Button level2025mayhem;
    private Button level2026mayhem;
    private Button levelXrp;
    private Button levelRobotEditor;

    void OnEnable()
    {
        root = document.rootVisualElement;

        level2020 = root.Q<Button>("level-2020");
        level2024 = root.Q<Button>("level-2024");
        level2025mayhem = root.Q<Button>("level-mayhem2025");
        level2026mayhem = root.Q<Button>("level-mayhem2026");
        levelXrp = root.Q<Button>("level-xrp");
        levelRobotEditor = root.Q<Button>("level-robot-editor");

        level2020.clicked += Level2020ButtonClicked;
        level2024.clicked += Level2024ButtonClicked;
        level2025mayhem.clicked += Level2025MayhemButtonClicked;
        level2026mayhem.clicked += Level2026MayhemButtonClicked;
        levelXrp.clicked += LevelXrpButtonClicked;
        levelRobotEditor.clicked += LevelRobotEditorButtonClicked;
    }

    void OnDisable()
    {
        level2020.clicked -= Level2020ButtonClicked;
        level2024.clicked -= Level2024ButtonClicked;
        level2025mayhem.clicked -= Level2025MayhemButtonClicked;
        level2026mayhem.clicked -= Level2026MayhemButtonClicked;
        levelXrp.clicked -= LevelXrpButtonClicked;
        levelRobotEditor.clicked -= LevelRobotEditorButtonClicked;
    }

    private void Level2020ButtonClicked()
    {
        SceneManager.LoadScene("2020Field");
    }

    private void Level2024ButtonClicked()
    {
        SceneManager.LoadScene("2024Field");
    }

    private void Level2025MayhemButtonClicked()
    {
        SceneManager.LoadScene("2025 M-Ayhem Field");
    }

    private void Level2026MayhemButtonClicked()
    {
        SceneManager.LoadScene("2026 M-Ayhem Field");
    }

    private void LevelXrpButtonClicked()
    {
        SceneManager.LoadScene("XRP Field");
    }

    private void LevelRobotEditorButtonClicked()
    {
        SceneManager.LoadScene("Robot Designer");
    }

    void Update()
    {
        robotBuilder.UpdateRobot();

        // Mirror.NetworkIdentity disables this object on while it's waiting for
        // a server connection, but we don't use networking in the
        // Main Menu scene.
        robotBuilder.gameObject.SetActive(true);
    }
}
