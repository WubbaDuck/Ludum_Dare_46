using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelHandler : MonoBehaviour
{
    public float secondsPerFuelUsed = 1f;
    public LayerMask fuelMask;
    public LayerMask damageMask;
    public LayerMask wallsMask;

    private float fuelLevel = 15f;
    private FlameAudioHandler flameAudioHandler;

    private float kickDistance = 1f;
    private float currentKickDistance = 0f;
    private float kickSpeed = 10f;

    private bool collisionDetectedRight = false;
    private bool collisionDetectedLeft = false;
    private float colliderSizeX;
    private float rayLength = 0.3f;
    private FlameMovementController flameMovementController;

    void Start()
    {
        flameMovementController = GetComponent<FlameMovementController>();
        flameAudioHandler = GetComponent<FlameAudioHandler>();
        colliderSizeX = GetComponent<CapsuleCollider2D>().size.x / 2f;
        InvokeRepeating("BurnFuel", secondsPerFuelUsed * 3f, secondsPerFuelUsed);
    }

    void Update()
    {
        Vector2 origin = new Vector2(transform.position.x - colliderSizeX + rayLength, transform.position.y);
        collisionDetectedRight = flameMovementController.Raycast(origin, transform.right, rayLength, (wallsMask));
        collisionDetectedLeft = flameMovementController.Raycast(origin, -transform.right, rayLength, (wallsMask));

        if (!collisionDetectedLeft && !collisionDetectedRight)
        {
            if (Mathf.Abs(currentKickDistance) > 0f)
            {
                if (currentKickDistance > 0)
                {
                    transform.position = new Vector2(transform.position.x - kickDistance * kickSpeed * Time.deltaTime, transform.position.y);
                    currentKickDistance -= kickDistance * kickSpeed * Time.deltaTime;
                }
                else if (currentKickDistance < 0)
                {
                    transform.position = new Vector2(transform.position.x + kickDistance * kickSpeed * Time.deltaTime, transform.position.y);
                    currentKickDistance += kickDistance * kickSpeed * Time.deltaTime;
                }

                if (currentKickDistance > -0.1 && currentKickDistance < 0.1)
                {
                    currentKickDistance = 0;
                }
            }
        }
        else
        {
            currentKickDistance = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)) == fuelMask.value)
        {
            fuelLevel += col.gameObject.GetComponent<FuelPickup>().fuelAmount;
            flameAudioHandler.PlaySound_Slosh();
            Destroy(col.gameObject);
        }
        else if (LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)) == damageMask.value)
        {
            fuelLevel -= col.gameObject.GetComponent<DamagePickup>().damageAmount;
            flameAudioHandler.PlaySound_Sizzle();
            Destroy(col.gameObject);

            // Kick flame
            if (transform.position.x <= col.gameObject.transform.position.x) // Kick left
            {
                currentKickDistance = kickDistance;
            }
            else if (transform.position.x > col.gameObject.transform.position.x) // Kick Right
            {
                currentKickDistance = kickDistance * -1;
            }
        }

        // Debug.Log(fuelLevel);
    }

    private void BurnFuel()
    {
        if (fuelLevel > 0)
        {
            fuelLevel--;
        }
    }

    private void BurnOut()
    {
        Destroy(gameObject);
    }

    public float GetCurrentFuelLevel()
    {
        return fuelLevel;
    }

    public bool IsKicking()
    {
        return currentKickDistance != 0;
    }
}
