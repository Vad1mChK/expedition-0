using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TaskEqualSign : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] renderers;
        [SerializeField] private int equalSignSlot = 1;
        [SerializeField] private int crossSignSlot = 2;

        [Header("Materials")]
        [SerializeField] private Material offMaterial;
        [SerializeField] private Material equalsMaterial;
        [SerializeField] private Material notEqualsMaterial;

        [SerializeField] private TaskEqualSignState initialState = TaskEqualSignState.Off;

        private TaskEqualSignState _state;

        public enum TaskEqualSignState
        {
            Off,
            Equals,
            NotEquals
        }

        private void Awake()
        {
            _state = initialState;
            UpdateView();
        }

        public void UpdateView()
        {
            foreach (var rend in renderers)
            {
                if (rend == null) continue;

                // copy array
                var mats = rend.materials;

                if (equalSignSlot >= 0 && equalSignSlot < mats.Length)
                {
                    mats[equalSignSlot] = _state switch
                    {
                        TaskEqualSignState.Equals => equalsMaterial,
                        TaskEqualSignState.NotEquals => notEqualsMaterial,
                        _ => offMaterial
                    };
                }

                if (crossSignSlot >= 0 && crossSignSlot < mats.Length)
                {
                    mats[crossSignSlot] = _state switch
                    {
                        TaskEqualSignState.NotEquals => notEqualsMaterial,
                        _ => offMaterial
                    };
                }

                // assign back
                rend.materials = mats;
            }
        }

        public void SetState(int stateIdx)
        {
            _state = (TaskEqualSignState)stateIdx;
            UpdateView();
        }
        
        public void SetState(TaskEqualSignState state)
        {
            if (_state != state)
            {
                _state = state;
                UpdateView();
            }
        }
    }
}