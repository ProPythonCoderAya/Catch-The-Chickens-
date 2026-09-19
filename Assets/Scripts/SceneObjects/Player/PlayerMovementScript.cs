using System;
using Globs;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float sprintMultiplier = 1.7f;
    
    public bool isSprinting;

    public float jumpSpeed = 4.0f;
    public float gravity = 9.81f;

    public CharacterController charCont;
    private Vector3 _moveDirection = Vector3.zero;
    public Camera playerCamera;
    
    [HideInInspector]
    public bool canMove = true;

    private PlayerInputActions _input;

    void Awake()
    {
        Globals.Player = gameObject;
        Globals.MainCamera = playerCamera;
        _input = new PlayerInputActions();
    }

    void OnEnable() {
        _input.Enable();
    }

    void OnDisable() {
        _input.Disable();
    }

    void Update() {
        HandlePlayerMove();
    }

    private void HandlePlayerMove() {

        float currentSpeed = moveSpeed;
        
        isSprinting = _input.Player.Sprint.IsPressed();
        
        if (isSprinting) {
            playerCamera.transform.position = new Vector3(0f, 0.5f, 0f) + transform.position;
            currentSpeed *= sprintMultiplier;
        } else {
            playerCamera.transform.position = new Vector3(0f, 0.5f, 0f) + transform.position;
        }

        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();

        float deltaX = moveInput.x * currentSpeed;
        float deltaZ = moveInput.y * currentSpeed;

        if (!canMove)
        {
            deltaX = 0f;
            deltaZ = 0f;
        }

        _moveDirection = new Vector3(deltaX, _moveDirection.y, deltaZ);
        
        if (!isActiveAndEnabled || !charCont || !charCont.enabled || !charCont.gameObject.activeInHierarchy)
            return;

        if (charCont.isGrounded) {
            _moveDirection.y = _input.Player.Jump.IsPressed() ? jumpSpeed : 0f;

            if (deltaX != 0 || deltaZ != 0) {
                // movement SFX etc
            }
        }

        ApplyMovement();
        
        if (transform.position.y < -15f) {
            transform.position = new Vector3(0f, 1f, 0f);
        }
    }

    private void ApplyMovement() {
        _moveDirection = transform.TransformDirection(_moveDirection);
        _moveDirection.y -= gravity * Time.deltaTime;

        charCont.Move(_moveDirection * Time.deltaTime);
    }
}
