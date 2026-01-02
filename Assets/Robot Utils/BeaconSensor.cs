using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class BeaconSensor : RobotSensor {
    public Transform[] beacons;

    public override CodeDeviceType DeviceType => CodeDeviceType.BEACON_POSITION_SENSOR;

    public override void RunSensor(CodeBufferBuilder feedbackValues) {
        Quaternion invRot = Quaternion.Inverse(transform.rotation);
        feedbackValues.DeviceData<int>(DeviceId, DeviceType, beacons.SelectMany(b =>
        {
            Vector3 position = transform.InverseTransformPoint(b.position);
            Vector3 rotation = (invRot * b.rotation).eulerAngles;
            // In robot code, X axis is forward, Y axis is left, Z axis is up
            return new[] {
                (int)(1000 * position.z), // x
                (int)(1000 * -position.x), // y
                (int)(1000 * position.y), // z
                (int)(1000 * ((rotation.y + 270) % 360)), // yaw
                (int)(1000 * rotation.x), // pitch
                (int)(1000 * rotation.z), // roll
            };
        }).ToArray());
    }
}