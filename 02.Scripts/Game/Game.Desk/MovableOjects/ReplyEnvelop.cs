using System.Collections.Generic;
using DG.Tweening;
using Game.Board;
using Game.Request;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Desk
{
    /// <summary>
    /// 클릭하면 생성될 편지 오브젝트들
    /// </summary>
    [System.Serializable]
    public class ReplyEnvelop : MonoBehaviour
    {
        [Header("생성될 편지 오브젝트")]
        [SerializeField] private List<RequestResult> replies = new List<RequestResult>();
        [SerializeField] private Button openButton;
        //0 - 마지막 답장 아님, 1 - 이리니 마지막, 2 - 엑스뱌른 마지막, 3 - 마이오시타르, 4 - 웨스트마크
        private int _finalReply = 0;

        void Start()
        {
            //생성될때 테이블 위로 올라오게
            GetComponent<RectTransform>().localPosition = Vector3.zero;
            openButton.onClick.AddListener(OpenEnvelope);
            openButton.interactable = true;
        }
        public void SetReply(List<RequestResult> replyList, int finalReply)
        {
            replies = replyList;
            _finalReply = finalReply;
        }
        public void OpenEnvelope()
        {
            GetComponent<Animator>().SetTrigger("Open");
            openButton.interactable = false;
        }
        public void SpawnLetters()
        {
            switch (_finalReply)
            {
                case 0:
                    break;
                case 1:
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_8");
                    break;
                case 2:
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_9");
                    break;
                case 3:
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_10");
                    break;
                case 4:
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_11");
                    break;
            }
            Vector3 letterPos = GetComponent<RectTransform>().position;
            if (GameData.Instance.FinalResultCode == 0)
            {
                foreach (var reply in replies)
                {
                    GameData.Instance.AddQuestReplyId(reply.replyId);

                    QuestManager.Instance.ShowReplyRecord(reply.replyId);
                    var letter = SpawnInsideArea(reply);
                    letter.GetComponent<RectTransform>().position = letterPos;
                }
            }
            Destroy(gameObject);
        }
        private GameObject SpawnInsideArea(RequestResult request)
        {
            string path = string.Format("Prefabs/QuestLetter/Reply/{0}/{1}", GameData.Instance.langauage, request.replyId);
            GameObject replySheet = Instantiate(Resources.Load<ReplySheet>(path), CanvasManager.Instance.MainDeskUI).gameObject;

            replySheet.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position; ;

            return replySheet;
        }
    }
}