using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家控制器，负责处理玩家移动、动画、碰撞检测和悬崖边缘检测等功能
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 5f;                    // 移动速度
    public float dampTime = 0.02f;                      // 阻尼时间
    public MainCameraController MCC;                    // 主相机控制器引用
    public EnvironmentChecker environmentChecker;       // 环境检查器引用
    public float rotSpeed = 600f;                       // 旋转速度
    private Quaternion requiredRotation;                // 目标旋转角度
    private bool PC = true;                             // 是否有玩家控制权限
    public bool playerInAction { get; private set; }    // 玩家是否正在执行动作

    [Header("Player Animator")] 
    public Animator animator;                           // 动画控制器

    [Header("Player Collision & Gravity")] 
    public CharacterController CC;                      // 角色控制器
    public float surfaceCheckRadius = 0.3f;             // 地面检测半径
    public Vector3 surfaceCheckOffset;                  // 地面检测偏移量
    public LayerMask surfaceLayer;                      // 地面层遮罩
    private bool onSurface;                             // 是否在地面上
    public bool playerOnLedge { get; set; }            // 玩家是否在悬崖边缘
    public bool playerHanging { get; set; }            // 玩家是否悬挂在悬崖上
    public LedgeInfo LedgeInfo { get; set; }           // 悬崖信息
    [SerializeField] public float fallingSpeed;         // 下落速度
    [SerializeField] public Vector3 moveDir;            // 移动方向
    [SerializeField] private Vector3 requiredMoveDir;   // 目标移动方向
    private Vector3 velocity;                           // 当前速度
    
    /// <summary>
    /// Unity更新方法，处理玩家状态更新、移动、地面检测等逻辑
    /// </summary>
    private void Update()
    {
        // if (PC && CC.enabled)
        //     PlayerMovement();
        if (!PC)
            return;
        
        if (playerHanging)
            return;
        
        velocity = Vector3.zero;
        
        if (onSurface)
        {
            fallingSpeed = 0f;
            velocity = moveDir * movementSpeed;

            playerOnLedge = environmentChecker.CheckLedge(moveDir, out LedgeInfo ledgeInfo);
            if (playerOnLedge)
            {
                LedgeInfo = ledgeInfo;
                PlayerLedgeMovement();
            }
        
            animator.SetFloat("movementValue", velocity.magnitude / movementSpeed, dampTime, Time.deltaTime);
        }
        else
        {
            fallingSpeed += Physics.gravity.y * Time.deltaTime;
            
            velocity = transform.forward * movementSpeed / 2;
        }
        velocity.y = fallingSpeed;
        
        PlayerMovement();
        SurfaceCheck();
        animator.SetBool("onSurface", onSurface);
    }

    /// <summary>
    /// 处理玩家移动逻辑，包括输入获取、方向计算和角色移动
    /// </summary>
    private void PlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        float movementAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

        var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;

        requiredMoveDir = MCC.flatRotation * movementInput;
        
        
        CC.Move(velocity * Time.deltaTime);
        
        if (movementAmount > 0 && moveDir != Vector3.zero && moveDir.magnitude > 0.2f)
        {
            requiredRotation = Quaternion.LookRotation(moveDir);
        }
        moveDir = requiredMoveDir;  

        transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 检测玩家是否站在地面上
    /// </summary>
    private void SurfaceCheck()
    {
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius, surfaceLayer);
    }

    /// <summary>
    /// 处理玩家在悬崖边缘时的移动限制
    /// </summary>
    private void PlayerLedgeMovement()
    {
        float angle = Vector3.Angle(LedgeInfo.surfaceHit.normal, requiredMoveDir);

        if (angle < 90)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
    }

    /// <summary>
    /// 在编辑器中绘制选中时的辅助线，显示地面检测球体
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius);
    }
    
    /// <summary>
    /// 执行指定的动画动作协程
    /// </summary>
    /// <param name="animationName">要播放的动画名称</param>
    /// <param name="ctp">匹配目标参数，用于动画中的位置匹配</param>
    /// <param name="requiredRotation">需要旋转到的目标角度</param>
    /// <param name="lookAtObstacle">是否看向障碍物</param>
    /// <param name="parkourActionDelay">跑酷动作延迟时间</param>
    /// <returns>协程枚举器</returns>
    public IEnumerator PerformAction(string animationName, CompareTargetParameter ctp = null, Quaternion requiredRotation = new Quaternion(),
        bool lookAtObstacle = false, float parkourActionDelay = 0f)
    {
        playerInAction = true;

        animator.CrossFadeInFixedTime(animationName, 0.2f);
        yield return null;

        yield return new WaitUntil(() => !animator.IsInTransition(0));

        yield return null;

        var animationState = animator.GetCurrentAnimatorStateInfo(0);
        
        if (!animationState.IsName(animationName))
        {
            Debug.LogWarning($"[Action] Cancelled, state mismatch. wanted={animationName}");
            playerInAction = false;
            yield break;
        }

        float animLength = animationState.length;
        float rotateStartTime = ctp != null ? ctp.startTime : 0f;
        float timerCounter = 0f;

        while (timerCounter < animLength)
        {
            timerCounter += Time.deltaTime;

            float normalizedTimerCounter = timerCounter / animationState.length;

            if (lookAtObstacle && normalizedTimerCounter > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);
            }

            if (ctp != null && 
                !animator.IsInTransition(0) && 
                animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                CompareTarget(ctp);
            }

            if (animator.IsInTransition(0) && timerCounter > 0.5f)
            {
                break;
            }

            yield return null;
        }
        
        yield return new WaitForSeconds(parkourActionDelay);

        playerInAction = false;
    }

    /// <summary>
    /// 执行动画目标匹配操作
    /// </summary>
    /// <param name="compareTargetParameter">目标匹配参数</param>
    private void CompareTarget(CompareTargetParameter compareTargetParameter)
    {
        animator.MatchTarget(
            compareTargetParameter.position,
            transform.rotation,
            compareTargetParameter.bodyPart,
            new MatchTargetWeightMask(compareTargetParameter.positionWeight, 0),
            compareTargetParameter.startTime,
            compareTargetParameter.endTime);
    }

    /// <summary>
    /// 设置玩家控制权限
    /// </summary>
    /// <param name="hasControl">是否有控制权限</param>
    public void SetControl(bool hasControl)
    {
        PC = hasControl;
        CC.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("movementValue", 0f);
            requiredRotation = transform.rotation;
        }
    }

    /// <summary>
    /// 启用或禁用角色控制器
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void EnableCC(bool enabled)
    {
        CC.enabled = enabled;
    }
    
    /// <summary>
    /// 重置目标旋转角度为当前旋转角度
    /// </summary>
    public void ResetRequiredRotation()
    {
        requiredRotation = transform.rotation;
    }

    /// <summary>
    /// 获取或设置玩家控制权限
    /// </summary>
    public bool HasPlayerControl
    {
        get => PC;
        set => SetControl(value);
    }
}

/// <summary>
/// 目标匹配参数类，用于动画中的位置匹配功能
/// </summary>
public class CompareTargetParameter
{
    public Vector3 position;                            // 匹配位置
    public AvatarTarget bodyPart;                       // 身体部位
    public Vector3 positionWeight;                      // 位置权重
    public float startTime;                             // 开始时间
    public float endTime;                               // 结束时间
}
