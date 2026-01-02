using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class BallStorage : NetworkBehaviour {
	public GameObject ballPrefab;

    public List<HoldObject> heldObjects;
    [SyncVar]
    public int holding = 0;

    [ServerCallback]
    void Update()
    {
        for (int i = 0; i < holding; ++i)
        {
            if (!heldObjects[i].held)
            {
                var obj = Instantiate(ballPrefab, this.transform.position, this.transform.rotation);
                NetworkServer.Spawn(obj);
                var rb = obj.GetComponent<Rigidbody>();

                rb.isKinematic = true;
                foreach (var c in obj.GetComponentsInChildren<Collider>())
                {
                    c.enabled = false;
                }
                obj.transform.parent = heldObjects[i].transform;
                obj.transform.localPosition = Vector3.zero;
                heldObjects[i].held = rb.gameObject;
            }
        }
    }

    public bool StoreBall(Rigidbody obj) {
        if (holding >= heldObjects.Count) {
            return false;
        }
        obj.isKinematic = true;
        foreach (var c in obj.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        obj.transform.parent = heldObjects[holding].transform;
        obj.transform.localPosition = Vector3.zero;
        heldObjects[holding].held = obj.gameObject;
        ++holding;
        return true;
    }

    public Rigidbody RemoveBall() {
        if (holding == 0) {
            return null;
        }
        --holding;
        var obj = heldObjects[holding].held.GetComponent<Rigidbody>();
        heldObjects[holding].held = null;
        obj.transform.parent = null;
        obj.isKinematic = false;
        foreach (var c in obj.GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }
        return obj;
    }
}
