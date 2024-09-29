using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller; // Reference to CharacterController2D script
    public Animator animator; // Reference to Animator

    public float runSpeed = 40f; // Running speed

    float horizontalMove = 0f; // Horizontal movement input
    bool jump = false; // Jump input
    bool crouch = false; // Crouch input
    bool isDashing = false; // Dash input

    private void Update()
    {
        // Get horizontal movement (left/right)
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        // Update the speed parameter for the animator
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        // Check for jump input
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("IsJumping", true); // Set jumping animation
        }

        // Check for crouch input
        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        // Check for dash input (for example, using key X)
        if (Input.GetKeyDown(KeyCode.X) && !isDashing)
        {
            StartCoroutine(HandleDash());
        }
    }

    public void OnLanding()
    {
        // Reset jumping animation when landing
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        // Set crouching animation based on whether the player is crouching
        animator.SetBool("IsCrouching", isCrouching);
    }

    private void FixedUpdate()
    {
        // Move the character
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false; // Reset jump after execution
    }

    private IEnumerator HandleDash()
    {
        isDashing = true;

        // Trigger dash animation
        animator.SetTrigger("Dash");

        // Start the Dash function in CharacterController2D
        yield return StartCoroutine(controller.Dash());

        isDashing = false;
    }
}
