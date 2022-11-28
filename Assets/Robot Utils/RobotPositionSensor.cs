using System.Collections.Generic;

public sealed class RobotPositionSensor : RobotSensor {
    const int ROBOT_X = 8;
    const int ROBOT_Y = 9;

    public sealed override IEnumerable<int> FeedbackValueIndices {
        get {
            return new[] { ROBOT_X, ROBOT_Y };
        }
    }

    public override void RunSensor(int[] feedbackValues) {
        feedbackValues[ROBOT_X] = (int)(transform.position.x * 1000);
        feedbackValues[ROBOT_Y] = (int)(transform.position.z * 1000);
    }
}