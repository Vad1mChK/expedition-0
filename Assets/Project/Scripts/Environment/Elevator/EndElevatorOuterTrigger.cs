using UnityEngine;

namespace Expedition0.Environment.Elevator
{
    public class EndElevatorOuterTrigger : MonoBehaviour
    {
        [SerializeField] private ElevatorController elevator;

        private void Reset()
        {
            if (!elevator)
                elevator = GetComponentInParent<ElevatorController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (elevator == null) return;
            if (elevator.Kind != ElevatorKind.EndOfLevel) return;
            if (elevator.Locked) return;

            elevator.OpenDoor();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (elevator == null) return;
            if (elevator.Kind != ElevatorKind.EndOfLevel) return;
            if (elevator.Locked) return;

            elevator.CloseDoor();
        }
    }
}