using UnityEngine;
using System;
using System.Collections;

public class IntakeArm : Wheel
{
    public override void Disable() {
        // No - op. Pneumatics should continue to apply force when the robot is disabled.
    }
}
