using System;
using Game.Attribute;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scroll
{
    [Serializable]
    public class ScrollInfo
    {
        [SerializeField] private string answer;
        public string Answer
        {
            get { return answer; }
        }

        [SerializeField] private string attributeId;
        public string AttributeId
        {
            get { return attributeId; }
        }

        [SerializeField] private int power;
        public int Power
        {
            get { return power; }
        }

        [SerializeField] private int duration;
        public int Duration
        {
            get { return duration; }
        }

        [SerializeField] private int money;
        public int Money
        {
            get { return money; }
            set { money = value; }
        }

        public ScrollInfo(string attributeId, int power, int duration, int money, string answer)
        {
            this.attributeId = attributeId;
            this.power = power;
            this.duration = duration;   // ScrollCreationManager.cs의 FinishScroll()에서 +1 해줌. {0, 1, 2} -> {1, 2, 3}
            this.money = money;
            this.answer = answer;

            // 이름으로 찾게 하기
            // string name = AttributeManager.Instance.attributeTable.Find(e => e.id == attributeId).GetKoreanName();
            // answer = string.Concat(name, power);
        }

    }

}
