using System.Collections.Generic;
using UnityEngine;

namespace Game.Element
{
    /// <summary>
    /// 원소 정보 저장 관리자
    /// </summary>
    public class ElementManager : Singleton<ElementManager>
    {
        [Header("원소 데이터 테이블")]
        [SerializeField]
        private List<ElementInfo> elementTable = new List<ElementInfo>();
        public List<ElementInfo> ElementTable
        {
            get { return elementTable; }
        }
        void Awake()
        {
            ReadFile();
        }

        public void ReadFile()
        {
            // 원소
            List<Dictionary<string, object>> lines = CSVReader.Read("CSV/Element");
            foreach (Dictionary<string, object> data in lines)
            {
                elementTable.Add(ElementInfo.FromDictionary(data));
            }
        }
        public string FindKoreanNameById(string id)
        {
            ElementInfo elementInfo = elementTable.Find(e => e.id == id);
            if (elementInfo != null)
                return elementInfo.GetKoreanName();
            else return "";
        }
        public string FindNameById(string id)
        {
            ElementInfo elementInfo = elementTable.Find(e => e.id == id);
            if (elementInfo != null)
                return elementInfo.name;
            else return "";
        }
        public string FindIdByName(string name)
        {
            ElementInfo element = elementTable.Find(e => e.name == name);
            if (element != null)
                return element.id;
            else return "";
        }
    }
}