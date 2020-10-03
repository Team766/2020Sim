using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Intake : MonoBehaviour {
    public float speed;

    public RobotController robotController;
    public IntakeArm intakeArm;
    
    HashSet<Rigidbody> contained = new HashSet<Rigidbody>();
	
	public Rigidbody Get() {
		// Discard objects that have been destroyed.
		contained.RemoveWhere(rb => !rb);

		Rigidbody holding = null;
		float bestDist = float.MaxValue;
		foreach (var c in contained) {
			float dist = Vector3.Distance(c.position, this.transform.position);
			if (dist < bestDist) {
				holding = c;
				bestDist = dist;
			}
		}
		return holding;
	}

	void OnTriggerEnter(Collider c) {
		if (c.tag == "Ball") {
			contained.Add(c.attachedRigidbody);
		}
	}
	void OnTriggerExit(Collider c) {
		contained.Remove(c.attachedRigidbody);
	}

    void Update() {
        if (speed > 0.5) {
            var obj = Get();
            if (obj) {
                robotController.Store(obj);
            }
        }
    }
}
