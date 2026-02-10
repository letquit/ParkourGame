using System;
using System.Collections;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    private EnvironmentChecker ec;
    public PlayerController playerController;

    private ClimbingPoint currentClimbPoint;
    
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
                    currentClimbPoint = climbInfo.transform.GetComponent<ClimbingPoint>();
                    
                    playerController.SetControl(false);
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.4f, 54f));
                }
            }
            else
            {
                float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
                float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));
                
                var inputDirection = new Vector2(horizontal, vertical);

                if (playerController.playerInAction || inputDirection == Vector2.zero) return;
                
                var neighbour = currentClimbPoint.GetNeighbour(inputDirection);
                
                if (neighbour == null) return;

                if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
                {
                    currentClimbPoint = neighbour.climbingPoint;

                    if (neighbour.pointDirection.y == 1)
                    {
                        StartCoroutine(ClimbToLedge("ClimbUp", currentClimbPoint.transform, 0.34f, 0.64f));
                    }
                    
                    if (neighbour.pointDirection.y == -1)
                    {
                        StartCoroutine(ClimbToLedge("ClimbDown", currentClimbPoint.transform, 0.341f, 0.68f));
                    }
                    
                    if (neighbour.pointDirection.x == 1)
                    {
                        StartCoroutine(ClimbToLedge("ClimbRight", currentClimbPoint.transform, 0.2f, 0.51f));
                    }
                    
                    if (neighbour.pointDirection.x == -1)
                    {
                        StartCoroutine(ClimbToLedge("ClimbLeft", currentClimbPoint.transform, 0.2f, 0.51f));
                    }
                }
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
