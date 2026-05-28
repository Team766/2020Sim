using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LineSensor : RobotSensor {
    public string detectedTag = "Line";

    public bool IsDetecting {
        get {
            return colliding.Count > 0;
        }
    }

    public override void UpdateSensorValue(Team766.Simulator.SensorProto value) {
        value.Digital = new() { Value = IsDetecting };
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
