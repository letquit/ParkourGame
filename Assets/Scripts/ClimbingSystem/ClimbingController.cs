using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (playerController.playerHanging && !playerController.playerInAction)
        {
            var st = playerController.animator.GetCurrentAnimatorStateInfo(0);
        }
        
        if (playerController.playerHanging && !playerController.playerInAction && Input.GetButton("Leave"))
        {
            if (currentClimbPoint != null && currentClimbPoint.mountPoint)
            {
                var downNeighbour = currentClimbPoint.GetNeighbour(Vector2.down);
                if (downNeighbour != null && downNeighbour.climbingPoint != null)
                {
                    currentClimbPoint = downNeighbour.climbingPoint;
                    InOutValue = 0.1f;
                    UpDownValue = -0.44f;
                    LeftRightValue = 0.25f;
                    StartCoroutine(ClimbToLedge("DropToFreeHang", currentClimbPoint.transform, 0.41f, 0.54f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                    return;
                }
            }
            StartCoroutine(JumpFromWall());
            return;
        }

        if (Input.GetButtonDown("Jump") && !playerController.playerInAction)
        {
            if (!playerController.playerHanging)
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    currentClimbPoint = climbInfo.transform.GetComponent<ClimbingPoint>();

                    playerController.SetControl(false);
                    InOutValue = -0.23f;
                    UpDownValue = -0.09f;
                    LeftRightValue = 0.15f;
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.4f, 54f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
            }
            else
            {
                float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
                float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));
                var inputDirection = new Vector2(horizontal, vertical);
                
                if (playerController.playerInAction || inputDirection == Vector2.zero)
                {
                    return;
                }

                if (currentClimbPoint != null && currentClimbPoint.mountPoint && inputDirection.y == 1)
                {
                    StartCoroutine(ClimbToTop());
                    return;
                }

                var neighbour = currentClimbPoint.GetNeighbour(inputDirection);

                if (neighbour == null)
                    return;

                if (neighbour.connectionType == ConnectionType.Jump && Input.GetButtonDown("Jump"))
                {
                    if (neighbour.climbingPoint != null)
                    {
                        currentClimbPoint = neighbour.climbingPoint;

                        if (neighbour.pointDirection.y == 1)
                        {
                            InOutValue = 0.1f;
                            UpDownValue = 0.05f;
                            LeftRightValue = 0.25f;
                            StartCoroutine(ClimbToLedge("ClimbUp", currentClimbPoint.transform, 0.34f, 0.64f,
                                playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                        }

                        if (neighbour.pointDirection.y == -1)
                        {
                            InOutValue = 0.2f;
                            UpDownValue = 0.05f;
                            LeftRightValue = 0.25f;
                            StartCoroutine(ClimbToLedge("ClimbDown", currentClimbPoint.transform, 0.341f, 0.68f,
                                playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                        }

                        if (neighbour.pointDirection.x == 1)
                        {
                            StartCoroutine(ClimbToLedge("ClimbRight", currentClimbPoint.transform, 0.2f, 0.51f));
                        }

                        if (neighbour.pointDirection.x == -1)
                        {
                            InOutValue = 0.1f;
                            UpDownValue = 0.04f;
                            LeftRightValue = 0.25f;
                            StartCoroutine(ClimbToLedge("ClimbLeft", currentClimbPoint.transform, 0.2f, 0.51f,
                                playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                        }
                    }
                }
                else if (neighbour.connectionType == ConnectionType.Move)
                {
                    if (neighbour.climbingPoint != null)
                    {
                        currentClimbPoint = neighbour.climbingPoint;

                        if (neighbour.pointDirection.x == 1)
                        {
                            InOutValue = 0.2f;
                            UpDownValue = 0.03f;
                            LeftRightValue = 0.25f;
                            StartCoroutine(ClimbToLedge("ShimmyRight", currentClimbPoint.transform, 0f, 0.3f,
                                playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                        }

                        if (neighbour.pointDirection.x == -1)
                        {
                            InOutValue = 0.2f;
                            UpDownValue = 0.03f;
                            LeftRightValue = 0.25f;
                            StartCoroutine(ClimbToLedge("ShimmyLeft", currentClimbPoint.transform, 0f, 0.3f,
                                AvatarTarget.LeftHand,
                                playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                        }
                    }
                }
            }
        }

        if (!playerController.playerHanging && !playerController.playerInAction && Input.GetButton("Leave"))
        {
            if (ec.CheckDropClimbPoint(out RaycastHit DropHit))
            {
                currentClimbPoint = GetNearestClimbingPoint(DropHit.transform, DropHit.point);

                playerController.SetControl(false);
                InOutValue = 0.15f;
                UpDownValue = 0.01f;
                LeftRightValue = 0.25f;
                StartCoroutine(ClimbToLedge("DropToFreeHang", currentClimbPoint.transform, 0.41f, 0.54f,
                    playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
            }
        }
    }

    private IEnumerator ClimbToLedge(string animationName, Transform ledgePoint, float compareStartTime,
        float compareEndTime, AvatarTarget hand = AvatarTarget.RightHand, Vector3? playerHandOffset = null)
    {
        var compareParams = new CompareTargetParameter
        {
            position = SetHandPosition(ledgePoint, hand, playerHandOffset),
            bodyPart = hand,
            positionWeight = Vector3.one,
            startTime = compareStartTime,
            endTime = compareEndTime
        };

        var requireRot = Quaternion.LookRotation(-ledgePoint.forward);

        yield return playerController.PerformAction(animationName, compareParams, requireRot, true);
        
        playerController.playerHanging = true;
    }

    private Vector3 SetHandPosition(Transform ledge, AvatarTarget hand, Vector3? playerHandOffset)
    {
        var offsetValue = (playerHandOffset != null)
            ? playerHandOffset.Value
            : new Vector3(InOutValue, UpDownValue, LeftRightValue);

        var handDirection = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offsetValue.x + Vector3.up * offsetValue.y - handDirection * offsetValue.z;
    }

    private IEnumerator JumpFromWall()
    {
        playerController.playerHanging = false;
        yield return playerController.PerformAction("JumpFromWall");
        playerController.ResetRequiredRotation();
        playerController.SetControl(true);
    }

    private IEnumerator ClimbToTop()
    {
        playerController.playerHanging = false;
        yield return playerController.PerformAction("ClimbToTop");
        
        playerController.EnableCC(true);
        
        yield return new WaitForSeconds(0.5f);
        
        playerController.ResetRequiredRotation();
        playerController.SetControl(true);
    }

    private ClimbingPoint GetNearestClimbingPoint(Transform dropClimbPoint, Vector3 hitpoint)
    {
        var points = dropClimbPoint.GetComponentsInChildren<ClimbingPoint>();

        ClimbingPoint nearestPoint = null;
        
        float nearestPointDistance = Mathf.Infinity;

        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitpoint);

            if (distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }
        
        return nearestPoint;
    }
}