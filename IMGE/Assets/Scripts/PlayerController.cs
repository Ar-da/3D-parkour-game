using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Sprint Variables")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float playerSprintSpeed = 8.0f;
    public bool isSprinting;
    
    [Header("Jump Variables")]
    [SerializeField] private float jumpHeight = 1.0f;
    
    private float gravityValue = -11f;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;
    
    
    [Header("Wall running Variables")]
    public float wallRunSpeed;
    public bool wallrunning;
    private float baseSpeed;
    private Vector2 cachedMoveInput;
    private bool hasWallJumped;

    [Header("Input Actions")] 
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;


    private void Awake()
    {
        baseSpeed = playerSpeed;
        controller = GetComponent<CharacterController>();
        Cursor.visible = false;
        cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        
        sprintAction.action.performed += OnSprint;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }
    
    public Vector2 GetMoveInput() => cachedMoveInput;

    private void Update()
    {
        Sprint();
        if (!wallrunning)
        {
            hasWallJumped = false;
            playerVelocity.y += gravityValue * Time.deltaTime;
        }
        else
        {
            if (playerVelocity.y < 0) playerVelocity.y = 0f;
        }


        cachedMoveInput = moveAction.action.ReadValue<Vector2>();
        var input = cachedMoveInput;

        var camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        var camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        var move = camForward * input.y + camRight * input.x;
        move = Vector3.ClampMagnitude(move, 1f);


        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        if (jumpAction.action.triggered && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        } 
        else if (jumpAction.action.triggered && wallrunning && !hasWallJumped)
        {
            hasWallJumped = true;
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }
        

        var finalMove = (move * playerSpeed) + (playerVelocity.y * Vector3.up);
        controller.Move(finalMove * Time.deltaTime);
    }

    private void OnSprint(InputAction.CallbackContext ctx) => StartSprint();


    private void StartSprint()
    {
        isSprinting = !isSprinting;
    }
    
    private void Sprint()
    {
        if (!groundedPlayer)
        {
            return;
        }
        
        playerSpeed = isSprinting ? playerSprintSpeed : baseSpeed;
    }
    

    public void SetWallRunning(bool active)
    {
        wallrunning = active;

        if (active)
        {
            playerSpeed = wallRunSpeed;
            if (playerVelocity.y > 0) playerVelocity.y = 0f;
        }
        else
        {
            playerSpeed = baseSpeed;
        }
    }
}