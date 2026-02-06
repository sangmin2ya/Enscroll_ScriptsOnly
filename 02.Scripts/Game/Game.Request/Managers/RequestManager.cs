using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Desk;
using System.Linq;
using Game.Scroll;
using System;
using Random = UnityEngine.Random;
using Game.Tutorial;
using Newtonsoft.Json;
using Game.Invironment;
using Game.Board;


/// <summary>
/// 의뢰 목록 로드, 장사 동안 의뢰 간격 설정, 완료된 의뢰 기록 및 피드백용 데이터 저장
/// </summary>
namespace Game.Request
{
    #region 결과 클래스

    [Serializable]
    public class RequestResult
    {
        public string requestId;
        public string replyId;
        public ScrollInfo submittedScroll; // 이제 제출한 스크롤 정보도 담을거임. 영수증 상세 의뢰 보기에 사용할거.
        public RequestInfo requestInfo;
    }

    [Serializable]
    public class IncorrectRequest : RequestResult
    {
        public bool isWrongBoth;
        public bool isWrongEffect;
        public bool isWrongDuration;
    }

    [Serializable]
    public class CorrectRequest : RequestResult
    {
        public int money;
    }
    #endregion


    public class RequestManager : Singleton<RequestManager>
    {
        [Header("현재 의뢰")]
        [SerializeField] private RequestInfo currentRequest;

        // ----------------------------------------------------------------- //
        // 모든 일반 의뢰 저장 리스트
        [SerializeField]
        private List<NormalRequestInfo> allRequest = new List<NormalRequestInfo>();
        // 모든 일반 의뢰 저장 리스트
        [SerializeField]
        private List<SpecialRequestInfo> allQuests = new List<SpecialRequestInfo>();

        // 오늘의 의뢰에 저장할 리스트
        [SerializeField] public List<RequestInfo> todayRequests = new List<RequestInfo>();

        [SerializeField] private List<RequestResult> prevRequestResults = new List<RequestResult>();

        // 역해석 의뢰 속성 모음
        [SerializeField] private HashSet<string> reverseInterpretations = new HashSet<string>() { "어둠", "최면", "시간", "이동" };
        public HashSet<string> ReverseInterpretations => reverseInterpretations;

        [Header("의뢰에 따라 바뀌는 창밖")]
        [SerializeField] private WindowViewController windowViewController;

        [Header("부엉이")]
        [SerializeField]
        private newArrivalAlarm owl;
        [SerializeField]
        private GameObject submitArea;
        public GameObject SubmitArea => submitArea;
        [Header("납부엉이")]
        [SerializeField]
        private NewSettlementAlarm paymentOwl;
        public bool isPaymentDay = false;
        [Header("엔딩부엉이")]
        [SerializeField]
        private NewEndingAlarm endingOwl;
        public NewEndingAlarm EndingOwl => endingOwl;

        [SerializeField]
        private RequestCSVReader requestCSVReader;
        [SerializeField]
        private DayManager dayManager;
        [SerializeField] private List<RequestInfo> assignedRequests = new List<RequestInfo>();
        public List<RequestInfo> AssignedRequests => assignedRequests;
        [SerializeField] private GameObject denyReturnUI;
        [SerializeField]
        private DragableObject stamp;
        public DragableObject Stamp => stamp;

        void Awake()
        {
            LoadNormalRequest(); // Resource 폴더 안에 CSV에 RequestList 파일 읽기
            LoadSpecialRequest();

            // 저장 데이터
            if (GameData.SaveInfo != null)
            {
                LoadAssignedRequests();
                LoadPrevResults();
                LoadSpecialQuestAttributes();
            }
        }

        void Start()
        {
            AssignRequestForToday();
        }

        /// <summary>
        /// CSV 파일에서 데이터 가져와 allRequest List에 저장하는 메서드
        /// </summary>
        public void LoadNormalRequest()
        {
            allRequest.Clear();

            var rawRequests = requestCSVReader.LoadRequests();

            foreach (var entry in rawRequests)
            {
                NormalRequestInfo newRequest = NormalRequestInfo.FromDictionary(entry);
                allRequest.Add(newRequest);
            }

            // TestLoadedRequests();
        }

        /// <summary>
        /// CSV 파일에서 데이터 가져와 allRequest List에 저장하는 메서드
        /// </summary>
        public void LoadSpecialRequest()
        {
            allQuests.Clear();

            List<Dictionary<string, object>> lines = CSVReader.Read("CSV/QuestList");
            foreach (Dictionary<string, object> data in lines)
            {
                SpecialRequestInfo request = SpecialRequestInfo.FromDictionary(data);
                allQuests.Add(request);
            }
        }
        public void ResetOwlFor13thDay()
        {
            todayRequests.Clear(); // 원래 있는 오늘의 의뢰 초기화
            owl.Reset();
            paymentOwl.Reset();
            endingOwl.Reset();
            submitArea.SetActive(false);
            windowViewController.StartTransition(todayRequests.Count);
        }
        /// <summary>
        // 오늘 플레이어에게 전달될 의뢰 리스트를 만드는 메서드
        /// </summary> 
        public void AssignRequestForToday()
        {
            todayRequests.Clear(); // 원래 있는 오늘의 의뢰 초기화
            owl.Reset();
            paymentOwl.Reset();
            endingOwl.Reset();
            submitArea.SetActive(false);

            int currentDay = dayManager.day;

            // 특수 의뢰 넣어두기
            List<SpecialRequestInfo> daySpeicalQuests = allQuests.Where(e => e.Day == currentDay).ToList();
            for (int i = 0; i < daySpeicalQuests.Count; i++)
            {
                string questId = daySpeicalQuests[i].QuestId;
                int index = daySpeicalQuests[i].QuestIndex;
                string target = daySpeicalQuests[i].Id[daySpeicalQuests[i].Id.Length - 1].ToString();

                // 0일차와 1일차 특수 의뢰는 튜토리얼 완료 시 제외
                if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == true
                    && (currentDay == 0 || currentDay == 1))
                {
                    break;
                }

                if (index > 0)
                {
                    int prevResult = GameData.Instance.GetResultOfQuest(questId, index - 1);
                    // 성공
                    if (prevResult == 1 && target.Equals("R"))
                    {
                        todayRequests.Add(daySpeicalQuests[i]);
                        if (daySpeicalQuests[i].FirstOrder)
                        {
                            var temp = todayRequests[0];
                            todayRequests[0] = todayRequests[todayRequests.Count - 1];
                            todayRequests[todayRequests.Count - 1] = temp;
                        }
                    }
                    // 실패
                    else if (prevResult == 2 && target.Equals("W"))
                    {
                        todayRequests.Add(daySpeicalQuests[i]);
                        if (daySpeicalQuests[i].FirstOrder)
                        {
                            var temp = todayRequests[0];
                            todayRequests[0] = todayRequests[todayRequests.Count - 1];
                            todayRequests[todayRequests.Count - 1] = temp;
                        }
                    }
                    else
                    {
                        // 중복 정답일 때 빠지지 않게
                        if (daySpeicalQuests[i].AnswerAttributes.Count == 1)
                            reverseInterpretations.ExceptWith(daySpeicalQuests[i].AnswerAttributes);
                    }
                }
                else
                {
                    todayRequests.Add(daySpeicalQuests[i]);
                    if (daySpeicalQuests[i].FirstOrder)
                    {
                        var temp = todayRequests[0];
                        todayRequests[0] = todayRequests[todayRequests.Count - 1];
                        todayRequests[todayRequests.Count - 1] = temp;
                    }
                }
            }
            // 튜토리얼 0일차일 때 고정 일반 퀘스트 설정
            if (DayManager.Instance.day == 0 && TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
            {
                var request998 = allRequest.Find(r => r.Id == "Request_998");
                if (request998 != null && !todayRequests.Contains(request998))
                {
                    todayRequests.Add(request998);
                }
            }

            // 튜토리얼 1일차일 때 고정 일반 퀘스트 설정
            if (DayManager.Instance.day == 1 && TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
            {
                // 강화가 필요한 Request_12를 두 번째 의뢰로 설정
                var request12 = allRequest.Find(r => r.Id == "Request_999");
                if (request12 != null && !todayRequests.Contains(request12))
                {
                    todayRequests.Add(request12);
                }

                // 심볼 3개가 필요한 Request_42를 세 번째 의뢰로 설정
                var request42 = allRequest.Find(r => r.Id == "Request_997");
                if (request42 != null && !todayRequests.Contains(request42))
                {
                    todayRequests.Add(request42);
                }
            }

            // 하루에 레벨이 총 3개까지 나올 수 있어서 3개로 나눠놈
            List<RequestInfo> lowerLevelRequests = new List<RequestInfo>();
            List<RequestInfo> matchingLevelRequests = new List<RequestInfo>();

            int highestLevel = dayManager.currentPlayerLevels.Max(); // 현재날짜의 최대 레벨
            int lowestLevel = dayManager.currentPlayerLevels.Min(); // 현재 날짜의 최소 레벨

            foreach (var request in allRequest)
            {
                if (request.Level <= highestLevel && request.Level >= lowestLevel)
                {
                    if (assignedRequests.Contains(request) || reverseInterpretations.Overlaps(request.AnswerAttributes))
                    {
                        continue;
                    }
                    matchingLevelRequests.Add(request);
                }
                else if (request.Level < lowestLevel)
                {
                    if (assignedRequests.Contains(request) || reverseInterpretations.Overlaps(request.AnswerAttributes))
                    {
                        continue;
                    }
                    lowerLevelRequests.Add(request);
                }
            }

            int totalRequests = dayManager.todayRequestCount;
            int remainingRequests = totalRequests - todayRequests.Count;
            int matchingLevelCount = remainingRequests;

            AddRequestsByPercentage(matchingLevelRequests, matchingLevelCount);

            // 부족힌 의뢰 수 보충하기
            remainingRequests = totalRequests - todayRequests.Count;
            if (remainingRequests > 0)
            {
                AddRequestsByPercentage(lowerLevelRequests, remainingRequests);
            }

            foreach (var request in todayRequests)
            {
                if (request is NormalRequestInfo)
                {
                    assignedRequests.Add(request);
                }
            }

            // 튜토리얼 끝났을 때만 셔플
            if (TutorialManager.Instance != null)
            {
                if (TutorialManager.Instance.TutorialComplete)
                    ShuffleListExceptFirst(todayRequests);
            }
            else
            {
                ShuffleListExceptFirst(todayRequests);
            }
            // 창밖 초기화
            windowViewController.StartTransition(todayRequests.Count);
        }

        // 특수 의뢰 포함해서(첫번째 제외) 셔플
        public void ShuffleListExceptFirst<T>(List<T> list)
        {
            if (list.Count > 0)
            {
                int index = 0;
                if (list[index] is SpecialRequestInfo sInfo)
                {
                    if (sInfo.FirstOrder)
                        index = 1;
                }

                for (int i = list.Count - 1; i > index; i--)
                {
                    int randomIndex = Random.Range(index, i + 1);
                    T temp = list[i];
                    list[i] = list[randomIndex];
                    list[randomIndex] = temp;
                }
            }
        }

        /// <summary>
        /// 해당 리스트에서 랜덤하게 의뢰를 뽑는 메서드
        /// </summary>
        /// <param name="requestList">랜덤하게 뽑을 리스트 종류</param>
        /// <param name="count">그 수</param>
        private void AddRequestsByPercentage(List<RequestInfo> requestList, int count)
        {
            // 튜토리얼 때는 어차피 퀘스트 다 고정이니까 패스. 교수님 편지 답을 다 되게 해놔서 에러남.
            if ((DayManager.Instance.day == 0 || DayManager.Instance.day == 1) && TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
            {
                return;
            }
            count = Mathf.Min(count, requestList.Count);

            // 무작위 선정
            while (count > 0 && requestList.Count > 0)
            {
                int randomIndex = Random.Range(0, requestList.Count);
                RequestInfo request = requestList[randomIndex];

                if (todayRequests.Find(e => e.AnswerAttributes.Overlaps(request.AnswerAttributes)) != null)
                {
                    requestList.RemoveAt(randomIndex); // 중복 제거

                    continue;
                }
                count--;
                todayRequests.Add(request);
                requestList.RemoveAt(randomIndex); // 중복 제거
            }
        }
        /// <summary>
        /// 의뢰답장를 보내는 메서드
        /// </summary>
        public void SendRequest(Desk.Scroll scroll)
        {
            // 빈 요청서 보내면 스킵처리
            if (scroll == null)
            {
                // 튜토리얼 때는 반송 금지
                if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
                {
                    // 튜토리얼에서는 반송할 수 없습니다. 라는 UI 띄우기
                    if (denyReturnUI != null)
                    {
                        denyReturnUI.SetActive(true);
                    }
                    return;
                }
                CheckResponse(null);
                // 도전과제 해금
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_12");
            }
            else
            {
                // 튜토리얼 때 제대로 보내면 경고 메세지 끔
                if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
                {
                    // 튜토리얼에서는 반송할 수 없습니다. 라는 UI 띄우기
                    if (denyReturnUI != null)
                    {
                        denyReturnUI.SetActive(false);
                    }
                }
                if (currentRequest is SpecialRequestInfo)
                    scroll.ScrollInfo.Money = MoneyManager.Instance.GetSpecialPaymentValue(dayManager.day);
                CheckResponse(scroll.ScrollInfo);
                Destroy(scroll.gameObject);
                DeskManager.Instance.CreateNewScroll();
                ScrollCreationManager.Instance.ResetData();

                submitArea.SetActive(false);
                owl.Send();
                Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);
            }
        }
        /// <summary>
        /// 납부금액을 보내는 메서드
        /// </summary>
        public void SendPayment()
        {
            submitArea.SetActive(false);
            isPaymentDay = false;
            paymentOwl.Send();
        }
        public void SendEnding(EndingLetter endingLetter)
        {
            endingOwl.Envelope = endingLetter.gameObject;
            endingOwl.Send();
            //답장 보내면 엔딩으로 이어지도록.
            //0 - 성공(취업엔딩) / 1 - 성공(자영업 엔딩) / 2 - 실패 (노동엔딩)
            switch (endingLetter.EndingSceneCode)
            {
                case 0:
                    GameData.Instance.SetEndingCode(0);
                    break;
                case 1:
                    GameData.Instance.SetEndingCode(1);
                    break;
                case 2:
                    GameData.Instance.SetEndingCode(2);
                    break;
            }
            CanvasManager.Instance.FadeOut();
        }
        //부엉이 띄우기
        public void SendNewArrivalAlarm()
        {
            if (todayRequests.Count == 0)
            {
                owl.gameObject.SetActive(false);
                DeskManager.Instance.CloseWindow();
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 23)
                {
                    TutorialManager.Instance.ShowTutorialStep(TutorialManager.Instance.CurrentStepIndex);
                }
                return;
            }
            //부엉이 등장하고 알림띄우기
            owl.Reset();
            owl.gameObject.SetActive(true);
            //일반의뢰
            //SoundManager.Instance.PlaySFX("BirdLanding");
            SoundManager.Instance.PlaySFX("BirdLanding");

        }
        //납부부엉이 띄우기
        public void SendNewPaymentAlarm(bool isPaymentDay)
        {
            paymentOwl.gameObject.SetActive(true);
            //독촉장
            //SoundManager.Instance.PlaySFX("BirdLanding");
            SoundManager.Instance.PlaySFX("BirdLanding");

            if (isPaymentDay)
            {
                paymentOwl.GetPayment();
                this.isPaymentDay = true;
                submitArea.SetActive(true);
            }
            else
            {
                var tuple = MoneyManager.Instance.GetNextSettlementOfDay(DayManager.Instance.day);
                int day = tuple.Item1; ;
                int amount = tuple.Item2;
                paymentOwl.GivePayment(amount, day);
            }
        }
        /// <summary>
        /// 13일에 엔딩 편지 봉투 보내주는 부엉이
        /// </summary>
        public void SendNewEndingAlarm()
        {
            if (dayManager.day != 13)
            {
                return;
            }
            //부엉이 등장하고 알림띄우기
            endingOwl.Reset();
            endingOwl.gameObject.SetActive(true);
            submitArea.SetActive(true);
            //일반의뢰
            // SoundManager.Instance.PlaySFX("BirdLanding");
            SoundManager.Instance.PlaySFX("BirdLanding");
        }
        public void ShowNextRequests()
        {
            currentRequest = todayRequests[0];
            // Debug.Log("Current Request : " + currentRequest.Id);
            DeskManager.Instance.CreateNewOrderSheet(currentRequest);
            // todayRequest.RemoveAt(0);
            submitArea.SetActive(true);
            // 창밖 뷰 변경시작
            windowViewController.TimePassKey(todayRequests.Count);

            if (currentRequest is SpecialRequestInfo currentSpecialRequest)
            {
                GameData.Instance.AddQuestLetterId(currentSpecialRequest.Id);
                QuestManager.Instance.ShowQuestRecord(currentRequest.Id);
            }

            // 튜토리얼의 심볼 3개 써야 하는 의뢰 처음 나온 상황에서(1일차 마지막 의뢰) 튜토리얼 UI 보여줌 
            if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 36 && todayRequests.Count == 1)
            {
                TutorialManager.Instance.ShowTutorialStep(TutorialManager.Instance.CurrentStepIndex);
            }
        }
        /// <summary>
        /// 납부서 생성하는 메서드
        /// </summary>
        public void ShowPaymentRequest(int amount, int dueDate)
        {
            DeskManager.Instance.CreateNewPayment(amount, dueDate);
        }
        /// <summary>
        /// 엔딩 편지 보여주는 메서드
        /// </summary>
        public void ShowEndingRequest()
        {
            DeskManager.Instance.CreateEndingLetter(GameData.Instance.FinalResultCode);
        }
        /// <summary>
        /// 현재 수주하고 있는 레시피를 반환하는 메서드
        /// </summary>
        public RequestInfo GetCurrentRequest()
        {
            return currentRequest;
        }

        ///<summary>
        ///특수 의뢰 반송하고 후처리
        ///</summary>
        ///
        public void ReturnSpecialRequest()
        {
            submitArea.SetActive(false);
            owl.Send();
            owl.SetFeedBack(false);
            Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);

            // 도전 과제
            GameData.Instance.isAllPass = false;
        }


        /// <summary>
        /// 제출한 의뢰와 답을 비교하는 메서드
        /// </summary>
        public void CheckResponse(ScrollInfo submitedScroll)
        {
            // 완료된 마법진의 정보와 기존의 저장된 의뢰 답 정보를 비교하고
            // 정답일 시 "돈 관리" 메서드의 돈 변수 +
            // 오답일 시 incorrectResponse List에서 넣어두고 정산 때, 오답 처리된 부분 알려주기
            // null 이면 오답처리 ( 스킵처리 )
            if (submitedScroll == null)
            {
                if (currentRequest is SpecialRequestInfo specialRequest)
                {

                    List<string> exceptQuestIds = new List<string>
                    {
                        "Quest5",
                        "Quest6",
                        "Quest7"
                    };

                    if (!exceptQuestIds.Contains(specialRequest.QuestId))
                        PopupManager.Instance.OpenQuestSkipAlert();
                    else
                    {
                        submitArea.SetActive(false);
                        owl.Send();
                        owl.SetFeedBack(false);
                        Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);

                        // 도전 과제
                        GameData.Instance.isAllPass = false;
                    }
                }
                else
                {
                    submitArea.SetActive(false);
                    owl.Send();
                    owl.SetFeedBack(false);
                    Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);

                    // 도전 과제
                    GameData.Instance.isAllPass = false;
                }
            }
            else if (currentRequest.AnswerIds.Contains(submitedScroll.Answer) && currentRequest.RequiredDurations.Contains(submitedScroll.Duration))
            {
                CorrectRequest correctRequest = new CorrectRequest();

                correctRequest.requestId = currentRequest.Id;
                correctRequest.money = submitedScroll.Money;
                if (currentRequest is SpecialRequestInfo specialRequest)
                {
                    correctRequest.replyId = specialRequest.RightReply;

                    string questId = specialRequest.QuestId;
                    int index = specialRequest.QuestIndex;

                    List<string> exceptQuestIds = new List<string>
                    {
                        "Quest5",
                        "Quest6",
                        "Quest7"
                    };

                    if (!exceptQuestIds.Contains(specialRequest.QuestId))
                        GameData.Instance.CompleteQuest(questId, index, 1);
                }

                correctRequest.submittedScroll = submitedScroll;    // 제출한 스크롤 저장
                correctRequest.requestInfo = currentRequest;           // 받은 의뢰 정보 저장

                prevRequestResults.Add(correctRequest);
                owl.SetFeedBack(true);
                Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);

                if (currentRequest.Id.Equals("Quest3_0R"))
                {
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_14");
                }
            }
            else
            {
                IncorrectRequest incorrectRequest = new IncorrectRequest();
                incorrectRequest.requestId = currentRequest.Id;
                if (currentRequest is SpecialRequestInfo specialRequest)
                {
                    incorrectRequest.replyId = specialRequest.WrongReply;

                    string questId = specialRequest.QuestId;
                    int index = specialRequest.QuestIndex;


                    List<string> exceptQuestIds = new List<string>
                    {
                        "Quest5",
                        "Quest6",
                        "Quest7"
                    };

                    if (!exceptQuestIds.Contains(specialRequest.QuestId))
                        GameData.Instance.CompleteQuest(questId, index, 2);
                }

                incorrectRequest.submittedScroll = submitedScroll;  // 제출한 스크롤 저장
                incorrectRequest.requestInfo = currentRequest;         // 받은 의뢰 정보 저장

                if (!currentRequest.AnswerIds.Contains(submitedScroll.Answer) && !currentRequest.RequiredDurations.Contains(submitedScroll.Duration))
                {
                    incorrectRequest.isWrongBoth = true;
                    incorrectRequest.isWrongEffect = true;
                    incorrectRequest.isWrongDuration = true;
                }
                else if (!currentRequest.AnswerIds.Contains(submitedScroll.Answer))
                {
                    incorrectRequest.isWrongEffect = true;
                }
                else
                {
                    incorrectRequest.isWrongDuration = true;
                }

                prevRequestResults.Add(incorrectRequest);
                owl.SetFeedBack(false);
                Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);

                // 도전 과제
                GameData.Instance.isAllPass = false;

                if (currentRequest.Id.Equals("Quest3_0R"))
                {
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_14");
                }
            }

            // 역해석처럼 답 하나인 애들에만 적용해주기(중복 정답이어서 빠지지 않게)
            if (currentRequest is SpecialRequestInfo cRequest && cRequest.AnswerAttributes.Count == 1)
            {
                reverseInterpretations.ExceptWith(cRequest.AnswerAttributes);
            }
            todayRequests.Remove(currentRequest);
        }

        public List<RequestResult> GetPreviousRequestResult()
        {
            return prevRequestResults;
        }

        public void ClearPreviousRequestResult()
        {
            prevRequestResults.Clear();
        }

        /// <summary>
        /// 하루가 끝났을 때 총 맞은 의뢰 수와 오답 목록 체크
        /// </summary>
        public void EndOfSummary()
        {
            // 돈이 얼마나 올랐는지 보여주고
            // 어떤 부분에서 틀렸는지 알려주기
            // UI와 연결되는 부분
        }

        /// <summary>
        /// ID를 기준으로 RequestInfo 반환. 영수증 상세 보기에 사용 ㅎ
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public RequestInfo GetRequestById(string requestId)
        {
            return allRequest.Find(request => request.Id == requestId);
        }


        #region 저장 데이터 로드
        public void LoadAssignedRequests()
        {
            foreach (RequestInfo requestInfo in GameData.SaveInfo.assignedRequests)
            {
                RequestInfo findRequest = allRequest.Find(e => e.Id == requestInfo.Id);
                assignedRequests.Add(findRequest);
            }
        }

        public void LoadPrevResults()
        {
            prevRequestResults = GameData.SaveInfo.prevResults;
        }

        public void LoadSpecialQuestAttributes()
        {
            reverseInterpretations = GameData.SaveInfo.reverseInterpretations;
        }


        #endregion

        #region QA용
        public void SendRequest(ScrollInfo scrollInfo)
        {
            // 빈 요청서 보내면 스킵처리
            if (scrollInfo == null)
            {
                // 튜토리얼 때는 반송 금지
                if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
                {
                    // 튜토리얼에서는 반송할 수 없습니다. 라는 UI 띄우기
                    if (denyReturnUI != null)
                    {
                        denyReturnUI.SetActive(true);
                    }
                    return;
                }
                CheckResponse(null);
            }
            else
            {
                // 튜토리얼 때 제대로 보내면 경고 메세지 끔
                if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
                {
                    // 튜토리얼에서는 반송할 수 없습니다. 라는 UI 띄우기
                    if (denyReturnUI != null)
                    {
                        denyReturnUI.SetActive(false);
                    }
                }
                if (currentRequest is SpecialRequestInfo)
                    scrollInfo.Money = MoneyManager.Instance.GetSpecialPaymentValue(dayManager.day);
                CheckResponse(scrollInfo);
                if (DeskManager.Instance.CurrentScroll != null)
                    Destroy(DeskManager.Instance.CurrentScroll.gameObject);
                DeskManager.Instance.CreateNewScroll();
                ScrollCreationManager.Instance.ResetData();

                submitArea.SetActive(false);
                owl.Send();
                Destroy(DeskManager.Instance.CurrentOrderSheet.gameObject);
            }
        }
        #endregion
    }
}