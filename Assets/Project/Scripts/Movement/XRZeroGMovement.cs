using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace Expedition0.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class XRZeroGMovement : MonoBehaviour
    {
        [Header("Mode Control")]
        [Tooltip("If true, Zero-G physics are active. If false, standard gravity applies.")]
        [SerializeField]
        private bool isZeroG = true;

        [Header("References")] [SerializeField]
        private XROrigin xrOrigin;

        [Tooltip("The standard Move Provider to disable when Zero-G is active.")] [SerializeField]
        private ContinuousMoveProvider standardMoveProvider;

        [Header("Input")] [SerializeField] private InputActionProperty moveInputSource;

        [Header("Zero-G Physics Settings")] [SerializeField]
        private float accelerationSpeed = 5.0f;

        [SerializeField] private float maxSpeed = 4.0f;

        [Tooltip("How fast you stop when releasing input (0 = no friction, 10 = fast stop)")] [SerializeField]
        private float drag = 1.0f;

        [Header("Gravity (Standard) Settings")] [SerializeField]
        private float gravityForce = -9.81f;

        [SerializeField] private float walkingSpeed = 2.5f;

        // Internal Physics State
        private CharacterController characterController;
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 gravityVelocity = Vector3.zero; // Separate vector for gravity logic

        void Start()
        {
            characterController = GetComponent<CharacterController>();

            // Auto-find references if missing
            if (xrOrigin == null) xrOrigin = GetComponentInParent<XROrigin>();
            if (standardMoveProvider == null) standardMoveProvider = GetComponent<ContinuousMoveProvider>();

            UpdateModeState();
        }

        void Update()
        {
            // 1. Read Input (Left Stick)
            Vector2 input = moveInputSource.action?.ReadValue<Vector2>() ?? Vector2.zero;

            if (isZeroG)
            {
                HandleZeroGMovement(input);
            }
            else
            {
                HandleGravityMovement(input);
            }
        }

        private void HandleZeroGMovement(Vector2 input)
        {
            Transform headTransform = xrOrigin.Camera.transform;

            // --- 1. Calculate Acceleration Vector ---
            // We use the Camera's raw Forward/Right vectors. 
            // This includes the Y-axis (Pitch), allowing you to fly where you look.
            Vector3 targetDirection = (headTransform.forward * input.y) + (headTransform.right * input.x);

            // Normalize to prevent faster diagonal movement, but keep magnitude for analog stick control
            if (targetDirection.magnitude > 1f) targetDirection.Normalize();

            // --- 2. Apply Acceleration ---
            // V = V + (Dir * Accel * dt)
            currentVelocity += targetDirection * (accelerationSpeed * Time.deltaTime);

            // --- 3. Apply Drag (Friction) ---
            // This stops us from accelerating infinitely and slows us down when input stops
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, drag * Time.deltaTime);

            // --- 4. Clamp Max Speed ---
            if (currentVelocity.magnitude > maxSpeed)
            {
                currentVelocity = currentVelocity.normalized * maxSpeed;
            }

            // --- 5. Apply Move ---
            characterController.Move(currentVelocity * Time.deltaTime);
        }

        private void HandleGravityMovement(Vector2 input)
        {
            // Standard floor movement logic
            Transform headTransform = xrOrigin.Camera.transform;

            // Flatten vectors to floor (ignore pitch)
            Vector3 forward = headTransform.forward;
            Vector3 right = headTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * input.y + right * input.x).normalized;

            // Gravity Logic
            if (characterController.isGrounded && gravityVelocity.y < 0)
            {
                gravityVelocity.y = -2f; // Stick to ground
            }

            gravityVelocity.y += gravityForce * Time.deltaTime;

            // Combine
            Vector3 finalMove = (moveDir * walkingSpeed) + gravityVelocity;
            characterController.Move(finalMove * Time.deltaTime);

            // Reset Zero-G velocity so we don't carry momentum when switching modes
            currentVelocity = Vector3.zero;
        }

        // Public method to toggle modes (call this from your triggers/events)
        public void SetZeroG(bool active)
        {
            isZeroG = active;
            UpdateModeState();

            // Reset momentum when switching to prevent glitches
            currentVelocity = Vector3.zero;
            gravityVelocity = Vector3.zero;
        }

        private void UpdateModeState()
        {
            // If we have a standard provider, disable it when this script is handling movement
            // to prevent "Fighting" between two movement scripts.
            if (standardMoveProvider != null)
            {
                standardMoveProvider.enabled = false; // We fully take over movement in BOTH modes
            }
        }
    }
}