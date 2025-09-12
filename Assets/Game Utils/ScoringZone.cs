using UnityEngine;

public class CommonScoringArea : MonoBehaviour
{
	public GameGUI gameGui;
	public int points;
	public AudioClip sound;

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Ball")
		{
			if (c.GetComponent<BallProperties>().isBlue)
				gameGui.addBlueScore(points);
			else
				gameGui.addRedScore(points);

			if (sound)
			{
				gameGui.PlaySound(sound);
			}
		}
	}

	void OnTriggerExit(Collider c)
	{
		if (c.tag == "Ball")
		{
			if (c.GetComponent<BallProperties>().isBlue)
				gameGui.addBlueScore(-points);
			else
				gameGui.addRedScore(-points);
		}
	}
}
