using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage2023 : MonoBehaviour
{
    public Rigidbody heldObject = null;

    public bool StoreBall(Rigidbody obj) {
        if (heldObject != null) {
            return false;
        }
        heldObject = obj;
        obj.transform.position = transform.position;
        obj.gameObject.AddComponent<FixedJoint>().connectedBody = GetComponentInParent<Rigidbody>();
        return true;
    }

    public Rigidbody RemoveBall() {
        if (heldObject == null) {
            return null;
        }
        var obj = heldObject;
        Destroy(obj.gameObject.GetComponent<FixedJoint>());
        heldObject = null;
        return obj;
    }
}
