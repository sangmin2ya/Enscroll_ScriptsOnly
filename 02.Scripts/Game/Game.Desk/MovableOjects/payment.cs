using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Request;
using DG.Tweening;
using UnityEngine.EventSystems;
using Unity.VisualScripting;


namespace Game.Desk
{
    public class Payment : MonoBehaviour, IPointerClickHandler, IDeskItem
    {
        [Header("책상 위치")]
        private bool _isPlacedOnDesk = true;
        // IDeskItem: DragableObject가 제출/서랍 복귀 시 호출
        bool IDeskItem.CanSubmit => Game.Request.RequestManager.Instance.isPaymentDay;
        void IDeskItem.OnSubmitted() { SendPayment(); }
        void IDeskItem.OnReturnedToDrawer() { }

        public bool IsPlacedOnDesk
        {
            get { return _isPlacedOnDesk; }
            set { _isPlacedOnDesk = value; }
        }
        [Header("지불 금액")]
        [SerializeField]
        private int paymentAmount;
        [SerializeField]
        private TextMeshProUGUI paymentText;
        [SerializeField]
        private TextMeshProUGUI additionalPaymentText;
        [SerializeField]
        private TextMeshProUGUI dueDateText;
        [SerializeField]
        private TextMeshProUGUI allPaymentText;

        [Header("지불 버튼")]
        [SerializeField]
        private Button yesButton;
        [SerializeField]
        private Button noButton;
        [SerializeField]
        private Image yesCheck;
        [SerializeField]
        private Image noCheck;
        [Header("지불 불가 경고")]
        [SerializeField]
        private TextMeshProUGUI warning;
        private bool areYouGonnaPay = false;
        public bool AreYouGonnaPay
        {
            get { return areYouGonnaPay; }
        }
        private RectTransform _rectTransform;
        private void Awake()
        {
            yesButton.onClick.AddListener(() =>
            {
                if (CheckIfYouCanPay())
                {
                    areYouGonnaPay = true;
                    //MoneyManager.Instance.AddMoney(-paymentAmount);
                    SelectPayment();
                }
                else
                {
                    Color color = warning.color;
                    warning.DOColor(new Color(color.r, color.g, color.b, 1), 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                    {
                        warning.DOColor(new Color(color.r, color.g, color.b, 1), 1f).SetEase(Ease.OutQuad).onComplete += () =>
                        warning.DOColor(new Color(color.r, color.g, color.b, 0), 0.3f).SetEase(Ease.OutQuad);
                    });
                }
            });
            noButton.onClick.AddListener(() =>
            {
                areYouGonnaPay = false;
                //MoneyManager.Instance.AddMoney(paymentAmount);
                SelectPayment();
            });

            yesCheck.gameObject.SetActive(false);
            noCheck.gameObject.SetActive(true);

            _rectTransform = GetComponent<RectTransform>();

            _rectTransform.DOLocalMoveY(0, 1f).SetEase(Ease.OutQuad);
            _rectTransform.DORotate(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _isPlacedOnDesk = true;
            });
        }

        public void LocatePaymentInDraweer()
        {
            _rectTransform.DOKill();
            GetComponent<DragableObject>().LocatePayment();
        }

        public void SetAmount(int amount, int dueDate)
        {
            paymentAmount = amount; //상환금액
            int unPaidAmount = MoneyManager.Instance.UncompletedSettlement; // 미납금액
            int additionalPaymentAmount = MoneyManager.Instance.GetAdditionalPaymentValue(); // 추가납부금액

            dueDateText.text = "Day" + (dueDate + 1).ToString();
            paymentText.text = amount.ToString();
            if (MoneyManager.Instance.UncompletedSettlement > 0)
            {
                additionalPaymentText.text = unPaidAmount + " <color=red>( +" + additionalPaymentAmount + " )</color>";
                paymentAmount = amount + unPaidAmount + additionalPaymentAmount;
            }
            else
            {
                additionalPaymentText.text = 0 + " G";
                paymentAmount = amount;
            }
            allPaymentText.text = paymentAmount.ToString() + " G";
        }
        private void SelectPayment()
        {
            yesCheck.gameObject.SetActive(areYouGonnaPay);
            noCheck.gameObject.SetActive(!areYouGonnaPay);
        }
        public bool CheckIfYouCanPay()
        {
            return MoneyManager.Instance.totalBalance >= paymentAmount;
        }
        /// <summary>
        /// 상환금 납부하는 메소드. 
        /// </summary>
        public void SendPayment()
        {
            if (areYouGonnaPay)
            {
                MoneyManager.Instance.SpendMoney(paymentAmount);
                paymentAmount = 0;
                MoneyManager.Instance.SettlementComplete();
                //납부 성공했음을 알려주는 이벤트 추가
                //마지막 날에는 바로 완납 처리
                if (DayManager.Instance.day == 13)
                {
                    // 도전 과제
                    if (GameData.Instance.isNoDelay)
                        SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_7");

                    GameData.Instance.SetFinalResultCode(0);
                    // 도전과제 해금
                    SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_6");
                }
                RequestManager.Instance.SendPayment();
                Destroy(gameObject);
            }
            else
            {
                // 만약 잔고가 넉넉한데 연기하려고 하면
                if (MoneyManager.Instance.totalBalance >= paymentAmount)
                {
                    PopupManager.Instance.OpenPaymentDelayAlert(paymentAmount, this.gameObject);
                }
                else
                {
                    SendPaymentDelay();
                }
            }
        }
        public void SendPaymentDelay()
        {
            // 도전 과제
            GameData.Instance.isNoDelay = false;

            MoneyManager.Instance.SettlementUncomplete(paymentAmount);
            if (DayManager.Instance.day == 13)
            {
                GameData.Instance.SetFinalResultCode(1);
            }
            RequestManager.Instance.SendPayment();
            Destroy(gameObject);
        }
        #region ZoomInZoomOut
        private Vector2 _previousPosition;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (GetComponent<DragableObject>().IsDragging)
            {
                GetComponent<DragableObject>().IsDragging = false;
                return;
            }
            if (_isPlacedOnDesk)
            {   // 튜토리얼 특정 단계에서는 줌인 수동으로 안되게 함
                // 영수증에 있는 애들은 되야 함
                ZoomInOrder();
            }
        }
        public void ZoomInOrder()
        {
            if (!_isPlacedOnDesk)
            {
                return;
            }
            _rectTransform.DOKill();
            DeskManager.Instance.BlurPanel.gameObject.SetActive(true);
            DeskManager.Instance.BlurPanel.GetComponent<Image>().DOFade(0.93f, 0.3f);
            GetComponent<DragableObject>().IsDraggable = false;
            _isPlacedOnDesk = false;

            _rectTransform.SetParent(DeskManager.Instance.BlurPanel);
            _previousPosition = _rectTransform.position;
            _rectTransform.SetAsLastSibling();
            transform.parent.SetAsLastSibling();
            _rectTransform.DOScale(new Vector3(1.8f, 1.8f, 1f), 0.4f).SetEase(Ease.OutQuad);
            _rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.4f).SetEase(Ease.OutQuad).onComplete = () =>
            {
                DeskManager.Instance.BlurPanel.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ZoomOutOrder();
                    DeskManager.Instance.BlurPanel.GetComponent<Button>().onClick.RemoveAllListeners();
                });
            };
        }
        public void ZoomOutOrder()
        {
            if (_isPlacedOnDesk)
            {
                return;
            }
            _rectTransform.DOKill();
            DeskManager.Instance.BlurPanel.GetComponent<Image>().DOFade(0, 0.4f);
            _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.4f).SetEase(Ease.OutQuad);
            _rectTransform.DOMove(_previousPosition, 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _rectTransform.SetParent(CanvasManager.Instance.MainDeskUI);
                DeskManager.Instance.BlurPanel.gameObject.SetActive(false);
                GetComponent<DragableObject>().IsDraggable = true;
                _isPlacedOnDesk = true;
            });

        }
    }
    #endregion
}