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
        if (playerController.playerHanging && !playerController.playerInAction && Input.GetButton("Leave"))
        {
            StartCoroutine(JumpFromWall());
            return;
        }
        
        if (Input.GetButton("Jump") && !playerController.playerInAction)
        {
            if (!playerController.playerHanging)
            {
                if (ec.CheckClimbing(transform.forward, out RaycastHit climbInfo))
                {
                    currentClimbPoint = climbInfo.transform.GetComponent<ClimbingPoint>();
                    
                    playerController.SetControl(false);
                    // InOutValue = -0.28f;
                    // UpDownValue = -0.15f;
                    // LeftRightValue = 0.15f;
                    InOutValue = -0.23f;
                    UpDownValue = -0.09f;
                    LeftRightValue = 0.15f;
                    StartCoroutine(ClimbToLedge("IdleToClimb", climbInfo.transform, 0.4f, 54f,
                        playerHandOffset: new Vector3(InOutValue, UpDownValue, LeftRightValue)));
                }
            }
            else
            {
                // if (Input.GetButton("Leave") && !playerController.playerInAction)
                // {
                //     StartCoroutine(JumpFromWall());
                //     return;
                // }
                
                float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
                float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));
                
                var inputDirection = new Vector2(horizontal, vertical);

                if (playerController.playerInAction || inputDirection == Vector2.zero) return;
                
                var neighbour = currentClimbPoint.GetNeighbour(inputDirection);
                
                if (neighbour == null) return;

                if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
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
        return ledge.position + ledge.forward * InOutValue + Vector3.up * UpDownValue - handDirection * LeftRightValue;
    }

    private IEnumerator JumpFromWall()
    {
        playerController.playerHanging = false;
        yield return playerController.PerformAction("JumpFromWall");
        playerController.ResetRequiredRotation();
        playerController.SetControl(true);
    }
}
