using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourControllerScript : MonoBehaviour
{
    public EnvironmentChecker environmentChecker;
    public float normalizedTransitionDuration = 0.2f;
    public Animator animator;
    public PlayerController playerController;
    [SerializeField] private NewParkourAction jumpDownParkourAction;

    [Header("Parkour Action Area")]
    public List<NewParkourAction> newParkourAction;

    private void Update()
    {
        if (Input.GetButton("Jump") && !playerController.playerInAction)
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

        if (playerController.playerOnLedge && !playerController.playerInAction && Input.GetButtonDown("Jump"))
        {
            if (playerController.LedgeInfo.angle <= 50)
            {
                playerController.playerOnLedge = false;
                StartCoroutine(PerformParkourAction(jumpDownParkourAction));
            }
        }
    }

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
