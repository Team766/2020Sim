using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHighGoal : MonoBehaviour {

    public GameObject robot;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {

        if (other == null)
            return;


        if (other.transform.root.tag == "Fuel")
        {
            Destroy(other);
            robot.GetComponent<RobotController>().addHighScored(1);
        }
    }
}
