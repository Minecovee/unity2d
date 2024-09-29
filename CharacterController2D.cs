using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
    // Dashing Variables
    [SerializeField] private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer dashTrail; // Dash trail effect

    // Movement variables
    [SerializeField] private float m_JumpForce = 400f; // Jump force
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f; // Crouch speed percentage
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f; // Movement smoothing factor
    [SerializeField] private bool m_AirControl = false; // Air control on/off
    [SerializeField] private LayerMask m_WhatIsGround; // Ground layer
    [SerializeField] private Transform m_GroundCheck; // Ground check transform
    [SerializeField] private Transform m_CeilingCheck; // Ceiling check transform
    [SerializeField] private Collider2D m_CrouchDisableCollider; // Collider disabled when crouching

    const float k_GroundedRadius = .2f; // Grounded radius for overlap circle
    private bool m_Grounded; // Whether or not the player is grounded
    const float k_CeilingRadius = .2f; // Ceiling radius for overlap circle
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true; // To check player's facing direction
    private Vector3 m_Velocity = Vector3.zero;

    // Events
    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;
    public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    private void Update()
    {
        if (isDashing)
            return;

        // Check for dash input
        if (Input.GetKeyDown(KeyCode.X) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // Ground check: determine if player is grounded by using a circle cast
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    public void Move(float move, bool crouch, bool jump)
    {
        if (!crouch)
        {
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        if (m_Grounded || m_AirControl)
        {
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                move *= m_CrouchSpeed;

                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move character
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // Flip player when changing directions
            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }
        }

        // Jump
        if (m_Grounded && jump)
        {
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }

    private void Flip()
    {
        // Reverse the player's facing direction
        m_FacingRight = !m_FacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        // Disable gravity during dash
        float originalGravity = m_Rigidbody2D.gravityScale;
        m_Rigidbody2D.gravityScale = 0f;

        // Apply dash velocity
        m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);

        // Enable dash trail
        if (dashTrail != null)
            dashTrail.emitting = true;

        // Wait for dash to finish
        yield return new WaitForSeconds(dashingTime);

        // Disable dash trail
        if (dashTrail != null)
            dashTrail.emitting = false;

        // Restore gravity
        m_Rigidbody2D.gravityScale = originalGravity;

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        isDashing = false;
    }
}
