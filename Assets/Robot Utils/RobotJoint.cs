using System.Collections;
using UnityEngine;

public abstract class RobotJoint : MonoBehaviour {
    public abstract void RunJoint(int[] commands);

    public abstract void Disable();

    public abstract void Destroy();
}

public abstract class StandardRobotJoint : RobotJoint {
    public int commandIndex;

    public sealed override void RunJoint(int[] commands) {
        float command = 0.0f;
        if (commandIndex < commands.Length) {
            command = commands[commandIndex] / 512.0f;
        }
        RunJoint(command);
    }

    public abstract void RunJoint(float command);

    void OnValidate() {
        if (commandIndex < 10) {
            Debug.LogError($"Robot joint {this.name} has invalid commandIndex", this);
        }
    }
}