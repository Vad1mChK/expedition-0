using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TernaryBinaryOperatorDigitView : TernaryBinaryOperatorNodeView
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<TernaryBinaryOperatorSpritePair> initialOperatorSprites;

        private Dictionary<TernaryBinaryOperatorType, Sprite> _sprites = new();

        private void Awake()
        {
            if (Model == null) BuildModel();
            _sprites = initialOperatorSprites.ToDictionary(x => x.op, x => x.sprite);
            UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();
            if (_sprites.TryGetValue(Node.op, out var sprite))
                spriteRenderer.sprite = sprite;
        }
    }
}