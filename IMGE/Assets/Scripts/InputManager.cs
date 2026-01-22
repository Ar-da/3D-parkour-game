using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    
    private InputAction playerControls;

    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();
        playerControls = playerInput.actions["Player"];
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
