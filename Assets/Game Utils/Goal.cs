using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

    public GameGUI gameGui;
    public bool isBlue;
    public int points;
	public Transform[] respawnPoints;
	public float respawnRandomRange = 0.0f;
	
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
	        gameGui.addBlueScore(points);
	    else
	        gameGui.addRedScore(points);

        yield return new WaitForSeconds(0.5f);

		var respawnPoint = respawnPoints[Random.Range(0, respawnPoints.Length)];
		var pointOffset = new Vector3(
			Random.Range(-respawnRandomRange, respawnRandomRange),
			0,
			Random.Range(-respawnRandomRange, respawnRandomRange));

		c.transform.position = respawnPoint.position + pointOffset;
		c.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		c.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	}
}
