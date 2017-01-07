using UnityEngine;
using System.Collections.Generic;

public class ActuatedDefense : MonoBehaviour {
    public Transform door;
    public HashSet<RobotController> actuations = new HashSet<RobotController>();

    public void Actuate(RobotController rc)
    {
        actuations.Add(rc);
        if (door.GetComponent<HingeJoint>() != null)
        {
            door.GetComponent<HingeJoint>().useMotor = true;
        }
        if (door.GetComponent<ConfigurableJoint>() != null)
        {
            var drive = door.GetComponent<ConfigurableJoint>().xDrive;
            drive.mode = JointDriveMode.Position;
            door.GetComponent<ConfigurableJoint>().xDrive = drive;
        }
        door.GetComponent<Rigidbody>().WakeUp();
    }

    void UnActuate(RobotController rc)
    {
        actuations.Remove(rc);
        if (actuations.Count > 0) return;

        if (door.GetComponent<HingeJoint>() != null)
        {
            door.GetComponent<HingeJoint>().useMotor = false;
        }
        if (door.GetComponent<ConfigurableJoint>() != null)
        {
            var drive = door.GetComponent<ConfigurableJoint>().xDrive;
            drive.mode = JointDriveMode.None;
            door.GetComponent<ConfigurableJoint>().xDrive = drive;
        }
        door.GetComponent<Rigidbody>().WakeUp();
    }

    static T FindComponentInAncestors<T>(Transform t)
    {
        if (t == null) return default(T);
        if (t.GetComponent<T>() != null) return t.GetComponent<T>();
        return FindComponentInAncestors<T>(t.parent);
    }

    void OnTriggerEnter(Collider c)
    {
        var rc = FindComponentInAncestors<RobotController>(c.transform);
        if (rc != null)
        {
            if (!rc.actuatedDefenses.ContainsKey(this))
            {
                rc.actuatedDefenses.Add(this, 0);
            }
            ++rc.actuatedDefenses[this];
        }
    }

    void OnTriggerExit(Collider c)
    {
        var rc = FindComponentInAncestors<RobotController>(c.transform);
        if (rc != null)
        {
            if (rc.actuatedDefenses.ContainsKey(this) && --rc.actuatedDefenses[this] == 0)
            {
                UnActuate(rc);
            }
        }
    }
}
