using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RobotController : NetworkBehaviour {
    public GameGUI gameGui;
    public GameObject ballPrefab;

    public Wheel[] leftWheels;
    public Wheel[] rightWheels;
    public Wheel[] centerWheel;
    
    public HoldObject[] heldObjects;
    [SyncVar]
    public int holding = 0;

    public float motorScaler;
    public float intakeScaler;

    public IntakeArm intakeArm;
    public Launcher launcher;
    public Intake intake;

    public LineSensor lineSensor1;
    public LineSensor lineSensor2;
    public LineSensor lineSensor3;

    private float headingPrev = 0.0f;

    public bool IsDisabled {
        get {
            return gameGui.RobotMode == RobotMode.Disabled;
        }
    }

    void FixedUpdate()
    {
        var current = Heading;
        var diff = Mathf.DeltaAngle(current, headingPrev);
        GyroAngle += diff;
        headingPrev = current;

        GyroRate = Vector3.Dot(transform.up, GetComponent<Rigidbody>().angularVelocity) * Mathf.Rad2Deg;
    }

    void Update()
    {
        if (IsDisabled) {
            Disable();
        }

        for (int i = 0; i < heldObjects.Length; ++i) {
            heldObjects[i].SetState(holding > i);
        }
    }

    public void Disable() {
        _SetMotors(0, 0, 0);
        _SetIntake(0);
        // Leave IntakeArm "enabled" because it's modeling a pneumatic cylinder,
        //     so it should retain it's last state.
    }

    public void SetMotors(float left, float right, float center)
    {
        if (IsDisabled) {
            return;
        }
        _SetMotors(left, right, center);
    }
    private void _SetMotors(float left, float right, float center)
    {
        //Debug.Log("Left: " + left + "Right: " + right + "Center: " + center);

        foreach (var h in leftWheels)
        {
            h.RunJoint(motorScaler * left);
        }
        foreach (var h in rightWheels)
        {
            h.RunJoint(motorScaler * right);
        }
        foreach (var h in centerWheel)
        {
            h.RunJoint(motorScaler * center);
        }
    }
    
    public bool Store(Rigidbody obj) {
        if (IsDisabled) {
            return false;
        }
        if (holding >= heldObjects.Length) {
            return false;
        }
        ++holding;
        NetworkServer.Destroy(obj.gameObject);
        return true;
    }

    public void SetIntake(float speed) {
        if (IsDisabled) {
            return;
        }
        _SetIntake(speed);
    }
    private void _SetIntake(float speed)
    {
        intake.speed = speed;
    }

    public void SetIntakeArm(bool state)
    {
        if (IsDisabled) {
            return;
        }
        _SetIntakeArm(state);
    }
    private void _SetIntakeArm(bool state)
    {
        intakeArm.RunJoint(intakeScaler * (state ? 1.0f : -1.0f));
    }

    public float ShootPower
    {
        get
        {
            return launcher.ShootPower;
        }
        set
        {
            launcher.ShootPower = value;
        }
    }

    public void Launch()
    {
        if (IsDisabled) {
            return;
        }
        if (holding == 0) {
            return;
        }
        --holding;
        var obj = Instantiate(ballPrefab, this.transform.position, this.transform.rotation);
        NetworkServer.Spawn(obj);
        launcher.Launch(obj.GetComponent<Rigidbody>());
    }

    public int LeftEncoder
    {
        get
        {
            if (leftWheels.Length == 0)
                return 0;
            return leftWheels[0].Encoder;
        }
    }

    public int RightEncoder
    {
        get
        {
            if (rightWheels.Length == 0)
                return 0;
            return rightWheels[0].Encoder;
        }
    }

    public int CenterEncoder
    {
        get
        {
            if (centerWheel.Length == 0)
                return 0;
            return centerWheel[0].Encoder;
        }
    }

    static float Angle360(Vector3 v1, Vector3 v2, Vector3 n)
    {
        //  Acute angle [0,180]
        float angle = Vector3.Angle(v1, v2);

        //  -Acute angle [180,-179]
        float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(v1, v2)));
        return angle * sign;
    }

    public float Heading
    {
        get
        {
            return Angle360(Vector3.forward, transform.forward, Vector3.up);
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

    public bool BallPresence
    {
        get
        {
            return holding > 0;
        }
    }
}

