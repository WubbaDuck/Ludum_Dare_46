using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameFlicker : MonoBehaviour
{
    public float flickerRate = 0.2f;
    private UnityEngine.Experimental.Rendering.Universal.Light2D flameLight;
    private bool flickerTick = false;
    private float baseIntensity;
    private FuelHandler fuelHandler;

    void Start()
    {
        flameLight = GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        fuelHandler = GetComponent<FuelHandler>();
        baseIntensity = flameLight.intensity;
        InvokeRepeating("Flicker", 0, flickerRate);
        InvokeRepeating("UpdateOuterRadius", 0, flickerRate);
    }

    void Update()
    {
        if (fuelHandler.GetCurrentFuelLevel() > 0)
        {
            flameLight.pointLightOuterRadius -= fuelHandler.secondsPerFuelUsed * 10 * Time.deltaTime;
        }
    }

    void Flicker()
    {
        if (flickerTick)
        {
            float max = flameLight.intensity * 1.20f;
            float min = flameLight.intensity * 0.80f;
            flameLight.intensity = Random.Range(min, max);
            flickerTick = false;
        }
        else
        {
            flameLight.intensity = baseIntensity;
            flickerTick = true;
        }
    }

    void UpdateOuterRadius()
    {
        if (fuelHandler.GetCurrentFuelLevel() > 0)
        {
            flameLight.pointLightOuterRadius = (fuelHandler.GetCurrentFuelLevel() / 2);
        }
        else
        {
            flameLight.pointLightOuterRadius = 0;
        }
    }
}
