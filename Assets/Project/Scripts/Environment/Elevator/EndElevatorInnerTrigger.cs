using UnityEngine;

namespace Expedition0.Environment.Elevator
{
    public class EndElevatorInnerTrigger : MonoBehaviour
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

            // Player stepped fully inside an unlocked end-of-level elevator:
            // close doors, then transport after delay.
            elevator.BeginTransportIfPossible();
        }
    }
}