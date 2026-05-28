using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    const int MOUSE_BUTTON = 0;

    public GameObject target;

    public float xSpeed = 2.0f;
    public float ySpeed = 2.0f;
    public float zoomSpeed = 2.0f;

    public float yMinLimit = -90;
    public float yMaxLimit = 90;

    public float distanceMinLimit = 1;
    public float distanceMaxLimit = 10;

    float x = 0.0f;
    float y = 0.0f;
    float distance = 10.0f;
    bool isDragging = false;

    public float autoOrbitSpeed = 0.0f;

    void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        distance = (target.transform.position - transform.position).magnitude;
    }

    void LateUpdate()
    {
        bool mouseOver = GetComponent<Camera>().pixelRect.Contains(Input.mousePosition);

        if (mouseOver)
        {
            distance -= Input.mouseScrollDelta.y * 2;
            distance = Mathf.Clamp(distance, distanceMinLimit, distanceMaxLimit);
        }

        if (!Input.GetMouseButton(MOUSE_BUTTON))
        {
            isDragging = false;
        }
        else if (Input.GetMouseButtonDown(MOUSE_BUTTON) && mouseOver)
        {
            isDragging = true;
        }

        if (isDragging)
        {
            var mousePositionDelta = Input.mousePositionDelta;
            x += mousePositionDelta.x * xSpeed;
            y -= mousePositionDelta.y * ySpeed;

            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }
        else
        {
            x += Time.deltaTime * autoOrbitSpeed;
        }

        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.transform.position;
        transform.rotation = rotation;
        transform.position = position;
    }
}