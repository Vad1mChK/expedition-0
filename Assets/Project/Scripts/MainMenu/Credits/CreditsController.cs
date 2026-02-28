using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Expedition0.MainMenu.Credits
{
    public class CreditsController : MonoBehaviour
    {
        private enum CreditsState { Idle, Running, Paused, Transitioning }
        
        [Header("Data Source")]
        [SerializeField] private CreditsBook creditsBook;

        [Header("UI References")]
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform contentHolder;

        [Header("Prefabs")]
        [SerializeField] private GameObject pagePrefab;
        [SerializeField] private GameObject headerEntryPrefab;
        [SerializeField] private GameObject textEntryPrefab;

        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 1.0f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private bool autoStart = true;
        
        [Header("Events")]
        public UnityEvent OnCreditsFinished;

        // Internal State
        private CreditsState _currentState = CreditsState.Idle;
        private float _pageWidth;
        private int _currentPageIndex = 0;
        private Coroutine _creditsRoutine;
        private List<CanvasGroup> _pageGroups = new List<CanvasGroup>();
        private float _timeOnCurrentPage = 0f; // Used for Pause/Resume

        // Public Accessors
        public bool IsPaused => _currentState == CreditsState.Paused;
        public bool IsRunning => _currentState == CreditsState.Running;

        // --- CORE UNITY METHODS ---

        private void Start()
        {
            if (autoStart)
            {
                BeginCredits();
            }
        }
        
        private void Update()
        {
            if (_currentState == CreditsState.Running)
            {
                // Increment time only when actively running
                _timeOnCurrentPage += Time.deltaTime;
                
                // Check for auto-advance threshold
                if (_currentPageIndex <= creditsBook.pages.Count && 
                    _timeOnCurrentPage >= creditsBook.pages[_currentPageIndex].displayDuration)
                {
                    ToNext();
                }
            }
        }

        // --- PUBLIC CONTROL METHODS (Hook to Buttons) ---

        public void BeginCredits()
        {
            if (creditsBook == null || creditsBook.pages.Count == 0)
            {
                Debug.LogWarning("CreditsBook is empty or null.");
                OnCreditsFinished?.Invoke();
                
                Debug.Log("Credits: Nothing to show, completed.");
                return;
            }

            GenerateCreditsUI();

            if (_creditsRoutine != null) StopCoroutine(_creditsRoutine);
            // Start the sequence with an immediate transition to the first page (index 0)
            _creditsRoutine = StartCoroutine(InitialStartSequence());
        }

        public void ToNext()
        {
            if (_currentState == CreditsState.Transitioning)
            {
                return; // Ignore if already moving or on the last page
            }
            
            // Immediately transition to the next page
            StopAutoAdvance();
            _creditsRoutine = StartCoroutine(TransitionPage(_currentPageIndex, _currentPageIndex + 1));
        }

        public void ToPrevious()
        {
            if (_currentState == CreditsState.Transitioning || _currentPageIndex <= 0)
            {
                return; // Ignore if already moving or on the first page
            }
            
            // Immediately transition to the previous page
            StopAutoAdvance();
            _creditsRoutine = StartCoroutine(TransitionPage(_currentPageIndex, _currentPageIndex - 1));
        }

        public void Pause()
        {
            if (_currentState == CreditsState.Running)
            {
                _currentState = CreditsState.Paused;
                Debug.Log("Credits Paused.");
            }
        }

        public void Resume()
        {
            if (_currentState == CreditsState.Paused)
            {
                // When resuming, we allow the Update loop to continue
                _currentState = CreditsState.Running;
                Debug.Log("Credits Resumed.");
            }
        }
        
        public void SkipAll()
        {
            StopAllCoroutines();
            _currentState = CreditsState.Transitioning; // Lock state temporarily
            
            // Set final page position and fade everything else out
            contentHolder.anchoredPosition = new Vector2(-1 * (creditsBook.pages.Count - 1) * _pageWidth, 0);

            // Instant fade out all but the last page
            for (int i = 0; i < _pageGroups.Count - 1; i++)
            {
                _pageGroups[i].alpha = 0f;
            }
            // Ensure the last page is fully opaque (if pages exist)
            if (_pageGroups.Count > 0)
            {
                _pageGroups[creditsBook.pages.Count - 1].alpha = 1f;
            }

            // Immediately finish
            _currentPageIndex = creditsBook.pages.Count;
            OnCreditsFinished?.Invoke();
            _currentState = CreditsState.Idle;
            
            Debug.Log("Credits: Skipped all, completed.");
        }

        // --- INTERNAL LOGIC ---
        
        private void StopAutoAdvance()
        {
            // Stop the current page's auto-advance timer
            if (_creditsRoutine != null) StopCoroutine(_creditsRoutine);
            _currentState = CreditsState.Transitioning;
            // The new transition coroutine will handle setting the state back to Running
        }

        private IEnumerator InitialStartSequence()
        {
            _currentPageIndex = 0;
            _timeOnCurrentPage = 0f;
            _currentState = CreditsState.Transitioning;

            // Ensure contentHolder is at the correct starting position (page 0)
            contentHolder.anchoredPosition = Vector2.zero;
            
            // Fade the first page in (handles requirement 1)
            yield return StartCoroutine(FadePageIn(0));
            
            _currentState = CreditsState.Running; // Allow Update loop to take over
        }

        private void ResetPageTimer()
        {
            _timeOnCurrentPage = 0f;
        }

        private void GenerateCreditsUI()
        {
            _pageGroups.Clear();
            foreach (Transform child in contentHolder)
            {
                Destroy(child.gameObject);
            }

            // We must calculate the viewport rect after the canvas updates, or we get 0
            Canvas.ForceUpdateCanvases();
            _pageWidth = viewport.rect.width;
            
            float currentXOffset = 0f;

            foreach (var pageData in creditsBook.pages)
            {
                // ... (Creation and positioning logic is the same) ...
                GameObject pageObj = Instantiate(pagePrefab, contentHolder);
                RectTransform pageRect = pageObj.GetComponent<RectTransform>();
                
                pageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _pageWidth);
                pageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewport.rect.height);
                
                pageRect.anchorMin = new Vector2(0, 0.5f);
                pageRect.anchorMax = new Vector2(0, 0.5f);
                pageRect.pivot = new Vector2(0, 0.5f);
                pageRect.anchoredPosition = new Vector2(currentXOffset, 0f);

                CanvasGroup canvasGroup = pageObj.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    Debug.LogError("Page Prefab is missing a CanvasGroup component!");
                    return;
                }
                
                canvasGroup.alpha = 0f;
                _pageGroups.Add(canvasGroup);

                Transform pageContentParent = pageObj.transform; 
                
                // Force layout immediately after setting content to prevent visible snapping (as before)
                foreach (var entry in pageData.entries)
                {
                    GameObject entryPrefab = entry.type == CreditsEntry.EntryType.Header 
                        ? headerEntryPrefab 
                        : textEntryPrefab;

                    GameObject entryObj = Instantiate(entryPrefab, pageContentParent);
                    if (entryObj.TryGetComponent<TMP_Text>(out var textComp))
                    {
                        textComp.text = entry.content;
                        
                        LayoutRebuilder.ForceRebuildLayoutImmediate(textComp.rectTransform);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(pageContentParent.GetComponent<RectTransform>());
                    }
                }
                
                currentXOffset += _pageWidth;
            }

            contentHolder.anchoredPosition = Vector2.zero;
        }

        private IEnumerator TransitionPage(int oldPageIndex, int newPageIndex)
        {
            // Prevent auto-advance check during transition
            _currentState = CreditsState.Transitioning;
            
            // Bounds check
            if (newPageIndex < 0 || newPageIndex >= _pageGroups.Count)
            {
                // This handles the end of the credits run
                OnCreditsFinished?.Invoke();
                _currentState = CreditsState.Idle;
                Debug.Log("Credits: Watched all, completed.");
                yield break;
            }

            // --- ANIMATION SETUP ---
            _currentPageIndex = newPageIndex; // Update index immediately
            
            float scrollTime = 0f;
            Vector2 startPos = contentHolder.anchoredPosition;
            Vector2 targetScrollPos = new Vector2(-1 * newPageIndex * _pageWidth, startPos.y);

            CanvasGroup oldGroup = _pageGroups[oldPageIndex];
            CanvasGroup newGroup = _pageGroups[newPageIndex];
            
            float fadeTime = 0f;
            float maxDuration = Mathf.Max(transitionDuration, fadeInDuration);

            // --- ANIMATION LOOP ---
            while (scrollTime < maxDuration)
            {
                scrollTime += Time.deltaTime;
                fadeTime += Time.deltaTime;
                
                // A. Smooth Scroll (Sine Interpolation)
                if (scrollTime < transitionDuration)
                {
                    float tScroll = scrollTime / transitionDuration;
                    // Sine Ease-In-Out formula
                    float sinSmoothedT = (1f - Mathf.Cos(Mathf.PI * tScroll)) / 2f; 
                    contentHolder.anchoredPosition = Vector2.Lerp(startPos, targetScrollPos, sinSmoothedT);
                }
                
                // B. Fade Control
                if (fadeTime < fadeInDuration)
                {
                    float tFade = fadeTime / fadeInDuration;
                    float sinSmoothedT = (1f - Mathf.Cos(Mathf.PI * tFade)) / 2f;
                    // Fade OUT the old page (can fade out faster than the new one fades in)
                    oldGroup.alpha = Mathf.Lerp(1f, 0f, sinSmoothedT); 
                    
                    // Fade IN the new page
                    newGroup.alpha = Mathf.Lerp(0f, 1f, sinSmoothedT); 
                }

                yield return null;
            }

            // C. Cleanup and Finalize
            contentHolder.anchoredPosition = targetScrollPos;
            oldGroup.alpha = 0f;
            newGroup.alpha = 1f;

            ResetPageTimer();
            _currentState = CreditsState.Running; // Resume auto-advance check
        }
        
        private IEnumerator FadePageIn(int pageIndex)
        {
            CanvasGroup group = _pageGroups[pageIndex];
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                group.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            group.alpha = 1f;
        }
    }
}