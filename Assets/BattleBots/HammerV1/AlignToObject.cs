using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignToObject : MonoBehaviour {
    public Transform other;

    void Update() {
        var rotation = new Quaternion();
        rotation.SetLookRotation(other.position - this.transform.position);
        this.transform.rotation = rotation;
    }
}
