using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CenterOfMass : MonoBehaviour
{
    public Vector3 centerOfMass;

    void Update()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb) {
            rb.centerOfMass = centerOfMass;
        }
        var articBody = GetComponent<ArticulationBody>();
        if (articBody) {
            articBody.centerOfMass = centerOfMass;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(centerOfMass), 0.02f);
    }
}
