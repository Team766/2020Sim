using UnityEngine;
using System.Collections;
using Mirror;

public class Goal : MonoBehaviour {

    public GameGUI gameGui;
    public bool isBlue;
    public int points;
	public Transform[] respawnPoints;
	public float respawnRandomRange = 0.0f;
	public AudioClip sound;
	
	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Ball")
		{
			StartCoroutine(ScoreBall(c));
		}
	}
	
	IEnumerator ScoreBall(Collider c)
	{
		var ballProperties = c.GetComponent<BallProperties>();
		if (!ballProperties || ballProperties.isBlue == this.isBlue)
		{
			if (isBlue)
				gameGui.addBlueScore(points);
			else
				gameGui.addRedScore(points);
		}

		if (sound)
		{
			gameGui.PlaySound(sound);
		}

		yield return new WaitForSeconds(0.4f);

		if (respawnPoints.Length > 0)
		{
			var respawnPoint = respawnPoints[Random.Range(0, respawnPoints.Length)];
			var pointOffset = new Vector3(
				Random.Range(-respawnRandomRange, respawnRandomRange),
				0,
				Random.Range(-respawnRandomRange, respawnRandomRange));

			c.transform.position = respawnPoint.position + pointOffset;
			c.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			c.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
		else
        {
			NetworkServer.Destroy(c.gameObject);
		}
	}
}
