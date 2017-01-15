using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour
{
	public float ShootPower;
	
	public GameObject robot;

    public GameObject fuel;
	
	public float maxForce;
	
	public void Launch()
	{
        //fuel.GetComponent<Rigidbody>();
        var projectile = GameObject.Instantiate(fuel, robot.transform.position, new Quaternion()).GetComponent<Rigidbody>();
        projectile.transform.position = this.transform.position;
		projectile.AddForce(this.transform.forward * Mathf.Clamp01(ShootPower) * maxForce, ForceMode.Impulse);
    }
}
