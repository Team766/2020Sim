using UnityEngine;
using System.Collections;

public sealed class Launcher2024 : StandardRobotJoint
{
	public BallStorage ballStorage;

	public float maxForce;

	public Intake intake;

	public Transform[] rollers;

	public float speed;

	private float lastLaunchTime = 0.0f;

	void Update() {
        foreach (var roller in rollers) {
            roller.Rotate(0, 3000 * speed * Time.deltaTime, 0);
        }
	}

	public void Launch()
	{
		var projectile = ballStorage.RemoveBall();
		if (projectile == null) {
			return;
		}

		projectile.transform.position = this.transform.position;
		projectile.transform.rotation = this.transform.rotation;
		projectile.AddForce(this.transform.forward * Mathf.Clamp01(speed) * maxForce, ForceMode.Impulse);

		lastLaunchTime = Time.time;
	}

	public override void RunJoint(float command)
	{
		speed = command;
		if (speed > 0.2 && intake.speed > 0.1 && (Time.time - lastLaunchTime) > 0.5f)
		{
			Launch();
		}
	}

	public override void Disable()
	{
		RunJoint(0.0f);
	}

	public override void Destroy()
	{
		Destroy(this);
	}
}