using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Arcade car controller driven by a Rigidbody and the new Input System.
/// W accelerates, S brakes and then drives in reverse, horizontal mouse movement steers (yaw),
/// and Space jumps (double jump by default) while gravity pulls the car down.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CarController : MonoBehaviour
{
    private const float GroundCheckMargin = 0.1f;

    private Rigidbody body;
    private Collider ownCollider;

    private float throttleInput;
    private float reverseInput;
    private float pendingYaw;
    private bool jumpRequested;

    private float currentSpeed;
    private int jumpsUsed;

    [Header("Speed")]
    [Tooltip("Maximum forward speed in units per second.")]
    [SerializeField] private float maxSpeed = 15f;

    [Tooltip("Speed gained per second while accelerating.")]
    [SerializeField] private float acceleration = 8f;

    [Tooltip("Maximum speed when driving in reverse, in units per second.")]
    [SerializeField] private float maxReverseSpeed = 6f;

    [Tooltip("Speed gained per second while accelerating in reverse.")]
    [SerializeField] private float reverseAcceleration = 6f;

    [Tooltip("Speed lost per second when pressing the opposite direction while still moving.")]
    [SerializeField] private float brakeDeceleration = 20f;

    [Tooltip("Speed lost per second when neither accelerating nor braking.")]
    [SerializeField] private float coastDeceleration = 3f;

    [Header("Steering")]
    [Tooltip("Degrees of rotation per pixel of horizontal mouse movement.")]
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Tooltip("Lock and hide the cursor while the car is active (press Esc in the Editor to release it).")]
    [SerializeField] private bool lockCursor = true;

    [Header("Jump")]
    [Tooltip("Maximum height reached by a jump, in units.")]
    [SerializeField] private float jumpHeight = 2f;

    [Tooltip("Maximum number of jumps before touching the ground again (2 = double jump).")]
    [Min(1)]
    [SerializeField] private int maxJumps = 2;

    [Tooltip("Layers considered as ground for the jump check.")]
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Input")]
    [SerializeField] private InputAction accelerateAction = new InputAction("Accelerate", InputActionType.Button);
    [FormerlySerializedAs("brakeAction")]
    [SerializeField] private InputAction reverseAction = new InputAction("Reverse", InputActionType.Button);
    [SerializeField] private InputAction steerAction = new InputAction("Steer", InputActionType.Value);
    [SerializeField] private InputAction jumpAction = new InputAction("Jump", InputActionType.Button);

    /// <summary>Current speed in units per second (negative when reversing).</summary>
    public float CurrentSpeed => currentSpeed;

    private void Reset()
    {
        // Default bindings, applied when the component is added or reset.
        accelerateAction = CreateButtonAction("Accelerate", "<Keyboard>/w");
        reverseAction = CreateButtonAction("Reverse", "<Keyboard>/s");
        steerAction = CreateDefaultSteerAction();
        jumpAction = CreateButtonAction("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
        ConfigureRigidbody(GetComponent<Rigidbody>());
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();

        // Safety net in case the actions were serialized without bindings.
        if (accelerateAction.bindings.Count == 0) accelerateAction = CreateButtonAction("Accelerate", "<Keyboard>/w");
        if (reverseAction.bindings.Count == 0) reverseAction = CreateButtonAction("Reverse", "<Keyboard>/s");
        if (steerAction.bindings.Count == 0) steerAction = CreateDefaultSteerAction();
        if (jumpAction.bindings.Count == 0) jumpAction = CreateButtonAction("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        accelerateAction.Enable();
        reverseAction.Enable();
        steerAction.Enable();
        jumpAction.Enable();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        accelerateAction.Disable();
        reverseAction.Disable();
        steerAction.Disable();
        jumpAction.Disable();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        throttleInput = accelerateAction.IsPressed() ? 1f : 0f;
        reverseInput = reverseAction.IsPressed() ? 1f : 0f;

        // Mouse delta is per frame, so accumulate it until the next physics step.
        pendingYaw += steerAction.ReadValue<float>() * mouseSensitivity;

        if (jumpAction.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        UpdateSpeed(Time.fixedDeltaTime);
        ApplySteering();

        // Horizontal velocity follows the car's facing; vertical velocity is left to gravity.
        Vector3 velocity = transform.forward * currentSpeed;
        velocity.y = body.linearVelocity.y;

        HandleJump(ref velocity);

        body.linearVelocity = velocity;
        body.angularVelocity = Vector3.zero; // Collisions must not spin the car.
    }

    private void UpdateSpeed(float deltaTime)
    {
        bool forward = throttleInput > 0f && reverseInput <= 0f;
        bool backward = reverseInput > 0f && throttleInput <= 0f;

        if (forward)
        {
            // If still rolling backwards, brake first; then accelerate forwards.
            currentSpeed += (currentSpeed < 0f ? brakeDeceleration : acceleration) * deltaTime;
        }
        else if (backward)
        {
            // If still rolling forwards, brake first; then accelerate in reverse.
            currentSpeed -= (currentSpeed > 0f ? brakeDeceleration : reverseAcceleration) * deltaTime;
        }
        else
        {
            // No input (or both keys): slowly roll to a stop in either direction.
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, coastDeceleration * deltaTime);
        }

        // Negative speed means driving in reverse.
        currentSpeed = Mathf.Clamp(currentSpeed, -maxReverseSpeed, maxSpeed);
    }

    private void ApplySteering()
    {
        if (Mathf.Approximately(pendingYaw, 0f))
        {
            return;
        }

        Quaternion turn = Quaternion.Euler(0f, pendingYaw, 0f);
        body.MoveRotation(body.rotation * turn);
        pendingYaw = 0f;
    }

    private void HandleJump(ref Vector3 velocity)
    {
        bool grounded = IsGrounded();

        // Refill jumps only when resting on the ground, not while still rising.
        if (grounded && velocity.y <= 0.01f)
        {
            jumpsUsed = 0;
        }

        if (!jumpRequested)
        {
            return;
        }

        jumpRequested = false;

        // Driving off a ledge counts as the first jump.
        if (!grounded && jumpsUsed == 0)
        {
            jumpsUsed = 1;
        }

        if (jumpsUsed < maxJumps)
        {
            jumpsUsed++;
            // v = sqrt(2 * g * h) gives exactly the desired jump height.
            velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
        }
    }

    /// <summary>
    /// Casts a short ray downwards from the center of the collider to detect ground.
    /// </summary>
    private bool IsGrounded()
    {
        Bounds bounds = ownCollider.bounds;
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

        rb.useGravity = true;
        // Only yaw is allowed, and it is driven by code; the car never tips over.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private static InputAction CreateButtonAction(string name, params string[] bindings)
    {
        var action = new InputAction(name, InputActionType.Button);
        foreach (string path in bindings)
        {
            action.AddBinding(path);
        }
        return action;
    }

    private static InputAction CreateDefaultSteerAction()
    {
        // Horizontal mouse movement in pixels per frame.
        var action = new InputAction("Steer", InputActionType.Value, expectedControlType: "Axis");
        action.AddBinding("<Mouse>/delta/x");
        return action;
    }
}
