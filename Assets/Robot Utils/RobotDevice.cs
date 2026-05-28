using UnityEngine;
using System.Collections;
using Team766.Simulator;

public abstract class RobotDevice : MonoBehaviour {
    public uint DeviceId;

    public abstract void RunJoint(CommandsPacket commands);

    public abstract void Disable();

    public abstract void Destroy();

    public abstract void RunSensor(FeedbackPacket feedbackValues);

    void Reset() {
        DeviceId = GetComponentInParent<RobotController>(true).ValidateDeviceIds(this);
    }

    void OnValidate() {
        GetComponentInParent<RobotController>(true)?.ValidateDeviceIds(this);
    }
}
