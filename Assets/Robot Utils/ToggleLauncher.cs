using UnityEngine;
using System.Collections;

public sealed class ToggleLauncher : StandardRobotJoint
{
	public BallStorage ballStorage;

	public float ShootPower;
	
	public float maxForce;

	private bool launched = false;
	
    public void Launch()
    {
		var projectile = ballStorage.RemoveBall();
        if (projectile == null) {
            return;
        }

        projectile.transform.position = this.transform.position;
		projectile.transform.rotation = this.transform.rotation;
		projectile.AddForce(this.transform.forward * Mathf.Clamp01(ShootPower) * maxForce, ForceMode.Impulse);
    }

    public override void RunJoint(float command) {
		if (command > 0) {
			if (!launched) {
				Launch();
			}
			launched = true;
		} else {
			launched = false;
		}
    }

	public override void Disable() {
        launched = false;
    }

    public override void Destroy() {
        Destroy(this);
    }

    public override int SensorPosition => launched ? 1 : 0;
    public override int SensorVelocity => 0;
}
