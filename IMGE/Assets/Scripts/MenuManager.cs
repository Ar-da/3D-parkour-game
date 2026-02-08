using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private MonoBehaviour lookScript;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private Canvas CompassBarCanvas;

    private bool isOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CloseMenu();
    }
    
    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePerformed;
    }
    
    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePerformed;
        pauseAction.action.Disable();
    }
    
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    private void OpenMenu()
    {
        isOpen = true;
        settingsPanel.SetActive(true);
        CompassBarCanvas.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        lookScript.enabled = false;
        Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        isOpen = false;
        settingsPanel.SetActive(false);
        CompassBarCanvas.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lookScript.enabled = true;
        Time.timeScale = 1f;
    }
}
