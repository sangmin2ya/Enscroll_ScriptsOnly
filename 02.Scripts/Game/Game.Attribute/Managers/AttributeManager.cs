using System.Collections.Generic;
using System.Linq;
using Game.Desk;
using Game.Element;
using UnityEngine;

namespace Game.Attribute
{
    public class AttributeManager : Singleton<AttributeManager>
    {
        [Header("속성 테이블")]
        public List<AttributeInfo> attributeTable = new List<AttributeInfo>();

        [SerializeField] private List<AttributeRecipeInfo> attributeRecipeInfos = new List<AttributeRecipeInfo>();
        public List<AttributeRecipeInfo> AttributeRecipeInfos
        {
            get { return attributeRecipeInfos; }
        }

        private ScrollBook scrollBook;

        void Awake()
        {
            scrollBook = FindObjectOfType<ScrollBook>();

            Init();
        }

        public void Init()
        {
            ReadFile();
            if (GameData.SaveInfo != null)
                LoadAttributeRecipeFromSaveInfo();
            else
                LoadAttributeRecipes();
        }

        private void ReadFile()
        {
            List<Dictionary<string, object>> lines = CSVReader.Read("CSV/Attribute");
            foreach (Dictionary<string, object> data in lines)
            {
                attributeTable.Add(AttributeInfo.FromDictionary(data));
            }
        }

        /// <summary>
        /// 속성 레시피 로드. 속성 테이블의 속성 정보 하나 하나를 레시피 형태로 저장.
        /// </summary>
        private void LoadAttributeRecipes()
        {
            foreach (AttributeInfo attributeInfo in attributeTable)
            {
                AttributeRecipeInfo recipeInfo = new AttributeRecipeInfo(attributeInfo);
                attributeRecipeInfos.Add(recipeInfo);
            }
        }

        /// <summary>
        /// 새 속성을 획득했을 때 호출할 메서드. 얻지 못한 속성들 리스트에서 해금한 속성 빼주기도 함.
        /// </summary>
        /// <param name="obtainedAttribute"></param>
        public void OnAttributeObtained(AttributeInfo obtainedAttribute)
        {
            AttributeInfo attributeInfo = attributeTable.Find(attr => attr.id == obtainedAttribute.id);
            if (attributeInfo == null)
            {
                return;
            }

            // 얻은 속성의 조합 정보를 UI에 바로 표시
            scrollBook.SetRecipeCombinationContent(attributeInfo);
        }

        /// <summary>
        /// 속성 ID로 속성을 획득
        /// </summary>
        /// <param name="attributeId"></param>
        public void ObtainAttributeById(string attributeId)
        {
            AttributeInfo attribute = attributeTable.Find(attr => attr.id == attributeId);
            if (attribute != null)
            {
                OnAttributeObtained(attribute);
            }
            else
            {
                Debug.LogWarning($"ID가 '{attributeId}'인 속성을 찾을 수 없습니다.");
            }
        }

        private bool IsUnlocked(AttributeInfo attributeInfo)
        {
            return attributeRecipeInfos.Find(e => e.attributeInfo == attributeInfo).isUnlocked;
        }

        public AttributeRecipeInfo GetAttributeRecipe(AttributeInfo info)
        {
            return attributeRecipeInfos.Find(e => e.attributeInfo == info);
        }

        /// <summary>
        /// CSV에 나타나는 조합(Combination0, Combination1)은 기획자의 편의를 위해 한국이름으로 정리한다. 
        /// 따라서 조합의 아이디를 찾을 때는 이름을 통해 아이디를 얻는 과정을 추가한다.
        /// 주의) ""빈칸으로 나오면 Attribute가 아닌 Element인 것이다.
        /// </summary>
        public string FindIdByName(string input)
        {
            AttributeInfo attributeInfo = attributeTable.Find(e => e.GetKoreanName() == input);
            if (attributeInfo != null)
                return attributeInfo.id;
            else return "";
        }
        public string FindKoreanNameById(string input)
        {
            AttributeInfo attributeInfo = attributeTable.Find(e => e.id == input);
            if (attributeInfo != null)
                return attributeInfo.GetKoreanName();
            else return "";
        }
        public string FindNameById(string input)
        {
            AttributeInfo attributeInfo = attributeTable.Find(e => e.id == input);
            if (attributeInfo != null)
                return attributeInfo.name;
            else return "";
        }
        public AttributeInfo FindAttributeById(string input)
        {
            return attributeTable.Find(e => e.id == input);
        }

        public bool IsNameUnlocked(string id)
        {
            return attributeRecipeInfos.Find(e => e.attributeInfo.id == id).isUnlocked;
        }

        #region 저장 데이터 로드
        public void LoadAttributeRecipeFromSaveInfo()
        {
            // 저장된 데이터가 있을 경우
            foreach (var savedRecipe in GameData.SaveInfo.recipeInfos)
            {
                // AttributeTable에서 matching되는 AttributeInfo 찾기
                AttributeInfo matchingAttribute = attributeTable.Find(attr => attr.id == savedRecipe.attributeInfo.id);
                if (matchingAttribute != null)
                {
                    // 새로운 AttributeRecipeInfo 생성하되, 저장된 상태값 복사
                    AttributeRecipeInfo recipeInfo = new AttributeRecipeInfo(matchingAttribute, savedRecipe.isFirstUnlocked, savedRecipe.isSecondUnlocked);

                    // 기타 필요한 상태값들 복사
                    attributeRecipeInfos.Add(recipeInfo);
                }
            }
        }
        #endregion
    }
}