using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RollerSet))]
public sealed class ContinuousLauncher : MonoBehaviour
{
	public List<BallStorage> ballStorage;

	public float maxForce;

	public float launchThreshold = 0.2f;

	public float flowRatePeriod = 0.5f;
	public float startupTime = 0.5f;

	private float nextLaunchTime = 0.0f;

	void Reset() {
		GetComponent<RollerSet>().maxDegreesPerSecond = 3000;
    }

	void FixedUpdate() {
		if (GetComponent<RollerSet>().command / launchThreshold >= 1.0f)
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

		var command = GetComponent<RollerSet>().command;

		projectile.transform.position = this.transform.position;
		projectile.transform.rotation = this.transform.rotation;
		projectile.AddForce(Mathf.Clamp01(command) * maxForce * this.transform.forward, ForceMode.Impulse);

		nextLaunchTime = Time.fixedTime + flowRatePeriod;
	}
}