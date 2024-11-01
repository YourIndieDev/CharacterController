using UnityEngine;

namespace Indie
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float crouchMultiplier = 0.5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float airControl = 0.3f;

        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private float slopeLimit = 45f;

        [Header("Movement Smoothing")]
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float movementSmoothing = 0.1f;

        [Header("Camera Settings")]
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;

        // Private variables
        private Rigidbody rb;
        private Vector3 moveDirection;
        private Vector3 currentVelocity;
        private float verticalRotation;
        private bool isGrounded;
        private bool isCrouching;
        private bool isSprinting;
        private float originalHeight;
        private Vector3 groundNormal;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            originalHeight = transform.localScale.y;

            if (playerCamera == null)
                playerCamera = Camera.main.transform;

            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleInput();
            UpdateGroundCheck();
            HandleRotation();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInput()
        {
            // Get input values
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Calculate move direction relative to camera
            moveDirection = new Vector3(horizontal, 0, vertical).normalized;
            moveDirection = transform.TransformDirection(moveDirection);

            // Handle jumping
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            // Handle crouching
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCrouch();
            }

            // Handle sprinting
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        private void HandleRotation()
        {
            // Mouse look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Vertical rotation (looking up/down)
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

            // Horizontal rotation (turning left/right)
            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleMovement()
        {
            float currentSpeed = moveSpeed;

            // Adjust speed based on state
            if (isCrouching)
                currentSpeed *= crouchMultiplier;
            else if (isSprinting)
                currentSpeed *= sprintMultiplier;

            // Apply air control if not grounded
            if (!isGrounded)
                currentSpeed *= airControl;

            // Calculate target velocity
            Vector3 targetVelocity = moveDirection * currentSpeed;

            // Project movement on slopes
            if (isGrounded && groundNormal != Vector3.up)
            {
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal);
            }

            // Smooth movement
            Vector3 smoothedVelocity = Vector3.SmoothDamp(
                rb.velocity,
                targetVelocity,
                ref currentVelocity,
                movementSmoothing
            );

            // Preserve vertical velocity
            smoothedVelocity.y = rb.velocity.y;

            // Apply movement
            rb.velocity = smoothedVelocity;
        }

        private void UpdateGroundCheck()
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                out hit,
                groundCheckDistance + 0.1f,
                groundMask
            );

            if (isGrounded)
            {
                groundNormal = hit.normal;
                // Check if slope is too steep
                if (Vector3.Angle(groundNormal, Vector3.up) > slopeLimit)
                {
                    isGrounded = false;
                }
            }
            else
            {
                groundNormal = Vector3.up;
            }
        }

        private void ToggleCrouch()
        {
            isCrouching = !isCrouching;
            Vector3 newScale = transform.localScale;
            newScale.y = isCrouching ? originalHeight * 0.5f : originalHeight;
            transform.localScale = newScale;

            // Adjust camera position
            Vector3 newCameraPos = playerCamera.localPosition;
            newCameraPos.y = isCrouching ? originalHeight * 0.5f : originalHeight;
            playerCamera.localPosition = newCameraPos;
        }

        // Public methods for external control
        public void SetMovementEnabled(bool enabled)
        {
            rb.isKinematic = !enabled;
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public bool IsGrounded()
        {
            return isGrounded;
        }

        public bool IsCrouching()
        {
            return isCrouching;
        }

        public bool IsSprinting()
        {
            return isSprinting;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize ground check
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                transform.position,
                transform.position + Vector3.down * groundCheckDistance
            );
        }
    }
}
