using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ChickenHealthScript : MonoBehaviour
{
    public Camera targetCamera;

    private void LateUpdate()
    {
        if (!targetCamera)
            return;

        transform.LookAt(
            transform.position + targetCamera.transform.rotation * Vector3.forward,
            targetCamera.transform.rotation * Vector3.up
        );
    }
}