using UnityEngine;
using Game.Request;
using Game.Scroll;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

namespace Game.Desk
{
    public class DeskManager : Singleton<DeskManager>
    {
        [Header("상호작용 가능한 오브젝트")]
        [SerializeField]
        private DragableObject[] interactableObjects;
        [SerializeField]
        private RectTransform window;
        [SerializeField]
        private RectTransform roomWindow;
        private bool isStartWork = false;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject memoRequestSheetPrefab;
        [SerializeField]
        private GameObject secretMemoRequestSheetPrefab;
        [SerializeField]
        private GameObject normalLetterRequestSheetPrefab;
        [SerializeField]
        private GameObject kidLetterRequestSheetPrefab;
        [SerializeField]
        private GameObject prettyLetterRequestSheetPrefab;
        [SerializeField]
        private GameObject royalLetterRequestSheetPrefab;
        [SerializeField]
        private GameObject normalNotificationRequestSheetPrefab;
        [SerializeField]
        private GameObject EmpireNotificationRequestSheetPrefab;
        [SerializeField]
        private GameObject orderSheetRequestSheetPrefab;

        [Header("영어 - 한국어 폰트")]
        [SerializeField] List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();

        [Header("일본어 폰트")]
        [SerializeField] List<TMP_FontAsset> fonts_jp = new List<TMP_FontAsset>();

        [Header("납부서")]
        [SerializeField]
        private GameObject paymentPrefab;
        [SerializeField]
        private GameObject finalPaymentPrefab;
        [Header("엔딩 편지가 담겨있는 엔딩 봉투")]
        [SerializeField]
        private GameObject endingEnvelope;

        [SerializeField]
        private GameObject scrollPrefab;
        [SerializeField]
        [Header("References")]
        private RectTransform deskCanvas;
        [SerializeField]
        private RectTransform blurPanel;
        public RectTransform BlurPanel
        {
            get { return blurPanel; }
        }
        private Scroll _currentScroll;
        public Scroll CurrentScroll
        {
            get { return _currentScroll; }
        }
        [SerializeField] private RequestSheet _currentOrderSheet;
        public RequestSheet CurrentOrderSheet
        {
            get { return _currentOrderSheet; }
        }
        [SerializeField] private Recipt recipt;
        [SerializeField] private Reply reply;
        [SerializeField] private DetailRequestPanel detailRequestPanel;

        public void Start()
        {
            #region 저장 데이터
            // 저장 데이터에 의해 남은게 있다면 이거 실행
            LoadPrevResult();

            List<int> days = new List<int> { 3, 4, 6, 7, 9, 10, 12, 13 };
            if (days.Contains(DayManager.Instance.day))
            {
                int targetDay;
                switch (DayManager.Instance.day)
                {
                    case 3:
                    case 4:
                        targetDay = 2;
                        break;
                    case 6:
                    case 7:
                        targetDay = 5;
                        break;
                    case 9:
                    case 10:
                        targetDay = 8;
                        break;
                    case 12:
                    case 13:
                        targetDay = 11;
                        break;
                    default:
                        targetDay = 5;
                        break;
                }
                var tuple = MoneyManager.Instance.GetNextSettlementOfDay(targetDay);
                int day = tuple.Item1;
                int amount = tuple.Item2;

                Payment payment = CreateNewPayment(amount, day);
                payment.LocatePaymentInDraweer();
            }
            #endregion

            if (DayManager.Instance.day != 13)
            {
                CreateNewScroll();
            }
        }

        public void LoadPrevResult()
        {
            recipt.CreateRecipt();
            reply.CreateReply();

            RequestManager.Instance.ClearPreviousRequestResult();
        }
        public void OrganizeDesk()
        {
            //날 지날때 책상에 물건 넣어주는 코드
            foreach (var obj in interactableObjects)
            {
                if (obj == null || obj.gameObject.activeSelf == false)
                {
                    continue;
                }
                obj.OrganizeObject();
            }

            isStartWork = false;
            CloseWindow();
        }

        public void OpenWindow()
        {
            if (!isStartWork)
            {
                SoundManager.Instance.PlaySFX("Windowopen");
                isStartWork = true;
                roomWindow.DOAnchorPos(new Vector2(-1, 79), 0.5f).SetEase(Ease.OutQuad);
                window.DOAnchorPos(new Vector2(0, 60), 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    // 납부일이라면
                    if (MoneyManager.Instance.GetSettlementOfDay(DayManager.Instance.day) != -1)
                    {
                        //납부서 제출용 납부 부엉이 소환!
                        RequestManager.Instance.SendNewPaymentAlarm(true);
                    }
                    // 납부일 하루 뒤라면
                    else if (MoneyManager.Instance.GetNextSettlementOfDay(DayManager.Instance.day) != null)
                    {
                        //납부서 가져다 주는 납부 부엉이 소환!
                        var amountToPay = MoneyManager.Instance.GetNextSettlementOfDay(DayManager.Instance.day);
                        RequestManager.Instance.SendNewPaymentAlarm(false);
                        return;
                    }
                    //아무것도 아니라면
                    else
                    {
                        RequestManager.Instance.SendNewArrivalAlarm();

                    }
                });
            }
        }

        public void CloseWindow()
        {
            window.DOAnchorPos(new Vector2(0, -170), 0.5f).SetEase(Ease.OutQuad);
            roomWindow.DOAnchorPos(new Vector2(-1, -80), 0.5f).SetEase(Ease.OutQuad);
            SoundManager.Instance.PlaySFX("Windowclosed");
        }
        public void SetStampArea(RectTransform stampArea)
        {
            interactableObjects[1].ScrollUIPosition = stampArea;
        }

        public void CreateNewOrderSheet(RequestInfo request)
        {
            RectTransform orderSheet = null;

            // 일반 의뢰
            if (request is NormalRequestInfo nRequest)
            {
                GameObject prefab = GetRequestPrefab(nRequest);

                if (prefab != null)
                {
                    orderSheet = Instantiate(prefab, deskCanvas).GetComponent<RectTransform>();
                    _currentOrderSheet = orderSheet?.GetComponent<RequestSheet>();
                    _currentScroll.RequestSheetPlace = orderSheet;
                    _currentOrderSheet.GetComponent<NormalRequestSheet>().SetData(nRequest);
                }
                else
                {
                    Debug.LogWarning($"프리팹이 설정되지 않은 RequestType: {nRequest.RequestType}");
                }
            }
            // 특수 의뢰
            else if (request is SpecialRequestInfo sRequest)
            {
                string path = string.Format("Prefabs/QuestLetter/Request/{0}/{1}", GameData.Instance.langauage, sRequest.Id);
                orderSheet = Instantiate(Resources.Load<GameObject>(path), deskCanvas).GetComponent<RectTransform>();
                _currentOrderSheet = orderSheet?.GetComponent<RequestSheet>();
                _currentScroll.RequestSheetPlace = orderSheet;
            }

        }
        /// <summary>
        /// 새로운 스크롤을 생성, 스크롤의 제출 위치 설정, 스크롤 생성 매니저에 스크롤 등록, 현재 스크롤을 설정
        /// </summary>
        public void CreateNewScroll()
        {
            var scroll = Instantiate(scrollPrefab, deskCanvas).GetComponent<Scroll>();
            scroll.transform.SetAsFirstSibling();
            ScrollCreationManager.Instance.RegisterNewScroll(scroll, scroll.EnchantArea);
            _currentScroll = scroll;
            MagicController.Instance.ableToEnchant = true;
        }
        /// <summary>
        /// 새로운 납부서를 생성
        /// </summary>
        public Payment CreateNewPayment(int amount, int dueDate)
        {
            if (DayManager.Instance.day >= 11)
            {
                Payment payment = Instantiate(finalPaymentPrefab, deskCanvas).GetComponent<Payment>();
                payment.SetAmount(amount, dueDate);
                return payment;
            }
            else
            {
                Payment payment = Instantiate(paymentPrefab, deskCanvas).GetComponent<Payment>();
                payment.SetAmount(amount, dueDate);
                return payment;
            }
            return null;
        }
        /// <summary>
        /// 엔딩 봉투 생성
        /// </summary>
        /// <param name="endingType">0 = 상환성공, 1 = 상환실패</param>
        public void CreateEndingLetter(int endingType)
        {
            Instantiate(endingEnvelope, deskCanvas).GetComponent<EndingEnvelope>();
        }

        public void EraseOrderSheet()
        {
            if (_currentOrderSheet != null)
                Destroy(CurrentOrderSheet.gameObject);
            _currentOrderSheet = null;
        }

        /// <summary>
        /// 영수증의 상세 의뢰 정보 버튼 클릭 시 호출
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="submittedScroll"></param>
        public void OpenDetail(RequestInfo requestInfo, ScrollInfo submittedScroll)
        {
            if (detailRequestPanel != null)
            {
                detailRequestPanel.UpdatePanel(requestInfo, submittedScroll);
            }
        }

        /// <summary>
        /// RequestInfo의 RequestType에 따라 적절한 프리팹 반환
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        public GameObject GetRequestPrefab(NormalRequestInfo requestInfo)
        {
            switch (requestInfo.RequestType)
            {
                case RequestType.아이편지:
                    return kidLetterRequestSheetPrefab;

                case RequestType.이쁜편지:
                    return prettyLetterRequestSheetPrefab;

                case RequestType.귀족편지:
                    return royalLetterRequestSheetPrefab;

                case RequestType.일반편지:
                    return normalLetterRequestSheetPrefab;

                case RequestType.제국공고:
                    return EmpireNotificationRequestSheetPrefab;

                case RequestType.주문서:
                    return orderSheetRequestSheetPrefab;

                case RequestType.메모:
                    return memoRequestSheetPrefab;

                case RequestType.일반공고:
                    return normalNotificationRequestSheetPrefab;

                case RequestType.비밀메모:
                    return secretMemoRequestSheetPrefab;

                default:
                    Debug.LogWarning($"RequestType '{requestInfo.RequestType}'에 맞는 프리팹이 없습니다.");
                    return null;
            }
        }

        public TMP_FontAsset GetRequestFont(int type)
        {
            switch (GameData.Instance.langauage)
            {
                case Langauage.Kor:
                case Langauage.Eng:
                    return fonts[type];

                case Langauage.Jp:
                    return fonts_jp[type];

                default:
                    return fonts[type];
            }
        }
    }
}