using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves the GameObject on the ground plane using the new Input System
/// (WASD, arrow keys or gamepad left stick: A/D = X axis, W/S = Z axis)
/// and jumps with Space (or the gamepad south button), allowing a
/// limited number of jumps (double jump by default) before landing.
/// Movement goes through the Rigidbody so colliders block the object
/// and gravity pulls it back down after a jump.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CubeMovement : MonoBehaviour
{
    private const float GroundCheckMargin = 0.1f;

    private Rigidbody body;
    private Collider ownCollider;
    private Vector2 currentInput;
    private bool jumpRequested;
    private int jumpsUsed;

    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("2D input action: X = left/right (X axis), Y = forward/backward (Z axis).")]
    [SerializeField] private InputAction moveAction = new InputAction("Move", InputActionType.Value);

    [Header("Jump")]
    [Tooltip("Maximum height reached by a jump, in units.")]
    [SerializeField] private float jumpHeight = 2f;

    [Tooltip("Maximum number of jumps before touching the ground again (2 = double jump).")]
    [Min(1)]
    [SerializeField] private int maxJumps = 2;

    [Tooltip("Button input action that triggers the jump.")]
    [SerializeField] private InputAction jumpAction = new InputAction("Jump", InputActionType.Button);

    [Tooltip("Layers considered as ground for the jump check.")]
    [SerializeField] private LayerMask groundLayers = ~0;

    private void Reset()
    {
        // Default bindings, applied when the component is added or reset.
        moveAction = CreateDefaultMoveAction();
        jumpAction = CreateDefaultJumpAction();
        ConfigureRigidbody(GetComponent<Rigidbody>());
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();

        // Safety net in case the actions were serialized without bindings.
        if (moveAction.bindings.Count == 0)
        {
            moveAction = CreateDefaultMoveAction();
        }

        if (jumpAction.bindings.Count == 0)
        {
            jumpAction = CreateDefaultJumpAction();
        }
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        // Read input every frame so no key press is missed.
        // Clamp so diagonal movement is not faster than straight movement.
        currentInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        // Store the press; it is consumed in FixedUpdate (physics step).
        if (jumpAction.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        // Horizontal movement is driven by input; vertical velocity is left to gravity.
        Vector3 velocity = body.linearVelocity;
        velocity.x = currentInput.x * moveSpeed;
        velocity.z = currentInput.y * moveSpeed;

        bool grounded = IsGrounded();

        // Refill jumps only when resting on the ground, not while still rising
        // from a jump (the ground check can stay true for a step after take-off).
        if (grounded && velocity.y <= 0.01f)
        {
            jumpsUsed = 0;
        }

        if (jumpRequested)
        {
            jumpRequested = false;

            // Walking off a ledge counts as the first jump, so only the air jump is left.
            if (!grounded && jumpsUsed == 0)
            {
                jumpsUsed = 1;
            }

            if (jumpsUsed < maxJumps)
            {
                jumpsUsed++;

                // Overwrite (not add) vertical speed so the air jump has the same
                // height even while falling. v = sqrt(2 * g * h).
                velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            }
        }

        body.linearVelocity = velocity;
    }

    /// <summary>
    /// Casts a short ray downwards from the center of the collider to detect ground.
    /// </summary>
    private bool IsGrounded()
    {
        Bounds bounds = ownCollider != null ? ownCollider.bounds : new Bounds(transform.position, Vector3.one);
        float distance = bounds.extents.y + GroundCheckMargin;

        // A ray that starts inside the own collider does not hit it, so no self-detection.
        return Physics.Raycast(bounds.center, Vector3.down, distance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private static void ConfigureRigidbody(Rigidbody rb)
    {
        if (rb == null)
        {
            return;
        }

        rb.useGravity = true; // Gravity brings the cube back down after jumping.
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Do not tumble on contact.
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth visual motion.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Avoid tunneling.
    }

    private static InputAction CreateDefaultMoveAction()
    {
        var action = new InputAction("Move", InputActionType.Value);

        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        action.AddBinding("<Gamepad>/leftStick");

        return action;
    }

    private static InputAction CreateDefaultJumpAction()
    {
        var action = new InputAction("Jump", InputActionType.Button);

        action.AddBinding("<Keyboard>/space");
        action.AddBinding("<Gamepad>/buttonSouth");

        return action;
    }
}
