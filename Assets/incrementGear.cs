using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class incrementGear : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (checkNull(other))
            return;
          
        if (other.transform.root.tag == "Player")
        {
            RobotController robot = other.transform.root.GetComponent<RobotController>();
            if (Input.GetButton("SpawnGear"))
            {
                if (!robot.holdingGear)
                    robot.setHoldingGear(true);
            }
            else
            {
                robot.fillHopper(true);
            }
            
        }
    }


    private bool checkNull(Collider other)
    {
        return (other == null || other.transform.root == null);
    }
}
