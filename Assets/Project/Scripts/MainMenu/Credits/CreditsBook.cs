using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.MainMenu.Credits
{
    [CreateAssetMenu(fileName = "CreditsBook", menuName = "UI/Credits Book")]
    public class CreditsBook : ScriptableObject
    {
        [Tooltip("Ordered list of credit pages.")]
        public List<CreditsPage> pages = new List<CreditsPage>();
    }
}