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
}

public class StartPositionDialog : MonoBehaviour
{
    const string START_POSITION_X_PREF_KEY_PREFIX = "robotStartPositionX:";
    const string START_POSITION_Y_PREF_KEY_PREFIX = "robotStartPositionY:";
    const string START_POSITION_HEADING_PREF_KEY_PREFIX = "robotStartPositionHeading:";

    public static void SetStartPosition(Vector2 position, float heading) {
        var sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetFloat(START_POSITION_X_PREF_KEY_PREFIX + sceneName, position.x);
        PlayerPrefs.SetFloat(START_POSITION_Y_PREF_KEY_PREFIX + sceneName, position.y);
        PlayerPrefs.SetFloat(START_POSITION_HEADING_PREF_KEY_PREFIX + sceneName, heading);
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

    public Transform red1;
    public Transform red2;
    public Transform red3;
    public Transform blue1;
    public Transform blue2;
    public Transform blue3;

    void OnEnable()
    {
        root = document.rootVisualElement;
        root.Q<Button>("button-save").clicked += SaveClicked;
        root.Q<Button>("button-cancel").clicked += CancelClicked;
        root.Q<Button>("button-red1").SetEnabled(red1);
        root.Q<Button>("button-red1").clicked += Red1Clicked;
        root.Q<Button>("button-red2").SetEnabled(red2);
        root.Q<Button>("button-red2").clicked += Red2Clicked;
        root.Q<Button>("button-red3").SetEnabled(red3);
        root.Q<Button>("button-red3").clicked += Red3Clicked;
        root.Q<Button>("button-blue1").SetEnabled(blue1);
        root.Q<Button>("button-blue1").clicked += Blue1Clicked;
        root.Q<Button>("button-blue2").SetEnabled(blue2);
        root.Q<Button>("button-blue2").clicked += Blue2Clicked;
        root.Q<Button>("button-blue3").SetEnabled(blue3);
        root.Q<Button>("button-blue3").clicked += Blue3Clicked;

        root.Q<Vector2Field>("position").value = GetStartPosition()?.position ?? new Vector2(red1.position.x, red1.position.z);
        root.Q<FloatField>("heading").value = GetStartPosition()?.rotation ?? red1.eulerAngles.y;
    }

    void OnDisable()
    {
        root.Q<Button>("button-save").clicked -= SaveClicked;
        root.Q<Button>("button-cancel").clicked -= CancelClicked;
        root.Q<Button>("button-red1").clicked -= Red1Clicked;
        root.Q<Button>("button-red2").clicked -= Red2Clicked;
        root.Q<Button>("button-red3").clicked -= Red3Clicked;
        root.Q<Button>("button-blue1").clicked -= Blue1Clicked;
        root.Q<Button>("button-blue2").clicked -= Blue2Clicked;
        root.Q<Button>("button-blue3").clicked -= Blue3Clicked;
    }

    private void SaveClicked()
    {
        SetStartPosition(root.Q<Vector2Field>("position").value, root.Q<FloatField>("heading").value);
        gameObject.SetActive(false);
    }

    private void CancelClicked()
    {
        gameObject.SetActive(false);
    }

    private void Red1Clicked()
    {
        root.Q<Vector2Field>("position").value = new Vector2(red1.position.x, red1.position.z);
        root.Q<FloatField>("heading").value = red1.eulerAngles.y;
    }

    private void Red2Clicked()
    {
        root.Q<Vector2Field>("position").value = new Vector2(red2.position.x, red2.position.z);
        root.Q<FloatField>("heading").value = red2.eulerAngles.y;
    }

    private void Red3Clicked()
    {
        root.Q<Vector2Field>("position").value = new Vector2(red3.position.x, red3.position.z);
        root.Q<FloatField>("heading").value = red3.eulerAngles.y;
    }

    private void Blue1Clicked()
    {
        root.Q<Vector2Field>("position").value = new Vector2(blue1.position.x, blue1.position.z);
        root.Q<FloatField>("heading").value = blue1.eulerAngles.y;
    }

    private void Blue2Clicked()
    {
        root.Q<Vector2Field>("position").value = new Vector2(blue2.position.x, blue2.position.z);
        root.Q<FloatField>("heading").value = blue2.eulerAngles.y;
    }

    private void Blue3Clicked()
    {
        root.Q<Vector2Field>("position").value = new Vector2(blue3.position.x, blue3.position.z);
        root.Q<FloatField>("heading").value = blue3.eulerAngles.y;
    }
}
