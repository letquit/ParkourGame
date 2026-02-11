using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 处理角色跑酷动作控制的脚本，包括跳跃、攀爬等动作的检测和执行
/// </summary>
public class ParkourControllerScript : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;
    public float normalizedTransitionDuration = 0.2f;
    public Animator animator;
    public PlayerController playerController;
    [SerializeField] private NewParkourAction jumpDownParkourAction;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourAction;

    /// <summary>
    /// 每帧更新检测玩家输入和环境状态，触发相应的跑酷动作
    /// </summary>
    private void Update()
    {
        // 检测跳跃输入并执行障碍物相关的跑酷动作
        if (Input.GetButton("Jump") && !playerController.playerInAction && !playerController.playerHanging)
        {
            var hitData = environmentChecker.CheckObstacle();
            
            if (hitData.hitFound)
            {
                foreach (var action in newParkourAction)
                {
                    if (action.CheckIfAvailable(hitData, transform))
                    {
                        // perform parkour action
                        StartCoroutine(PerformParkourAction(action));
                        break;
                    }
                }
            }
        }

        // 检测在边缘时的跳跃输入，执行向下跳跃动作
        if (playerController.playerOnLedge && !playerController.playerInAction && Input.GetButtonDown("Jump"))
        {
            if (playerController.LedgeInfo.angle <= 50)
            {
                playerController.playerOnLedge = false;
                StartCoroutine(PerformParkourAction(jumpDownParkourAction));
            }
        }
    }

    /// <summary>
    /// 执行指定的跑酷动作，包括动画播放和目标匹配
    /// </summary>
    /// <param name="action">要执行的跑酷动作对象</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerController.SetControl(false);

        CompareTargetParameter compareTargetParameter = null;
        if (action.AllowTargetMatching)
        {
            compareTargetParameter = new CompareTargetParameter
            {
                position = action.ComparePosition,
                bodyPart = action.CompareBodyPart,
                positionWeight = action.ComparePositionWeight,
                startTime = action.CompareStartTime,
                endTime = action.CompareEndTime
            };
        }

        yield return playerController.PerformAction(action.AnimationName, compareTargetParameter,
            action.RequiredRotation, action.LookAtObstacle, action.ParkourActionDelay);
        
        playerController.SetControl(true);
    }

    /// <summary>
    /// 执行动画目标匹配，用于精确控制角色身体部位的位置
    /// </summary>
    /// <param name="action">包含目标匹配参数的跑酷动作</param>
    private void CompareTarget(NewParkourAction action)
    {
        animator.MatchTarget(
            action.ComparePosition,
            transform.rotation,
            action.CompareBodyPart,
            new MatchTargetWeightMask(action.ComparePositionWeight, 0),
            action.CompareStartTime,
            action.CompareEndTime);
    }
}
