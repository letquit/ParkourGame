using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourControllerScript : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;
    public float normalizedTransitionDuration = 0.2f;
    private bool playerInAction;
    public Animator animator;
    public PlayerController playerController;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourAction;

    private void Update()
    {
        if (Input.GetButton("Jump") && !playerInAction)
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
    }

    private IEnumerator PerformParkourAction(NewParkourAction action)
    {
        playerInAction = true;
        playerController.SetControl(false);

        animator.CrossFade(action.AnimationName, normalizedTransitionDuration);
        yield return null; // 等待 CrossFade 触发

        yield return new WaitUntil(() => !animator.IsInTransition(0));

        yield return null;

        var currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (!currentState.IsName(action.AnimationName))
        {
            Debug.LogWarning($"Expected animation '{action.AnimationName}' but got '{currentState.fullPathHash}'");
            playerController.SetControl(true);
            playerInAction = false;
            yield break;
        }

        float animLength = currentState.length;
        float timeCounter = 0f;

        while (timeCounter < animLength)
        {
            timeCounter += Time.deltaTime;

            if (action.LookAtObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.RequiredRotation, playerController.rotSpeed * Time.deltaTime);
            }

            if (action.AllowTargetMatching && 
                !animator.IsInTransition(0) && 
                animator.GetCurrentAnimatorStateInfo(0).IsName(action.AnimationName))
            {
                CompareTarget(action);
            }

            yield return null;
        }

        // 在 yield 循环后，恢复控制权前
        playerController.SetControl(true);

        // 重置PlayerController物理状态，让它跟动画驱动的结果完全一致
        playerController.fallingSpeed = 0f; // 让角色不再有垂直下落速度
        playerController.moveDir = Vector3.zero; // 防止有残余运动向量

        // TODO:
        
        // transform.position = action.ComparePosition;

        playerInAction = false;
    }

    private void CompareTarget(NewParkourAction action)
    {
        animator.MatchTarget(action.ComparePosition, transform.rotation, action.CompareBodyPart,
            new MatchTargetWeightMask(new Vector3(0, 1, 0), 0), action.CompareStartTime, action.CompareEndTime);
    }
}
