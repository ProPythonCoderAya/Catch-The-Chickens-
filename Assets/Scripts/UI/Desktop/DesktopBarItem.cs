using System;
using UnityEngine;
using UnityEngine.UI;

public class DesktopBarItem : MonoBehaviour
{
    public GameObject dropDown;
    
    private Button _button;
    
    private void Start()
    {
        dropDown.SetActive(false);
        _button = GetComponent<Button>();
    }

    private void Update()
    {
        if (_button.IsPressedDirect() || _button.IsSelectedDirect())
        {
            dropDown.SetActive(true);
        }
        else
        {
            dropDown.SetActive(false);
        }
    }
}
