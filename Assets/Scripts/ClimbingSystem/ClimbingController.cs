using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 攀爬控制器，处理玩家的攀爬相关逻辑，包括攀爬点检测、移动、跳跃等操作
/// </summary>
public class ClimbingController : MonoBehaviour
{
    private EnvironmentChecker ec;
    public PlayerController playerController;

    private ClimbingPoint currentClimbPoint;
    
    public float InOutValue;
    public float UpDownValue;
    public float LeftRightValue;

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        ec = GetComponent<EnvironmentChecker>();
    }

    /// <summary>
    /// 更新攀爬状态和处理用户输入
    /// </summary>
    private void Update()
    {
        // 处理离开攀爬状态的输入
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

        // 处理跳跃按钮输入，实现攀爬功能
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

                // 处理向上攀爬到顶部
                if (currentClimbPoint != null && currentClimbPoint.mountPoint && inputDirection.y == 1)
                {
                    StartCoroutine(ClimbToTop());
                    return;
                }

                var neighbour = currentClimbPoint.GetNeighbour(inputDirection);

                if (neighbour == null)
                    return;

                // 处理跳跃类型的连接
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
                // 处理移动类型的连接
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

        // 处理从高处下降到攀爬状态
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

    /// <summary>
    /// 执行攀爬到指定位置的协程
    /// </summary>
    /// <param name="animationName">要播放的动画名称</param>
    /// <param name="ledgePoint">目标边缘点的变换组件</param>
    /// <param name="compareStartTime">动画比较开始时间</param>
    /// <param name="compareEndTime">动画比较结束时间</param>
    /// <param name="hand">使用的手部目标（默认为右手）</param>
    /// <param name="playerHandOffset">玩家手部偏移量</param>
    /// <returns>协程枚举器</returns>
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

    /// <summary>
    /// 设置手部在边缘上的位置
    /// </summary>
    /// <param name="ledge">边缘变换组件</param>
    /// <param name="hand">手部目标</param>
    /// <param name="playerHandOffset">玩家手部偏移量</param>
    /// <returns>计算后的手部世界位置</returns>
    private Vector3 SetHandPosition(Transform ledge, AvatarTarget hand, Vector3? playerHandOffset)
    {
        var offsetValue = (playerHandOffset != null)
            ? playerHandOffset.Value
            : new Vector3(InOutValue, UpDownValue, LeftRightValue);

        var handDirection = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offsetValue.x + Vector3.up * offsetValue.y - handDirection * offsetValue.z;
    }

    /// <summary>
    /// 从墙壁上跳下的协程
    /// </summary>
    /// <returns>协程枚举器</returns>
    private IEnumerator JumpFromWall()
    {
        playerController.playerHanging = false;
        yield return playerController.PerformAction("JumpFromWall");
        playerController.ResetRequiredRotation();
        playerController.SetControl(true);
    }

    /// <summary>
    /// 攀爬到顶部的协程
    /// </summary>
    /// <returns>协程枚举器</returns>
    private IEnumerator ClimbToTop()
    {
        playerController.playerHanging = false;
        yield return playerController.PerformAction("ClimbToTop");
        
        playerController.EnableCC(true);
        
        yield return new WaitForSeconds(0.5f);
        
        playerController.ResetRequiredRotation();
        playerController.SetControl(true);
    }

    /// <summary>
    /// 获取最近的攀爬点
    /// </summary>
    /// <param name="dropClimbPoint">掉落攀爬点的变换组件</param>
    /// <param name="hitpoint">碰撞点位置</param>
    /// <returns>距离碰撞点最近的攀爬点</returns>
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
