using Expedition0.Save.Experimental;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Expedition0.Tasks.Experimental
{
    public abstract class LogicTaskView : MonoBehaviour
    {
        [Header("Events on Attempt Solution")]
        public UnityEvent onFirstCorrect;
        public UnityEvent onCorrect;
        public UnityEvent onIncorrect;
        public UnityEvent onNthIncorrect;

        [Header("Solution Validation")]
        [SerializeField, Min(1)] protected int maxErrorsCount = 3;

        [Header("Debug, Status")]
        private int _attemptsCount;
        private int _errorsCount;
        private int _consecutiveErrorsCount;
        private bool _hasBeenCorrectlySolved;

        
        public int AttemptsCount => _attemptsCount;
        public int ErrorsCount => _errorsCount;
        public int ConsecutiveErrorsCount => _consecutiveErrorsCount;
        public bool HasBeenCorrectlySolved => _hasBeenCorrectlySolved;
        
        
        public virtual void ValidateTask()
        {
            ++_attemptsCount;
            
            bool correct = ValidateTaskInternal();
            if (correct)
            {
                if (!_hasBeenCorrectlySolved)
                {   // Only on first correct solution 
                    onFirstCorrect?.Invoke();
                    _hasBeenCorrectlySolved = true;
                }
                
                _consecutiveErrorsCount = 0;
                onCorrect?.Invoke();
            } else {
                ++_errorsCount;
                ++_consecutiveErrorsCount;
                onIncorrect?.Invoke();

                if (_consecutiveErrorsCount != 0 && _consecutiveErrorsCount % maxErrorsCount == 0)
                {  // Only on every Nth consecutive error
                    onNthIncorrect?.Invoke();
                }
            }
        }

        protected abstract bool ValidateTaskInternal();
    }
}