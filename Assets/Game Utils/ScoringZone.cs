using UnityEngine;

public class CommonScoringArea : MonoBehaviour
{
	private GameGUI gameGui;
	public int points;
	public AudioClip sound;

	private void Awake()
	{
		gameGui = FindAnyObjectByType<GameGUI>();
	}

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
