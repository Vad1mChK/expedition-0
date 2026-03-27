using System.Collections.Generic;
using Expedition0.Items.Data;
using Expedition0.Items.Inventory;
using Expedition0.Items.Core;
using Expedition0.Items.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Expedition0.Items.Equipment
{
    public sealed class HandEquipment : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventory;
        [SerializeField] private InventoryWheel inventoryWheel;
        [SerializeField] private Transform heldItemMount;

        [Header("Input Actions (Right Hand)")]
        [SerializeField] private InputActionProperty triggerPressedAction;       // Trigger press
        [SerializeField] private InputActionProperty buttonAAction;             // Equip/Holster
        [SerializeField] private InputActionProperty buttonBAction;             // Inventory open/close
        [SerializeField] private InputActionProperty rightStickAction;          // Vector2 or Axis; we use X

        [Header("Inventory Spin")]
        [Range(0.1f, 0.95f)] [SerializeField] private float spinThreshold = 0.7f;
        [Range(0.0f, 0.9f)]  [SerializeField] private float spinRearmThreshold = 0.3f;
        [Min(0.0f)]          [SerializeField] private float spinCooldownSeconds = 0.18f;

        [Header("Suppression While Inventory Open (disable snap turn)")]
        [SerializeField] private Behaviour[] disableBehavioursWhenInventoryOpen;
        [SerializeField] private InputActionProperty[] disableActionsWhenInventoryOpen;

        [Header("Suppression While Item Equipped (disable 'usual trigger' interactions)")]
        [SerializeField] private Behaviour[] disableBehavioursWhenItemEquipped;
        [SerializeField] private InputActionProperty[] disableActionsWhenItemEquipped;

        private GameObject _heldObject;
        private ItemHeld _heldItem;
        private ItemData _heldData;

        private bool _inventoryOpen;

        private bool _spinArmed = true;
        private float _nextSpinAllowedTime;

        private readonly HashSet<InputAction> _actionsDisabledByInventory = new();
        private readonly HashSet<InputAction> _actionsDisabledByEquip = new();

        private void Awake()
        {
            if (inventory == null) inventory = InventoryManager.Instance;
        }

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.Changed += HandleInventoryChanged;
                inventory.ItemAdded += HandleItemAdded;
            }

            BindAction(triggerPressedAction, OnTriggerPerformed, OnTriggerCanceled);
            BindAction(buttonAAction, OnButtonAPerformed, null);
            BindAction(buttonBAction, OnButtonBPerformed, null);

            BindAction(rightStickAction, OnRightStickPerformed, OnRightStickCanceled);

            // Ensure inventory starts closed (unless you want otherwise).
            SetInventoryOpen(_inventoryOpen);
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.Changed -= HandleInventoryChanged;
                inventory.ItemAdded -= HandleItemAdded;
            }

            UnbindAction(triggerPressedAction, OnTriggerPerformed, OnTriggerCanceled);
            UnbindAction(buttonAAction, OnButtonAPerformed, null);
            UnbindAction(buttonBAction, OnButtonBPerformed, null);

            UnbindAction(rightStickAction, OnRightStickPerformed, OnRightStickCanceled);

            // Restore anything we suppressed.
            ApplyInventorySuppression(false);
            ApplyEquippedSuppression(false);
        }

        private void HandleItemAdded(ItemData data)
        {
            if (data == null) return;

            // Make the picked-up item the "current" wheel selection if the wheel supports it.
            // If you don't implement SelectItemById, this call can be removed.
            if (inventoryWheel) inventoryWheel.SelectItemById(data.itemId);

            // Equip immediately on pickup if it has a held prefab.
            if (data.heldPrefab != null)
                Equip(data);
        }

        private void HandleInventoryChanged()
        {
            // If the currently held item disappears from inventory (consumed to 0, removed, etc.), holster it.
            if (_heldData == null || inventory == null) return;

            if (inventory.GetCount(_heldData.itemId) <= 0)
                Holster();
        }

        private void OnButtonBPerformed(InputAction.CallbackContext ctx)
        {
            SetInventoryOpen(!_inventoryOpen);
        }

        private void SetInventoryOpen(bool open)
        {
            _inventoryOpen = open;

            if (inventoryWheel != null)
                inventoryWheel.gameObject.SetActive(open);

            ApplyInventorySuppression(open);

            // When opening, arm spin so the first flick registers.
            if (open)
            {
                _spinArmed = true;
                _nextSpinAllowedTime = 0f;
            }
        }

        private void OnButtonAPerformed(InputAction.CallbackContext ctx)
        {
            // A: equip/holster the current (selected) item, if any.
            if (inventoryWheel == null)
            {
                // If wheel is missing, treat A as "holster current".
                if (_heldItem != null) Holster();
                return;
            }

            if (!inventoryWheel.TryGetSelectedItemData(out var selectedData) || selectedData == null)
            {
                if (_heldItem != null) Holster();
                return;
            }

            // Toggle: if selected already equipped -> holster; else equip.
            if (_heldData == selectedData)
                Holster();
            else
                Equip(selectedData);
        }

        private void OnRightStickPerformed(InputAction.CallbackContext ctx)
        {
            if (!_inventoryOpen) return;
            if (inventoryWheel == null) return;

            float x = ReadStickX(ctx);
            HandleSpin(x);
        }

        private void OnRightStickCanceled(InputAction.CallbackContext ctx)
        {
            // Rearm when stick returns to rest.
            _spinArmed = true;
        }

        private void HandleSpin(float x)
        {
            float abs = Mathf.Abs(x);

            if (abs <= spinRearmThreshold)
            {
                _spinArmed = true;
                return;
            }

            if (!_spinArmed) return;
            if (abs < spinThreshold) return;

            float now = Time.unscaledTime;
            if (now < _nextSpinAllowedTime) return;

            int delta = x > 0f ? 1 : -1;
            inventoryWheel.MoveSelection(delta);

            _spinArmed = false;
            _nextSpinAllowedTime = now + spinCooldownSeconds;
        }

        private void OnTriggerPerformed(InputAction.CallbackContext ctx)
        {
            // Trigger:
            // - If an item is equipped and inventory is closed: use it.
            // - Otherwise do nothing, letting "usual" trigger logic run elsewhere.
            if (_inventoryOpen) return;

            if (_heldItem != null)
                _heldItem.ProcessTrigger(true);
        }

        private void OnTriggerCanceled(InputAction.CallbackContext ctx)
        {
            if (_inventoryOpen) return;

            if (_heldItem != null)
                _heldItem.ProcessTrigger(false);
        }

        public bool Equip(ItemData data)
        {
            if (inventory == null || data == null) return false;

            if (heldItemMount == null)
            {
                Debug.LogError("HandEquipment: heldItemMount is not assigned.");
                return false;
            }

            if (inventory.GetCount(data.itemId) <= 0)
                return false;

            if (data.heldPrefab == null)
                return false;

            // If equipping the same item, do nothing.
            if (_heldData == data && _heldObject != null)
                return true;

            Holster();

            _heldObject = Instantiate(data.heldPrefab, heldItemMount, false);
            _heldData = data;

            _heldItem = _heldObject.GetComponentInChildren<ItemHeld>(true);
            if (_heldItem != null)
            {
                _heldItem.Initialize(data, inventory);
                _heldItem.OnEquip();
            }
            else
            {
                Debug.LogWarning($"Held prefab for '{data.itemId}' has no ItemHeld component.");
            }

            ApplyEquippedSuppression(true);
            return true;
        }

        public void Holster()
        {
            if (_heldItem != null)
                _heldItem.OnHolster();

            if (_heldObject != null)
                Destroy(_heldObject);

            _heldObject = null;
            _heldItem = null;
            _heldData = null;

            ApplyEquippedSuppression(false);
        }

        private void ApplyInventorySuppression(bool suppress)
        {
            SetBehavioursEnabled(disableBehavioursWhenInventoryOpen, !suppress);
            SetActionsEnabled(disableActionsWhenInventoryOpen, !suppress, _actionsDisabledByInventory);
        }

        private void ApplyEquippedSuppression(bool suppress)
        {
            SetBehavioursEnabled(disableBehavioursWhenItemEquipped, !suppress);
            SetActionsEnabled(disableActionsWhenItemEquipped, !suppress, _actionsDisabledByEquip);
        }

        private static void SetBehavioursEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null) return;

            for (int i = 0; i < behaviours.Length; i++)
            {
                var b = behaviours[i];
                if (b == null) continue;
                b.enabled = enabled;
            }
        }

        private static void SetActionsEnabled(
            InputActionProperty[] properties,
            bool enabled,
            HashSet<InputAction> trackingSet)
        {
            if (properties == null) return;

            for (int i = 0; i < properties.Length; i++)
            {
                var action = properties[i].action;
                if (action == null) continue;

                if (!enabled)
                {
                    if (action.enabled)
                    {
                        action.Disable();
                        trackingSet.Add(action);
                    }
                }
                else
                {
                    // Only re-enable actions we disabled ourselves.
                    if (trackingSet.Remove(action))
                        action.Enable();
                }
            }
        }

        private static float ReadStickX(InputAction.CallbackContext ctx)
        {
            // Works if your rightStickAction is either:
            // - Vector2 (thumbstick)
            // - Float (already mapped to X)
            var valueType = ctx.valueType;

            if (valueType == typeof(Vector2))
                return ctx.ReadValue<Vector2>().x;

            if (valueType == typeof(float))
                return ctx.ReadValue<float>();

            // Fallback: attempt Vector2, else 0.
            try { return ctx.ReadValue<Vector2>().x; }
            catch { return 0f; }
        }

        private static void BindAction(
            InputActionProperty actionProperty,
            System.Action<InputAction.CallbackContext> performed,
            System.Action<InputAction.CallbackContext> canceled)
        {
            var action = actionProperty.action;
            if (action == null) return;

            action.Enable();

            if (performed != null) action.performed += performed;
            if (canceled != null) action.canceled += canceled;
        }

        private static void UnbindAction(
            InputActionProperty actionProperty,
            System.Action<InputAction.CallbackContext> performed,
            System.Action<InputAction.CallbackContext> canceled)
        {
            var action = actionProperty.action;
            if (action == null) return;

            if (performed != null) action.performed -= performed;
            if (canceled != null) action.canceled -= canceled;

            action.Disable();
        }
    }
}
