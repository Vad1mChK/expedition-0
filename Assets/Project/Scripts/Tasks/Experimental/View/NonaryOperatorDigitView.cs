using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public sealed class NonaryOperatorDigitView : NonaryOperatorNodeView
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<NonaryOperatorSpritePair> initialOperatorSprites;

        private Dictionary<NonaryOperatorType, Sprite> _sprites = new();

        private void Awake()
        {
            _sprites = initialOperatorSprites.ToDictionary(x => x.op, x => x.sprite);
        }

        public override void UpdateView()
        {
            base.UpdateView();
            if (_sprites.TryGetValue(Node.op, out var sprite))
                spriteRenderer.sprite = sprite;
        }
    }
}