using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Board
{
    public class PersonalQuest : MonoBehaviour
    {
        [Header("Quest Record")]
        [SerializeField] private List<GameObject> questRecordList = new List<GameObject>();
        [SerializeField] private List<GameObject> questRecordPreSet = new List<GameObject>();
        [SerializeField] private GameObject ways;
        [Header("Page Button")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        private int currentPage = 0;
        public void Start()
        {
            prevButton.onClick.AddListener(PrevPage);
            nextButton.onClick.AddListener(NextPage);
            closeButton.onClick.AddListener(() =>
            {
                HideWay();
                gameObject.SetActive(false);
            });
            UpdatePage();
        }
        public void HideWay()
        {
            foreach (var way in ways.transform.GetComponentsInChildren<ShowWay>())
            {
                way.HideAllWay();
            }
        }
        void OnEnable()
        {
            currentPage = 0;
            UpdatePage();
        }
        //데이터 불러와서 세팅하는 함수
        public void ShowQuestRecord(int index, string summary)
        {
            questRecordPreSet[index].GetComponent<QuestRecord>().ShowRequest(summary);
            questRecordList.Add(questRecordPreSet[index]);
            questRecordList[index].SetActive(true);
            UpdatePage();
        }
        public void ShowReplyRecord(int index, string summary)
        {
            questRecordList[index].GetComponent<QuestRecord>().ShowReply(summary);
        }
        public void NextPage()
        {
            if (currentPage < questRecordList.Count - 1)
            {
                currentPage++;
                UpdatePage();
            }
        }
        public void PrevPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePage();
            }
        }
        private void UpdatePage()
        {
            for (int i = 0; i < questRecordList.Count; i++)
            {
                if (i == currentPage)
                {
                    questRecordList[i].SetActive(true);
                }
                else
                {
                    questRecordList[i].SetActive(false);
                }
            }
            if (currentPage == questRecordList.Count - 1)
            {
                nextButton.interactable = false;
            }
            else
            {
                nextButton.interactable = true;
            }
            if (currentPage == 0)
            {
                prevButton.interactable = false;
            }
            else
            {
                prevButton.interactable = true;
            }
        }
    }
}