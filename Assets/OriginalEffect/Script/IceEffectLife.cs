using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceEffectLife : MonoBehaviour
{
    [SerializeField]float lifeTime;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer == 0 && !this.transform.Find("Lance").GetComponent<ParticleSystem>().loop)
        {
            Debug.Log("enter");
            timer += Time.deltaTime;
        }

        if(timer != 0)
        {
            timer += Time.deltaTime;
            if(timer > lifeTime)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
