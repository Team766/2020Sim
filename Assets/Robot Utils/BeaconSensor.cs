using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconSensor : RobotSensor {
    const int BEACON_SENSOR_START = 120;
    const int BEACON_SENSOR_STRIDE = 6;

    public Transform[] beacons;

    public sealed override IEnumerable<int> FeedbackValueIndices {
        get {
            int[] channels = new int[beacons.Length * BEACON_SENSOR_STRIDE];
            for (int i = 0; i < beacons.Length * BEACON_SENSOR_STRIDE; ++i) {
                channels[i] = BEACON_SENSOR_START + i;
            }
            return channels;
        }
    }

    public override void RunSensor(int[] feedbackValues) {
        Quaternion invRot = Quaternion.Inverse(transform.rotation);
        for (int i = 0; i < beacons.Length; ++i) {
            Vector3 position = transform.InverseTransformPoint(beacons[i].position);
            Vector3 rotation = (invRot * beacons[i].rotation).eulerAngles;
            // In robot code, X axis is forward, Y axis is left, Z axis is up
            feedbackValues[BEACON_SENSOR_START + i * BEACON_SENSOR_STRIDE + 0] = (int)(1000 * position.z); // x
            feedbackValues[BEACON_SENSOR_START + i * BEACON_SENSOR_STRIDE + 1] = (int)(1000 * -position.x); // y
            feedbackValues[BEACON_SENSOR_START + i * BEACON_SENSOR_STRIDE + 2] = (int)(1000 * position.y); // z
            feedbackValues[BEACON_SENSOR_START + i * BEACON_SENSOR_STRIDE + 3] = (int)(1000 * ((rotation.y + 270) % 360)); // yaw
            feedbackValues[BEACON_SENSOR_START + i * BEACON_SENSOR_STRIDE + 4] = (int)(1000 * rotation.x); // pitch
            feedbackValues[BEACON_SENSOR_START + i * BEACON_SENSOR_STRIDE + 5] = (int)(1000 * rotation.z); // roll
        }
    }
}