using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Tutorial;

namespace Game.Request
{
    public class newArrivalAlarm : MonoBehaviour
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

        [Header("간접 피드백")]
        [SerializeField] GameObject emojiBubble;
        [SerializeField] Image emoji;
        [SerializeField] List<Sprite> emojies = new List<Sprite>();
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

            _envelopeImage.color = new Color(1f, 1f, 1f, 1f);
            _owlMouse.color = new Color(1f, 1f, 1f, 1f);
            _owlImage.color = new Color(1f, 1f, 1f, 1f);

            _initialPosition = envelope.transform.localPosition;
        }

        void Start()
        {
            //Reset();
        }

        void OnEnable()
        {
            emojiBubble.gameObject.SetActive(false);
            emojiBubble.GetComponent<Image>().color = new Color(0, 0, 0, 1);
            emoji.color = new Color(0, 0, 0, 1);
            owlInRoom.SetActive(true);

            _owlImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
            _owlMouse.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
            _owlEye1.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
            _owlEye2.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine);
            _envelopeImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.7f).SetEase(Ease.InSine).onComplete += () =>
            {
                GetComponent<Button>().interactable = true;
                //GetComponent<Animator>().speed = 1;
            };
        }
        void OnDisable()
        {
            owlInRoom.SetActive(false);
        }
        public void Reset()
        {
            emojiBubble.gameObject.SetActive(false);
            GetComponent<Button>().interactable = false;
            _envelopeImage.color = new Color(1, 1, 1, 0);
            _owlImage.color = new Color(1, 1, 1, 0);
            _owlMouse.color = new Color(1, 1, 1, 0);
            _owlEye1.color = new Color(1, 1, 1, 0);
            _owlEye2.color = new Color(1, 1, 1, 0);
            envelope.transform.localPosition = _initialPosition;
            envelope.transform.rotation = Quaternion.Euler(0, 0, 0);
            gameObject.SetActive(false);
            owlInRoom.SetActive(false);
        }
        public void OnButtonClick()
        {
            GetComponent<Button>().interactable = false;
            if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 13)
            {   // 올빼미 처음으로 누르고 했을 때, 올빼미 누르면 다음 튜토리얼 스텝
                TutorialManager.Instance.NextStep();
            }
            envelope.transform.DOLocalMoveY(-300, 0.5f).SetEase(Ease.InSine);
            envelope.transform.DORotate(new Vector3(0, 0, 60), 0.5f).SetEase(Ease.InSine).onComplete += () =>
            {
                RequestManager.Instance.ShowNextRequests();
            };
        }
        public void Send()
        {
            SoundManager.Instance.PlaySFX("BirdGone");
            //SoundManager.Instance.PlayRandomSFX("Bird");
            envelope.transform.localPosition = new Vector3(-15, -3, 0);
            envelope.transform.rotation = Quaternion.Euler(0, 0, 0);

            //GetComponent<Animator>().speed = 0;

            _owlMouse.DOColor(new Color(1, 1, 1, 0), 1f);
            _envelopeImage.DOColor(new Color(1, 1, 1, 0), 1f);
            _owlEye1.DOColor(new Color(1, 1, 1, 0), 1f);
            _owlEye2.DOColor(new Color(1, 1, 1, 0), 1f);
            _owlImage.DOColor(new Color(1, 1, 1, 0), 1f).onComplete += () =>
            {
                emoji.DOColor(new Color(1, 1, 1, 0), 1f);
                emojiBubble.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 1f);
                _owlImage.DOColor(new Color(1, 1, 1, 0), 1f).onComplete += () =>
                {
                    owlInRoom.SetActive(false);
                    gameObject.SetActive(false);
                    RequestManager.Instance.SendNewArrivalAlarm();
                };
            };

            if (TutorialManager.Instance != null)
            {
                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 21)
                {   // 교수님의 첫 번째 편지에 대한 답장을 보낼 때 다음 스텝으로 넘어감
                    TutorialManager.Instance.NextStep();
                }

                if (TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 34)
                {   // 교수님의 두 번째 편지에 대한 답장을 보낼 때 다음 스텝으로 넘어감
                    TutorialManager.Instance.NextStep();
                }
            }
        }

        public void SetFeedBack(bool success)
        {
            if (success)
                emoji.sprite = emojies[0];
            else
                emoji.sprite = emojies[1];

            emojiBubble.SetActive(true);
            emojiBubble.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            var rt = emojiBubble.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;
            rt.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }
}