using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Element
{
    public enum ElementType
    {
        Fire,
        Water,
        Wind,
        Earth,
        None
    }
    /// <summary>
    /// 원소 정보 구조
    /// </summary>
    [Serializable]
    public class ElementInfo
    {
        [SerializeField] private string _id;
        public string id
        {
            get { return _id; }
            // set { _id = value; }
        }

        [SerializeField] private string _name => _names[(int)GameData.Instance.language];
        public string name
        {
            get { return _name; }
        }
        [SerializeField]
        private List<string> _names = new List<string>();
        public List<string> Names
        {
            get { return _names; }
        }

        [SerializeField] private int _diagram;
        public int diagram
        {
            get { return _diagram; }
        }

        public ElementInfo(string id, List<string> names)
        {
            _id = id;
            _names = names;

            _diagram = Getdiagram(id);
        }

        public string GetKoreanName()
        {
            return Names[0];
        }

        public static ElementInfo FromDictionary(Dictionary<string, object> data)
        {
            string id = data["Id"].ToString();

            List<string> names = new List<string>();
            names.Add("");
            names.Add("");
            names.Add("");
            names[(int)Language.Kor] = data["Name_Kor"].ToString();
            names[(int)Language.Eng] = data["Name_Eng"].ToString();
            names[(int)Language.Jp] = data["Name_Jp"].ToString();

            return new ElementInfo(id, names);
        }

        public static int Getdiagram(string input)
        {
            switch (input)
            {
                // 불
                case "Element_0":
                    return 0;

                // 물
                case "Element_1":
                    return 1;

                // 공기
                case "Element_2":
                    return 2;

                // 흙
                case "Element_3":
                    return 3;

                default:
                    Debug.Log("이상한 도형 등장");
                    return 0;
            }
        }
    }
}
