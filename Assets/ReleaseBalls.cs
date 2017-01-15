using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseBalls : MonoBehaviour {
    public GameObject hopper1;
    public GameObject hopper2;
    public GameObject ball;

    public Vector3 outputVel;

    public bool empty;
    public int ballsPerHopper = 5;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (empty || other == null)
            return;
        if(other.transform.parent.tag == "Player")
        {
            for (int i = 0; i < ballsPerHopper; i++)
            {
                GameObject.Instantiate(ball, hopper1.transform.position, new Quaternion()).GetComponent<Rigidbody>().velocity = outputVel;
                GameObject.Instantiate(ball, hopper2.transform.position, new Quaternion()).GetComponent<Rigidbody>().velocity = outputVel;
            }
            empty = true;
        }
    }
}
