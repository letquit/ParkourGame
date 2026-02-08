using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 5f;
    public float dampTime = 0.02f;
    public MainCameraController MCC;
    public float rotSpeed = 600f;
    private Quaternion requiredRotation;

    [Header("Player Animator")] 
    public Animator animator;

    [Header("Player Collision & Gravity")] 
    public CharacterController CC;
    public float surfaceCheckRadius = 0.3f;
    public Vector3 surfaceCheckOffset;
    public LayerMask surfaceLayer;
    private bool onSurface;
    [SerializeField] private float fallingSpeed;
    [SerializeField] private Vector3 moveDir;

    private void Update()
    {
        if (onSurface)
        {
            fallingSpeed = 0f;
        }
        else
        {
            fallingSpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * movementSpeed;
        velocity.y = fallingSpeed;
        
        PlayerMovement();
        SurfaceCheck();
    }

    private void PlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        float movementAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

        var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;

        var movementDirection = MCC.flatRotation * movementInput;
        
        
        CC.Move(movementDirection * movementSpeed * Time.deltaTime);
        
        if (movementAmount > 0)
        {
            requiredRotation = Quaternion.LookRotation(movementDirection);
        }

        movementDirection = moveDir;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);
        
        animator.SetFloat("movementValue", movementAmount, dampTime, Time.deltaTime);
    }

    private void SurfaceCheck()
    {
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius, surfaceLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius);
    }
}
