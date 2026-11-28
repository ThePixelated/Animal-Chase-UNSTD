
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] lanePivots;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform visualTurnPivot;
    [SerializeField] private LayerMask groundLayers;


    [Header("Lane Movement")]
    [SerializeField] private float laneSwitchSpeed = 30f;

    [Header("Jump Physics")]
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private float gravity = 25f;
    [SerializeField] private float fastFallSpeed = 30f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private float groundProbeOffset = 0.1f;
    [SerializeField] private float groundProbeDistance = 0.35f;
    [SerializeField] private float groundSkin = 0.02f;

    [Header("Input Buffer")]
    [SerializeField] private float jumpBufferDuration = 0.15f;
    [SerializeField] private float fastFallBufferDuration = 0.15f;

    [Header("Lane Rotation")]
    [SerializeField] private float maxTurnAngle = 15f;
    [SerializeField] private float turnSmoothTime = 0.12f;

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator;

    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private Rigidbody rb;

    private PlayerPosition currentLane = PlayerPosition.CENTER;

    private float targetX;
    private float verticalVelocity;

    private float jumpBufferTimer;
    private float fastFallBufferTimer;

    private bool isGrounded;
    private bool isFastFalling;

    private float currentYaw;
    private float yawVelocity;

    private Quaternion baseVisualRotation;

    public PlayerPosition CurrentLane => currentLane;
    public bool IsGrounded => isGrounded;
    public bool IsFastFalling => isFastFalling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (lanePivots == null || lanePivots.Length != 3 ||
            groundCheck == null || visualTurnPivot == null)
        {
            Debug.LogError("PlayerMovement references are incomplete!");
            enabled = false;
            return;
        }

        if (groundLayers.value == 0)
        {
            Debug.LogError("Ground Layers is empty! Assign the Ground layer.");
            enabled = false;
            return;
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        targetX = lanePivots[(int)currentLane].position.x;

        baseVisualRotation = visualTurnPivot.localRotation;
    }

    // =====================================
    // LANE MOVEMENT
    // =====================================

    public void ChangeLane(int direction)
    {
        int nextLane = Mathf.Clamp(
            (int)currentLane + direction,
            0,
            2
        );

        if (nextLane == (int)currentLane)
            return;

        currentLane = (PlayerPosition)nextLane;

        targetX = lanePivots[nextLane].position.x;
    }

    // =====================================
    // INPUT REQUESTS
    // =====================================

    public void Jump()
    {
        jumpBufferTimer = jumpBufferDuration;
    }

    public void FastFall()
    {
        // Only buffer fast fall when already airborne.
        if (!isGrounded)
        {
            fastFallBufferTimer = fastFallBufferDuration;
        }
    }

    // =====================================
    // GROUND DETECTION
    // =====================================

    private bool CheckGround(out RaycastHit hit)
    {
        hit = default;

        // First check direct overlap around the feet.
        bool overlapping = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        if (overlapping)
            return true;

        // Probe slightly downward to detect nearby ground.
        Vector3 origin = groundCheck.position +
                         Vector3.up * groundProbeOffset;

        float probeLength = groundProbeDistance +
                            groundProbeOffset;

        return Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out hit,
            probeLength,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    // =====================================
    // PHYSICS MOVEMENT
    // =====================================

    private void FixedUpdate()
    {
        if (GameManager.CurrentGameState != GameState.PLAYING)
            return;

        float dt = Time.fixedDeltaTime;

        jumpBufferTimer = Mathf.Max(
            0f,
            jumpBufferTimer - dt
        );

        fastFallBufferTimer = Mathf.Max(
            0f,
            fastFallBufferTimer - dt
        );

        Vector3 currentPosition = rb.position;

        // -----------------------------
        // HORIZONTAL MOVEMENT
        // -----------------------------

        float newX = Mathf.MoveTowards(
            currentPosition.x,
            targetX,
            laneSwitchSpeed * dt
        );

        // -----------------------------
        // VERTICAL MOVEMENT
        // -----------------------------

        float newY = currentPosition.y;

        isGrounded = false;

        if (verticalVelocity <= 0f)
        {
            if (CheckGround(out RaycastHit hit))
            {
                isGrounded = true;
                verticalVelocity = 0f;
                isFastFalling = false;

                // Correct position when approaching ground.
                if (hit.collider != null)
                {
                    float correction = Mathf.Max(
                        0f,
                        hit.distance -
                        groundProbeOffset -
                        groundSkin
                    );

                    newY -= correction;
                }
            }
        }

        // Jump buffer: execute as soon as grounded.
        if (isGrounded && jumpBufferTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(
                2f * gravity * jumpHeight
            );

            jumpBufferTimer = 0f;
            isGrounded = false;

            // Play jump animation
            playerAnimator.SetTrigger("Jump");
        }

        if (!isGrounded)
        {
            // Fast fall buffer.
            if (fastFallBufferTimer > 0f)
            {
                verticalVelocity = -fastFallSpeed;

                fastFallBufferTimer = 0f;
                isFastFalling = true;
            }

            verticalVelocity -= gravity * dt;

            newY += verticalVelocity * dt;
        }

        // -----------------------------
        // APPLY MOVEMENT
        // -----------------------------

        Vector3 newPosition = new Vector3(
            newX,
            newY,
            currentPosition.z
        );

        rb.MovePosition(newPosition);
    }

    // =====================================
    // VISUAL ROTATION
    // =====================================

    private void Update()
    {
        if (GameManager.CurrentGameState != GameState.PLAYING)
            return;

        float laneDifference = targetX - rb.position.x;

        float targetYaw = 0f;

        if (Mathf.Abs(laneDifference) > 0.03f)
        {
            targetYaw = Mathf.Sign(laneDifference) * maxTurnAngle;
        }

        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref yawVelocity,
            turnSmoothTime
        );

        visualTurnPivot.localRotation =
            baseVisualRotation *
            Quaternion.Euler(0f, currentYaw, 0f);
    }

    // =====================================
    // DEBUG VISUALIZATION
    // =====================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = isGrounded ? Color.green : Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );

        Gizmos.DrawLine(
            groundCheck.position +
            Vector3.up * groundProbeOffset,

            groundCheck.position +
            Vector3.down * groundProbeDistance
        );
    }
}

public enum PlayerPosition
{
    LEFT,
    CENTER,
    RIGHT
}
