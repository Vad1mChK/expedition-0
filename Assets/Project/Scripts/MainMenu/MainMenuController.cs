using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Expedition0.Save;
using Expedition0.Save.Experimental;

namespace Expedition0.MainMenu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        public enum Panel
        {
            MainMenu,
            Settings,
            About
        }

        [Serializable]
        public struct PanelRef
        {
            public Panel panel;
            public CanvasGroup group; // root for that page
        }

        [Header("Panels")] [SerializeField] private List<PanelRef> panels;
        [SerializeField] private Panel startPanel = Panel.MainMenu;

        [Header("Buttons")] [SerializeField] private Button continueButton;

        [Header("Scene Loading")] [Tooltip("Scene for a fresh start (New Game).")] [SerializeField]
        private string newGameSceneName;

        [Tooltip("Fallback scene if no rule matches (can be empty to hide Continue).")] [SerializeField]
        private string defaultContinueSceneName;

        private readonly Dictionary<Panel, CanvasGroup> _map = new();

        private void Awake()
        {
            _map.Clear();
            foreach (var p in panels)
                if (p.group)
                    _map[p.panel] = p.group;
        }

        private void Start()
        {
            ShowPanel(startPanel);
            UpdateContinueButton();
        }

        // ——— UI hooks ———
        public void OnPressNewGame()
        {
            // if (!string.IsNullOrEmpty(newGameSceneName))
            // {
            //     SaveManager.ResetSave();
            //     SceneManager.LoadScene(newGameSceneName);
            // }
            PlaythroughLifecycleManager.Instance?.ResetPlaythroughProgress();
            PlaythroughLifecycleManager.Instance?.LoadRespawnLevel();
        }

        public void OnPressContinue()
        {
            // var scene = ResolveContinueScene(SaveManager.LoadProgress());
            // if (!string.IsNullOrEmpty(scene))
            //     SceneManager.LoadScene(scene);
            PlaythroughLifecycleManager.Instance?.LoadRespawnLevel();
        }

        public void OnPressSettings()
        {
            ShowPanel(Panel.Settings);
        }

        public void OnPressAbout()
        {
            ShowPanel(Panel.About);
        }

        public void OnPressBack()
        {
            ShowPanel(Panel.MainMenu);
        }

        public void OnPressExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        // ——— Internals ———
        private void ShowPanel(Panel target)
        {
            foreach (var kv in _map)
                SetGroup(kv.Value, kv.Key == target);
        }

        private void UpdateContinueButton()
        {
            if (!continueButton) return;
            var can = PlaythroughLifecycleManager.Instance.CurrentData.taskSolvedCount > 0;
            Debug.Log($"Can Continue: {can}");
            continueButton.gameObject.SetActive(can);
            continueButton.interactable = can;
        }

        private static void SetGroup(CanvasGroup g, bool on)
        {
            if (!g) return;
            g.alpha = on ? 1f : 0f;
            g.interactable = on;
            g.blocksRaycasts = on;
            // Keep objects active for layout; CG handles interaction.
            g.gameObject.SetActive(true);
        }
    }
}