using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Items.Data
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Expedition0/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Main Data")]
        public string itemName;
        public bool isStackable;
        [ShowIf(nameof(isStackable))] [Range(1, 99)] public int maxStack = 99;
        public bool isConsumable;
        [Header("Prefabs")]
        public GameObject pickupPrefab;
        public GameObject heldPrefab;
        public GameObject inventoryPrefab; // Small "hologram" of item
        [Header("Sounds")]
        public AudioClip equipSound; // Equip or pickup
        public AudioClip holsterSound;
    }
}