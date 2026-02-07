using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 4f;

    public MainCameraController MCC;
    public float rotSpeed = 600f;
    private Quaternion requiredRotation;

    private void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        float movementAmount = Mathf.Abs(horizontal) + Mathf.Abs(vertical);

        var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;

        var movementDirection = MCC.flatRotation * movementInput;
        
        if (movementAmount > 0)
        {
            transform.position += movementDirection * movementSpeed * Time.deltaTime;
            requiredRotation = Quaternion.LookRotation(movementDirection);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);
    }
}
