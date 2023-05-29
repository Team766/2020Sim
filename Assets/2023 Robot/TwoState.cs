using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TwoState : MonoBehaviour
{
    public bool state;
    public Vector3 truePosition;
    public Quaternion trueRotation;
    public Vector3 falsePosition;
    public Quaternion falseRotation;

    void Reset() {
        truePosition = falsePosition = transform.localPosition;
        trueRotation = falseRotation = transform.localRotation;
    }

    void Update()
    {
        transform.localPosition = state ? truePosition : falsePosition;
        transform.localRotation = state ? trueRotation : falseRotation;
    }
}
