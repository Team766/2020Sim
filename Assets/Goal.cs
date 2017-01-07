using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

    public bool isBlue;
    public bool highGoal;
	public Transform respawnPoint;
	public GUIStyle messageStyle;
	bool messageShowing = false;
	
	void OnGUI()
	{
		if (messageShowing)
		{
			GUILayout.Label((isBlue ? "BLUE" : "RED") + (highGoal ? " HIGH GOAL!" : " LOW GOAL!"), messageStyle);
		}
	}
	
	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Ball")
		{
			StartCoroutine(RespawnBall(c));
		}
	}
	
	IEnumerator RespawnBall(Collider c)
	{
		messageShowing = true;

        yield return new WaitForSeconds(0.5f);

		c.transform.position = respawnPoint.position;
		c.GetComponent<Rigidbody>().velocity = Vector3.zero;
		c.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(1.5f);
		
		messageShowing = false;
	}
}
