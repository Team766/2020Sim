using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RollerSet))]
public sealed class ContinuousLauncher : MonoBehaviour
{
	public List<BallStorage> ballStorage;

	public float maxForce;

	public float launchThreshold = 600f;

	public float flowRatePeriod = 0.5f;
	public float startupTime = 0.5f;

	private float nextLaunchTime = 0.0f;

	void Reset() {
		GetComponent<RollerSet>().mechanicalScalar = 12;
    }

	void FixedUpdate() {
		if (GetComponent<RollerSet>().percentVelocity / launchThreshold >= 1.0f)
		{
			if (Time.fixedTime >= nextLaunchTime)
			{
				Launch();
			}
		}
		else
		{
			nextLaunchTime = Time.fixedTime + startupTime;
		}
	}

	public void Launch()
	{
		Rigidbody projectile = null;
		foreach (var store in ballStorage)
		{
			projectile = store.RemoveBall();
			if (projectile != null)
			{
				break;
			}
		}
		if (projectile == null) {
			return;
		}

		var rollers = GetComponent<RollerSet>();

		projectile.transform.position = this.transform.position;
		projectile.transform.rotation = this.transform.rotation;
		var force = Mathf.Clamp01(rollers.percentVelocity * Mathf.Sign(launchThreshold)) * maxForce * this.transform.forward;
		projectile.AddForce(force, ForceMode.Impulse);

		Debug.Log("Launch " + projectile.name + " at " + force);

		nextLaunchTime = Time.fixedTime + flowRatePeriod;
	}
}