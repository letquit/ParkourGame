using System;
using System.Collections;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    private EnvironmentChecker ec;
    public PlayerController playerController;

    public float InOutValue;
    public float UpDownValue;
    public float LeftRightValue;

    private void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
    }

    private void Update()
    {
        if (Input.GetButton("Jump") && !playerController.playerInAction)
        {
            if (!playerController.playerHanging)
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    playerController.SetControl(false);
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.4f, 54f));
                }
            }
            else
            {
                // Ledge to Ledge parkour actions
            }
        }
    }

    private IEnumerator ClimbToLedge(string animationName, Transform ledgePoint, float compareStartTime,
        float compareEndTime)
    {
        var compareParams = new CompareTargetParameter
        {
            position = SetHandPosition(ledgePoint),
            bodyPart = AvatarTarget.RightHand,
            positionWeight = Vector3.one,
            startTime = compareStartTime,
            endTime = compareEndTime
        };

        var requireRot = Quaternion.LookRotation(-ledgePoint.forward);

        yield return playerController.PerformAction(animationName, compareParams, requireRot, true);
        
        playerController.playerHanging = true;
    }

    private Vector3 SetHandPosition(Transform ledge)
    {
        // InOutValue = -0.18f;
        // UpDownValue = -0.13f;
        // LeftRightValue = 0.15f;
        InOutValue = -0.28f;
        UpDownValue = -0.15f;
        LeftRightValue = 0.15f;

        return ledge.position + ledge.forward * InOutValue + Vector3.up * UpDownValue - ledge.right * LeftRightValue;
    }
}
