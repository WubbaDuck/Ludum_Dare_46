using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string winSceneName;
    public string loseSceneName;
    public GameObject flame;
    public LayerMask winGateMask;

    private UnityEngine.Experimental.Rendering.Universal.Light2D flameLight;
    private FuelHandler fuelHandler;

    // Start is called before the first frame update
    void Start()
    {
        flameLight = flame.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        fuelHandler = GetComponent<FuelHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if(flameLight.pointLightOuterRadius <= 0)
        {
            StartCoroutine("BurnOut");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)) == winGateMask.value)
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    private IEnumerator BurnOut()
    {
        yield return new WaitForSeconds(3);
        
        if(fuelHandler.GetCurrentFuelLevel() <= 0)
        {
            SceneManager.LoadScene(loseSceneName);
        }
    }
}
