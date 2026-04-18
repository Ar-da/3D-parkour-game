using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform cam;            // deine Kamera (oder Camera.main.transform)
    [SerializeField] private Transform orientation;    // optional, z.B. Player-Body/Yaw

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference dropAction; // optional (z.B. crouch)
    
    [Header("Detection")]
    [SerializeField] private LayerMask whatIsLedge;
    [SerializeField] private float wallCheckDistance = 1.2f;
    [SerializeField] private float wallCheckRadius = 0.25f;
    [SerializeField] private float chestHeight = 1.2f;
    [SerializeField] private float topCheckHeight = 1.8f;
    [SerializeField] private float downCheckDistance = 2.5f;

    [Header("Hang Settings")]
    [SerializeField] private float maxGrabDistance = 1.1f;
    [SerializeField] private float hangWallOffset = 0.45f;   // Abstand von der Wand
    [SerializeField] private float hangDownOffset = 1.0f;    // wie weit "unten" unter der Oberkante hängen
    [SerializeField] private float handHeightFromTop = 1.35f; // fein-tuning
    [SerializeField] private float snapSpeed = 18f;
    [SerializeField] private float minTimeOnLedge = 0.15f;

    [Header("Exit / Jump")]
    [SerializeField] private float exitCooldown = 0.25f;
    [SerializeField] private float ledgeJumpUpVelocity = 6.5f;
    [SerializeField] private float ledgeJumpForwardVelocity = 3.5f;
    
    private CharacterController cc;

    private bool holding;
    private bool exiting;
    private float timeOnLedge;
    private float exitTimer;

    private Transform lastLedge;
    private Vector3 hangTargetPos;
    private Vector3 hangWallNormal;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (jumpAction != null) jumpAction.action.Enable();
        if (dropAction != null) dropAction.action.Enable();
    }

    private void OnDisable()
    {
        if (jumpAction != null) jumpAction.action.Disable();
        if (dropAction != null) dropAction.action.Disable();
    }

    private void Update()
    {
        if (playerController == null || cc == null || !cc.enabled || !gameObject.activeInHierarchy)
            return;

        if (exiting)
        {
            exitTimer -= Time.deltaTime;
            if (exitTimer <= 0f) exiting = false;
        }

        if (!holding && !exiting)
            TryDetectAndEnter();

        if (holding)
            HandleHolding();
    }

    private void TryDetectAndEnter()
    {
        // Nur greifen, wenn du nicht grounded bist 
        if (cc.isGrounded) return;

        // Wenn Wallrun aktiv ist: lieber erst beenden, sonst Konflikte
        if (playerController.CurrentState == PlayerController.PlayerState.WallRun)
            return;

        Vector3 origin = transform.position + Vector3.up * chestHeight;
        Vector3 dir = cam != null ? cam.forward : transform.forward;

        // 1) Wand vor dir finden
        if (!Physics.SphereCast(origin, wallCheckRadius, dir, out RaycastHit wallHit, wallCheckDistance, whatIsLedge))
            return;

        // Doppelt auf gleiche Kante vermeiden
        if (wallHit.transform == lastLedge)
        {
            Debug.Log("Blocked by last ledge");
            return;
        }
            

        // 2) "Oberkante" finden: Von über der Wand nach unten casten
        Vector3 topOrigin = wallHit.point + Vector3.up * topCheckHeight - wallHit.normal * 0.3f; // leicht von der Wand weg
        if (!Physics.Raycast(topOrigin, Vector3.down, out RaycastHit topHit, downCheckDistance, whatIsLedge))
        {
            Debug.Log("No top hit");
            return;
        }
        
        // Distanzcheck (wie nah ist die Kante wirklich)
        float dist = Vector3.Distance(transform.position, wallHit.point);
        if (dist > maxGrabDistance)
        {
            Debug.Log("Too far");
            return;
        }
            

        // 3) Clearance Check: ist Platz oben (damit du nicht in Geometrie snappst)
        // Wir checken, ob die Capsule am HangTarget frei ist (grob).
        Vector3 wallNormal = wallHit.normal;
        Vector3 candidateHangPos = wallHit.point + wallNormal * hangWallOffset + Vector3.up * (topCheckHeight - handHeightFromTop);

        
        /*
        bool free = IsPositionFree(candidateHangPos);
        if (!free)
        {
            Debug.Log("Blocked by IsPositionFree()");
            return;
        }
        */
        
        // Enter Hang
        hangTargetPos = candidateHangPos;
        hangWallNormal = wallNormal;
        lastLedge = wallHit.transform;

        EnterHold();
    }

    private bool IsPositionFree(Vector3 pos)
    {
        // simple capsule check based on CC dimensions
        float radius = Mathf.Max(0.05f, cc.radius * 0.95f);
        float height = Mathf.Max(radius * 2f, cc.height * 0.95f);

        Vector3 center = pos + cc.center;
        Vector3 p1 = center + Vector3.up * (height / 2f - radius);
        Vector3 p2 = center + Vector3.down * (height / 2f - radius);

        // CheckCapsule returns true if something overlaps
        return !Physics.CheckCapsule(p1, p2, radius, ~0, QueryTriggerInteraction.Ignore);
    }

    private void EnterHold()
    {
        holding = true;
        timeOnLedge = 0f;

        // State setzen – PlayerController stoppt dann eigenes Move/Gravity (bei dir via State)
        playerController.SetState(PlayerController.PlayerState.LedgeHang);

        // optional: Sprint aus
        playerController.isSprinting = false;

        // vertikale velocity sauber resetten (Patch unten im PlayerController)
        playerController.ForceSetVerticalVelocity(0f);

        // Player zur Wand ausrichten (optional)
        FaceWall();
    }

    private void HandleHolding()
    {
        timeOnLedge += Time.deltaTime;

        // Snap / lock position
        Vector3 delta = hangTargetPos - transform.position;
        Vector3 step = Vector3.ClampMagnitude(delta, snapSpeed * Time.deltaTime);
        cc.Move(step);

        FaceWall();

        // Exit by move input (wie du es wolltest)
        Vector2 moveInput = playerController.GetMoveInput();
        bool anyMoveInput = moveInput.sqrMagnitude > 0.0001f;

        if (timeOnLedge > minTimeOnLedge && anyMoveInput)
        {
            ExitHoldToAir();
            return;
        }

        // Drop
        if (dropAction != null && dropAction.action.triggered && timeOnLedge > minTimeOnLedge)
        {
            ExitHoldToAir();
            return;
        }

        // Jump off ledge
        if (jumpAction != null && jumpAction.action.triggered && timeOnLedge > minTimeOnLedge)
        {
            LedgeJump();
        }
    }

    private void FaceWall()
    {
        if (orientation == null) return;

        Vector3 lookDir = -hangWallNormal;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude < 0.0001f) return;

        orientation.forward = Vector3.Lerp(orientation.forward, lookDir.normalized, 20f * Time.deltaTime);
    }

    private void LedgeJump()
    {
        // Exit first
        holding = false;

        exiting = true;
        exitTimer = exitCooldown;

        playerController.SetState(PlayerController.PlayerState.Air);

        // Jump velocity (Patch unten)
        playerController.ForceSetVerticalVelocity(ledgeJumpUpVelocity);
        playerController.AddExternalHorizontalVelocity((cam != null ? cam.forward : transform.forward) * ledgeJumpForwardVelocity);

        Invoke(nameof(ResetLastLedge), 0.8f);
    }

    private void ExitHoldToAir()
    {
        holding = false;

        exiting = true;
        exitTimer = exitCooldown;

        playerController.SetState(PlayerController.PlayerState.Air);

        Invoke(nameof(ResetLastLedge), 0.8f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }
}