using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public InputActionReference lookAction;
    public float sensitivity = 50f;

    private float xRotation = 0f;
    public Transform playerBody;

    void OnEnable() => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();

    private void Start()
    {
        xRotation = 0f;
        transform.localRotation = Quaternion.identity; 
        playerBody.rotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 look = lookAction.action.ReadValue<Vector2>();

        float mouseX = look.x * sensitivity * Time.deltaTime;
        float mouseY = look.y * sensitivity * Time.deltaTime;

        // Vertikale Rotation (Kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontale Rotation (Player)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
