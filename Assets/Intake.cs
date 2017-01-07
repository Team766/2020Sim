using UnityEngine;
using System.Collections;

public class Intake : MonoBehaviour
{
    public float speed;

    public Rigidbody holding;
    public HoldObject payload;
    public Transform holdPosition;
    public IntakeArm intakeArm;

    Quaternion holdRotation = Quaternion.identity;

    public void Drop()
    {
        payload.Drop(holding);

        holding = null;
    }

    public void setSpeed(float s) {
        speed = s;
    }

    public void Grab()
    {
            holding = payload.Get();

            if (holding)
                holdRotation = Quaternion.Inverse(payload.transform.rotation) * holding.transform.rotation;
    }

    public void KickOut()
    {
        if (holding)
            holding.transform.position = payload.transform.position;
        holding = null;
    }

    void Update()
    {
        Debug.Log(intakeArm.Angle);
        if (payload.Get() != null && speed > 0 && holding == null && intakeArm.Angle > -5) {
            Grab();
        }



            if (holding)
            {
                holding.transform.position = holdPosition.transform.position;
                holding.transform.rotation = holdPosition.transform.rotation * holdRotation;
            }
    }
}
