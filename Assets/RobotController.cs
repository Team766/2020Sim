﻿using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class RobotController : NetworkBehaviour {
    public static string robotVariant = "";
    [SyncVar]
    private string clientRobotVariant = null;
    private string activeRobotVariant = null;

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

    #region BusterSwordV1
    public Wheel twacker;
    public float twackerScaler;
    #endregion

    public LineSensor lineSensor1;
    public LineSensor lineSensor2;
    public LineSensor lineSensor3;

    private float headingPrev = 0.0f;

    public bool IsDisabled {
        get {
            return gameGui.RobotMode == RobotMode.Disabled;
        }
    }

    void Awake()
    {
        UpdateVariant();
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
        UpdateVariant();

        GetComponent<Rigidbody>().isKinematic = IsDisabled;
        if (IsDisabled) {
            Disable();
        }

        if (intakeArm) {
            intakeArm.GetComponent<Rigidbody>().isKinematic = !isServer;
        }

        #region BusterSwordV1
        if (twacker) {
            twacker.GetComponent<Rigidbody>().isKinematic = !isServer;
        }
        #endregion

        for (int i = 0; i < heldObjects.Length; ++i) {
            heldObjects[i].SetState(holding > i);
        }
    }

    void UpdateVariant() {
        if (isServer) {
            clientRobotVariant = robotVariant;
        } else if (clientRobotVariant != null) {
            robotVariant = clientRobotVariant;
        }
        if (activeRobotVariant != robotVariant) {
            Debug.Log("Activating robot variant: " + robotVariant);
            var variantObj = transform.Find(robotVariant);
            if (variantObj != null) {
                Debug.Log("Found robot variant object");
                variantObj.gameObject.SetActive(true);
            }
            activeRobotVariant = robotVariant;
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
        if (intake != null) {
            intake.speed = speed;
        }
        #region BusterSwordV1
        if (twacker != null) {
            twacker.RunJoint(twackerScaler * speed);
        }
        #endregion
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
        if (intakeArm != null) {
            intakeArm.RunJoint(intakeScaler * (state ? 1.0f : -1.0f));
        }
    }

    public float ShootPower
    {
        get
        {
            if (launcher == null) {
                return 0;
            }
            return launcher.ShootPower;
        }
        set
        {
            if (launcher != null) {
                launcher.ShootPower = value;
            }
        }
    }

    public void Launch()
    {
        if (launcher == null) {
            return;
        }
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

    public int MechanismEncoder
    {
        get
        {
            if (twacker) {
                return twacker.Encoder;
            }
            return 0;
        }
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

