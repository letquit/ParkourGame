using UnityEngine;

/// <summary>
/// 控制角色下落移动状态的行为类，继承自StateMachineBehaviour
/// 用于在角色进入下落状态时禁用玩家控制，在退出下落状态时重新启用玩家控制
/// </summary>
public class ControlFallingMovement : StateMachineBehaviour
{
    /// <summary>
    /// 当动画状态机进入当前状态时调用的方法
    /// </summary>
    /// <param name="animator">触发此事件的动画组件</param>
    /// <param name="animatorStateInfo">当前动画状态的信息</param>
    /// <param name="layerIndex">动画层的索引</param>
    private new void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        // 禁用玩家控制器中的玩家控制功能
        animator.GetComponent<PlayerController>().HasPlayerControl = false;
    }
    
    /// <summary>
    /// 当动画状态机退出当前状态时调用的方法
    /// </summary>
    /// <param name="animator">触发此事件的动画组件</param>
    /// <param name="animatorStateInfo">当前动画状态的信息</param>
    /// <param name="layerIndex">动画层的索引</param>
    private new void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        // 启用玩家控制器中的玩家控制功能
        animator.GetComponent<PlayerController>().HasPlayerControl = true;
    }
}
