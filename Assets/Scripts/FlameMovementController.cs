using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class FlameMovementController : MonoBehaviour
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

    private Vector2 lastRayHitPoint = new Vector2();

    private FuelHandler fuelHandler;
    private FlameAudioHandler flameAudioHandler;

    void Start()
    {
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

        if (fuelHandler.GetCurrentFuelLevel() > 0)
        {
            horizontalMovementRaw = Input.GetAxisRaw("Horizontal"); // Get the horizontal movement
            verticalMovementRaw = Input.GetAxisRaw("Vertical"); // Get the vertial movement
        }
        else
        {
            horizontalMovementRaw = 0;
            verticalMovementRaw = 0;
        }

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

        transform.position += velocity * Time.deltaTime; // Move the charactedir, ColliderSize, maskr;

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
        CollisionDetectionWalls();
        CollisionDetectionDown(!platformDrop);
        CollisionDetectionCeiling();

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
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
        bool buttonDown = false;
        bool buttonHeld = false;

        if (fuelHandler.GetCurrentFuelLevel() > 0)
        {
            buttonDown = Input.GetButtonDown("Jump");
            buttonHeld = Input.GetButton("Jump");
        }
        if (buttonDown)
        {
            currentJumpButtonTimer = jumpButtonTimerWindow;
        }

        if (!buttonHeld)
        {
            currentJumpButtonTimer = 0;
        }

        if (isOnGround)
        {
            currentIsGroundedTimer = isGroundedTimerWindow;
        }

        if (jumping && !(currentJumpButtonTimer > 0) && !buttonHeld)
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

    public bool Raycast(Vector2 origin, Vector2 direction, float distance, LayerMask mask)
    {
        Vector2 origin1 = origin;
        Vector2 origin2 = origin;
        Vector2 origin3 = origin;

        RaycastHit2D ray1 = new RaycastHit2D();
        RaycastHit2D ray2 = new RaycastHit2D();
        RaycastHit2D ray3 = new RaycastHit2D();

        if (direction.Equals(-transform.up)) // Down
        {
            origin1.x = origin1.x + (colliderSizeX / 2);
            origin3.x = origin3.x - (colliderSizeX / 2);
        }
        else if (direction.Equals(transform.up)) // Up
        {
            origin1.x = origin1.x - (colliderSizeX / 2);
            origin3.x = origin3.x + (colliderSizeX / 2);
        }
        else if (direction.Equals(transform.right)) // Right
        {
            origin1.y = origin1.y - (colliderSizeY / 2);
            origin3.y = origin3.y + (colliderSizeY / 2);
        }
        else if (direction.Equals(-transform.right)) // Left
        {
            origin1.y = origin1.y + (colliderSizeY / 2);
            origin3.y = origin3.y - (colliderSizeY / 2);
        }

        // lastRayHitPoint Physics2D.Raycast(origin1, direction, distance, mask);
        ray1 = Physics2D.Raycast(origin1, direction, distance, mask);
        ray2 = Physics2D.Raycast(origin2, direction, distance, mask);
        ray3 = Physics2D.Raycast(origin3, direction, distance, mask);

        UnityEngine.Debug.DrawRay(origin1, direction, Color.red, Time.deltaTime);
        UnityEngine.Debug.DrawRay(origin2, direction, Color.red, Time.deltaTime);
        UnityEngine.Debug.DrawRay(origin3, direction, Color.red, Time.deltaTime);

        if (ray2.collider != null)
        {
            lastRayHitPoint = ray2.point;
            return true;
        }
        else if (ray1.collider != null)
        {
            lastRayHitPoint = ray1.point;
            return true;
        }
        else if (ray3.collider != null)
        {
            lastRayHitPoint = ray3.point;
            return true;
        }

        return false;
    }

    private void CollisionDetectionDown(bool checkAll = true)
    {
        bool collisionDetected = false;

        Vector2 origin = new Vector2(transform.position.x, transform.position.y - colliderSizeY + rayLength);;

        if (velocity.y <= 0)
        {
            if (checkAll)
            {
                collisionDetected = Raycast(origin, -transform.up, rayLength + 0.05f, (wallsMask | ceilingsMask | platformsMask));
            }
            else
            {
                collisionDetected = Raycast(origin, -transform.up, rayLength + 0.05f, (wallsMask | ceilingsMask));
            }
        }

        if (collisionDetected)
        {
            transform.position = new Vector3(transform.position.x, lastRayHitPoint.y + colliderSizeY, transform.position.z);
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
            Vector2 origin = new Vector2(transform.position.x - colliderSizeX + rayLength, transform.position.y);
            collisionDetected = Raycast(origin, -transform.right, rayLength, (wallsMask));

            if (collisionDetected)
            {
                transform.position = new Vector3(lastRayHitPoint.x + colliderSizeX, transform.position.y, transform.position.z);
                velocity.x = 0;
            }
        }
        else if (velocity.x > 0) // Check right if we are moving right
        {
            Vector2 origin = new Vector2(transform.position.x + colliderSizeX - rayLength, transform.position.y);
            collisionDetected = Raycast(origin, transform.right, rayLength, (wallsMask));

            if (collisionDetected)
            {
                transform.position = new Vector3(lastRayHitPoint.x - colliderSizeX, transform.position.y, transform.position.z);
                velocity.x = 0;
            }
        }
    }

    private void CollisionDetectionCeiling()
    {
        bool collisionDetected = false;

        if (velocity.y >= 0)
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y + colliderSizeY - rayLength);
            collisionDetected = Raycast(origin, transform.up, rayLength, (ceilingsMask));
        }

        if (collisionDetected)
        {
            transform.position = new Vector3(transform.position.x, lastRayHitPoint.y - colliderSizeY, transform.position.z);
            velocity.y = 0;
        }
    }

}
