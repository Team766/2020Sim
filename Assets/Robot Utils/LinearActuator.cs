using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class LinearActuator : MonoBehaviour {
    public Vector3 positionScale;
    
    public bool extended;

    void Update() {
        var joint = GetComponent<ConfigurableJoint>();
        if (extended) {
            joint.targetPosition = positionScale;
        } else {
            joint.targetPosition = -positionScale;
        }
    }
}
