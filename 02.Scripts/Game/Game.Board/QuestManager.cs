using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Game.Board
{
    public struct QuestResult
    {
        public int npcNum { get; set; }
        public int questNum { get; set; }
        public bool IsComplete { get; set; } // R이면 true, W면 false
        public QuestResult(int npcNum, int questNum, bool isComplete)
        {
            this.npcNum = npcNum;
            this.questNum = questNum;
            IsComplete = isComplete;
        }
    }
    public class QuestManager : Singleton<QuestManager>
    {
        [Header("인물 퀘스트")]
        //0 이리니 / 1 엑스뱌른 / 2 마이오시따르 / 3 웨스트마크
        [SerializeField] private List<Button> personalQuestPanel;
        [Header("퀘스트 줌인 보드")]
        [SerializeField] private GameObject QuestPanel;
        [SerializeField] private List<PersonalQuest> personalQuests;
        private List<ScrapInfo> qusetInfos = new List<ScrapInfo>(); // 답변 목록 저장해두는 데이터
        public void InitQuestBoard()
        {
            LoadQuestInfos();
            GameData.Instance.LoadQuestBoard();
        }
        public void Start()
        {
            for (int i = 0; i < personalQuestPanel.Count; i++)
            {
                int index = i;
                personalQuestPanel[i].onClick.AddListener(() => ZoomInQuestBoard(index));
            }
            // QuestPanel.GetComponent<Button>().onClick.AddListener(ZoomOutQuestBoard);
        }
        /// <summary>
        /// 퀘스트 의뢰 보여주기
        /// </summary>
        /// <param name="qusetId">퀘스트 ID</param>
        public void ShowQuestRecord(string qusetId)
        {
            QuestResult questResult = GetQuestResult(qusetId);
            int idx = questResult.npcNum;
            int index = questResult.questNum;
            string summaryId = qusetId.Split('_')[0];
            bool isSuccess = questResult.IsComplete;
            if (idx >= 4)
                return;
            if (index == 0)
            {
                personalQuestPanel[idx].gameObject.SetActive(true);
            }
            personalQuests[idx].ShowQuestRecord(index, qusetInfos.Find(x => x.QuestId == summaryId).Summaries[index]);
            Debug.Log("ShowQuest : " + qusetInfos.Find(x => x.QuestId == summaryId).Summaries[index]);
            personalQuestPanel[idx].transform.Find("알람").gameObject.SetActive(true);
        }
        /// <summary>
        /// 퀘스트 답장 보여주기
        /// </summary>
        /// <param name="replyId">답장 ID</param>
        public void ShowReplyRecord(string replyId)
        {
            QuestResult questResult = GetQuestResult(replyId);
            int idx = questResult.npcNum;
            int index = questResult.questNum;
            bool isSuccess = questResult.IsComplete;
            string summaryId = isSuccess ? replyId.Split('_')[0] + "_R" : replyId.Split('_')[0] + "_W";
            if (idx >= 4)
                return;
            personalQuests[idx].ShowReplyRecord(index, qusetInfos.Find(x => x.QuestId == summaryId).Summaries[index]);

            personalQuestPanel[idx].transform.Find("알람").gameObject.SetActive(true);
        }
        public void TurnOffAlarm()
        {
            foreach (var personalQuest in personalQuestPanel)
            {
                personalQuest.transform.Find("알람").gameObject.SetActive(false);
            }
        }
        public void ZoomInQuestBoard(int index)
        {
            QuestPanel.SetActive(true);
            foreach (var personalQuest in personalQuests)
            {
                personalQuest.GetComponent<PersonalQuest>().HideWay();
                personalQuest.gameObject.SetActive(false);
            }
            personalQuests[index].gameObject.SetActive(true);
        }
        public void ZoomOutQuestBoard()
        {
            QuestPanel.SetActive(false);
            foreach (var personalQuest in personalQuests)
            {
                personalQuest.HideWay();
                personalQuest.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 이전 사전 데이터 재활용
        /// </summary>
        private void LoadQuestInfos()
        {
            List<Dictionary<string, object>> lines = CSVReader.Read("CSV/QuestSummary");
            foreach (Dictionary<string, object> data in lines)
            {
                qusetInfos.Add(ScrapInfo.FromDictionary(data));
            }
        }

        public void GetRequestInfoById(string id, int index)
        {

        }
        public void GetReplyInfoById(string id, int index, bool isSuccess)
        {

        }
        /// <summary>
        /// 퀘스트 번호 추출하는 함수 (QuestX_YW/R) -> (X, Y, False/True) 로 변홤해줌
        /// </summary>
        public QuestResult GetQuestResult(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("ID cannot be null or empty");
            }

            // "QuestX_YR" 또는 "QuestX_YW" 형식인지 확인
            if (id.StartsWith("Quest") && id.Contains("_") && (id.EndsWith("R") || id.EndsWith("W")))
            {
                try
                {
                    // "Quest" 이후의 숫자와 "_" 이후의 숫자, 마지막 문자를 추출
                    string[] parts = id.Substring(5, id.Length - 6).Split('_');
                    int questNumber = int.Parse(parts[0]);
                    int subNumber = int.Parse(parts[1]);
                    char completionChar = id[^1];

                    // 범위 확인 (Quest0~4, subNumber는 0 이상)
                    if (questNumber >= 0 && questNumber <= 7 && subNumber >= 0)
                    {
                        bool isComplete = completionChar == 'R'; // 'R'은 true, 'W'는 false
                        return new QuestResult(questNumber, subNumber, isComplete);
                    }
                    else
                    {
                        throw new ArgumentException("Quest or sub number out of range");
                    }
                }
                catch (Exception ex)
                {
                    throw new FormatException("Invalid ID format", ex);
                }
            }
            else if (id.StartsWith("Reply") && id.Contains("_") && (id.EndsWith("R") || id.EndsWith("W")))
            {
                try
                {
                    // "Reply" 이후의 숫자와 "_" 이후의 숫자, 마지막 문자를 추출
                    string[] parts = id.Substring(5, id.Length - 6).Split('_');
                    int questNumber = int.Parse(parts[0]);
                    int subNumber = int.Parse(parts[1]);
                    char completionChar = id[^1];

                    // 범위 확인 (Quest0~4, subNumber는 0 이상)
                    if (questNumber >= 0 && questNumber <= 4 && subNumber >= 0)
                    {
                        bool isComplete = completionChar == 'R'; // 'R'은 true, 'W'는 false
                        return new QuestResult(questNumber, subNumber, isComplete);
                    }
                    else
                    {
                        throw new ArgumentException("Quest or sub number out of range");
                    }
                }
                catch (Exception ex)
                {
                    throw new FormatException("Invalid ID format", ex);
                }
            }
            else
            {
                throw new FormatException("ID must be in the format 'QuestX_YR' or 'QuestX_YW' or 'ReplyX_YR' or 'ReplyX_YW'");
            }
        }
    }
}