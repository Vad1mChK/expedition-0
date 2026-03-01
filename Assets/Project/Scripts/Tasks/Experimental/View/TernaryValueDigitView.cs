using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TernaryValueDigitView : TernaryValueNodeView
    {
        [Header("Value Display")]
        [SerializeField] protected bool balancedView = false;
        [SerializeField] protected bool colored = false;
        [SerializeField] protected List<TritSpritePair> initialUnbalancedSprites;
        [SerializeField] protected List<TritSpritePair> initialBalancedSprites;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        private Dictionary<Trit, Sprite> _unbalancedSprites;
        private Dictionary<Trit, Sprite> _balancedSprites;

        private void Awake()
        {
            if (Model == null) BuildModel();
            _unbalancedSprites = initialUnbalancedSprites
                .ToDictionary(p => p.trit, p => p.sprite);
            _balancedSprites = initialBalancedSprites
                .ToDictionary(p => p.trit, p => p.sprite);
            UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();

            var value = (Model != null) ? (Trit)Model.EvaluateInt() : Trit.Neutral;
            if (
                (balancedView ? _balancedSprites : _unbalancedSprites)
                    .TryGetValue(value, out var sprite)
                )
            {
                spriteRenderer.sprite = sprite;
            }

            if (colored)
            {
                spriteRenderer.color = TaskColorUtil.GetColorForTrit(value) * 3;
            }
        }
    }
}