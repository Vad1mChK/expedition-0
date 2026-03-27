using Expedition0.Items.Data;
using Expedition0.Items.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Items.Core
{
    /// <summary>
    /// Base class for anything held in the hand. 
    /// Receives input events from the VR Controller.
    /// </summary>
    public class ItemHeld : MonoBehaviour
    {
        public UnityEvent onTriggerPressed;
        public UnityEvent onTriggerReleased;
        
        public ItemData ItemData { get; private set; }
        public InventoryManager Inventory { get; private set; }
        
        /// <summary>
        /// Called by the hand/equipment code immediately after instantiating the held prefab.
        /// </summary>
        public void Initialize(ItemData itemData, InventoryManager inventory)
        {
            ItemData = itemData;
            Inventory = inventory;
        }

        // Called by the InventoryWheel or HandController
        public void ProcessTrigger(bool pressed)
        {
            if (pressed) OnTriggerPressed();
            else OnTriggerReleased();
        }

        public virtual void OnEquip() 
        {
            // Reset animations or play sounds
        }

        public virtual void OnHolster()
        {
            
        }

        public virtual void OnTriggerPressed()
        {
            onTriggerPressed?.Invoke();
        }

        public virtual void OnTriggerReleased()
        {
            onTriggerReleased?.Invoke();
        }
    }
}