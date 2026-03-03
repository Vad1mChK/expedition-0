using UnityEngine;
using UnityEngine.UI;
using System;

namespace Expedition0.MainMenu.UI
{
    [Serializable]
    public struct TabPair
    {
        public string tabName;
        public Button tabButton;
        public GameObject tabPanel;
    }
}