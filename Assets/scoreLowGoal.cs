using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scoreLowGoal : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other){

        if (other == null)
            return; 
        

        if(other.transform.root.tag == "Player")
        {
            RobotController robot = other.transform.root.GetComponent<RobotController>();
            robot.scoreAllBalls();
        }     
    }
}
