using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPop : MonoBehaviour
{
    [SerializeField]GameObject fire;
    GameObject AppearingObject;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Instantiate( 生成するオブジェクト,  場所, 回転 );  回転はそのままなら↓
            AppearingObject = Instantiate(fire, gameObject.transform.position, Quaternion.identity);
            AppearingObject.GetComponent<Rigidbody>().velocity = new Vector3(10, 0, 0);
        }
    }
}
