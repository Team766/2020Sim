using System.Collections.Generic;
using UnityEngine;

public sealed class GyroSensor : RobotSensor {
    private float headingPrev = 0.0f;

    void FixedUpdate()
    {
        var current = Heading;
        var diff = Mathf.DeltaAngle(current, headingPrev);
        GyroAngle += diff;
        headingPrev = current;

        var articBody = GetComponent<ArticulationBody>();
        Vector3 angularVelocity = articBody ?
            articBody.angularVelocity :
            GetComponent<Rigidbody>().angularVelocity;
        GyroRate = Vector3.Dot(transform.up, angularVelocity) * Mathf.Rad2Deg;
    }

    public override void UpdateSensorValue(Team766.Simulator.SensorProto value) {
        value.Imu = new() {
            Yaw = GyroAngle,
            Pitch = GyroPitch,
            Roll = GyroRoll,
            YawRate = GyroRate,
        };
    }

    public float Heading
    {
        get
        {
            return transform.eulerAngles.y;
        }
    }

    public float GyroAngle {
        get;
        private set;
    }

    public float GyroRate {
        get;
        private set;
    }

    public float GyroPitch {
        get
        {
            return transform.eulerAngles.x;
        }
    }

    public float GyroRoll {
        get
        {
            return transform.eulerAngles.z;
        }
    }
}