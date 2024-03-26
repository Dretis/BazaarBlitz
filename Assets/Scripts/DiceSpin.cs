using UnityEngine;

public class DiceSpin : MonoBehaviour
{
    public float xSpeed;
    public float ySpeed;
    public float zSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime));
    }
}
