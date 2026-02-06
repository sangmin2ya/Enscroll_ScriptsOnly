using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Magic
{
    public class RuneStoneEffect : MonoBehaviour
    {
        [SerializeField]
        private Sprite originalSprite;
        [SerializeField]
        private Sprite runeStoneSprite;
        private RectTransform _rectTransform;
        private Vector2 initialPosition;
        private Tween _rectTween;
        private float scaleAmount = 0.03f;  // Scale 변화량
        private float scaleDuration = 1f;  // Scale 변화 시간
        private float circleRadius = 3f;  // 원형 궤적 반지름
        private float circleDuration = 1f; // 원형 궤적 회전 시간
        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            initialPosition = _rectTransform.anchoredPosition;
        }
        public void StartRuneStoneEffect()
        {
            //1. sprite 변경
            GetComponent<Image>().sprite = runeStoneSprite;
            GetComponent<Image>().raycastTarget = false;
            _rectTween = DOTween.To(() => 0f, angle =>
                {
                    float x = Mathf.Cos(angle) * circleRadius;
                    float y = Mathf.Sin(angle) * circleRadius;
                    _rectTransform.anchoredPosition = initialPosition + new Vector2(x, y);
                }, Mathf.PI * 2, circleDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            _rectTransform.DOScale(Vector3.one * 1.05f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {

                // 2. 원형 궤적 애니메이션


                _rectTransform.DOScale(Vector3.one * (1 + scaleAmount), scaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            });
        }
        public void EndRuneStoneEffect()
        {
            if (_rectTransform.anchoredPosition == null)
            {
                return;
            }
            GetComponent<Image>().raycastTarget = true;
            _rectTween.Kill();
            _rectTransform.DOKill();
            _rectTransform.DOAnchorPos(initialPosition, 0.3f).SetEase(Ease.InQuad);
            _rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InQuad);
            GetComponent<Image>().sprite = originalSprite;
        }
    }
}