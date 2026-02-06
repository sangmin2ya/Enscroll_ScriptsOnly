using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.Desk
{
    public class Coin : MonoBehaviour
    {
        [Header("코인 가치")]
        private int coinValue;
        public int CoinValue => coinValue;
        [Header("코인 이미지")]
        [SerializeField]
        private Sprite goldCoin;
        [SerializeField]
        private Sprite silverCoin;
        [SerializeField]
        private Sprite bronzeCoin;
        private Image _coinImage;
        [Header("저장 장소")]
        private RectTransform storage;
        private RectTransform _transform;
        void Start()
        {
            _coinImage = GetComponent<Image>();
            _transform = GetComponent<RectTransform>();
        }
        public void SetCoinValue(int value)
        {
            coinValue = value;
            SetCoinImage();
        }
        public void SetCoinImage()
        {
            if (coinValue >= 25)
            {
                _coinImage.sprite = goldCoin;
            }
            else if (coinValue >= 5)
            {
                _coinImage.sprite = silverCoin;
            }
            else
            {
                _coinImage.sprite = bronzeCoin;
            }
        }
        public void PutCoin()
        {
            _transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InQuart);
            _transform.DOMove(storage.position, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                MoneyManager.Instance.AddMoney(coinValue);
                Destroy(gameObject);
            });
        }
    }
}