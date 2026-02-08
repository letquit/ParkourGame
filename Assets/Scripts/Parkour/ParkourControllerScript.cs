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
        yield return null;

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if (!animationState.IsName(action.AnimationName))
            Debug.Log("Animation Name is Incorrect");

        yield return new WaitForSeconds(animationState.length);
        
        playerController.SetControl(true);
        playerInAction = false;
    }
}
