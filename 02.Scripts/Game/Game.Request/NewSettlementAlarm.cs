using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Game.Request;
using Game.Desk;

public class NewSettlementAlarm : MonoBehaviour
{
    [Header("부엉이")]
    [SerializeField]
    private GameObject envelope;
    private Image _envelopeImage;
    private Image _owlImage;
    [SerializeField]
    private Image _owlMouse;
    [SerializeField]
    private Image _owlEye1;
    [SerializeField]
    private Image _owlEye2;
    [Header("지불 금액")]
    private int _amount;
    [Header("납부 기한")]
    private int _dueDate;
    private Vector3 _initialPosition;
    [Header("방 부엉이 이미지")]
    [SerializeField]
    private GameObject owlInRoom;
    void Awake()
    {
        _envelopeImage = envelope.GetComponent<Image>();
        _owlImage = GetComponent<Image>();

        _envelopeImage.alphaHitTestMinimumThreshold = 0.1f;
        _owlImage.alphaHitTestMinimumThreshold = 0.1f;
        _owlMouse.alphaHitTestMinimumThreshold = 0.1f;

        _envelopeImage.color = new Color(1f, 1f, 1f, 0f);
        _owlMouse.color = new Color(1f, 1f, 1f, 0f);
        _owlImage.color = new Color(1f, 1f, 1f, 0f);

        _initialPosition = envelope.transform.localPosition;
    }
    void Start()
    {
    }
    void OnEnable()
    {
        _owlImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
        _owlMouse.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
        _envelopeImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
        _owlEye1.DOFade(1, 0.7f).SetEase(Ease.InSine);
        _owlEye2.DOFade(1, 0.7f).SetEase(Ease.InSine);
        owlInRoom.SetActive(true);
        GetComponent<Animator>().speed = 1;
    }
    void OnDisable()
    {
        owlInRoom.SetActive(false);
    }
    public void Reset()
    {
        _envelopeImage.color = new Color(1, 1, 1, 0);
        _owlMouse.color = new Color(1, 1, 1, 0);
        _owlImage.color = new Color(1, 1, 1, 0);
        _owlEye1.color = new Color(1, 1, 1, 0);
        _owlEye2.color = new Color(1, 1, 1, 0);
        owlInRoom.SetActive(false);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// 납부서 주는 모드로 전환
    /// </summary>
    public void GivePayment(int amount, int dueDate)
    {
        _amount = amount;
        _dueDate = dueDate;
        GetComponent<Button>().interactable = true;
        _envelopeImage.DOFade(1, 0.7f).SetEase(Ease.InSine);
        _owlImage.DOFade(1, 0.7f).SetEase(Ease.InSine);
        _owlMouse.DOFade(1, 0.7f).SetEase(Ease.InSine);
        envelope.transform.localPosition = _initialPosition;
        envelope.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    /// <summary>
    /// 납부서 받는 모드로 전환
    /// </summary>
    public void GetPayment()
    {
        GetComponent<Button>().interactable = false;
        _envelopeImage.DOKill();
        _owlMouse.DOKill();
        _envelopeImage.color = new Color(1, 1, 1, 0);
        _owlMouse.color = new Color(1, 1, 1, 0);
    }
    /// <summary>
    /// 납부서 받기
    /// </summary>
    public void OnButtonClick()
    {
        GetComponent<Button>().interactable = false;
        envelope.transform.DOLocalMoveY(-300, 0.5f).SetEase(Ease.InSine);
        envelope.transform.DORotate(new Vector3(0, 0, 60), 0.5f).SetEase(Ease.InSine).onComplete += () =>
        {
            RequestManager.Instance.ShowPaymentRequest(_amount, _dueDate);
            FadeOwl();
        };
    }
    /// <summary>
    /// 납부서 보내기
    /// </summary>
    public void Send()
    {
        //납부엉이 날라가기
        SoundManager.Instance.PlaySFX("BirdGone");
        envelope.transform.localPosition = new Vector3(-15, -3, 0);
        envelope.transform.rotation = Quaternion.Euler(0, 0, 0);
        _owlMouse.DOColor(new Color(1, 1, 1, 0), 1f);
        _envelopeImage.DOColor(new Color(1, 1, 1, 0), 1f);
        _owlEye1.DOFade(0, 1f);
        _owlEye2.DOFade(0, 1f);
        GetComponent<Animator>().speed = 0;
        _owlImage.DOColor(new Color(1, 1, 1, 0), 1f).onComplete += () =>
        {
            if (DayManager.Instance.day != 13)
            {
                gameObject.SetActive(false);
                RequestManager.Instance.SendNewArrivalAlarm();
            }
            if (DayManager.Instance.day == 13)
            {
                RequestManager.Instance.SendNewEndingAlarm();
            }
        };

    }
    /// <summary>
    /// 납부서 주고 사라지기
    /// </summary>
    private void FadeOwl()
    {
        GetComponent<Animator>().speed = 0;
        GetComponent<Button>().interactable = false;
        _owlMouse.DOColor(new Color(1, 1, 1, 0), 1f);
        _envelopeImage.DOColor(new Color(1, 1, 1, 0), 1f);
        _owlEye1.DOFade(0, 1f);
        _owlEye2.DOFade(0, 1f);
        _owlImage.DOColor(new Color(1, 1, 1, 0), 1f).onComplete += () =>
        {
            if (DayManager.Instance.day != 13)
            {
                owlInRoom.SetActive(false);
                RequestManager.Instance.SendNewArrivalAlarm();
                gameObject.SetActive(false);
            }
        };
    }
}