using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Grounded,
        Air,
        WallRun,
        LedgeHang
    }
    
    [Header("Sprint Variables")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float playerSprintSpeed = 8.0f;
    public bool isSprinting;
    private bool sprintToggleMode;
    
    [Header("Jump Variables")]
    [SerializeField] public float jumpHeight = 1.0f; //
    
    [Header("Wall running Variables")]
    public float wallRunSpeed;
    public bool wallrunning;
    private float baseSpeed;
    public bool hasWallJumped; //

    [Header("Input Actions")] 
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    
    // other
    public float gravityValue = -11f; //
    private CharacterController controller;
    public Vector3 playerVelocity; //
    private bool groundedPlayer;
    private Transform cameraTransform;
    private Vector2 cachedMoveInput;
    private PlayerState playerState;
    private Vector3 externalHorizontalVelocity;
    
    public PlayerState CurrentState => playerState;
    
    private void Awake()
    {
        baseSpeed = playerSpeed;
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        
        sprintAction.action.started += OnSprint;
        sprintAction.action.canceled += OnSprint;
        sprintAction.action.performed += OnSprint;
    }

    private void OnDisable()
    {
        sprintAction.action.started -= OnSprint;
        sprintAction.action.canceled -= OnSprint;
        sprintAction.action.performed -= OnSprint;
        
        moveAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }
    
    public Vector2 GetMoveInput() => cachedMoveInput;

    private void Update()
    {
        if (!controller.enabled || !gameObject.activeInHierarchy)
            return;
        UpdateState();
        HandleMovement();
    }

    private void UpdateState()
    {
        groundedPlayer = controller.isGrounded;
        
        if (playerState == PlayerState.LedgeHang)
            return;
        if (wallrunning)
        {
            playerState = PlayerState.WallRun;
        } 
        else if (groundedPlayer)
        {
            playerState = PlayerState.Grounded;
        }
        else
        {
            playerState = PlayerState.Air;
        }
    }

    private void HandleMovement()
    {
        cachedMoveInput = moveAction.action.ReadValue<Vector2>();
        
        if (playerState == PlayerState.LedgeHang) // no movement bc on ledge
            return;
        
        var camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        var camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        var move = camForward * cachedMoveInput.y + camRight * cachedMoveInput.x;
        move = Vector3.ClampMagnitude(move, 1f);

        switch (playerState)
        {
            case PlayerState.Grounded:
                HandleGrounded();
                break;
            case PlayerState.Air:
                HandleAir();
                break;
            case PlayerState.LedgeHang:
                HandleLedgeHang();
                break;
        }
        Sprint();
        
        externalHorizontalVelocity = Vector3.Lerp(externalHorizontalVelocity, Vector3.zero, 8f * Time.deltaTime);
        var finalMove = (move * playerSpeed) + (playerVelocity.y * Vector3.up) + externalHorizontalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleGrounded()
    {
        hasWallJumped = false;
        
        if (playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        if (jumpAction.action.triggered)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
            playerState = PlayerState.Air;
        }
    }

    private void HandleAir()
    {
        hasWallJumped = false;
        playerVelocity.y += gravityValue * Time.deltaTime;
    }

    private void HandleLedgeHang()
    {
        playerVelocity = Vector3.zero;
        isSprinting = false;
    }
    
    private void Sprint()
    {
        if (!groundedPlayer)
            return;
        
        playerSpeed = isSprinting ? playerSprintSpeed : baseSpeed;
    }
    
    private void OnSprint(InputAction.CallbackContext ctx)
    {
        if (sprintToggleMode)
        {
            if (ctx.performed) isSprinting = !isSprinting; // sprint until button pressed again
        }
        else
        {
            if (ctx.started) isSprinting = true; // only sprint if button is pressed
            if (ctx.canceled) isSprinting = false;
        } 
        
    }
    
    public void SetSprintToggleMode(bool value)
    {
        sprintToggleMode = value;
        PlayerPrefs.SetInt("SprintToggle", value ? 1 : 0);
    }
    
    public void SetWallRunning(bool active)
    {
        wallrunning = active;

        if (active)
        {
            playerSpeed = wallRunSpeed;
            if (playerVelocity.y > 0) 
                playerVelocity.y = 0f;
            playerState = PlayerState.WallRun;
        }
        else
        {
            playerSpeed = baseSpeed;
        }
    }
    
    public void SetState(PlayerState newState)
    {
        playerState = newState;
    }
    
    public void ForceSetVerticalVelocity(float y)
    {
        // setzt nur Y, damit Jump sauber startet
        playerVelocity.y = y;
    }

    public void AddExternalHorizontalVelocity(Vector3 v)
    {
        externalHorizontalVelocity += v;
    }
}