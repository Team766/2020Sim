using UnityEngine;
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

    #region IkeaV1
    public Wheel ikea;
    public float ikeaScale;
    #endregion

    #region NinjaV1
    public Wheel ninja;
    public float ninjaScale;
    #endregion

    #region PetPaloozaV1
    public LinearActuator bopper;
    #endregion

    #region BillboardV1
    public Wheel billboard;
    public float billboardScale;
    #endregion

    #region RBG_V1
    public Wheel rbg;
    public float rbgScale;
    public Rigidbody rbg2;
    #endregion

    #region PhalangesV1
    public Elevator phalangesElevator;
    public float phalangesElevatorScale;
    public ElevatorKinematic[] phalangesGrippers = new ElevatorKinematic[0];
    public float phalangesGripperScale;
    #endregion

    public Wheel auxWheel;
    public float auxWheelScale;

    public Wheel aux2Wheel;
    public float aux2WheelScale;

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

        if (!isServer) {
            GetComponent<Rigidbody>().useGravity = false;
            foreach (Wheel w in leftWheels) {
                if (w) {
                    Destroy(w);
                    Destroy(w.GetComponent<HingeJoint>());
                    Destroy(w.GetComponent<Rigidbody>());
                    //w.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
            foreach (Wheel w in rightWheels) {
                if (w) {
                    Destroy(w);
                    Destroy(w.GetComponent<HingeJoint>());
                    Destroy(w.GetComponent<Rigidbody>());
                    //w.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
            foreach (Wheel w in centerWheel) {
                if (w) {
                    Destroy(w);
                    Destroy(w.GetComponent<HingeJoint>());
                    Destroy(w.GetComponent<Rigidbody>());
                    //w.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }

        #region BusterSwordV1
        if (twacker && !isServer) {
            Destroy(twacker);
            Destroy(twacker.GetComponent<HingeJoint>());
            Destroy(twacker.GetComponent<Rigidbody>());
            //twacker.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region IkeaV1
        if (ikea && !isServer) {
            Destroy(ikea);
            Destroy(ikea.GetComponent<HingeJoint>());
            Destroy(ikea.GetComponent<Rigidbody>());
            //ikea.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region NinjaV1
        if (ninja && !isServer) {
            Destroy(ninja);
            Destroy(ninja.GetComponent<HingeJoint>());
            Destroy(ninja.GetComponent<Rigidbody>());
            //ninja.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region PetPaloozaV1
        if (bopper && !isServer) {
            Destroy(bopper);
            Destroy(bopper.GetComponent<ConfigurableJoint>());
            Destroy(bopper.GetComponent<Rigidbody>());
            //bopper.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region HammerV1
        if (intakeArm && !isServer) {
            Destroy(intakeArm);
            var intakeHinge = intakeArm.GetComponent<HingeJoint>();
            if (intakeHinge) {
                Destroy(intakeHinge);
            }
            var intakeCjoint = intakeArm.GetComponent<ConfigurableJoint>();
            if (intakeCjoint) {
                Destroy(intakeCjoint);
            }
            Destroy(intakeArm.GetComponent<Rigidbody>());
            //intakeArm.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region BillboardV1
        if (billboard && !isServer) {
            Destroy(billboard);
            Destroy(billboard.GetComponent<HingeJoint>());
            Destroy(billboard.GetComponent<Rigidbody>());
            //billboard.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region RBG_V1
        if (rbg && !isServer) {
            Destroy(rbg2.GetComponent<HingeJoint>());
            Destroy(rbg2);
            //rbg2.isKinematic = true;
            Destroy(rbg);
            Destroy(rbg.GetComponent<HingeJoint>());
            Destroy(rbg.GetComponent<Rigidbody>());
            //rbg.GetComponent<Rigidbody>().isKinematic = true;
        }
        #endregion
        #region PhalangesV1
        if (phalangesElevator && !isServer) {
            Destroy(phalangesElevator);
            Destroy(phalangesElevator.GetComponent<ConfigurableJoint>());
            Destroy(phalangesElevator.GetComponent<Rigidbody>());
            //phalangesElevator.GetComponent<Rigidbody>().isKinematic = true;
        }
        foreach (var g in phalangesGrippers) {
            if (g && !isServer) {
                Destroy(g);
                /*Destroy(g.GetComponent<ConfigurableJoint>());
                Destroy(g.GetComponent<Rigidbody>());*/
                //g.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
        #endregion
        if (auxWheel && !isServer) {
            Destroy(auxWheel);
            Destroy(auxWheel.GetComponent<HingeJoint>());
            Destroy(auxWheel.GetComponent<Rigidbody>());
            //auxWheel.GetComponent<Rigidbody>().isKinematic = true;
        }
        if (aux2Wheel && !isServer) {
            Destroy(aux2Wheel);
            Destroy(aux2Wheel.GetComponent<HingeJoint>());
            Destroy(aux2Wheel.GetComponent<Rigidbody>());
            //aux2Wheel.GetComponent<Rigidbody>().isKinematic = true;
        }

        for (int i = 0; i < heldObjects.Length; ++i) {
            if (heldObjects[i]) {
                heldObjects[i].SetState(holding > i);
            }
        }
    }

    void UpdateVariant() {
        if (isServer) {
            clientRobotVariant = robotVariant;
        } else if (clientRobotVariant != null) {
            robotVariant = clientRobotVariant;
        }
        if (activeRobotVariant != robotVariant) {
            if (activeRobotVariant != null) {
                Debug.Log("Deactivating robot variant: " + activeRobotVariant);
                var oldVariantObj = transform.Find(activeRobotVariant);
                if (oldVariantObj != null) {
                    Debug.Log("Found robot variant object");
                    oldVariantObj.gameObject.SetActive(false);
                }
            }
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
        _SetAuxiliaryMotor(0);
        _SetAuxiliary2Motor(0);
        
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
            if (h)
            {
                h.RunJoint(motorScaler * left);
            }
        }
        foreach (var h in rightWheels)
        {
            if (h)
            {
                h.RunJoint(motorScaler * right);
            }
        }
        foreach (var h in centerWheel)
        {
            if (h)
            {
                h.RunJoint(motorScaler * center);
            }
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

    public void SetAuxiliaryMotor(float speed) {
        if (IsDisabled) {
            return;
        }
        _SetAuxiliaryMotor(speed);
    }
    private void _SetAuxiliaryMotor(float speed) {
        #region PhalangesV1
        foreach (var g in phalangesGrippers) {
            g.RunJoint(phalangesGripperScale * speed);
        }
        #endregion
        if (auxWheel != null) {
            auxWheel.RunJoint(auxWheelScale * speed);
        }
    }

    public void SetAuxiliary2Motor(float speed) {
        if (IsDisabled) {
            return;
        }
        _SetAuxiliary2Motor(speed);
    }
    private void _SetAuxiliary2Motor(float speed) {
        if (aux2Wheel != null) {
            aux2Wheel.RunJoint(aux2WheelScale * speed);
        }
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
        #region IkeaV1
        if (ikea != null) {
            ikea.RunJoint(ikeaScale * speed);
        }
        #endregion
        #region NinjaV1
        if (ninja != null) {
            ninja.RunJoint(ninjaScale * speed);
        }
        #endregion
        #region BillboardV1
        if (billboard != null) {
            billboard.RunJoint(billboardScale * speed);
        }
        #endregion
        #region RGB_V1
        if (rbg != null) {
            rbg.RunJoint(rbgScale * speed);
        }
        #endregion
        #region PhalangesV1
        if (phalangesElevator != null) {
            phalangesElevator.RunJoint(phalangesElevatorScale * speed);
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
        #region PetPaloozaV1
        if (bopper != null) {
            bopper.extended = state;
        }
        #endregion
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
            #region BusterSwordV1
            if (twacker && twacker.Encoder != 0) {
                return twacker.Encoder;
            }
            #endregion
            #region IkeaV1
            if (ikea && ikea.Encoder != 0) {
                return ikea.Encoder;
            }
            #endregion
            #region NinjaV1
            if (ninja && ninja.Encoder != 0) {
                return ninja.Encoder;
            }
            #endregion
            #region BillboardV1
            if (billboard && billboard.Encoder != 0) {
                return billboard.Encoder;
            }
            #endregion
            #region RBG_V1
            if (rbg && rbg.Encoder != 0) {
                return rbg.Encoder;
            }
            #endregion
            #region PhalangesV1
            if (phalangesElevator && phalangesElevator.Encoder != 0) {
                return phalangesElevator.Encoder;
            }
            #endregion
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
            return transform.eulerAngles.y;
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

    public float GyroPitch {
        get
        {
            return transform.eulerAngles.x;
        }
    }

    public float GyroRoll {
        get
        {
            return transform.eulerAngles.z;
        }
    }

    public bool BallPresence
    {
        get
        {
            return holding > 0;
        }
    }
}

