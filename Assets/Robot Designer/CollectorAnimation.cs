using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CollectorAnimation : MonoBehaviour
{
    public RollerSet collector;
    public Vector2 speed;

    void Update()
    {
        GetComponent<Renderer>().material.mainTextureOffset +=
                speed * collector.percentVelocity * Time.deltaTime;
    }
}
