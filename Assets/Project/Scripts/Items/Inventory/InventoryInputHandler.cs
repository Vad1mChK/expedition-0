using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Expedition0.Items.Inventory
{
    public class InventoryInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager manager;
        [SerializeField] private InventoryWheel wheelUI;
        [SerializeField] private SnapTurnProvider snapTurnProvider;

        [Header("Input Actions")]
        [SerializeField] private InputActionProperty openButton; // A Button
        [SerializeField] private InputActionProperty holsterButton; // B Button
        [SerializeField] private InputActionProperty scrollJoystick; // Right Stick
        [SerializeField] private InputActionProperty triggerButton;

        private bool _isInventoryOpen = false;
        private float _scrollThreshold = 0.5f;
        private bool _canScroll = true;

        private void Update()
        {
            if (!_isInventoryOpen)
            {
                // Closed State: A opens
                if (openButton.action.WasPressedThisFrame() && manager.HasItems)
                    SetInventoryState(true);

                // Closed State: B holsters/equips
                if (holsterButton.action.WasPressedThisFrame())
                    manager.ToggleHolsterLast();
            }
            else
            {
                HandleInventoryInput();
            }
        }

        private void SetInventoryState(bool open)
        {
            _isInventoryOpen = open;
            wheelUI.gameObject.SetActive(open);
            // Requirement: Disable snap turns when open
            snapTurnProvider.enabled = !open;
            
            if (open) wheelUI.Refresh();
        }

        private void HandleInventoryInput()
        {
            // B exits and holsters
            if (holsterButton.action.WasPressedThisFrame())
            {
                manager.Holster();
                SetInventoryState(false);
                return;
            }

            // A selects and exits
            if (openButton.action.WasPressedThisFrame())
            {
                manager.EquipItem(manager.GetCurrentData());
                SetInventoryState(false);
                return;
            }

            if (triggerButton.action.WasPressedThisFrame())
            {
                manager.PassTriggerInput(true);
            }

            if (triggerButton.action.WasReleasedThisFrame())
            {
                manager.PassTriggerInput(false);
            }

            // Joystick Ring Buffer Scrolling
            Vector2 stick = scrollJoystick.action.ReadValue<Vector2>();
            if (Mathf.Abs(stick.x) > _scrollThreshold && _canScroll)
            {
                manager.ChangeSelection(stick.x > 0 ? 1 : -1);
                wheelUI.Refresh();
                _canScroll = false; // Prevent rapid scrolling
            }
            else if (Mathf.Abs(stick.x) < 0.2f)
            {
                _canScroll = true;
            }
        }
    }
}