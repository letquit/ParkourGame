using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 5f;
    public float dampTime = 0.02f;
    public MainCameraController MCC;
    public EnvironmentChecker environmentChecker;
    public float rotSpeed = 600f;
    private Quaternion requiredRotation;
    private bool PC = true;
    public bool playerInAction { get; private set; }

    [Header("Player Animator")] 
    public Animator animator;

    [Header("Player Collision & Gravity")] 
    public CharacterController CC;
    public float surfaceCheckRadius = 0.3f;
    public Vector3 surfaceCheckOffset;
    public LayerMask surfaceLayer;
    private bool onSurface;
    public bool playerOnLedge { get; set; }
    public bool playerHanging { get; set; }
    public LedgeInfo LedgeInfo { get; set; }
    [SerializeField] public float fallingSpeed;
    [SerializeField] public Vector3 moveDir;
    [SerializeField] private Vector3 requiredMoveDir;
    private Vector3 velocity;
    
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

    private void SurfaceCheck()
    {
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius, surfaceLayer);
    }

    private void PlayerLedgeMovement()
    {
        float angle = Vector3.Angle(LedgeInfo.surfaceHit.normal, requiredMoveDir);

        if (angle < 90)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius);
    }
    
    public IEnumerator PerformAction(string animationName, CompareTargetParameter ctp = null, Quaternion requiredRotation = new Quaternion(),
        bool lookAtObstacle = false, float parkourActionDelay = 0f)
    {
        playerInAction = true;

        animator.CrossFadeInFixedTime(animationName, 0.2f);
        yield return null; // 等待 CrossFade 触发

        yield return new WaitUntil(() => !animator.IsInTransition(0));

        yield return null;

        var animationState = animator.GetCurrentAnimatorStateInfo(0);
        if (!animationState.IsName(animationName))
        {
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

    public void ResetRequiredRotation()
    {
        requiredRotation = transform.rotation;
    }

    public bool HasPlayerControl
    {
        get => PC;
        set => SetControl(value);
    }
}

public class CompareTargetParameter
{
    public Vector3 position;
    public AvatarTarget bodyPart;
    public Vector3 positionWeight;
    public float startTime;
    public float endTime;
}
