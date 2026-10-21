using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Desk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class PaymentResultLetter : MonoBehaviour, IPointerClickHandler
{
    [Header("결제 결과 이미지")]
    [SerializeField]
    private Image paymentResultImage;
    [Tooltip("[0] = 완납, [1] = 채납")]
    [SerializeField]
    private List<Sprite> KoreanImageList = new List<Sprite>();
    [SerializeField]
    private List<Sprite> EnglishImageList = new List<Sprite>();
    [SerializeField]
    private List<Sprite> JapaneseImageList = new List<Sprite>();

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        switch (GameData.Instance.language)
        {
            case Language.Kor:
                paymentResultImage.sprite = KoreanImageList[GameData.Instance.FinalResultCode];
                break;
            case Language.Eng:
                paymentResultImage.sprite = EnglishImageList[GameData.Instance.FinalResultCode];
                break;
            case Language.Jp:
                paymentResultImage.sprite = JapaneseImageList[GameData.Instance.FinalResultCode];
                break;
            default:
                paymentResultImage.sprite = EnglishImageList[GameData.Instance.FinalResultCode];
                break;
        }
        Vector3 randomTargetPos = new Vector3(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(-50, 50), 0);
        transform.DOLocalMove(transform.localPosition + randomTargetPos, 1f).SetEase(Ease.OutExpo);
    }
    #region ZoomInZoomOut
    private Vector2 _previousPosition;
    private bool _isPlacedOnDesk = true;
    private RectTransform _rectTransform;
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
    #endregion
}