using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RampBalanceScore : MonoBehaviour
{
    private GameGUI gameGui;
    public Quaternion balancedOrientation;
    public float maxAngleForPoints;
    public int numPointsWhenBalanced;
    public bool isBlue;

    public Material lightOnMaterial;
    public Material lightOffMaterial;
    public Renderer[] lightRenderers;

    // These are provided as feedback to the user for debugging purposes.
    public Quaternion currentOrientation;
    public bool isBalanced;
    public bool robotOnRamp;

    private bool wasPreviouslyScored = false;
    private int robotCollisionCount = 0;

    void Start()
    {
        // Don't call FindAnyObjectByType in Awake because of script ordering issues.
        gameGui = FindAnyObjectByType<GameGUI>(FindObjectsInactive.Include);
    }

    void Update()
    {
        currentOrientation = transform.rotation;

        isBalanced = Mathf.Abs(Quaternion.Angle(transform.rotation, balancedOrientation)) < maxAngleForPoints;

        robotOnRamp = robotCollisionCount > 0;

        bool isScored = isBalanced && robotOnRamp;

        if (isScored && !wasPreviouslyScored) {
            if (isBlue)
                gameGui.addBlueScore(numPointsWhenBalanced);
            else
                gameGui.addRedScore(numPointsWhenBalanced);
        } else if (!isScored && wasPreviouslyScored) {
            if (isBlue)
                gameGui.addBlueScore(-numPointsWhenBalanced);
            else
                gameGui.addRedScore(-numPointsWhenBalanced);
        }
        
        wasPreviouslyScored = isScored;

        foreach (Renderer r in lightRenderers)
        {
            r.material = isBalanced ? lightOnMaterial : lightOffMaterial;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.transform.root.GetComponent<RobotController>() != null) {
            ++robotCollisionCount;
        }
    }

    void OnCollisionExit(Collision c)
    {
        if (c.transform.root.GetComponent<RobotController>() != null) {
            --robotCollisionCount;
        }
    }
}
