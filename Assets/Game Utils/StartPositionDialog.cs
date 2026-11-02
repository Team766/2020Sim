using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public struct Pose2
{
    public Vector2 position;
    public float rotation;

    public Pose2(Vector2 position, float rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public Pose2(Pose pose)
    {
        this.position = new Vector2(pose.position.x, pose.position.z);
        this.rotation = pose.rotation.eulerAngles.y;
    }
}

public class StartPositionDialog : MonoBehaviour
{
    const string START_POSITION_X_PREF_KEY_PREFIX = "robotStartPositionX:";
    const string START_POSITION_Y_PREF_KEY_PREFIX = "robotStartPositionY:";
    const string START_POSITION_HEADING_PREF_KEY_PREFIX = "robotStartPositionHeading:";
    const string START_POSITION_COORDINATE_TEAM_PREF_KEY = "robotStartPositionCoordinateTeam";

    public static void SetStartPosition(Pose2 pose) {
        var sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetFloat(START_POSITION_X_PREF_KEY_PREFIX + sceneName, pose.position.x);
        PlayerPrefs.SetFloat(START_POSITION_Y_PREF_KEY_PREFIX + sceneName, pose.position.y);
        PlayerPrefs.SetFloat(START_POSITION_HEADING_PREF_KEY_PREFIX + sceneName, pose.rotation);
    }

    public static Pose2? GetStartPosition() {
        var sceneName = SceneManager.GetActiveScene().name;
        float x = PlayerPrefs.GetFloat(START_POSITION_X_PREF_KEY_PREFIX + sceneName, float.NaN);
        float y = PlayerPrefs.GetFloat(START_POSITION_Y_PREF_KEY_PREFIX + sceneName, float.NaN);
        float heading = PlayerPrefs.GetFloat(START_POSITION_HEADING_PREF_KEY_PREFIX + sceneName, float.NaN);
        if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(heading)) {
            return null;
        }
        return new Pose2(new Vector2(x, y), heading);
    }

    public UIDocument document;

    private VisualElement root;

    public Transform blue1;
    public Transform blue2;
    public Transform blue3;

    public float fieldLength;

    private int coordinateTeam = 0; // 0 = blue, 1 = red (mirrored)

    void OnEnable()
    {
        coordinateTeam = PlayerPrefs.GetInt(START_POSITION_COORDINATE_TEAM_PREF_KEY, 0);

        root = document.rootVisualElement;

        root.Q<Button>("button-save").clicked += SaveClicked;
        root.Q<Button>("button-cancel").clicked += CancelClicked;
        root.Q<Button>("button-team1").SetEnabled(blue1);
        root.Q<Button>("button-team1").clicked += Team1Clicked;
        root.Q<Button>("button-team2").SetEnabled(blue2);
        root.Q<Button>("button-team2").clicked += Team2Clicked;
        root.Q<Button>("button-team3").SetEnabled(blue3);
        root.Q<Button>("button-team3").clicked += Team3Clicked;

        root.Q<RadioButtonGroup>("coordinate-team").value = coordinateTeam;
        root.Q<RadioButtonGroup>("coordinate-team").RegisterValueChangedCallback(CoordinateTeamChanged);

        DisplayPose(GetStartPosition().MapValue(EncodeCoordinates) ?? new Pose2(blue1.GetPose()));
    }

    void OnDisable()
    {
        root.Q<Button>("button-save").clicked -= SaveClicked;
        root.Q<Button>("button-cancel").clicked -= CancelClicked;
        root.Q<Button>("button-team1").clicked -= Team1Clicked;
        root.Q<Button>("button-team2").clicked -= Team2Clicked;
        root.Q<Button>("button-team3").clicked -= Team3Clicked;
    }

    private Pose2 EncodeCoordinates(Pose2 pose) {
        if (coordinateTeam == 1) {
            // Red coordinates - mirrored
            return new Pose2(new Vector2(fieldLength - pose.position.x, pose.position.y), Mathf.Repeat(pose.rotation + 180, 360f));
        } else {
            // Blue coordinates - normal
            return new Pose2(new Vector2(pose.position.x, pose.position.y), Mathf.Repeat(pose.rotation, 360f));
        }
    }

    private Pose2 DecodeCoordinates(Pose2 pose) {
        if (coordinateTeam == 1) {
            // Red coordinates - mirrored
            return new Pose2(new Vector2(fieldLength - pose.position.x, pose.position.y), Mathf.Repeat(pose.rotation - 180, 360f));
        } else {
            // Blue coordinates - normal
            return new Pose2(new Vector2(pose.position.x, pose.position.y), Mathf.Repeat(pose.rotation, 360f));
        }
    }

    private void DisplayPose(Pose2 pose)
    {
        root.Q<Vector2Field>("position").value = pose.position;
        root.Q<FloatField>("heading").value = pose.rotation;
    }

    private Pose2 CapturePose()
    {
        return new Pose2(root.Q<Vector2Field>("position").value, root.Q<FloatField>("heading").value);
    }

    private void SaveClicked()
    {
        SetStartPosition(DecodeCoordinates(CapturePose()));
        PlayerPrefs.SetInt(START_POSITION_COORDINATE_TEAM_PREF_KEY, coordinateTeam);
        gameObject.SetActive(false);
    }

    private void CancelClicked()
    {
        gameObject.SetActive(false);
    }

    private void CoordinateTeamChanged(ChangeEvent<int> evt)
    {
        coordinateTeam = evt.newValue;
    }

    private void Team1Clicked()
    {
        DisplayPose(new Pose2(blue1.GetPose()));
    }

    private void Team2Clicked()
    {
        DisplayPose(new Pose2(blue2.GetPose()));
    }

    private void Team3Clicked()
    {
        DisplayPose(new Pose2(blue3.GetPose()));
    }
}
