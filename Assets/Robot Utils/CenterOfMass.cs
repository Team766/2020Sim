using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteInEditMode]
public class CenterOfMass : MonoBehaviour
{
    public Vector3 centerOfMass;

    void Update()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(GetComponent<Rigidbody>().centerOfMass), 0.02f);
    }
}
