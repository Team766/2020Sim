using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class HoldObject : MonoBehaviour {
    public void SetState(bool holding) {
        GetComponent<MeshRenderer>().enabled = holding;
    }
}
