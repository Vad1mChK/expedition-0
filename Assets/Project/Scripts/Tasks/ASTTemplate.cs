using System.Collections.Generic;

namespace Expedition0.Tasks
{
    // Хранит шаблон, доступ к слотам и ожидаемый ответ (правая часть)
    public class ASTTemplate
    {
        public ASTNode Root { get; }
        public Trit Answer { get; }

        // Упорядоченные списки слотов, чтобы удобно заполнять по индексу
        public IReadOnlyList<ValueSlotNode> ValueSlots => _valueSlots;
        public IReadOnlyList<OperatorSlotNode> OperatorSlots => _operatorSlots;

        private readonly List<ValueSlotNode> _valueSlots = new List<ValueSlotNode>();
        private readonly List<OperatorSlotNode> _operatorSlots = new List<OperatorSlotNode>();

        public ASTTemplate(ASTNode root, Trit answer)
        {
            Root = root;
            Answer = answer;
            CollectSlots(root);
        }

        // Конструктор с ручным порядком слотов для специальных случаев
        public ASTTemplate(ASTNode root, Trit answer, List<ValueSlotNode> valueSlots, List<OperatorSlotNode> operatorSlots)
        {
            Root = root;
            Answer = answer;
            _valueSlots.AddRange(valueSlots);
            _operatorSlots.AddRange(operatorSlots);
        }

        // Фабрика простого бинарного шаблона: OP(V, V)
        public static ASTTemplate CreateBinaryTemplate(Operator op, Trit answer)
        {
            var left = new ValueSlotNode();
            var right = new ValueSlotNode();
            var opNode = new OperatorSlotNode(left, right);
            opNode.SetOperator(op);
            return new ASTTemplate(opNode, answer);
        }

        // Рекурсивно собираем ссылки на слоты
        private void CollectSlots(ASTNode node)
        {
            if (node is ValueSlotNode vs)
            {
                _valueSlots.Add(vs);
            }
            else if (node is OperatorSlotNode os)
            {
                _operatorSlots.Add(os);
            }

            if (node.Children == null) return;
            for (int i = 0; i < node.Children.Count; i++)
            {
                CollectSlots(node.Children[i]);
            }
        }
    }
}


