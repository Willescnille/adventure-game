using System;
using System.Collections;
using System.Xml.Schema;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    [SerializeField] private LayerMask JumpableGround;
    [SerializeField] private LayerMask WallLayer;
    [SerializeField] private Transform WallCheckPoint;
    [SerializeField] private Vector2 WallCheckSize;

    private float dirx = 0f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpheight = 14f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 14f);
    [SerializeField] private float wallSlideSpeed = 2f;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool facingRight = true;

    private bool doubleJump;
    public bool havedoublejump = false;

    private bool CanDash = true;
    private bool IsDashing;
    [SerializeField] private float DashingPower = 1f;
    [SerializeField] private float DashingTime = 0.2f;
    [SerializeField] private float DashingCooldown = 1f;
    private Vector2 dashingDir;
    
    private enum MovementState { idle, Running, Jumping, Falling, DoubleJump, WallSliding, WallJumping }

    [SerializeField] private AudioSource JumpSoundEffect;
    [SerializeField] private TrailRenderer tr;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        Debug.Log(isWallSliding);
        dirx = Input.GetAxisRaw("Horizontal");
        Move();

        isGrounded = IsGrounded();
        CheckWallSliding();
       
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isWallSliding)
            {
                WallJump();
            }
            else if (doubleJump)
            {
                DoubleJump();
            }
        }
        if (Input.GetButtonDown("Dash") && CanDash) 
        {
          IsDashing = true;
            CanDash = false;
            if (tr != null)
            {
                tr.emitting = true;
            }
            dashingDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (dashingDir == Vector2.zero)
            {
                dashingDir = new Vector2(transform.localScale.x, 0);
            }
           
            StartCoroutine(stopDashing());
           
        }
        if (IsDashing)
        {
            rb.velocity = dashingDir.normalized * DashingPower;
            return;
        }
        if(IsGrounded())
        {
            CanDash = true;
        }


        UpdateAnimationState();
    }
 
    private void Move()
    {
        if (!isWallSliding)
        {
            rb.velocity = new Vector2(dirx * moveSpeed, rb.velocity.y);

            if (dirx > 0 && !facingRight)
            {
                WallCheckPoint.localPosition = new Vector2(WallCheckPoint.localPosition.x * -1, WallCheckPoint.localPosition.y);

            }
            else if (dirx < 0 && facingRight)
            {
                WallCheckPoint.localPosition = new Vector2(WallCheckPoint.localPosition.x * -1, WallCheckPoint.localPosition.y);

            }

            if (dirx > 0)
            {

                sprite.flipX = false;
                facingRight = true;
            }
            else if (dirx < 0)
            {

                facingRight = false;
                sprite.flipX = true;
            }


        }
    }

    private void Jump()
    {
        JumpSoundEffect.Play();
        rb.velocity = new Vector2(rb.velocity.x, jumpheight);
        doubleJump = true; // Reset double jump when jumping off the ground
    }

    private void WallJump()
    {
        JumpSoundEffect.Play();
        int wallDirection = sprite.flipX ? 1 : -1; // Determine jump direction
        rb.velocity = new Vector2(wallJumpForce.x * wallDirection, wallJumpForce.y);
        isWallSliding = false;
    }

    private void CheckWallSliding()
    {
        if (facingRight)
        {
            isTouchingWall = Physics2D.OverlapBox(WallCheckPoint.position, WallCheckSize, 0f, WallLayer);
        }
        else
        {
            isTouchingWall = Physics2D.OverlapBox(WallCheckPoint.position, WallCheckSize, 0f, WallLayer);
        }

        if (isTouchingWall && !isGrounded )
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed); // Limit downward speed
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void DoubleJump()
    {
        if (havedoublejump)
        {
            JumpSoundEffect.Play();
            rb.velocity = new Vector2(rb.velocity.x, jumpheight);
            doubleJump = false; // Prevent additional double jumps

            
        }
    }

    private IEnumerator stopDashing() 
    {
    yield return new WaitForSeconds(DashingTime);
        tr.emitting = false;
        IsDashing = false;

        yield return new WaitForSeconds(DashingCooldown); // Wait for cooldown period
        CanDash = true;
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirx > 0f)
        {
            state = MovementState.Running;
        }
        else if (dirx < 0f)
        {
            state = MovementState.Running;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            if (doubleJump)
                state = MovementState.Jumping;
            else
                state = MovementState.DoubleJump;
        }
        else if (rb.velocity.y < -.1f)
        {
            if (isWallSliding)
                state = MovementState.WallSliding;
            else
                state = MovementState.Falling;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, JumpableGround);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DoubleJump"))
        {
            Destroy(collision.gameObject);
            havedoublejump = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(WallCheckPoint.position, WallCheckSize);
    }
}
