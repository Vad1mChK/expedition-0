using System;
using System.Collections;
using System.Collections.Generic;
using Expedition0.Save;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Environment.Artifacts
{
    public class Artifact : MonoBehaviour
    {
        [Header("Artifact Impact")]
        [SerializeField] private ArtifactType artifactType;
        [SerializeField] private GameProgress progressAddition;
        [SerializeField] private UnityEvent onAcquire;
        [Header("VFX")]
        [SerializeField] private MeshRenderer[] meshRenderers;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private float delayUntilDestroy = 1f;

        private bool acquired = false;

        private void Awake()
        {
            if (meshRenderers == null || meshRenderers.Length == 0) 
                meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        public void Acquire()
        {
            if (acquired) return;
            acquired = true;
            
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
            // Save artifact acquisition
            SaveArtifact();
            onAcquire?.Invoke();

            Instantiate(pickupPrefab, transform.position, transform.rotation);
            
            // Delay to allow particle effects to play, then destroy
            StartCoroutine(DestroyAfterDelay());
        }

        private void SaveArtifact()
        {
            Debug.Log($"Before acquisition: {SaveManager.GetSaveBinary()}");
            // Add progress to the game state
            SaveManager.SetCompleted(progressAddition);
            Debug.Log($"After acquisition: {SaveManager.GetSaveBinary()}");
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(delayUntilDestroy);
            
            Destroy(gameObject);
        }

        // Optional: Add a trigger collider for automatic acquisition
        private void OnTriggerEnter(Collider other)
        {
            // Check if player entered
            if (other.CompareTag("Player") && !acquired)
            {
                Acquire();
            }
        }

        // Optional: Gizmo for visualization in editor
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}