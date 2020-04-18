using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelHandler : MonoBehaviour
{
    public float secondsPerFuelUsed = 1f;
    public LayerMask fuelMask;
    public LayerMask damageMask;

    private float fuelLevel = 25f;
    
    void Start()
    {
        InvokeRepeating("BurnFuel", secondsPerFuelUsed * 3f, secondsPerFuelUsed);
    }

    void Update()
    {
        // Check for burnout
        if(fuelLevel <= 0)
        {
            BurnOut();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)) == fuelMask.value)
        {
            fuelLevel += col.gameObject.GetComponent<FuelPickup>().fuelAmount;
            Destroy(col.gameObject);
        }
        else if(LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)) == damageMask.value)
        {
            fuelLevel -= col.gameObject.GetComponent<FuelPickup>().fuelAmount;
            Destroy(col.gameObject);
        }
        Debug.Log(fuelLevel);
    }

    private void BurnFuel()
    {
        fuelLevel--;
        Debug.Log(fuelLevel);
    }

    private void BurnOut()
    {
        Destroy(gameObject);
    }

    public float GetCurrentFuelLevel()
    {
        return fuelLevel;
    }
}
