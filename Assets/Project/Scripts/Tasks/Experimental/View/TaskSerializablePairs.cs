using System;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    [Serializable]
    public struct TritSpritePair
    {
        public Trit trit;
        public Sprite sprite;
    }
    
    [Serializable]
    public struct TernaryBinaryOperatorSpritePair
    {
        public TernaryBinaryOperatorType op;
        public Sprite sprite;
    }
    
    [Serializable]
    public struct TernaryBinaryOperatorPrefabPair
    {
        public TernaryBinaryOperatorType op;
        public GameObject prefab;
    }
    
    [Serializable]
    public struct TernaryUnaryOperatorSpritePair
    {
        public TernaryUnaryOperatorType op;
        public Sprite sprite;
    }
    
    [Serializable]
    public struct TernaryUnaryOperatorPrefabPair
    {
        public TernaryUnaryOperatorType op;
        public GameObject prefab;
    }
    
    [Serializable]
    public struct NonaryDigitSpritePair
    {
        public int digit;       // 0..8
        public Sprite sprite;
    }
    
    [Serializable]
    public struct NonaryOperatorSpritePair
    {
        public NonaryOperatorType op;
        public Sprite sprite;
    }
}