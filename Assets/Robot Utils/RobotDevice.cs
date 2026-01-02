using UnityEngine;
using System.Collections;

public abstract class RobotDevice : MonoBehaviour {
    public byte DeviceId;

    public abstract CodeDeviceType DeviceType { get; }

    public abstract void RunJoint(CodeBufferView commands);

    public abstract void Disable();

    public abstract void Destroy();

    public abstract void RunSensor(CodeBufferBuilder feedbackValues);

    void Reset() {
        DeviceId = GetComponentInParent<RobotController>(true).ValidateDeviceIds(this);
    }

    void OnValidate() {
        GetComponentInParent<RobotController>(true)?.ValidateDeviceIds(this);
    }
}
