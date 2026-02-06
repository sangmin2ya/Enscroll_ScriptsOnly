using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Desk
{
    public class FileController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Button fileCloseButton;
        [SerializeField]
        private GameObject pageButtons;
        private Animator _fileAnimator;
        //서랍안에 넣어둘거면 false로
        private bool _isPlacedOnDesk = false;
        private bool _isOpen = false;
        public bool IsOpen => _isOpen;
        public bool IsPlacedOnDesk
        {
            get { return _isPlacedOnDesk; }
            set { _isPlacedOnDesk = value; }
        }
        void Start()
        {
            _fileAnimator = GetComponent<Animator>();
            fileCloseButton.GetComponent<Button>().onClick.AddListener(CloseFile);
            fileCloseButton.gameObject.SetActive(false);
        }
        public void NextPage()
        {
            _fileAnimator.SetTrigger("Next");
        }
        public void OpenFile()
        {
            if (_isOpen)
            {
                return;
            }
            _fileAnimator.SetTrigger("Open");
            _isOpen = true;
            fileCloseButton.gameObject.SetActive(true);
            pageButtons.SetActive(true);

        }
        public void CloseFile()
        {
            if (!_isOpen)
            {
                return;
            }
            _isOpen = false;
            _fileAnimator.SetTrigger("Close");
            fileCloseButton.gameObject.SetActive(false);
            pageButtons.SetActive(false);
        }

        public void ChangeCloseButtonForNormal()
        {
            fileCloseButton.onClick.RemoveAllListeners();
            fileCloseButton.onClick.AddListener(() => { CloseFile(); });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OpenFile();

        }
    }
}