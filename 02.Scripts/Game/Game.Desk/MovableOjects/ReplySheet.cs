using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using Game.Request;

public class ReplySheet : MonoBehaviour, IPointerClickHandler
{
    [Header("답장 정보")]
    public int questIndex;

    private Canvas _canvas;
    private RectTransform _rectTransform;
    [SerializeField] private RectTransform _parentRectTransform;

    private bool _isPlacedOnDesk = true;
    public bool IsPlacedOnDesk
    {
        get { return _isPlacedOnDesk; }
        set { _isPlacedOnDesk = value; }
    }
    private Vector2 _previousPosition;

    public bool isOnFile = false;

    Button closeBtn;

    void Start()
    {
        questIndex = RequestCsv.ParseQuestIndex(gameObject.name);

        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = _rectTransform.parent as RectTransform;
        closeBtn = transform.Find("내부컨텐츠").Find("CloseButton").GetComponent<Button>();
        closeBtn.onClick.AddListener(CloseSheet);

        Vector3 randomTargetPos = new Vector3(UnityEngine.Random.Range(-40, 40), UnityEngine.Random.Range(-30, 30), 0);
        transform.DOLocalMove(transform.localPosition + randomTargetPos, 1f).SetEase(Ease.OutExpo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GetComponent<DragableObject>().IsDragging)
            {
                GetComponent<DragableObject>().IsDragging = false;
                return;
            }
            if (_isPlacedOnDesk)
            {
                ZoomInOrder();
            }
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
        _rectTransform.DOScale(new Vector3(1.6f, 1.6f, 1f), 0.5f).SetEase(Ease.OutQuad);
        _rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.OutQuad).onComplete = () =>
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
        DeskManager.Instance.BlurPanel.GetComponent<Image>().DOFade(0, 0.3f);
        _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.OutQuad);
        _rectTransform.DOMove(_previousPosition, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            _rectTransform.SetParent(_parentRectTransform, true);
            GetComponent<DragableObject>().IsDraggable = true;
            DeskManager.Instance.BlurPanel.gameObject.SetActive(false);
            _isPlacedOnDesk = true;
        });
    }

    public void CloseSheet()
    {
        if (DeskManager.Instance.BlurPanel.gameObject.activeSelf)
        {
            DeskManager.Instance.BlurPanel.GetComponent<Image>().DOFade(0, 0.3f).OnComplete(() =>
            {
                DeskManager.Instance.BlurPanel.gameObject.SetActive(false);
            });
        }
        // gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
