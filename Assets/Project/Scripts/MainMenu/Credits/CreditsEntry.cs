using System;
using UnityEngine;

namespace Expedition0.MainMenu.Credits
{
    [Serializable]
    public class CreditsEntry
    {
        public enum EntryType
        {
            Header,
            Text
        }

        public EntryType type;

        [TextArea(1, 4)]
        public string content;
    }
}
    