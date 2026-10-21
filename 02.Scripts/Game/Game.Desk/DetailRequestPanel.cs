using UnityEngine;
using TMPro;
using Game.Scroll;
using Game.Attribute;
using Game.Request;
using UnityEngine.Localization.Settings;

namespace Game.Desk
{
    public class DetailRequestPanel : MonoBehaviour
    {
        [SerializeField]
        private Transform requestPrefabParent; // 의뢰서 프리팹을 배치할 부모 UI
        private GameObject detailedRequestObject;   // 패널에 표시 될 의뢰서 오브젝트

        [SerializeField]
        private GameObject requestSheetPlace;   // 확대 보기 끝난 후 원래 자리로 돌아갈 때 씀

        [SerializeField] private TextMeshProUGUI scrollEffectText;
        [SerializeField] private TextMeshProUGUI scrollDurationText;

        public void UpdatePanel(RequestInfo requestInfo, ScrollInfo submittedScroll)
        {
            // gameObject.SetActive(true);
            // gameObject.GetComponent<RectTransform>().SetAsLastSibling();
            transform.parent.gameObject.SetActive(true);
            transform.parent.SetAsLastSibling();

            // 기존에 표시 중인 의뢰서 프리팹 제거
            if (detailedRequestObject != null)
            {
                Destroy(detailedRequestObject);
                detailedRequestObject = null;
            }

            if (requestInfo is NormalRequestInfo normalRequest)
                detailedRequestObject = Instantiate(DeskManager.Instance.GetRequestPrefab(normalRequest), requestPrefabParent);
            else if (requestInfo is SpecialRequestInfo specialRequest)
                detailedRequestObject = Instantiate(Resources.Load<GameObject>(string.Format("Prefabs/QuestLetter/Request/{0}/{1}", GameData.Instance.language, specialRequest.Id)), requestPrefabParent);

            // 확대 보기 후 제자리 돌아가기 위한 의뢰칸 설정
            //detailedRequestObject.GetComponent<RequestSheet>().OrderSheetPlace = requestSheetPlace.GetComponent<RectTransform>();

            // RequestSheet 데이터 설정
            var requestSheet = detailedRequestObject.GetComponent<DragableObject>();
            if (requestSheet != null)
            {
                if (requestSheet.GetComponent<DragableObject>() != null)
                    requestSheet.GetComponent<DragableObject>().enabled = false;
            }
            if (requestSheet.GetComponent<NormalRequestSheet>() != null)
            {
                requestSheet.GetComponent<NormalRequestSheet>().SetData(requestInfo as NormalRequestInfo);
            }
            if (detailedRequestObject.GetComponent<PuzzleRequest>() != null)
            {
                detailedRequestObject.GetComponent<PuzzleRequest>().enabled = false;
            }


            // 내가 제출한 스크롤에 관한 정보 보여주는 부분
            if (submittedScroll != null)
            {
                // 스크롤의 효과 계산
                string effect = CalculateScrollEffect(submittedScroll);

                // 지속 시간 변환
                string durationText = LocalizationSettings.StringDatabase.GetLocalizedString("Localizing", ConvertDurationToText(submittedScroll.Duration));
                scrollEffectText.text = $"{effect}";
                scrollDurationText.text = $"{durationText}";
            }
            //gameObject.SetActive(true);
        }

        public void ClosePanel()
        {
            //gameObject.SetActive(false);
            transform.parent.gameObject.SetActive(false);
        }

        private string CalculateScrollEffect(ScrollInfo scrollInfo)
        {
            // AttributeId로 속성을 찾고, Power에 따라 효과 가져오기
            var attribute = AttributeManager.Instance.FindAttributeById(scrollInfo.AttributeId);
            if (attribute != null)
            {
                int powerIndex = Mathf.Clamp(scrollInfo.Power, 0, 2);
                return attribute.powers[powerIndex];
            }

            return "알 수 없는 효과"; // 속성을 찾지 못했을 경우
        }

        private string ConvertDurationToText(int duration)
        {
            switch (duration)
            {
                case 1:
                    return "하루";
                case 2:
                    return "일주일";
                case 3:
                    return "한 달";
                default:
                    return "알 수 없음"; // 예상치 못한 값
            }
        }
    }
}