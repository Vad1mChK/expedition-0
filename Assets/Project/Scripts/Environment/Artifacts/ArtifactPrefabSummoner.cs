using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Environment.Elevator;
using Expedition0.Items.Core;
using Expedition0.Items.Data;
using Expedition0.Save.Experimental;
using UnityEngine;

namespace Expedition0.Environment
{
    public class ArtifactPrefabSummoner : PrefabSummoner
    {
        [SerializeField] private List<ElevatorController> elevatorControllers = new();
        [SerializeField] private bool autoFindElevators = true;

        private void Start()
        {
            if (elevatorControllers.Count == 0 && autoFindElevators)
            {
                elevatorControllers = FindObjectsByType<ElevatorController>(
                        FindObjectsInactive.Exclude, FindObjectsSortMode.None
                        ).ToList();
            }
        }

        public override void SummonAllResolved()
        {
            foreach (var entry in summonResolver.Resolve())
            {
                var gameObj = SummonEntry(entry);
                
                if (gameObj && gameObj.TryGetComponent<ItemPickup>(out var pickup))
                {
                    pickup.onBeforeDestroy?.AddListener(OnBeforeDestroy);
                }
            }
        }

        private void OnBeforeDestroy()
        {
            foreach (var elevator in elevatorControllers)
            {
                if (!elevator) continue;
                elevator.ReevaluateLockState();
            }
        }
    }
}