using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSpin : MonoBehaviour
{

    public float xSpeed;
    public float ySpeed;
    public float zSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime));
    }
}
