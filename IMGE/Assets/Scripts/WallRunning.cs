using System;
using UnityEngine;

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
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    private float wallRunSpeed;

    [Header("References")] 
    public Transform orientation;
    private PlayerController playerMovement;
    private CharacterController controller;
    
    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;
    
    private void Start()
    {
        playerMovement = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
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
        var isGrounded = AboveGround();

        if (!playerMovement.wallrunning)
        {
            if (isWall && isMoving && isGrounded)
            {
                StartWallRun();
            }
        }
        else
        {
            if (!isWall || !isMoving || !isGrounded)
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
        
        wallRunTimer -= Time.fixedDeltaTime;
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
        controller.Move(move * Time.fixedDeltaTime);
    }
    
    private void StopWallRun()
    {
        playerMovement.SetWallRunning(false);
    }
}
