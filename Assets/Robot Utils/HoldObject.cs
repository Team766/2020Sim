using UnityEngine;
using System.Collections.Generic;

public class HoldObject : MonoBehaviour {
    public Rigidbody held;

    void Start() {
        // Legacy version of this component modulated a MeshRenderer to display
        // the held object. We now keep the full GameObject that's held, so we
        // should just keep the MeshRenderer disabled.
        var mr = GetComponent<MeshRenderer>();
        if (mr) {
            mr.enabled = false;
        }
    }
}
