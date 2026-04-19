using UnityEngine;
using UnityEngine.InputSystem;

namespace Expedition0.Command
{
    public sealed class CommandDisplayInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CommandResponseDisplay display;
        
        [Header("Input Action")]
        [Tooltip("Link the 'XRI LeftHand/Activate' or a custom 'Dismiss' action here.")]
        [SerializeField] private InputActionReference dismissActionReference;

        private void OnEnable()
        {
            if (dismissActionReference == null) return;

            // 1. Subscribe to the 'performed' event (when the trigger is pressed)
            dismissActionReference.action.performed += OnDismissPerformed;
            
            // 2. Ensure the action is enabled
            dismissActionReference.action.Enable();
        }

        private void OnDisable()
        {
            if (dismissActionReference == null) return;

            // 3. Unsubscribe to prevent memory leaks or errors when the object is destroyed
            dismissActionReference.action.performed -= OnDismissPerformed;
        }

        private void OnDismissPerformed(InputAction.CallbackContext context)
        {
            // Only dismiss if the display is currently active
            if (display != null && display.IsVisible)
            {
                display.Dismiss();
            }
        }
    }
}