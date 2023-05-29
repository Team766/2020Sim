using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public Vector3 speed;
    public float maxPosition;

    // Update is called once per frame
    void Update()
    {
        transform.position += speed * Time.deltaTime;
        if (Vector3.Dot(transform.position, speed) >= maxPosition * speed.magnitude) {
            speed *= -1.0f;
        }
    }
}
