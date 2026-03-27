using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Tasks.Experimental.Hint;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Expedition0.Tasks.Experimental.View
{
    public class HintView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer panelRenderer;
        [SerializeField] private RectTransform hintCanvasRect;
        [SerializeField] private CanvasGroup hintCanvasGroup;
        [SerializeField] private TMP_Text hintTextElement;
        
        [Header("Timing")]
        [SerializeField] private float revertDelay = 5f;
        [SerializeField] private float flickerSpeed = 0.15f;
        [SerializeField] private float canvasAnimationDuration = 0.5f;
        
        [Header("Canvas Animation")]
        [SerializeField] private AnimationCurve emergenceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("State Configuration")]
        [SerializeField] private List<HintStateSpriteListPair> stateSprites;

        [Header("Colors (HDRI/Overdrive)")]
        [SerializeField, ColorUsage(false, true)] private Color idleColor = new Color(0.0274f, 0.5451f, 0.7882f) * 2f;
        [SerializeField, ColorUsage(false, true)] private Color loadingColor = new Color(0.6392f, 0.6242f, 0.6542f) * 2f;
        [SerializeField, ColorUsage(false, true)] private Color successColor = new Color(0.5804f, 0.7373f, 0.0549f) * 2f;
        [SerializeField, ColorUsage(false, true)] private Color warningColor = new Color(0.9607f, 0.4901f, 0.1373f) * 2f; 
        [SerializeField, ColorUsage(false, true)] private Color errorColor = new Color(0.8392f, 0f, 0.1176f) * 2f;
        [SerializeField, ColorUsage(false, true)] private Color offlineColor = new Color(0.0627f, 0.0627f, 0.0627f) * 2f;

        private Dictionary<HintViewState, Sprite[]> _spriteMap;
        private Coroutine _activeStateRoutine;
        private Coroutine _activeCanvasRoutine;
        
        // Target 0 = hidden, Target 1 = shown
        private float _canvasCurrentT = 0f;

        private void Awake()
        {
            _spriteMap = stateSprites.ToDictionary(x => x.state, x => x.sprites);
            
            // Initialize canvas state
            if (hintCanvasRect != null) hintCanvasRect.localScale = Vector3.zero;
            if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;
        }

        public void SetOffline() => ApplyState(HintViewState.Offline);
        public void SetIdle() => ApplyState(HintViewState.Idle);
        public void SetLoading() => ApplyState(HintViewState.Loading);
        public void SetError() => ApplyState(HintViewState.Error);

        public void HandleHintResponse(HintResponseMetadataDto metadata)
        {
            var successStates = new[] { 
                LogicTaskSolverState.Solved, 
                LogicTaskSolverState.Solvable, 
                LogicTaskSolverState.SolvableIncomplete 
            };

            HintViewState targetState = successStates.Contains(metadata.status) 
                ? HintViewState.Success 
                : HintViewState.Warning;

            hintTextElement.text = metadata.text;
            ApplyState(targetState);
        }

        public void DismissHint()
        {
            UpdateCanvasVisibility(false);
        }

        private void ApplyState(HintViewState state)
        {
            if (_activeStateRoutine != null) StopCoroutine(_activeStateRoutine);
            _activeStateRoutine = StartCoroutine(StateRoutine(state));
        }

        private IEnumerator StateRoutine(HintViewState state)
        {
            panelRenderer.color = GetColorForState(state);
            
            // Trigger animation: Success/Warning show the canvas, others hide it
            if (state == HintViewState.Success || state == HintViewState.Warning)
                UpdateCanvasVisibility(true);

            float elapsed = 0;
            Sprite[] sprites = _spriteMap.GetValueOrDefault(state);

            while (true)
            {
                // Sprite Cycling
                if (sprites != null && sprites.Length > 0)
                {
                    int index = (int)(Time.time / flickerSpeed) % sprites.Length;
                    panelRenderer.sprite = sprites[index];
                }

                // Temporary State Logic
                if (state == HintViewState.Success || state == HintViewState.Warning || state == HintViewState.Error)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed >= revertDelay)
                    {
                        // Transition back to Idle
                        ApplyState(HintViewState.Idle);
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private void UpdateCanvasVisibility(bool visible)
        {
            if (_activeCanvasRoutine != null) StopCoroutine(_activeCanvasRoutine);
            _activeCanvasRoutine = StartCoroutine(CanvasAnimationRoutine(visible));
        }

        private IEnumerator CanvasAnimationRoutine(bool visible)
        {
            float target = visible ? 1f : 0f;
            
            // If the canvas is already at the target, do nothing
            while (!Mathf.Approximately(_canvasCurrentT, target))
            {
                // Move currentT towards the target independently of framerate
                _canvasCurrentT = Mathf.MoveTowards(_canvasCurrentT, target, Time.deltaTime / canvasAnimationDuration);
                
                // Sample the curve based on the current progress
                float evaluatedValue = emergenceCurve.Evaluate(_canvasCurrentT);

                // Apply values
                if (hintCanvasRect != null)
                    hintCanvasRect.localScale = Vector3.one * evaluatedValue;
                
                if (hintCanvasGroup != null)
                    hintCanvasGroup.alpha = evaluatedValue;

                yield return null;
            }
            
            _canvasCurrentT = target;
        }

        private Color GetColorForState(HintViewState state) => state switch
        {
            HintViewState.Offline => offlineColor,
            HintViewState.Loading => loadingColor,
            HintViewState.Success => successColor,
            HintViewState.Warning => warningColor,
            HintViewState.Error   => errorColor,
            _                     => idleColor
        };
    }
}