using UnityEngine;
using System.Collections.Generic;

public class HoldObject : MonoBehaviour {

    public Rigidbody holding;
    
    public bool Grab(Rigidbody obj) {
        if (holding)
            return false;
        
        holding = obj;
        holding.transform.position = this.transform.position;
        holding.transform.rotation = this.transform.rotation;
        holding.transform.parent = this.transform;
        holding.isKinematic = true;
        
        return true;
    }

    public Rigidbody Drop() {
        var obj = holding;
        holding = null;
        if (obj) {
            obj.isKinematic = false;
            obj.transform.parent = null;
        }
        return obj;
    }
}
