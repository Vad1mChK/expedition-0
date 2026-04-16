using System.Collections.Generic;
using Expedition0.Items.Data;
using Expedition0.Items.Inventory;
using Expedition0.Items.Core;
using Expedition0.Items.UI;
using Expedition0.Save.Experimental;
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

        [Header("Input Actions")]
        [SerializeField] private InputActionProperty triggerPressedAction;
        [SerializeField] private InputActionProperty buttonAAction; // Equip/Holster
        [SerializeField] private InputActionProperty buttonBAction; // Inventory Toggle
        [SerializeField] private InputActionProperty rightStickAction;

        [Header("Spin Settings")]
        [SerializeField] private float spinThreshold = 0.7f;
        [SerializeField] private float spinRearmThreshold = 0.3f;
        [SerializeField] private float spinCooldownSeconds = 0.15f;

        [Header("Suppression")]
        [SerializeField] private Behaviour[] disableWhenInventoryOpen;
        [SerializeField] private Behaviour[] disableWhenItemEquipped;

        private GameObject _heldObject;
        private ItemHeld _heldItem;
        private ItemData _heldData;

        private bool _inventoryOpen;
        private bool _spinArmed = true;
        private float _nextSpinAllowedTime;

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

            SetInventoryOpen(false);
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
        }

        private void HandleItemAdded(ItemData data)
        {
            if (data == null) return;

            // Update the wheel selection and auto-equip
            if (inventoryWheel) inventoryWheel.SelectItemById(data.itemId);
            if (_heldObject == null && data.heldPrefab != null) Equip(data);
        }

        private void HandleInventoryChanged()
        {
            // If our held item is no longer in the SaveData inventory, put it away.
            if (_heldData != null && inventory.GetCount(_heldData.itemId) <= 0)
            {
                Holster();
            }
        }

        private void OnButtonBPerformed(InputAction.CallbackContext ctx) => SetInventoryOpen(!_inventoryOpen);

        private void SetInventoryOpen(bool open)
        {
            _inventoryOpen = open;
            if (inventoryWheel != null) inventoryWheel.gameObject.SetActive(open);
            
            // Hand/UI Mutual Exclusion
            // If the inventory is OPEN, the hand should be HIDDEN.
            // If the inventory is CLOSED, the hand should be SHOWN (if we have an item).
            if (_heldObject != null)
            {
                _heldObject.SetActive(!open);
            }
            else if (!open)
            {
                if (inventoryWheel != null && inventoryWheel.TryGetSelectedItemData(out var selected))
                {
                    Equip(selected);
                }
            }
            
            // Toggle behaviours (like snap turn)
            foreach (var b in disableWhenInventoryOpen) 
                if (b) b.enabled = !open;

            if (open)
            {
                _spinArmed = true;
                _nextSpinAllowedTime = 0f;
            }
        }

        private void OnButtonAPerformed(InputAction.CallbackContext ctx)
        {
            if (inventoryWheel == null)
            {
                if (_heldItem != null) Holster();
                return;
            }

            if (inventoryWheel.TryGetSelectedItemData(out var selectedData))
            {
                if (_heldData == selectedData) Holster();
                else Equip(selectedData);
            }
        }

        private void OnRightStickPerformed(InputAction.CallbackContext ctx)
        {
            if (!_inventoryOpen) return;
            
            float x = ctx.ReadValue<Vector2>().x;
            float absX = Mathf.Abs(x);

            if (absX <= spinRearmThreshold)
            {
                _spinArmed = true;
                return;
            }

            if (_spinArmed && absX >= spinThreshold && Time.unscaledTime >= _nextSpinAllowedTime)
            {
                inventoryWheel.MoveSelection(x > 0 ? 1 : -1);
                _spinArmed = false;
                _nextSpinAllowedTime = Time.unscaledTime + spinCooldownSeconds;
            }
        }

        private void OnRightStickCanceled(InputAction.CallbackContext ctx) => _spinArmed = true;

        public void Equip(ItemData data)
        {
            if (data == null || data.heldPrefab == null) return;
            if (_heldData == data) return;

            Holster();

            _heldObject = Instantiate(data.heldPrefab, heldItemMount, false);
            
            // NEW: Ensure the newly equipped item is hidden if the inventory is currently open
            _heldObject.SetActive(!_inventoryOpen);
            
            _heldData = data;
            _heldItem = _heldObject.GetComponentInChildren<ItemHeld>(true);

            if (_heldItem)
            {
                _heldItem.Initialize(data, inventory);
                _heldItem.OnEquip();
            }

            foreach (var b in disableWhenItemEquipped) if (b) b.enabled = false;
        }

        public void Holster()
        {
            if (_heldItem != null) _heldItem.OnHolster();
            if (_heldObject != null) Destroy(_heldObject);

            _heldObject = null;
            _heldItem = null;
            _heldData = null;

            foreach (var b in disableWhenItemEquipped) if (b) b.enabled = true;
        }

        private void OnTriggerPerformed(InputAction.CallbackContext ctx)
        {
            if (!_inventoryOpen && _heldItem != null) _heldItem.ProcessTrigger(true);
        }

        private void OnTriggerCanceled(InputAction.CallbackContext ctx)
        {
            if (_heldItem != null) _heldItem.ProcessTrigger(false);
        }

        // --- Input Helpers ---

        private void BindAction(InputActionProperty prop, System.Action<InputAction.CallbackContext> perf, System.Action<InputAction.CallbackContext> canc)
        {
            var a = prop.action;
            if (a == null) return;
            a.Enable();
            if (perf != null) a.performed += perf;
            if (canc != null) a.canceled += canc;
        }

        private void UnbindAction(InputActionProperty prop, System.Action<InputAction.CallbackContext> perf, System.Action<InputAction.CallbackContext> canc)
        {
            var a = prop.action;
            if (a == null) return;
            if (perf != null) a.performed -= perf;
            if (canc != null) a.canceled -= canc;
        }
    }
}