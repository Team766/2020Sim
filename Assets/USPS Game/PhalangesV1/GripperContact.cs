using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripperContact : MonoBehaviour
{
    public HashSet<Collider> colliding = new HashSet<Collider>();public int collidingCount;

    void Update()
    {
        collidingCount = colliding.Count;
    }

    void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }

    void OnCollisionExit(Collision collision)
    {
        OnTriggerExit(collision.collider);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Ball" || collider.tag == "Crate")
        {
            colliding.Add(collider);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        colliding.Remove(collider);
    }
}
