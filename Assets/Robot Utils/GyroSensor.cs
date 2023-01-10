using System.Collections.Generic;
using UnityEngine;

public sealed class GyroSensor : RobotSensor {
    const int HEADING = 12;
    const int HEADING_PRECISE = 15;
    const int HEADING_RATE = 16;
    const int GYRO_PITCH = 80;
    const int GYRO_ROLL = 81;

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

    public sealed override IEnumerable<int> FeedbackValueIndices {
        get {
            return new[] { HEADING, HEADING_PRECISE, HEADING_RATE, GYRO_PITCH, GYRO_ROLL };
        }
    }

    public override void RunSensor(int[] feedbackValues) {
        feedbackValues[HEADING] = (int)GyroAngle;
        feedbackValues[HEADING_PRECISE] = (int)(GyroAngle * 10);
        feedbackValues[HEADING_RATE] = (int)(GyroRate * 100);
        feedbackValues[GYRO_PITCH] = (int)(GyroPitch * 10);
        feedbackValues[GYRO_ROLL] = (int)(GyroRoll * 10);
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