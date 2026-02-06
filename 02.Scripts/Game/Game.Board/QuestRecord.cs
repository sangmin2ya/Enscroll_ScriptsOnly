using UnityEngine;
using TMPro;

namespace Game.Board
{
    public class QuestRecord : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI requestText;
        [SerializeField] private TextMeshProUGUI replyText;
        [SerializeField] private ShowWay showWay;
        void OnEnable()
        {
            showWay.gameObject.SetActive(true);
            showWay.OnEnableEvent();
        }
        void OnDisable()
        {
            //showWay.gameObject.SetActive(false);
        }
        public void HideWay()
        {
            showWay.HideAllWay();
        }
        //데이터 불러와서 성공 실패 텍스트 변경
        public void ShowReply(string summary)
        {
            replyText.text = summary;
        }
        public void ShowRequest(string summary)
        {
            requestText.text = summary;
        }
    }
}