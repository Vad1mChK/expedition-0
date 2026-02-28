using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.MainMenu.Credits
{ 
    [Serializable]
    public class CreditsPage
    {
        [Tooltip("Optional page title (can be unused in UI).")]
        public string pageName;

        [Tooltip("Time this page stays on screen before auto-advancing.")]
        public float displayDuration = 8f;

        [Tooltip("Entries in display order.")]
        public List<CreditsEntry> entries = new List<CreditsEntry>();
    }
}