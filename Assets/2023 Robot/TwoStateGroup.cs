using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TwoStateGroup : StandardRobotJoint
{
    public bool state;
    public TwoState[] objects;

    void Update()
    {
        foreach (var o in objects) {
            if (o) {
                o.state = state;
            }
        }
    }

    public override void RunJoint(float command)
    {
        if (command > 0.5) {
            state = true;
        }
        if (command < -0.5) {
            state = false;
        }
    }

    public override void Disable() {
        // No-op
    }

    public override void Destroy() {
        Destroy(this);
        foreach (var o in objects) {
            if (o) {
                Destroy(o);
            }
        }
    }
}
