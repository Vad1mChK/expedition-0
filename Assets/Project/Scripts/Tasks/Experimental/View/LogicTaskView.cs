using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Tasks.Experimental
{
    public abstract class LogicTaskView : MonoBehaviour
    {
        public UnityEvent onCorrect;
        public UnityEvent onIncorrect;
        public UnityEvent onNthIncorrect;
        [SerializeField, Min(1)] protected int maxErrorsCount = 3;

        [Header("Debug, Status")]
        private int _attemptsCount;
        private int _errorsCount;
        private int _consecutiveErrorsCount;

        
        public int AttemptsCount => _attemptsCount;
        public int ErrorsCount => _errorsCount;
        public int ConsecutiveErrorsCount => _consecutiveErrorsCount;
        
        
        public virtual void ValidateTask()
        {
            ++_attemptsCount;
            
            bool correct = ValidateTaskInternal();
            if (correct)
            {
                _consecutiveErrorsCount = 0;
                onCorrect?.Invoke();
            } else {
                ++_errorsCount;
                ++_consecutiveErrorsCount;
                onIncorrect?.Invoke();

                if (_consecutiveErrorsCount != 0 && _consecutiveErrorsCount % maxErrorsCount == 0)
                {
                    onNthIncorrect?.Invoke();
                }
            }
        }

        protected abstract bool ValidateTaskInternal();
    }
}