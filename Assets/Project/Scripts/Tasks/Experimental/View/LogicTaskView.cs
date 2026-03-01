using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Tasks.Experimental
{
    public abstract class LogicTaskView : MonoBehaviour
    {
        public UnityEvent onCorrect;
        public UnityEvent onIncorrect;
        public UnityEvent onNthIncorrect;
        [SerializeField] protected int maxErrorsCount = 3;
        private int _errorsCount;
        
        public virtual void ValidateTask()
        {
            bool correct = ValidateTaskInternal();

            if (correct)
            {
                onCorrect?.Invoke();
                _errorsCount = 0;
                return;
            }

            onIncorrect?.Invoke();
            ++_errorsCount;

            if (_errorsCount >= maxErrorsCount)
            {
                onNthIncorrect?.Invoke();
                _errorsCount = 0;
            }
        }

        protected abstract bool ValidateTaskInternal();
    }
}