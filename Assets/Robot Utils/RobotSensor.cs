using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RobotSensor : MonoBehaviour {
    public abstract void RunSensor(int[] feedbackValues);

    public abstract IEnumerable<int> FeedbackValueIndices {
        get;
    }

    void OnValidate() {
        GetComponentInParent<RobotController>().ValidateSensorIndices(this);
    }
}

public abstract class StandardRobotSensor : RobotSensor {
    public int feedbackIndex;

    public sealed override IEnumerable<int> FeedbackValueIndices {
        get {
            return new[] { feedbackIndex };
        }
    }

    public sealed override void RunSensor(int[] feedbackValues) {
        feedbackValues[feedbackIndex] = SensorValue;
    }

    public abstract int SensorValue {
        get;
    }
}