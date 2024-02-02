using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.DownArrow))
            gameObject.transform.localPosition += new Vector3(0, 0, -10f * Time.deltaTime);
        else if (Input.GetKey(KeyCode.UpArrow))
            gameObject.transform.localPosition += new Vector3(0, 0, 10f * Time.deltaTime);
        else if (Input.GetKey(KeyCode.LeftArrow))
            gameObject.transform.localPosition += new Vector3(-10f * Time.deltaTime, 0, 0);
        else if (Input.GetKey(KeyCode.RightArrow))
            gameObject.transform.localPosition += new Vector3(10f * Time.deltaTime, 0, 0);
    }
}
