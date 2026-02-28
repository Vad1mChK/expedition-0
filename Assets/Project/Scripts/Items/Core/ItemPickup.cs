using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Expedition0.Items.Data;
using Expedition0.Items.Inventory;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Expedition0.Items.Core
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemData data;
        
        [SerializeField] private XRGrabInteractable _interactable;
        [SerializeField] private Outline _outline;

        private void Awake()
        {
            if (_interactable == null) _interactable = GetComponent<XRGrabInteractable>();
            if (_outline == null) _outline = GetComponent<Outline>();
            
            // XRI Event: When grabbed, add to inventory
            _interactable.selectEntered.AddListener(OnPickedUp);
            
            if (_outline) _outline.enabled = false;
            _interactable.firstHoverEntered.AddListener(_ => { if(_outline) _outline.enabled = true; });
            _interactable.lastHoverExited.AddListener(_ => { if(_outline) _outline.enabled = false; });
        }

        private void OnPickedUp(SelectEnterEventArgs args)
        {
            // Find manager in scene (or use a Singleton/Service Locator)
            var manager = FindFirstObjectByType<InventoryManager>();
            if (manager)
            {
                manager.AddItem(data);
                Destroy(gameObject);
            }
        }
    }
}