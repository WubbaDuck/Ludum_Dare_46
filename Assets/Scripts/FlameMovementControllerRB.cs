using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class FlameMovementControllerRB : MonoBehaviour
{
    public float maxMoveSpeed = 5f;
    public float maxVerticalSpeed = 25f;
    public float acceleration = 10f;
    public float slideAcceleration = 20f;
    public float gravity = -9.81f;
    public float jumpStrength = 6f;
    public float jumpCanceledGravityMultiplier = 6f;

    public LayerMask wallsMask;
    public LayerMask ceilingsMask;
    public LayerMask platformsMask;

    private Rigidbody2D rb;

    private float horizontalMovementRaw;
    private float verticalMovementRaw;
    private Vector2 newMovement;
    private Vector3 velocity;
    private bool jumping = false;
    private bool isOnGround = false;
    private bool jumpCanceled = false;
    private float colliderSizeX;
    private float colliderSizeY;
    private bool platformDrop = false;
    private float platformDropCooldown = 0.2f;
    private float rayLength = 0.2f;

    private Stopwatch stopwatch;

    private float jumpButtonTimerWindow = 0.2f;
    private float isGroundedTimerWindow = 0.15f;
    private float currentJumpButtonTimer = 0f;
    private float currentIsGroundedTimer = 0f;

    private RaycastHit2D lastRayHitResult = new RaycastHit2D();

    private FuelHandler fuelHandler;
    private FlameAudioHandler flameAudioHandler;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colliderSizeX = GetComponent<CapsuleCollider2D>().size.x / 2f;
        colliderSizeY = GetComponent<CapsuleCollider2D>().size.y / 2f;
        fuelHandler = GetComponent<FuelHandler>();
        flameAudioHandler = GetComponent<FlameAudioHandler>();
        stopwatch = new Stopwatch();
    }

    void Update()
    {
        bool canJump = false;
        currentJumpButtonTimer -= Time.deltaTime;
        currentIsGroundedTimer -= Time.deltaTime;

        horizontalMovementRaw = Input.GetAxisRaw("Horizontal"); // Get the horizontal movement
        verticalMovementRaw = Input.GetAxisRaw("Vertical"); // Get the vertial movement

        if (horizontalMovementRaw != 0 && !fuelHandler.IsKicking()) // Player is inputing movement commands
        {
            if (velocity.x == 0 || Mathf.Sign(horizontalMovementRaw) == Mathf.Sign(velocity.x))
            {
                velocity.x += horizontalMovementRaw * acceleration * Time.deltaTime;
            }
            else
            {
                velocity.x += horizontalMovementRaw * slideAcceleration * Time.deltaTime;
            }
        }
        else if (velocity.x != 0) // Not movement commands but the character is still moving
        {
            velocity.x -= Mathf.Sign(velocity.x) * acceleration * Time.deltaTime;

            // Zero the velocity if close enough to zero
            if (velocity.x < 0.1f && velocity.x > -0.1f)
            {
                velocity.x = 0;
            }
        }

        // Jumping
        canJump = GetJumpAvailability();

        if (verticalMovementRaw >= 0 && (currentIsGroundedTimer > 0) && canJump)
        {
            isOnGround = false;
            velocity.y = jumpStrength;
        }

        // Cancel jump if falling
        if (Mathf.Sign(velocity.y) < 0)
        {
            jumpCanceled = false;
        }

        // Handle platform dropdown
        if (!platformDrop && verticalMovementRaw < 0 && isOnGround && canJump)
        {
            StartCoroutine(DropThroughPlatform());
        }

        // Clamp the velocity
        velocity.x = Mathf.Clamp(velocity.x, -maxMoveSpeed, maxMoveSpeed);
        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalSpeed, maxVerticalSpeed);

        rb.transform.position += velocity * Time.deltaTime; // Move the charactedir, ColliderSize, maskr;

        // Mmmm Gravity
        if (!isOnGround)
        {
            if (jumpCanceled)
            {
                velocity.y += gravity * Time.deltaTime * jumpCanceledGravityMultiplier;

            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        // Check collisions
        // CollisionDetectionWalls();
        // CollisionDetectionDown(!platformDrop);
        // CollisionDetectionCeiling();
        // UnityEngine.Debug.Log(isOnGround);
    }

    private IEnumerator DropThroughPlatform()
    {
        platformDrop = true;
        isOnGround = false;
        yield return new WaitForSeconds(platformDropCooldown);
        platformDrop = false;
    }

    private bool GetJumpAvailability()
    {
        bool buttonDown = Input.GetButton("Jump");

        if (buttonDown)
        {
            currentJumpButtonTimer = jumpButtonTimerWindow;
        }
        else
        {
            currentJumpButtonTimer = 0;
        }

        if (isOnGround)
        {
            currentIsGroundedTimer = isGroundedTimerWindow;
        }

        if (jumping && !(currentJumpButtonTimer > 0))
        {
            jumping = false;
            jumpCanceled = true;
        }

        if ((currentJumpButtonTimer > 0) && (currentIsGroundedTimer > 0) && !jumping)
        {
            jumping = true;
            jumpCanceled = false;
            flameAudioHandler.PlaySound_Jump();
            return true;
        }

        return false;
    }

    private bool Raycast(Vector2 origin, Vector2 direction, float diststance, LayerMask mask)
    {
        lastRayHitResult = Physics2D.Raycast(origin, direction, diststance, mask);
        // UnityEngine.Debug.DrawRay(origin, direction, Color.red, Time.deltaTime);

        if (lastRayHitResult.collider != null)
        {
            return true;
        }

        return false;
    }

    private void CollisionDetectionDown(bool checkAll = true)
    {
        bool collisionDetected = false;

        Vector2 origin = new Vector2(rb.transform.position.x, rb.transform.position.y - colliderSizeY + rayLength);;

        if (velocity.y <= 0)
        {
            if (checkAll)
            {
                collisionDetected = Raycast(origin, -transform.up, rayLength + 0.1f, (wallsMask | ceilingsMask | platformsMask));
            }
            else
            {
                collisionDetected = Raycast(origin, -transform.up, rayLength + 0.1f, (wallsMask | ceilingsMask));
            }
        }

        if (collisionDetected)
        {
            rb.transform.position = new Vector3(rb.transform.position.x, lastRayHitResult.point.y + colliderSizeY, rb.transform.position.z);
            velocity.y = 0;
            isOnGround = true;
        }
        else
        {
            isOnGround = false;
            stopwatch.Start();
        }
    }

    private void CollisionDetectionWalls()
    {
        bool collisionDetected = false;

        if (velocity.x < 0) // Check left if we are moving left
        {
            Vector2 origin = new Vector2(rb.transform.position.x - colliderSizeX + rayLength, rb.transform.position.y);
            collisionDetected = Raycast(origin, -transform.right, rayLength, (wallsMask));

            if (collisionDetected)
            {
                rb.transform.position = new Vector3(lastRayHitResult.point.x + colliderSizeX, rb.transform.position.y, rb.transform.position.z);
                velocity.x = 0;
            }
        }
        else if (velocity.x > 0) // Check right if we are moving right
        {
            Vector2 origin = new Vector2(rb.transform.position.x + colliderSizeX - rayLength, rb.transform.position.y);
            collisionDetected = Raycast(origin, transform.right, rayLength, (wallsMask));

            if (collisionDetected)
            {
                rb.transform.position = new Vector3(lastRayHitResult.point.x - colliderSizeX, rb.transform.position.y, rb.transform.position.z);
                velocity.x = 0;
            }
        }
    }

    private void CollisionDetectionCeiling()
    {
        bool collisionDetected = false;

        if (velocity.y >= 0)
        {
            Vector2 origin = new Vector2(rb.transform.position.x, rb.transform.position.y + colliderSizeY - rayLength);
            collisionDetected = Raycast(origin, transform.up, rayLength, (ceilingsMask));
        }

        if (collisionDetected)
        {
            rb.transform.position = new Vector3(rb.transform.position.x, lastRayHitResult.point.y - colliderSizeY, rb.transform.position.z);
            velocity.y = 0;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider.IsTouchingLayers(LayerMask.GetMask("Platforms", "Ceilings", "Walls")))
        {
            if (!platformDrop)
            {
                UnityEngine.Debug.Log(collision.contacts.Length);
                rb.transform.position = new Vector3(rb.transform.position.x, collision.GetContact(0).point.y + colliderSizeY, rb.transform.position.z);
                velocity.y = 0;
                isOnGround = true;
            }
            else
            {
                isOnGround = false;
                stopwatch.Start();
            }
        }
    }

}
