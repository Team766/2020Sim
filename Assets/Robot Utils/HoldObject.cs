using UnityEngine;
using Mirror;

public class HoldObject : NetworkBehaviour {
    [SyncVar]
    public GameObject held;
}
