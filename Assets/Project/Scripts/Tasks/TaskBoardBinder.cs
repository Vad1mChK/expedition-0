using UnityEngine;
using UnityEngine.UI;

namespace Expedition0.Tasks
{
    // Привязывает ASTTemplate к элементам сцены (по индексам)
    public class TaskBoardBinder : MonoBehaviour
    {
        [Header("Mapping by index")]
        public GameObject[] valueSlots;
        public GameObject[] operatorSlots;

        [Header("Optional direct Image mapping (bypass *View)")]
        public Image[] valueImages;
        public Image[] operatorImages;

        [Header("Digit Sprites")]
        public Sprite digit0Sprite;
        public Sprite digit1Sprite;
        public Sprite digit2Sprite;

        [Header("Operator Sprites")]
        public Sprite notSprite;
        public Sprite andSprite;
        public Sprite orSprite;
        public Sprite xorSprite;
        public Sprite implySprite;
        public Sprite nandSprite;
        public Sprite norSprite;
        public Sprite equivSprite;
        public Sprite implyLukSprite;

        [Header("Answer (right side)")]
        public Image answerImage;

        public void Bind(ASTTemplate template)
        { 
            // Значения
            var vs = template.ValueSlots;
            for (int i = 0; i < valueSlots.Length; i++)
            {
                if (i >= vs.Count) break;
                var slotNode = vs[i];
                var go = valueSlots[i];
                if (go != null)
                {
                    var view = go.GetComponentInChildren<ValueSlotView>();
                    if (view != null) view.BindNode(slotNode);
                }

                if (valueImages != null && i < valueImages.Length && valueImages[i] != null)
                {
                    ApplyDigitImage(valueImages[i], slotNode.CurrentValue);
                }
            }

            // Операторы
            var os = template.OperatorSlots;
            for (int i = 0; i < operatorSlots.Length; i++)
            {
                if (i >= os.Count) break;
                var slotNode = os[i];
                var go = operatorSlots[i];
                if (go != null)
                {
                    var view = go.GetComponentInChildren<OperatorSlotView>();
                    if (view != null)
                    {
                        view.BindNode(slotNode); // Правильная привязка узла!
                        Debug.Log($"TaskBoardBinder: Bound OperatorSlotView {i} to AST node with operator {slotNode.CurrentOperator}");
                    }
                }

                if (operatorImages != null && i < operatorImages.Length && operatorImages[i] != null)
                {
                    ApplyOperatorImage(operatorImages[i], slotNode.CurrentOperator);
                }
            }

            // Ответ (правая часть)
            if (answerImage != null)
            {
                ApplyDigitImage(answerImage, template.Answer);
            }
        }

        private void ApplyDigitImage(Image image, Trit? value)
        {
            if (image == null) return;

            if (!value.HasValue)
            {
                image.sprite = null;
                image.enabled = false;
                return;
            }

            image.sprite = GetDigitSprite(value.Value);
            image.enabled = image.sprite != null;
        }

        private void ApplyOperatorImage(Image image, Operator? op)
        {
            if (image == null) return;

            if (!op.HasValue)
            {
                image.sprite = null;
                image.enabled = false;
                return;
            }

            image.sprite = GetOperatorSprite(op.Value);
            image.enabled = image.sprite != null;
        }

        private Sprite GetDigitSprite(Trit value)
        {
            switch (value.ToInt())
            {
                case 0: return digit0Sprite;
                case 1: return digit1Sprite;
                case 2: return digit2Sprite;
                default: return null;
            }
        }

        private Sprite GetOperatorSprite(Operator op)
        {
            switch (op)
            {
                case Operator.NOT: return notSprite;
                case Operator.AND: return andSprite;
                case Operator.OR: return orSprite;
                case Operator.XOR: return xorSprite;
                case Operator.IMPLY: return implySprite;
                case Operator.NAND: return nandSprite;
                case Operator.NOR: return norSprite;
                case Operator.EQUIV: return equivSprite;
                case Operator.IMPLY_LUK: return implyLukSprite;
                case Operator.PLUS:
                case Operator.MINUS:
                    return null; // Арифметические операторы не поддерживаются в логических заданиях
                default: return null;
            }
        }
    }
}


