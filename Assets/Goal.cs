using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

    public GameGUI gameGui;
    public bool isBlue;
    public int points;
	public Transform respawnPoint;
	
	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Ball")
		{
			StartCoroutine(RespawnBall(c));
		}
	}
	
	IEnumerator RespawnBall(Collider c)
	{
	    StartCoroutine(gameGui.ShowMessage(
	        (isBlue ? "Blue" : "Red") + " scored " + points + " points!"));
	    
	    if (isBlue)
	        gameGui.blueScore += points;
	    else
	        gameGui.redScore += points;

        yield return new WaitForSeconds(0.5f);

		c.transform.position = respawnPoint.position;
		c.GetComponent<Rigidbody>().velocity = Vector3.zero;
		c.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	}
}
