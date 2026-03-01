using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public sealed class NonaryValueDigitView : NonaryValueNodeView
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<NonaryDigitSpritePair> initialDigitSprites;
        [SerializeField] private bool colored = false;

        private Dictionary<int, Sprite> _sprites;

        private void Awake()
        {
            if (Model == null) BuildModel();
            _sprites = initialDigitSprites.ToDictionary(x => x.digit, x => x.sprite);
            UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();
            var digit = Mathf.Clamp(Node.currentValue, 0, 8);

            if (_sprites.TryGetValue(digit, out var sprite))
                spriteRenderer.sprite = sprite;
            
            if (colored)
            {
                spriteRenderer.color = TaskColorUtil.GetColorFor(digit / 9f) * 3;
            }
        }
    }
}