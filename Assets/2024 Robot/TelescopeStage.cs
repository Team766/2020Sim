using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeStage : MonoBehaviour
{
    public ConfigurableJoint endStage;
    public float maxPosition;

    private Vector3 neutralPosition;
    private Vector3 endStageNeutralPosition;

    void Awake()
    {
        neutralPosition = transform.localPosition;
        endStageNeutralPosition = endStage.transform.localPosition;
    }

    void Update()
    {
        float endStagePosition = Vector3.Dot(endStage.transform.localPosition - endStageNeutralPosition, endStage.axis);
        float position = Mathf.Min(endStagePosition, maxPosition);
        transform.localPosition = neutralPosition + position * endStage.axis;
    }
}
