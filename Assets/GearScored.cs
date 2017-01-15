using UnityEngine;
using System.Collections;

public class GearScored : MonoBehaviour {
   
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

        if(other.transform.parent.tag == "Intake")
        {
            RobotController robot = other.transform.root.GetComponent<RobotController>();
            if (robot.holdingGear)
            {
                robot.incrementScore(1);
                robot.setHoldingGear(false);
            }
        }
    }
}
