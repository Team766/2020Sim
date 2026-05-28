using UnityEngine;
using System.Linq;

[ExecuteAlways]
public class WireframeCube : MonoBehaviour
{
    public Vector3 size = Vector3.one;
    public float lineWidth = 0.01f;

    private Transform[] legs;

    void OnEnable()
    {
        legs = transform.Cast<Transform>().ToArray();
    }

    void LateUpdate()
    {
        Vector3 adjPos = size / 2f - Vector3.one * (lineWidth / 2f - 2e-3f);

        legs[0].localPosition = new Vector3(-adjPos.x, -adjPos.y, 0);
        legs[0].localScale = new Vector3(lineWidth, lineWidth, size.z);
        legs[1].localPosition = new Vector3(-adjPos.x, adjPos.y, 0);
        legs[1].localScale = new Vector3(lineWidth, lineWidth, size.z);
        legs[2].localPosition = new Vector3(adjPos.x, -adjPos.y, 0);
        legs[2].localScale = new Vector3(lineWidth, lineWidth, size.z);
        legs[3].localPosition = new Vector3(adjPos.x, adjPos.y, 0);
        legs[3].localScale = new Vector3(lineWidth, lineWidth, size.z);

        legs[4].localPosition = new Vector3(-adjPos.x, 0, -adjPos.z);
        legs[4].localScale = new Vector3(lineWidth, size.y, lineWidth);
        legs[5].localPosition = new Vector3(-adjPos.x, 0, adjPos.z);
        legs[5].localScale = new Vector3(lineWidth, size.y, lineWidth);
        legs[6].localPosition = new Vector3(adjPos.x, 0, -adjPos.z);
        legs[6].localScale = new Vector3(lineWidth, size.y, lineWidth);
        legs[7].localPosition = new Vector3(adjPos.x, 0, adjPos.z);
        legs[7].localScale = new Vector3(lineWidth, size.y, lineWidth);

        legs[8].localPosition = new Vector3(0, -adjPos.y, -adjPos.z);
        legs[8].localScale = new Vector3(size.x, lineWidth, lineWidth);
        legs[9].localPosition = new Vector3(0, -adjPos.y, adjPos.z);
        legs[9].localScale = new Vector3(size.x, lineWidth, lineWidth);
        legs[10].localPosition = new Vector3(0, adjPos.y, -adjPos.z);
        legs[10].localScale = new Vector3(size.x, lineWidth, lineWidth);
        legs[11].localPosition = new Vector3(0, adjPos.y, adjPos.z);
        legs[11].localScale = new Vector3(size.x, lineWidth, lineWidth);
    }
}