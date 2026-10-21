using UnityEngine;
using DG.Tweening;
using Game.Request;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Desk
{
    public enum endingType
    {
        GOOD, BAD
    }
    public class EndingLetter : MonoBehaviour, IPointerClickHandler
    {
        [Header("엔딩 코드")]
        [SerializeField]
        private endingType endingType;
        public endingType EndingType => endingType;
        [Header("엔딩 씬 코드")]
        [Tooltip("0 : 스카우트 엔딩, 1 : 자영업 엔딩, 2 : 압류 엔딩")]
        [Range(0, 2)]
        [SerializeField]
        private int endingSceneCode;
        public int EndingSceneCode => endingSceneCode;

        [SerializeField]
        private Image resultImage;
        [SerializeField]
        private Sprite KoreanImage;
        [SerializeField]
        private Sprite EnglishImage;
        [SerializeField]
        private Sprite JapaneseImage;

        [SerializeField]
        private GameObject stamp;
        private RectTransform _stampRect;
        private bool _isStamped = false;
        public bool IsStamped => _isStamped;
        void Start()
        {
            _stampRect = stamp.GetComponent<RectTransform>();
            _rectTransform = GetComponent<RectTransform>();
            stamp.SetActive(false);
            if (endingType == endingType.GOOD)
            {
                endingSceneCode = 1;
            }
            else
            {
                endingSceneCode = 2;
            }
            Vector3 randomTargetPos = new Vector3(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(-50, 50), 0);
            transform.DOLocalMove(transform.localPosition + randomTargetPos, 1f).SetEase(Ease.OutExpo);
            RequestManager.Instance.Stamp.EndingLetterUI = this.GetComponent<RectTransform>();

            switch (GameData.Instance.language)
            {
                case Language.Kor:
                    resultImage.sprite = KoreanImage;
                    break;
                case Language.Eng:
                    resultImage.sprite = EnglishImage;
                    break;
                case Language.Jp:
                    resultImage.sprite = JapaneseImage;
                    break;
                default:
                    resultImage.sprite = EnglishImage;
                    break;
            }
        }
        public void Stamp(Vector3 stampPos)
        {
            if (_isStamped)
            {
                return;
            }
            _isStamped = true;
            _stampRect.position = stampPos;
            stamp.SetActive(true);
            if (endingType == endingType.GOOD)
            {
                endingSceneCode = 0;
            }
            else
            {
                endingSceneCode = 2;
            }
        }
        public void SendLetter()
        {
            RequestManager.Instance.SendEnding(this);
        }
        #region ZoomInZoomOut
        private Vector2 _previousPosition;
        private bool _isPlacedOnDesk = true;
        public bool IsPlacedOnDesk
        {
            get { return _isPlacedOnDesk; }
            set { _isPlacedOnDesk = value; }
        }
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
}