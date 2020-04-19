using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuFlameFlicker : MonoBehaviour
{
    public float flickerRate = 0.2f;
    private UnityEngine.Experimental.Rendering.Universal.Light2D flameLight;
    private bool flickerTick = false;
    private float baseIntensity;
    private FuelHandler fuelHandler;

    void Start()
    {
        flameLight = GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        baseIntensity = flameLight.intensity;
        InvokeRepeating("Flicker", 0, flickerRate);
    }

    void Flicker()
    {
        if(flickerTick)
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
        // flameLight.pointLightOuterRadius = fuelHandler.GetCurrentFuelLevel()/2;
    }
}
