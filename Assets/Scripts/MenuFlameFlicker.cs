using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFlameFlicker : MonoBehaviour
{
    public float flickerRate = 0.2f;
    public string nextSceneName;
    private UnityEngine.Experimental.Rendering.Universal.Light2D flameLight;
    private bool flickerTick = false;
    private float baseIntensity;
    private FuelHandler fuelHandler;
    private float fadeRate = 0f;
    
    void Start()
    {
        flameLight = GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        baseIntensity = flameLight.intensity;
        InvokeRepeating("Flicker", 0, flickerRate);
    }

    void Update()
    {
        flameLight.pointLightOuterRadius -= fadeRate * Time.deltaTime;
        
        if(flameLight.pointLightOuterRadius <= 0)
        {
            // Load next scene
            SceneManager.LoadScene(nextSceneName);
        }
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

    public void FadeOut()
    {
        fadeRate = 4f;
    }
}
