using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Expedition0.MainMenu.UI
{
    public class TabGroupController : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private List<TabPair> tabs;
        [SerializeField] private int defaultTabIndex = 0;

        private void Start()
        {
            if (tabs == null || tabs.Count == 0) return;

            // Initialize buttons
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i; // Capture index for the lambda
                tabs[i].tabButton.onClick.AddListener(() => OnTabSelected(index));
            }

            // Set default tab
            OnTabSelected(defaultTabIndex);
        }

        public void OnTabSelected(int index)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                bool isActive = (i == index);
                
                // Toggle the Panel
                if (tabs[i].tabPanel != null)
                {
                    tabs[i].tabPanel.SetActive(isActive);
                }

                // Handle Sprite Swap Visuals
                if (isActive)
                {
                    // This forces the Button into the "Selected" state defined in its Transition settings
                    tabs[i].tabButton.Select();
                }

                // Optionally, you can also change the title text based on the selected tab
                if (isActive && titleText != null)
                {
                    titleText.text = tabs[i].tabName; // Assuming tabName is a string field in TabPair
                }
            }
        }
    }
}