using System;
using Newtonsoft.Json;
/// <summary>
/// 속성 정보와 그 속성이 해금되었는지 여부를 담는 클래스
/// </summary>
namespace Game.Attribute
{
    [Serializable]
    public class AttributeRecipeInfo
    {
        public AttributeInfo attributeInfo;
        [JsonProperty] private bool isFirstCombinationUnlocked;
        [JsonProperty] private bool isSecondCombinationUnlocked;
        public bool isUnlocked => isFirstCombinationUnlocked && isSecondCombinationUnlocked;
        public bool isFirstUnlocked => isFirstCombinationUnlocked;
        public bool isSecondUnlocked => isSecondCombinationUnlocked;

        public AttributeRecipeInfo(AttributeInfo attributeInfo, bool isFirstCombinationUnlocked = false, bool isSecondCombinationUnlocked = false)
        {
            this.attributeInfo = attributeInfo;
            this.isFirstCombinationUnlocked = isFirstCombinationUnlocked;
            this.isSecondCombinationUnlocked = isSecondCombinationUnlocked;
        }

        // 첫 번째 요구 속성 해금
        public void UnlockFirstSubElement()
        {
            isFirstCombinationUnlocked = true;
        }
        // 두 번째 요구 속성 해금
        public void UnlockSecondSubElement()
        {
            isSecondCombinationUnlocked = true;
        }
    }
}