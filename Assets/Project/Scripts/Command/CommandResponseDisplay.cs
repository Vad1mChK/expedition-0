using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expedition0.Command
{
    public class CommandResponseDisplay : MonoBehaviour
    {
        public enum TruthTableOperator
        {
            // Ternary Unary:
            Identity,
            Not,
            // Ternary Binary:
            And,
            Or,
            Xor,
            ImplKleene,
            ImplLukasiewicz,
            // Nonary Binary:
            NonaryPlus,
            NonaryMinus,
            NonaryConcat
        }
        
        [Serializable]
        public class TruthTableSpriteEntry
        {
            public TruthTableOperator op;
            public bool balanced;
            public Sprite sprite;
        }

        [Header("Timing")]
        [Tooltip("How long the UI stays fully visible before auto-fading.")]
        [SerializeField, Range(0, 20)] private float stayTimeSeconds = 3f;
        [Tooltip("Duration of the fade-out animation.")]
        [SerializeField, Range(0.1f, 10)] private float fadeTimeSeconds = 1.5f;
        [SerializeField] private AnimationCurve fadeAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("UI References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private TMP_Text recognizedTextElement;
        [SerializeField] private TMP_Text responseTextElement;
        [SerializeField] private Image spriteImage;
        
        [Header("Resizing")]
        [SerializeField] private float largeHeight = 384f;
        [SerializeField] private float smallHeight = 216f;
        [SerializeField] private float largeTextSize = 24;
        [SerializeField] private float smallTextSize = 20;
        
        [Header("Truth Table Specific")]
        [SerializeField] private List<TruthTableSpriteEntry> truthTableSprites;
        private Dictionary<(TruthTableOperator, bool), Sprite> _truthTableSpritesDict = new();

        private Coroutine _displayRoutine;
        public bool IsVisible => canvas.enabled;

        private void Awake()
        {
            _truthTableSpritesDict = truthTableSprites.ToDictionary(
                entry => (entry.op, entry.balanced),
                entry => entry.sprite
            );
            
            // Start hidden
            canvas.enabled = false;
            canvasGroup.alpha = 0f;
        }

        public void DisplayCommandResult(
            CommandOpcode opcode,
            string recognizedText = null,
            string responseText = null,
            [CanBeNull] Dictionary<string, object> recognizedArgs = null
        )
        {
            if (_displayRoutine != null) StopCoroutine(_displayRoutine);

            // Configure Visuals
            SetRecognizedText(recognizedText ?? "");
            SetResponseText(responseText ?? "");

            switch (opcode)
            {
                case CommandOpcode.HINT_TRUTHTABLE:
                    string op = recognizedArgs?.GetValueOrDefault("operator", "Xor")?.ToString() ?? "Xor";
                    bool balanced = Convert.ToBoolean(recognizedArgs?.GetValueOrDefault("balanced", false));
                    bool hasSprite = SetTruthTable(op, balanced);
                    
                    AdjustLayout(hasSprite ? largeHeight : smallHeight, hasSprite ? smallTextSize : largeTextSize);
                    break;
                default:
                    DisableTruthTable();
                    AdjustLayout(smallHeight, largeTextSize);
                    break;
            }

            // Show UI
            canvas.enabled = true;
            canvasGroup.alpha = 1f;
            
            // Start the auto-hide sequence
            _displayRoutine = StartCoroutine(DisplaySequence());
        }

        /// <summary>
        /// Public method to be called by input actions (e.g. Left Trigger) to close the UI immediately.
        /// </summary>
        public void Dismiss()
        {
            if (!canvas.enabled) return;
            
            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            _displayRoutine = StartCoroutine(FadeOutRoutine());
        }

        private void AdjustLayout(float height, float fontSize)
        {
            if (contentPanel != null)
                contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, height);
            
            responseTextElement.fontSize = fontSize;
        }

        private IEnumerator DisplaySequence()
        {
            // 1. Stay visible
            yield return new WaitForSeconds(stayTimeSeconds);
            
            // 2. Begin Fade
            yield return FadeOutRoutine();
        }

        private IEnumerator FadeOutRoutine()
        {
            float timer = fadeTimeSeconds;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                // t goes from 1 to 0
                float t = timer / fadeTimeSeconds;
                canvasGroup.alpha = fadeAnimationCurve.Evaluate(t);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvas.enabled = false;
            _displayRoutine = null;
        }

        private void SetResponseText(string text)
        {
            responseTextElement.enabled = !(string.IsNullOrWhiteSpace(text));
            responseTextElement.text = text;
        }

        private void SetResponseFontSize(float size)
        {
            responseTextElement.fontSize = size;
        }

        private void SetRecognizedText(string text)
        {
            recognizedTextElement.enabled = !(string.IsNullOrWhiteSpace(text));
            recognizedTextElement.text = text;
        }
        
        private void AdjustPanelHeight(float targetHeight)
        {
            if (contentPanel == null) return;
            contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, targetHeight);
        }

        private bool SetTruthTable(string opString, bool balanced)
        {
            // Normalize string: convert "impl_kleene" or "implkleene" to "ImplKleene" if needed
            // or just use a case-insensitive match.
            TruthTableOperator op = opString.ToLower() switch
            {
                "identity"        => TruthTableOperator.Identity,
                "not"             => TruthTableOperator.Not,
                "and"             => TruthTableOperator.And,
                "or"              => TruthTableOperator.Or,
                "xor"             => TruthTableOperator.Xor,
                "implkleene"      => TruthTableOperator.ImplKleene,
                "impllukasiewicz" => TruthTableOperator.ImplLukasiewicz,
                "nonaryplus"      => TruthTableOperator.NonaryPlus,
                "nonaryminus"     => TruthTableOperator.NonaryMinus,
                "nonaryconcat"    => TruthTableOperator.NonaryConcat,
                _                 => TruthTableOperator.Identity
            };

            if (!_truthTableSpritesDict.TryGetValue((op, balanced), out var foundSprite))
            {
                spriteImage.enabled = false;
                spriteImage.gameObject.SetActive(false);
                return false;
            }
    
            spriteImage.enabled = true;
            spriteImage.gameObject.SetActive(true);
            spriteImage.sprite = foundSprite;
            return true;
        }
        
        private void DisableTruthTable()
        {
            spriteImage.gameObject.SetActive(false);
            spriteImage.enabled = false;
        }
    }
}