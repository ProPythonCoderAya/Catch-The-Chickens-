using UnityEngine;

public class MenuCameraScript : MonoBehaviour
{
    public float rotationSpeed = 5f;
    
    void Update()
    {
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
    }
}
