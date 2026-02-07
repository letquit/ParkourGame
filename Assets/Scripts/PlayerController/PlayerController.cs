using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 4f;

    private void Update()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;
        
        transform.position += movementInput * movementSpeed * Time.deltaTime;
    }
}
