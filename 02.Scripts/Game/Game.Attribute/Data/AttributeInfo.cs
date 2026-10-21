using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Attribute
{
    [Serializable]
    public class AttributeInfo
    {
        [SerializeField] private int _level;
        public int level
        {
            get { return _level; }
        }

        [SerializeField] private string _id;
        public string id
        {
            get { return _id; }
        }

        [SerializeField] private string _name => _names[(int)GameData.Instance.language];
        public string name
        {
            get { return _name; }
        }
        private List<string> _names = new List<string>();
        public List<string> Names
        {
            get { return _names; }
        }
        [SerializeField] private string _desc => _descs[(int)GameData.Instance.language];
        public string desc
        {
            get { return _desc; }
        }
        private List<string> _descs = new List<string>();

        // Lobby에서 바로 데이터로드할때 기존 없을 경우
        [SerializeField]
        private List<string> _powers => _powersPerLanguages[(int)GameData.Instance.language < _powersPerLanguages.Count ? (int)GameData.Instance.language : 1];
        public List<string> powers
        {
            get { return _powers; }
        }
        [JsonProperty][SerializeField] private List<List<string>> _powersPerLanguages = new List<List<string>>();

        [SerializeField] private List<string> _combinations = new List<string>();
        public List<string> combinations
        {
            get { return _combinations; }
            set { _combinations = value; }
        }

        public AttributeInfo(int level, string id, List<string> names, List<string> descs, List<List<string>> powersPerLanguages, List<string> combinations)
        {
            _level = level;
            _id = id;
            _names = names;
            _descs = descs;
            _powersPerLanguages = powersPerLanguages;
            _combinations = combinations;
            _combinations.Sort();
        }

        /// <summary>
        /// 따라서 조합의 아이디를 찾을 때를 위한 한국 이름 찾기
        /// </summary>
        public string GetKoreanName()
        {
            return _names[0];
        }

        public bool isMatch(List<string> inputs)
        {
            // 정렬해서 줄 것
            return inputs.SequenceEqual(combinations);
        }

        public static AttributeInfo FromDictionary(Dictionary<string, object> data)
        {
            int level; int.TryParse(data["Level"].ToString(), out level);

            string id = data["Id"].ToString();

            List<string> names = new List<string>();
            names.Add("");
            names.Add("");
            names.Add("");
            names[(int)Language.Kor] = data["Name_Kor"].ToString();
            names[(int)Language.Eng] = data["Name_Eng"].ToString();
            names[(int)Language.Jp] = data["Name_Jp"].ToString();

            List<string> descs = new List<string>();
            descs.Add("");
            descs.Add("");
            descs.Add("");
            descs[(int)Language.Kor] = data["Desc_Kor"].ToString().Replace("@", "\n"); ;
            descs[(int)Language.Eng] = data["Desc_Eng"].ToString().Replace("@", "\n"); ;
            descs[(int)Language.Jp] = data["Desc_Jp"].ToString().Replace("@", "\n"); ;

            List<List<string>> powersPerLanguages = new List<List<string>>();
            powersPerLanguages.Add(new List<string>());
            powersPerLanguages.Add(new List<string>());
            powersPerLanguages.Add(new List<string>());
            powersPerLanguages[(int)Language.Kor].AddRange(new List<string>
        {
            data["Power0_Kor"].ToString(),
            data["Power1_Kor"].ToString(),
            data["Power2_Kor"].ToString(),
        });
            powersPerLanguages[(int)Language.Eng].AddRange(new List<string>
        {
            data["Power0_Eng"].ToString(),
            data["Power1_Eng"].ToString(),
            data["Power2_Eng"].ToString(),
        });

            powersPerLanguages[(int)Language.Jp].AddRange(new List<string>
        {
            data["Power0_Jp"].ToString(),
            data["Power1_Jp"].ToString(),
            data["Power2_Jp"].ToString(),
        });

            List<string> combinations = new List<string>();
            combinations.Add(data["Combination0"].ToString());
            combinations.Add(data["Combination1"].ToString());

            return new AttributeInfo(level, id, names, descs, powersPerLanguages, combinations);
        }
    }

}