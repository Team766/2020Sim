using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoGripper : MonoBehaviour
{
    public GripperContact contact1;
    public GripperContact contact2;

    Dictionary<Collider, Vector3> colliding = new Dictionary<Collider, Vector3>();
    public int collidingCount;

    private List<Collider> toRemove = new List<Collider>();

    public ElevatorKinematic gripperElevator;

    void FixedUpdate()
    {
        /*colliding.Clear();
        colliding.UnionWith(contact1.colliding);
        colliding.IntersectWith(contact2.colliding);*/

        if (!gripperElevator.isStuck) {
            foreach (var c in contact1.colliding)
            {
                if (contact2.colliding.Contains(c) && !colliding.ContainsKey(c))
                {
                    Debug.Log("Disable " + c.name);
                    var joint = c.attachedRigidbody.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = this.GetComponentInParent<Rigidbody>();
                    colliding.Add(c, c.attachedRigidbody.transform.localPosition);
                }
            }
            toRemove.Clear();
            foreach (var kvp in colliding)
            {
                var c = kvp.Key;
                if (!contact1.colliding.Contains(c) || !contact2.colliding.Contains(c))
                {
                    Debug.Log("Reenable " + c.name);
                    Destroy(c.attachedRigidbody.GetComponent<FixedJoint>());
                    c.attachedRigidbody.WakeUp();
                    StartCoroutine("WaitAndWake", c.attachedRigidbody);
                    toRemove.Add(c);
                }
            }
            foreach (var c in toRemove)
            {
                colliding.Remove(c);
            }
        }

        collidingCount = colliding.Count;
    }

    private IEnumerator WaitAndWake(Rigidbody r)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        r.WakeUp();
    }

    /*void FixedUpdate()
    {
        foreach (var kvp in colliding)
        {
            var c = kvp.Key;
            c.attachedRigidbody.transform.position = kvp.Value + transform.position;
            c.attachedRigidbody.velocity = Vector3.zero;
            c.attachedRigidbody.angularVelocity = Vector3.zero;
        }
    }*/
}
