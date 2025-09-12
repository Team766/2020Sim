using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PassiveIntake : MonoBehaviour
{
    public BallStorage ballStorage;
    public Storage2023 storage2023;

    HashSet<Rigidbody> contained = new HashSet<Rigidbody>();

    public Rigidbody Get()
    {
        // Discard objects that have been destroyed.
        contained.RemoveWhere(rb => !rb);

        Rigidbody holding = null;
        float bestDist = float.MaxValue;
        foreach (var c in contained)
        {
            float dist = Vector3.Distance(c.position, this.transform.position);
            if (dist < bestDist)
            {
                holding = c;
                bestDist = dist;
            }
        }
        return holding;
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.tag == "Ball")
        {
            contained.Add(c.attachedRigidbody);
        }
    }
    void OnTriggerExit(Collider c)
    {
        contained.Remove(c.attachedRigidbody);
    }

    void Update()
    {
        var obj = Get();
        if (obj)
        {
            if (ballStorage)
            {
                ballStorage.StoreBall(obj);
            }
            if (storage2023)
            {
                storage2023.StoreBall(obj);
            }
            contained.Remove(obj);
        }
    }
}
