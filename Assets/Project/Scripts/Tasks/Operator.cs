using UnityEngine;

namespace Expedition0.Tasks
{
    public enum Operator
    {
    NOT,
    AND,
    OR,
    XOR,
    IMPLY,
    PLUS,
    MINUS,
    NAND,      // NOT AND
    NOR,       // NOT OR
    EQUIV,     // Equivalence (NOT XOR)
    IMPLY_LUK, // Lukasiewicz implication
    }   
}
