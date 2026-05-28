using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class BallStorage : NetworkBehaviour {
    public List<HoldObject> heldObjects;

    public List<Rigidbody> initialHeldObjects;

    void Start() {
        if (isServer) {
            foreach (var spec in initialHeldObjects) {
                if (NumHolding == heldObjects.Count) {
                    break;
                }
                Rigidbody obj;
                if (spec.gameObject.scene.IsValid()) {
                    // Object is already in the scene
                    obj = spec;
                } else {
                    // Object is a prefab
                    obj = Instantiate(spec);
                    NetworkServer.Spawn(obj.gameObject);
                }
                StoreBall(obj);
            }
        }
    }

    public int NumHolding => heldObjects.Count(h => h.held);

    public bool StoreBall(Rigidbody obj) {
        var holder = heldObjects.Find(h => !h.held);
        if (!holder) {
            return false;
        }
        obj.isKinematic = true;
        foreach (var c in obj.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        obj.transform.parent = holder.transform;
        obj.transform.localPosition = Vector3.zero;
        holder.held = obj.gameObject;
        return true;
    }

    public Rigidbody RemoveBall() {
        var holder = heldObjects.FindLast(h => h.held);
        if (!holder) {
            return null;
        }
        var obj = holder.held.GetComponent<Rigidbody>();
        holder.held = null;
        obj.transform.parent = null;
        obj.isKinematic = false;
        foreach (var c in obj.GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }
        return obj;
    }
}
