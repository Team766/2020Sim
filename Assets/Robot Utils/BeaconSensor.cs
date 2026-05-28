using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Team766.Simulator;

public sealed class BeaconSensor : RobotSensor {
    public Transform[] beacons;

    public override void UpdateSensorValue(SensorProto value) {
        Quaternion invRot = Quaternion.Inverse(transform.rotation);
        value.Beacons = new ();
        for (int i = 0; i < beacons.Length; ++i) {
            var b = beacons[i];
            if (!b) {
                continue;
            }
            Vector3 position = transform.InverseTransformPoint(b.position);
            Vector3 rotation = (invRot * b.rotation).eulerAngles;
            // In robot code, X axis is forward, Y axis is left, Z axis is up
            var beacon = new BeaconsSensorProto.Types.Beacon();
            beacon.Id = i;
            beacon.Pose = new() {
                X = position.z,
                Y = -position.x,
                Z = position.y,
                Yaw = (rotation.y + 270) % 360,
                Pitch = rotation.x,
                Roll = rotation.z,
            };
            value.Beacons.Beacon.Add(beacon);
        }
    }
}