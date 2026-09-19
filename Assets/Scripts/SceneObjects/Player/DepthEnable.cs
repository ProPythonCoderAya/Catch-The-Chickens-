using UnityEngine;

public class DepthEnable : MonoBehaviour
{
    void OnEnable()
    {
        var component = GetComponent<Camera>();
        if (component) component.depthTextureMode |= DepthTextureMode.Depth;
    }
}
