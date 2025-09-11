using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Intake : StandardRobotJoint {
    public float speed;

    public BallStorage ballStorage;
    public Storage2023 storage2023;
    public Transform[] rollers;
    
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
        foreach (var roller in rollers) {
            roller.Rotate(0, 800 * speed * Time.deltaTime, 0);
        }

        if (speed > 0.5) {
            var obj = Get();
            if (obj) {
                if (ballStorage) {
                    ballStorage.StoreBall(obj);
                }
                if (storage2023) {
                    storage2023.StoreBall(obj);
                }
            }
        }
    }

	public override void RunJoint(float command)
    {
        speed = command;
    }

    public override void Disable() {
        RunJoint(0.0f);
    }

    public override void Destroy() {
        Destroy(this);
    }
}
