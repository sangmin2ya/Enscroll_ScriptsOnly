using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Game.Board
{
    public class ShowWay : MonoBehaviour
    {
        [SerializeField] private Image roadWay;
        [SerializeField] private ShowWay prevWay;
        [SerializeField] private ShowWay nextWay;
        private void Start()
        {
            if (roadWay != null)
            {
                roadWay.fillAmount = 0;
            }
            gameObject.SetActive(false);
        }
        private void ShowTheWay()
        {
            roadWay.DOFillAmount(1, 1).SetEase(Ease.Linear);
        }
        void OnEnable()
        {

        }
        public void OnEnableEvent()
        {
            if (prevWay != null)
            {
                prevWay.ShowAllWay();
            }
            if (nextWay != null)
            {
                nextWay.HideAllWay();
            }
            if (roadWay == null)
            {
                return;
            }
            roadWay.gameObject.SetActive(true);
            ShowTheWay();
        }
        void OnDisable()
        {

        }
        public void ShowAllWay()
        {
            if (roadWay == null)
            {
                return;
            }
            roadWay.DOKill();
            roadWay.fillAmount = 1;
        }
        public void HideAllWay()
        {
            if (roadWay != null)
            {
                roadWay.DOKill();
                roadWay.fillAmount = 0;
                roadWay.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        }
    }
}