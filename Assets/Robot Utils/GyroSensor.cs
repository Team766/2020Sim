using System.Collections.Generic;
using UnityEngine;

public sealed class GyroSensor : RobotSensor {
    const int HEADING = 0;
    const int HEADING_PRECISE = 1;
    const int HEADING_RATE = 2;
    const int GYRO_PITCH = 3;
    const int GYRO_ROLL = 4;

    public override CodeDeviceType DeviceType => CodeDeviceType.GYRO_SENSOR;

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

    public override void RunSensor(CodeBufferBuilder feedbackValues) {
        int[] values = new int[5];
        values[HEADING] = (int)GyroAngle;
        values[HEADING_PRECISE] = (int)(GyroAngle * 10);
        values[HEADING_RATE] = (int)(GyroRate * 100);
        values[GYRO_PITCH] = (int)(GyroPitch * 10);
        values[GYRO_ROLL] = (int)(GyroRoll * 10);
        feedbackValues.DeviceData<int>(DeviceId, DeviceType, values);
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