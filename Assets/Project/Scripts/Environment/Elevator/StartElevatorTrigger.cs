using UnityEngine;

namespace Expedition0.Environment.Elevator
{
    public class StartElevatorTrigger : MonoBehaviour
    {
        [SerializeField] private ElevatorController elevator;

        private void Reset()
        {
            if (!elevator)
                elevator = GetComponentInParent<ElevatorController>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (elevator == null) return;
            if (elevator.Kind != ElevatorKind.StartOfLevel) return;

            // Player leaves: close and lock forever.
            elevator.CloseDoor();
            elevator.SetLocked(true);
        }
    }
}