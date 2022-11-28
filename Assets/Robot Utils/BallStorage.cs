using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class BallStorage : NetworkBehaviour {
	public GameObject ballPrefab;

    public HoldObject[] heldObjects;
    [SyncVar]
    public int holding = 0;

    void Update() {
        for (int i = 0; i < heldObjects.Length; ++i) {
            if (heldObjects[i]) {
                heldObjects[i].SetState(holding > i);
            }
        }
    }

    public bool StoreBall(Rigidbody obj) {
        if (holding >= heldObjects.Length) {
            return false;
        }
        ++holding;
        NetworkServer.Destroy(obj.gameObject);
        return true;
    }

    public Rigidbody RemoveBall() {
        if (holding == 0) {
            return null;
        }
        --holding;
        var obj = Instantiate(ballPrefab, this.transform.position, this.transform.rotation);
		NetworkServer.Spawn(obj);
        return obj.GetComponent<Rigidbody>();
    }
}
