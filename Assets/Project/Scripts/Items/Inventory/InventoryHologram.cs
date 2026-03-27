using TMPro;
using UnityEngine;

namespace Expedition0.Items.Inventory
{
    public sealed class InventoryHologram : MonoBehaviour
    {
        [SerializeField] private GameObject countRoot;
        [SerializeField] private TMP_Text countText;

        public void SetCount(int count, bool show)
        {
            if (countRoot != null) countRoot.SetActive(show);
            if (show && countText != null) countText.text = count.ToString();
        }
    }
}