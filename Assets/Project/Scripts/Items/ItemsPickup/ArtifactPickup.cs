using System;
using Expedition0.Items.Core;
using Expedition0.Save;
using Expedition0.Save.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Expedition0.Items.ItemsPickup
{
    public class ArtifactPickup : ItemPickup
    {
        [Header("Artifact-Specific Settings")]
        // [SerializeField] private GameProgress progressIncrement; // Replace with the newer progress system once it's ready
        [SerializeField] private UnityEvent onAcquire;
        [SerializeField] private UnityEvent onBeforeDestroy;

        protected override void OnPickedUp(SelectEnterEventArgs args)
        {
            // SaveManager.SetCompleted(progressIncrement); // Replace with the newer progress system once it's ready
            onAcquire?.Invoke();
            
            base.OnPickedUp(args);
        }

        private void OnDestroy()
        {
            onBeforeDestroy?.Invoke();
        }
    }
}