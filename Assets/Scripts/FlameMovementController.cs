using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameMovementController : MonoBehaviour
{
    public float maxMoveSpeed = 5f;
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
    private float rayLength = 0.1f;

    private RaycastHit2D lastRayHitResult = new RaycastHit2D();

    void Start()
    {
        colliderSizeX = GetComponent<BoxCollider2D>().size.x / 2f;
        colliderSizeY = GetComponent<BoxCollider2D>().size.y / 2f;
    }

    void Update()
    {
        bool canJump = false;

        horizontalMovementRaw = Input.GetAxisRaw("Horizontal"); // Get the horizontal movement
        verticalMovementRaw = Input.GetAxisRaw("Vertical"); // Get the vertial movement

        if (horizontalMovementRaw != 0) // Player is inputing movement commands
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

        if (verticalMovementRaw >= 0 && isOnGround && canJump)
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
        if(!platformDrop && verticalMovementRaw < 0 && isOnGround && canJump)
        {
            StartCoroutine(DropThroughPlatform());
        }

        // Clamp the velocity
        velocity.x = Mathf.Clamp(velocity.x, -maxMoveSpeed, maxMoveSpeed);
        // velocity.y = Mathf.Clamp(velocity.y, -maxMoveSpeed, maxMoveSpeed);
    }

    void FixedUpdate()
    {
        transform.position += velocity * Time.fixedDeltaTime; // Move the charactedir, ColliderSize, maskr;

        // Mmmm Gravity
        if (!isOnGround)
        {
            if (jumpCanceled)
            {
                velocity.y += gravity * Time.fixedDeltaTime * jumpCanceledGravityMultiplier;

            }
            else
            {
                velocity.y += gravity * Time.fixedDeltaTime;
            }
        }

        // Check collisions
        CollisionDetectionDown(!platformDrop);
        CollisionDetectionWalls();
        CollisionDetectionCeiling();
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
        
        if (jumping && !buttonDown)
        {
            jumping = false;
            jumpCanceled = true;
        }

        if (isOnGround && buttonDown && !jumping)
        {
            jumping = true;
            jumpCanceled = false;
            return true;
        }

        return false;
    }

    private bool Raycast(Vector2 origin, Vector2 direction, float diststance, LayerMask mask)
    {
        lastRayHitResult = Physics2D.Raycast(origin, direction, diststance, mask);
        Debug.DrawRay(origin, direction, Color.red, Time.deltaTime);
        
        if (lastRayHitResult.collider != null)
        {
            return true;
        }

        return false;
    }

    private void CollisionDetectionDown(bool checkAll = true)
    {
        bool collisionDetected = false;

        if (velocity.y <= 0)
        {
            if (checkAll)
            {
                Vector2 origin = new Vector2(transform.position.x, transform.position.y - colliderSizeY + rayLength);
                collisionDetected = Raycast(origin,  -transform.up, rayLength, (wallsMask | ceilingsMask | platformsMask));
            }
            else
            {
                Vector2 origin = new Vector2(transform.position.x, transform.position.y - colliderSizeY + rayLength);
                collisionDetected = Raycast(origin, -transform.up, rayLength, (wallsMask | ceilingsMask));
            }

        }

        if (collisionDetected)
        {
            transform.position = new Vector3(transform.position.x, lastRayHitResult.point.y + colliderSizeY, transform.position.z);
            velocity.y = 0;
            isOnGround = true;
        }
        else
        {
            isOnGround = false;
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
                transform.position = new Vector3(lastRayHitResult.point.x + colliderSizeX, transform.position.y, transform.position.z);
                velocity.x = 0;
            }
        }
        else if (velocity.x > 0) // Check right if we are moving right
        {
            Vector2 origin = new Vector2(transform.position.x + colliderSizeX - rayLength, transform.position.y);
            collisionDetected = Raycast(origin, transform.right, rayLength, (wallsMask));

            if (collisionDetected)
            {
                transform.position = new Vector3(lastRayHitResult.point.x - colliderSizeX, transform.position.y, transform.position.z);
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
            transform.position = new Vector3(transform.position.x, lastRayHitResult.point.y - colliderSizeY, transform.position.z);
            velocity.y = 0;
        }
    }

}
