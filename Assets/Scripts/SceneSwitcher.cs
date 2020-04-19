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

    // Start is called before the first frame update
    void Start()
    {
        flameLight = flame.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(flameLight.pointLightOuterRadius <= 0)
        {
            SceneManager.LoadScene(loseSceneName);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)));
        if(LayerMask.GetMask(LayerMask.LayerToName(col.gameObject.layer)) == winGateMask.value)
        {
            SceneManager.LoadScene(winSceneName);
        }
    }
}
