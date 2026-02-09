using System;
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

    [Header("Player Animator")] 
    public Animator animator;

    [Header("Player Collision & Gravity")] 
    public CharacterController CC;
    public float surfaceCheckRadius = 0.3f;
    public Vector3 surfaceCheckOffset;
    public LayerMask surfaceLayer;
    private bool onSurface;
    public bool playerOnLedge { get; set; }
    public LedgeInfo LedgeInfo { get; set; }
    [SerializeField] public float fallingSpeed;
    [SerializeField] public Vector3 moveDir;
    [SerializeField] private Vector3 requiredMoveDir;
    private Vector3 velocity;
    
    private void Update()
    {
        if (PC && CC.enabled)
            PlayerMovement();
        if (!PC)
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

    public bool HasPlayerControl
    {
        get => PC;
        set => SetControl(value);
    }
}
