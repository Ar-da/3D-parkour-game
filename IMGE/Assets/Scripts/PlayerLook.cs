using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public InputActionReference lookAction;
    public float sensitivity = 30f;

    private float xRotation;
    public Transform playerBody;

    private void OnEnable() => lookAction.action.Enable();
    private void OnDisable() => lookAction.action.Disable();

    private void Start()
    {
        xRotation = 0f;
        transform.localRotation = Quaternion.identity; 
        playerBody.rotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        var look = lookAction.action.ReadValue<Vector2>();

        var mouseX = look.x * sensitivity * Time.deltaTime;
        var mouseY = look.y * sensitivity * Time.deltaTime;

        // Vertikale Rotation (Kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontale Rotation (Player)
        playerBody.Rotate(Vector3.up * mouseX);
    }
    
    public void SetSensitivity(float newSensitivity) => this.sensitivity = newSensitivity;
}
