using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour
{
	public int playerNumber = 1;
	public RobotController robot;
	public float powerChangeSpeed = 1.0f;
	public bool tankDrive;
	
	public Rect guiRect;

    private bool launchIsPressed = false;
	
	void OnGUI()
	{
        Rect r = guiRect;
        if (r.x < 0) r.x += Screen.width;
        if (r.y < 0) r.y += Screen.height;
    
		GUILayout.BeginArea(r);
		
		tankDrive = GUILayout.Toggle(tankDrive, "Arcade/Tank Drive");
		
		GUILayout.EndArea();
	}

	// Update is called once per frame
	void Update ()
	{
		float leftPower, rightPower, centerPower;
		if (tankDrive)
		{
			leftPower = Input.GetAxis("P" + playerNumber + " Left");
			rightPower = Input.GetAxis("P" + playerNumber + " Right");
            centerPower = Input.GetAxis("P" + playerNumber + " Center");
		}
		else
		{
            float drive = Input.GetAxis("P" + playerNumber + " Vertical");
			float steer = Input.GetAxis("P" + playerNumber + " Horizontal");
			leftPower = Mathf.Clamp(drive + steer, -1, 1);
			rightPower = Mathf.Clamp(drive - steer, -1, 1);
            centerPower = Input.GetAxis("P" + playerNumber + " Center");
		}
		
		robot.SetMotors(leftPower, rightPower, centerPower);

        float intake = Input.GetAxis("P" + playerNumber + " intake");
        robot.SetIntake(intake);

        float gripper = Input.GetAxis ("P" + playerNumber + " Gripper");
		if (gripper > 0)
			robot.SetIntakeArm(true);
		else if (gripper < 0)
			robot.SetIntakeArm(false);

        //if (Input.GetButton("P" + playerNumber + " Actuate"))
		//	robot.Actuate();

        //TODO: robot.ShootPower = Mathf.Clamp01(robot.ShootPower + Input.GetAxis ("P" + playerNumber + " Shoot Power") * powerChangeSpeed * Time.deltaTime);

        bool launchButton = Input.GetButton("P" + playerNumber + " Launch");
        if (launchButton && launchIsPressed == false)
        {
            robot.Launch();
        }
        launchIsPressed = launchButton;

        //if (Input.GetButton("P" + playerNumber + " Launch2"))
        //    robot.Launch2();
    }
}
