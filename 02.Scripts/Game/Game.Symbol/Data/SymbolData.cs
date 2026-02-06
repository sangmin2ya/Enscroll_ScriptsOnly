using System;
using Game.Element;
using UnityEngine;

namespace Game.Symbols
{
    [Serializable]
    public class SymbolData
    {
        [Header("Symbol Data")]
        [SerializeField]
        private string _id;// element나 attribute의 id
        public string Id
        {
            get { return _id; }
        }
        private int diagram => ElementInfo.Getdiagram(_id); // 다이어그램 번호
        public int Diagram
        {
            get { return diagram; }
        }
        [SerializeField]
        private SymbolType _type; // 심볼 타입
        public SymbolType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        public SymbolData(string id, SymbolType type)
        {
            _id = id;
            _type = type;
        }
    }
}