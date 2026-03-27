using Expedition0.Items.Core;
using Expedition0.Save;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Expedition0.Items.ItemsPickup
{
    public class ArtifactPickup : ItemPickup
    {
        [Header("Artifact-Specific Settings")]
        [SerializeField] private GameProgress progressIncrement; // Or replace with the newer progress system once it's ready
        [SerializeField] private UnityEvent onAcquire;

        protected override void OnPickedUp(SelectEnterEventArgs args)
        {
            SaveManager.SetCompleted(progressIncrement);
            onAcquire?.Invoke();
            
            base.OnPickedUp(args);
        }
    }
}