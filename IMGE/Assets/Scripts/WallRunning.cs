using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")] 
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float maxWallRunTime;
    private float wallRunTimer;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    [Header("Detection")] 
    public float wallCheckDistance;
    public float minJumpHeight;
    public RaycastHit leftWallHit; //
    public RaycastHit rightWallHit; //
    public bool wallLeft; //
    public bool wallRight; //
    private float wallRunSpeed;
    private bool hasJumped;

    [Header("References")] 
    public Transform orientation;
    private PlayerController playerMovement;
    private CharacterController controller;
    public InputActionReference jumpAction;
    
    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;
    
    private void Start()
    {
        playerMovement = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        jumpAction.action.Enable();
    }
    
    private void OnDisable()
    {
        jumpAction.action.Disable();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
        if (playerMovement.wallrunning) WallRunningMovement();
    }

    private void CheckForWall()
    {
        var origin = transform.position + controller.center;
        
        wallRight = Physics.Raycast(origin, orientation.right, out rightWallHit, wallCheckDistance,whatIsWall);
        wallLeft = Physics.Raycast(origin, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);

        Debug.DrawRay(origin, orientation.right * wallCheckDistance, wallRight ? Color.green: Color.red);
        Debug.DrawRay(origin, -orientation.right * wallCheckDistance, wallLeft ? Color.green: Color.red);

    }

    private bool AboveGround()
    {
        return !controller.isGrounded;
    }

    private void StateMachine()
    {
        var moveInput = playerMovement.GetMoveInput();
        var verticalInput = moveInput.y;
        
        var isWall = wallLeft || wallRight;
        var isMoving = verticalInput > 0.1f;
        var isAboveGround = AboveGround();

        if (!playerMovement.wallrunning)
        {
            if (isWall && isMoving && isAboveGround)
            {
                StartWallRun();
            }
        }
        else
        {
            if (!playerMovement.hasWallJumped && jumpAction.action.triggered) // prevents double jump on wall
            {
                WallRunJump();
            }
            if (!isWall || !isMoving || !isAboveGround)
            {
                StopWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        playerMovement.SetWallRunning(true);
        wallRunTimer = maxWallRunTime;
    }

    private void WallRunningMovement()
    {
        wallRunTimer -= Time.deltaTime;
        if (wallRunTimer <= 0f)
        {
            StopWallRun();
            return;
        }
        
        var wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        var wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;

        // Bewegung an Spieler-Ausrichtung anpassen
        if (Vector3.Dot(wallForward, orientation.forward) < 0f)
            wallForward = -wallForward;

        // Bewegung an der Wand
        var move = wallForward * playerMovement.wallRunSpeed;
        controller.Move(move * Time.deltaTime);
    }
    
    private void WallRunJump()
    {
        playerMovement.hasWallJumped = true;
        
        var wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        var forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        
        if (playerMovement.playerVelocity.y < 0)
            playerMovement.playerVelocity.y = 0f;
        
        controller.Move(forceToApply);
        
        Debug.Log("We Jumped!");
        
    }
    
    private void StopWallRun()
    {
        playerMovement.SetWallRunning(false);
    }
}
