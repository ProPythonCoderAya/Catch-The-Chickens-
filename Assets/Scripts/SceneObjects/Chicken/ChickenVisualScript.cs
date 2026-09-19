using UnityEngine;
using System.Collections;

public class ChickenVisualScript : MonoBehaviour
{
    private Renderer[] _renderers;
    private Color[] _originalColors;

    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColors[i] = _renderers[i].material.color;
        }
    }

    public void FlashRed(float duration = 0.2f)
    {
        StartCoroutine(FlashCoroutine(duration));
    }

    private IEnumerator FlashCoroutine(float duration)
    {
        // set red
        foreach (var r in _renderers)
        {
            r.material.color = Color.red;
        }

        yield return new WaitForSeconds(duration);

        // restore original
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material.color = _originalColors[i];
        }
    }
    
    public void TurnRight(float duration = 0.3f)
    {
        StartCoroutine(TurnRoutine(duration));
    }

    private IEnumerator TurnRoutine(float dur)
    {
        Quaternion start = transform.rotation;
        Quaternion target = start * Quaternion.Euler(0f, 0f, -90f);

        float t = 0f;
        
        float duration = dur * 0.5f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float normalized = t / duration;

            // ease-out (fast start → slow end)
            float eased = 1f - Mathf.Pow(1f - normalized, 2f);

            transform.rotation = Quaternion.Slerp(start, target, eased);

            yield return null;
        }

        duration = dur;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = target; // snap until end

            yield return null;
        }
    }
}
