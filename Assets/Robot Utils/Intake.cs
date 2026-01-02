using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RollerSet))]
public class Intake : MonoBehaviour {
    public List<BallStorage> ballStorage;

	public float intakeThreshold = 0.2f;

	public float flowRatePeriod = 0.2f;
	public float startupTime = 0.0f;

	public List<string> compatiblePieceTypes;

    private HashSet<Rigidbody> contained = new HashSet<Rigidbody>();

	private float nextBallTime;

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
		if (c.CompareTag("Ball") &&
            (compatiblePieceTypes.Count == 0 ||
             compatiblePieceTypes.Contains(c.GetComponent<BallProperties>()?.gamePieceType)))
        {
			contained.Add(c.attachedRigidbody);
		}
	}
	void OnTriggerExit(Collider c) {
		contained.Remove(c.attachedRigidbody);
	}

    void FixedUpdate() {
        if (GetComponent<RollerSet>().command / intakeThreshold >= 1.0f) {
			if (Time.fixedTime >= nextBallTime) {
				var obj = Get();
				if (obj) {
					foreach (var store in ballStorage) {
						if (store.StoreBall(obj)) {
							contained.Remove(obj);
							break;
						}
					}

					nextBallTime = Time.fixedTime + flowRatePeriod;
				}
			}
        } else {
			nextBallTime = Time.fixedTime + startupTime;
		}
    }
}
