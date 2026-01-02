using UnityEngine;
using System;
using System.Collections;

public class IntakeArm : RotationalJoint
{
    public override void Disable() {
        // No - op. Pneumatics should continue to apply force when the robot is disabled.
    }
}
