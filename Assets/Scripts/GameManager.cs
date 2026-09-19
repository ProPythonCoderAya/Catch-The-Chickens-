using System;
using Globs;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

public class GameManager : MonoBehaviour
{
    [Header("Objects")] public GameObject player;
    public Camera playerCamera;
    public Camera menuCamera;

    [Header("Menus")] public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject pauseMenuPanel;

    public bool isPlaying;

    private void Awake()
    {
        Globals.GameManager = this;
    }

    private void Start()
    {
        if (!playerCamera)
        {
            playerCamera = player.GetComponent<Camera>(); // try to get from player
            if (!playerCamera)
            {
                playerCamera = Camera.main; // fallback
            }
        }

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        isPlaying = false;
        player.SetActive(false);

        playerCamera.enabled = false;
        menuCamera.enabled = true;

        SetCameraAudio(playerCamera, false);
        SetCameraAudio(menuCamera, true);

        mainMenuPanel.SetActive(true);
        hudPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowPauseMenu()
    {
        player.GetComponent<PlayerMovementScript>().canMove = false;
        
        playerCamera.enabled = true;

        pauseMenuPanel.SetActive(true);
        
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePauseMenu()
    {
        player.GetComponent<PlayerMovementScript>().canMove = true;
        
        player.SetActive(true);
        
        playerCamera.enabled = true;
        
        pauseMenuPanel.SetActive(false);
        
        Time.timeScale = 1f;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StartGame()
    {
        isPlaying = true;
        player.SetActive(true);

        menuCamera.enabled = false;
        playerCamera.enabled = true;
        
        SetCameraAudio(playerCamera, true);
        SetCameraAudio(menuCamera, false);
        
        mainMenuPanel.SetActive(false);
        hudPanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void SetCameraAudio(Camera cam, bool on)
    {
        cam.GetComponent<AudioListener>().enabled = on;
    }

    private void Update()
    {
        if (isPlaying && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ShowPauseMenu();
        }
    }
}
