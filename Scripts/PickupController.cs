using UnityEngine;
using System.Collections.Generic;

namespace Indie
{
    // Interface for objects that can be picked up
    public interface IPickupable
    {
        public void OnPickup(GameObject holder); // Called when the object is picked up
        public void OnDrop(Vector3 dropPosition, Vector3 dropVelocity); // Called when the object is dropped
        public float GetWeight(); // Returns the weight of the object (affects movement speed)
        public bool CanBePickedUp(); // Checks if the item is available for pickup
    }

    public interface IOnHold
    {
        public void OnHold();
        public void OnRelease();
    }

    public class PickupController : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false; // Enable debug mode to print messages

        [Header("Pickup Settings")]
        [SerializeField] private Transform holdPosition; // Position where the picked-up item will be held
        [SerializeField] private float pickupRange = 3f; // Range within which items can be picked up
        [SerializeField] private float throwForce = 10f; // Force with which items are thrown
        [SerializeField] private LayerMask pickupLayer; // Layer to specify which objects are pickupable
        [SerializeField] private float weightSpeedMultiplier = 0.7f; // Multiplier for movement speed based on held object's weight

        [Header("UI Settings")]
        [SerializeField] private bool showPickupPrompt = true; // Whether to show the pickup prompt text
        [SerializeField] private string pickupPromptText = "Press E to pickup"; // Text for pickup prompt
        [SerializeField] private string dropPromptText = "Press E to drop"; // Text for drop prompt


        // References
        private CharacterController characterController; // Reference to CharacterController
        private Camera playerCamera; // Reference to player camera
        private GameObject heldObject; // Reference to the currently held object
        private Rigidbody heldRigidbody; // Rigidbody of the held object
        private IPickupable heldPickupable; // Interface reference for the held pickupable object
        private float originalMoveSpeed; // Stores the original movement speed of the character

        // State tracking
        private bool isHolding; // Whether the player is currently holding an object
        private Vector3 lastHeldObjectPosition; // Stores the last position of the held object
        private float holdDistance; // Distance at which the object is held from the player

        private void Start()
        {
            // Get references
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("No CharacterController found on this GameObject!");
                enabled = false;
                return;
            }

            playerCamera = Camera.main;
            if (holdPosition == null)
            {
                // Create hold position if not set
                GameObject holdPositionObj = new GameObject("HoldPosition");
                holdPosition = holdPositionObj.transform;
                holdPosition.SetParent(playerCamera.transform);
                holdPosition.localPosition = new Vector3(0, -0.5f, 2f); // Slightly below and in front of camera
            }

            // Store original move speed
            originalMoveSpeed = characterController.GetMoveSpeed();

            if (debugMode) Debug.Log("PickupController initialized.");
        }

        private void Update()
        {
            // Handle input for pickup and drop
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isHolding)
                {
                    TryPickup();
                }
                else
                {
                    DropHeldObject();
                }
            }

            // Handle input for throwing the object (right mouse button)
            if (Input.GetMouseButtonDown(1) && isHolding)
            {
                ThrowHeldObject();
            }

            // Update position of held object or check for pickupable items
            if (isHolding)
            {
                UpdateHeldObjectPosition();
            }
            else
            {
                CheckForPickupable();
            }
        }

        private void TryPickup()
        {
            // Find all objects within the pickup range
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

            foreach (Collider collider in colliders)
            {
                IPickupable pickupable = collider.GetComponent<IPickupable>();
                if (pickupable != null && pickupable.CanBePickedUp())
                {
                    PickupObject(collider.gameObject, pickupable);
                    if (debugMode) Debug.Log("Picked up object: " + collider.gameObject.name);
                    return; // Exit after picking up the first valid object
                }
            }

            if (debugMode) Debug.Log("No pickupable object within range.");
        }

        private void PickupObject(GameObject obj, IPickupable pickupable)
        {
            heldObject = obj;
            heldPickupable = pickupable;
            heldRigidbody = obj.GetComponent<Rigidbody>();

            if (heldRigidbody != null)
            {
                //heldRigidbody.isKinematic = true; // Prevent held object from moving
                heldRigidbody.interpolation = RigidbodyInterpolation.None;
            }

            // Calculate hold distance based on object size
            Bounds bounds = obj.GetComponent<Collider>().bounds;
            holdDistance = bounds.extents.magnitude + 1f;

            isHolding = true;
            lastHeldObjectPosition = obj.transform.position;

            // Modify movement speed based on weight
            float weightMultiplier = Mathf.Lerp(1f, weightSpeedMultiplier, pickupable.GetWeight());
            characterController.SetMoveSpeed(originalMoveSpeed * weightMultiplier);

            obj.TryGetComponent(out Collider collider);
            collider.enabled = false; // Disable collider to prevent clipping through walls

            obj.TryGetComponent(out IOnHold onHold);
            onHold?.OnHold();

            // Notify the object that it has been picked up
            pickupable.OnPickup(gameObject);
        }

        private void UpdateHeldObjectPosition()
        {
            if (heldObject != null)
            {
                Vector3 targetPosition = holdPosition.position;
                Vector3 smoothedPosition = Vector3.Lerp(lastHeldObjectPosition, targetPosition, Time.deltaTime * 10f);

                // Check for collisions to prevent held object from clipping through surfaces
                Ray ray = new Ray(playerCamera.transform.position, (targetPosition - playerCamera.transform.position).normalized);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, holdDistance))
                {
                    if (hit.collider.gameObject != heldObject)
                    {
                        smoothedPosition = hit.point;
                    }
                }

                heldObject.transform.position = smoothedPosition;
                lastHeldObjectPosition = smoothedPosition;

                // Optional: Rotate object to face the same direction as the camera
                heldObject.transform.rotation = Quaternion.Lerp(
                    heldObject.transform.rotation,
                    playerCamera.transform.rotation,
                    Time.deltaTime * 5f
                );

                if (debugMode) Debug.Log("Updating held object position.");
            }
        }

        private void DropHeldObject()
        {
            if (heldObject != null && heldPickupable != null)
            {
                Vector3 dropVelocity = characterController.GetComponent<Rigidbody>().velocity;

                if (heldRigidbody != null)
                {
                    //heldRigidbody.isKinematic = false; // Enable physics on drop
                    heldRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                    heldRigidbody.velocity = dropVelocity; // Apply velocity on drop
                }

                // Notify the object that it has been dropped
                heldPickupable.OnDrop(heldObject.transform.position, dropVelocity);

                heldObject.TryGetComponent(out Collider collider);
                collider.enabled = true; // Enable collider to prevent clipping through walls

                heldObject.TryGetComponent(out IOnHold onHold);
                onHold?.OnRelease();

                if (debugMode) Debug.Log("Dropped object: " + heldObject.name);

                // Reset held object
                ResetHeldObject();
            }
        }

        private void ThrowHeldObject()
        {
            if (heldObject != null && heldPickupable != null)
            {
                Vector3 throwDirection = playerCamera.transform.forward;
                Vector3 throwVelocity = throwDirection * throwForce;

                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = false; // Enable physics for throwing
                    heldRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                    heldRigidbody.velocity = throwVelocity;
                }

                // Notify the object that it has been thrown
                heldPickupable.OnDrop(heldObject.transform.position, throwVelocity);

                if (debugMode) Debug.Log("Threw object: " + heldObject.name);

                // Reset held object
                ResetHeldObject();
            }
        }

        private void ResetHeldObject()
        {
            heldObject = null;
            heldPickupable = null;
            heldRigidbody = null;
            isHolding = false;

            // Reset movement speed
            characterController.SetMoveSpeed(originalMoveSpeed);

            if (debugMode) Debug.Log("Reset held object.");
        }

        private void CheckForPickupable()
        {
            // Check for objects within pickup range
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

            foreach (Collider collider in colliders)
            {
                IPickupable pickupable = collider.GetComponent<IPickupable>();
                if (pickupable != null && pickupable.CanBePickedUp() && showPickupPrompt)
                {
                    // Show pickup prompt (log prompt to console if debugMode is on)
                    if (debugMode) Debug.Log(pickupPromptText);
                    break; // Only show prompt for one nearby object
                }
            }
        }

        // Public methods for external control
        public bool IsHoldingObject()
        {
            return isHolding;
        }

        public GameObject GetHeldObject()
        {
            return heldObject;
        }

        public void SetPickupRange(float range)
        {
            pickupRange = range;
        }

        public void SetThrowForce(float force)
        {
            throwForce = force;
        }
    }
}
