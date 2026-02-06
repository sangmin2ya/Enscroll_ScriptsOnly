using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Game.Desk;

namespace Game.Request
{
    /// <summary>
    /// 마지막 엔딩 메시지 가져다 주는 부엉이
    /// </summary>
    public class NewEndingAlarm : MonoBehaviour
    {
        [Header("부엉이 이미지")]
        [SerializeField]
        private Image _owlImage;
        [SerializeField]
        private Image _owlMouse;
        [SerializeField]
        private Image _envelopeImage;
        [SerializeField]
        private Image _owlEye1;
        [SerializeField]
        private Image _owlEye2;

        [Header("답장 오브젝트 - 편지 올릴시 자동으로 할당")]
        [SerializeField]
        private GameObject envelope;
        public GameObject Envelope { get => envelope; set => envelope = value; }
        private Vector3 _initialPosition;
        [Header("방 부엉이 이미지")]
        [SerializeField]
        private GameObject owlInRoom;

        void OnEnable()
        {
            _owlImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
            _envelopeImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
            _owlEye1.DOFade(1, 0.7f).SetEase(Ease.InSine);
            _owlEye2.DOFade(1, 0.7f).SetEase(Ease.InSine);
            owlInRoom.SetActive(true);
            _owlMouse.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine).onComplete += () =>
            {
                GetComponent<Button>().interactable = true;
                //GetComponent<Animator>().speed = 1;
            };

        }
        void OnDisable()
        {
            owlInRoom.SetActive(false);
        }
        /// <summary>
        /// 부엉이 값 설정 엔딩 타입
        /// </summary>
        public void Reset()
        {
            GetComponent<Button>().interactable = false;
            _owlImage.color = new Color(1, 1, 1, 0);
            _owlMouse.color = new Color(1, 1, 1, 0);
            _owlEye1.color = new Color(1, 1, 1, 0);
            _owlEye2.color = new Color(1, 1, 1, 0);
            _envelopeImage.color = new Color(1, 1, 1, 0);
            owlInRoom.SetActive(false);
            gameObject.SetActive(false);
        }
        /// <summary>
        /// 부엉이 클릭시 엔딩 편지 가져다 주는 메소드
        /// </summary>
        public void GiveEndingEnvelope()
        {
            GetComponent<Button>().interactable = false;
            _envelopeImage.transform.DOLocalMoveY(-300, 0.5f).SetEase(Ease.InSine);
            _envelopeImage.transform.DORotate(new Vector3(0, 0, 60), 0.5f).SetEase(Ease.InSine).onComplete += () =>
            {
                RequestManager.Instance.ShowEndingRequest();
            };
            //봉투 떨어뜨리는 효과
        }
        public void Send()
        {
            SoundManager.Instance.PlaySFX("BirdGone");
            //SoundManager.Instance.PlayRandomSFX("Bird");
            envelope.transform.localPosition = _initialPosition;
            //GetComponent<Animator>().speed = 0;
            envelope.transform.rotation = Quaternion.Euler(0, 0, 0);
            _owlMouse.DOColor(new Color(1, 1, 1, 0), 1f);
            envelope.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 1f);
            _owlEye1.DOFade(0, 1f);
            _owlEye2.DOFade(0, 1f);
            _owlImage.DOColor(new Color(1, 1, 1, 0), 1f).onComplete += () =>
            {
                _owlImage.DOColor(new Color(1, 1, 1, 0), 1f).onComplete += () =>
                {
                    owlInRoom.SetActive(false);
                    gameObject.SetActive(false);
                };
            };

        }
    }
}