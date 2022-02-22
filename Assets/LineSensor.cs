﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSensor : MonoBehaviour {
    public string detectedTag = "Line";

    public bool IsDetecting {
        get {
            return colliding.Count > 0;
        }
    }
    
    private HashSet<Collider> colliding = new HashSet<Collider>();
    
    void OnTriggerEnter(Collider c) {
		if (c.tag == detectedTag) {
			colliding.Add(c);
		}
	}
	void OnTriggerExit(Collider c) {
		colliding.Remove(c);
	}
}
