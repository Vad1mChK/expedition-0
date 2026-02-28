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

        // Called by the InventoryWheel or HandController
        public virtual void ProcessTrigger(bool pressed)
        {
            if (pressed) onTriggerPressed?.Invoke();
            else onTriggerReleased?.Invoke();
        }

        public virtual void OnEquip() 
        {
            // Reset animations or play sounds
        }

        public virtual void OnTriggerPressed()
        {
            ProcessTrigger(true);
        }

        public virtual void OnTriggerReleased()
        {
            ProcessTrigger(false);
        }
    }
}