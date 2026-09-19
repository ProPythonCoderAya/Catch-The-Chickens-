using System;
using UnityEngine;

public class PlayerLookScript : MonoBehaviour
{
    public float sensHorizontal = 10f;
    public float sensVertical = 10f;

    public Transform cameraTransform;

    private float _pitch;

    public PlayerMovementScript playerMovementScript;

    private PlayerInputActions _input;
    
    public bool canMove = true;

    void Awake()
    {
        _input = new PlayerInputActions();
    }

    void OnEnable()
    {
        _input.Enable();
    }

    void OnDisable()
    {
        _input.Disable();
    }

    private void Start()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    sensHorizontal /= 5f;
    sensVertical /= 5f;
#endif
    }

    void Update()
    {
        if (!playerMovementScript.canMove) return;

        Vector2 look = _input.Player.Look.ReadValue<Vector2>();

        float mouseX = look.x * sensHorizontal;
        float mouseY = look.y * sensVertical;

        transform.Rotate(0f, mouseX, 0f);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -85f, 85f);

        cameraTransform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }
}