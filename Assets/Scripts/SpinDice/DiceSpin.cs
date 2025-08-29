using UnityEngine;

public class DiceSpin : MonoBehaviour
{
    public float xSpeed;
    public float ySpeed;
    public float zSpeed;

    public int frameSkip;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime));

        /*
        if (Time.frameCount % frameSkip == 0)
        {
            //transform.Rotate(new Vector3(xSpeed * Time.time, ySpeed * Time.time, zSpeed * Time.time));
            //transform.Rotate(new Vector3(xSpeed, ySpeed, zSpeed));
        }
        */
    }
}
