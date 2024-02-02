using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceEffectPop : MonoBehaviour
{
    [SerializeField] GameObject ice;
    GameObject AppearingEffect;
    bool isAppearing;
    float timer;

    private void Start()
    {
        isAppearing = false;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isAppearing)
        {
            isAppearing = true;
            AppearingEffect = Instantiate(ice, gameObject.transform.position, gameObject.transform.rotation);
        }else if(Input.GetKeyDown(KeyCode.I) && isAppearing)
        {
            AppearingEffect.transform.Find("Lance").GetComponent<ParticleSystem>().loop = false;
            AppearingEffect.transform.Find("Dust").GetComponent<ParticleSystem>().loop = false;
            isAppearing = false;
        }
    }
}
