using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour
{
	public float ShootPower;
	
	public float maxForce;
	
	public void Launch(Rigidbody projectile)
	{
        projectile.transform.position = this.transform.position;
		projectile.AddForce(this.transform.forward * Mathf.Clamp01(ShootPower) * maxForce, ForceMode.Impulse);
    }
}
