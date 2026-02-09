using UnityEngine;

public class ControlFallingMovement : StateMachineBehaviour
{
    private new void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.GetComponent<PlayerController>().HasPlayerControl = false;
    }
    
    private new void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.GetComponent<PlayerController>().HasPlayerControl = true;
    }
}
