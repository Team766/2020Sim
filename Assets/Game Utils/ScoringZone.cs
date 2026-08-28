using UnityEngine;
using System;
using System.Collections.Generic;

public class ScoringZone : MonoBehaviour
{
	public enum Team
    {
		Red,
		Blue,
		BallColor,
    }

	private GameGUI gameGui;
	public int points;
	public AudioClip sound;
	public Team team = Team.BallColor;
	public bool requireEntirelyInZone = false;
	public List<GameObject> scored = new();

	void Start()
	{
		// Don't call FindAnyObjectByType in Awake because of script ordering issues.
		gameGui = FindAnyObjectByType<GameGUI>();
	}

	private bool ScoreForBlue(Component c)
    {
		return team switch
		{
			Team.Red => false,
			Team.Blue => true,
			Team.BallColor => c.GetComponent<BallProperties>().isBlue,
			_ => throw new ArgumentOutOfRangeException(nameof(team), team, "Invalid enum value."),
		};
	}

	void OnTriggerStay(Collider c)
	{
		if (!c.CompareTag("Ball"))
			return;

		if (requireEntirelyInZone && !ColliderUtils.WorldSpaceBoundsContains(GetComponent<Collider>(), c.gameObject)) {
			if (scored.Contains(c.gameObject))
			{
				Debug.Log("Spill " + c);
				OnTriggerExit(c);
			}
			return;
		}

		if (scored.Contains(c.gameObject))
			return;

		Debug.Log("Enter " + c);

		scored.Add(c.gameObject);

		if (ScoreForBlue(c))
			gameGui.addBlueScore(points);
		else
			gameGui.addRedScore(points);

		if (sound)
		{
			gameGui.PlaySound(sound);
		}
	}

	void OnTriggerExit(Collider c)
	{
		Debug.Log("Exit " + c);
		if (!scored.Remove(c.gameObject))
			return;

		if (ScoreForBlue(c))
			gameGui.addBlueScore(-points);
		else
			gameGui.addRedScore(-points);
	}
}
