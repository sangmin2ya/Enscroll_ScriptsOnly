using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Desk
{
    /// <summary>
    /// 클릭하면 생성될 편지 오브젝트들
    /// </summary>
    [System.Serializable]
    public class EndingEnvelope : MonoBehaviour
    {
        [Header("생성될 편지 오브젝트")]
        [SerializeField] private GameObject[] successLetterSprites = new GameObject[2];
        [SerializeField] private GameObject[] failLetterSprites = new GameObject[2];
        void Start()
        {
            //생성될때 테이블 위로 올라오게
            GetComponent<RectTransform>().DOLocalMove(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutQuad);
        }
        public void OpenEnvelope()
        {
            GetComponent<Animator>().SetTrigger("Open");
        }
        public void SpawnLetters()
        {
            Vector3 letterPos = GetComponent<RectTransform>().position;
            if (GameData.Instance.FinalResultCode == 0)
            {
                var letter1 = Instantiate(successLetterSprites[0], CanvasManager.Instance.MainDeskUI);
                var letter2 = Instantiate(successLetterSprites[1], CanvasManager.Instance.MainDeskUI);

                letter1.GetComponent<RectTransform>().position = letterPos;
                letter2.GetComponent<RectTransform>().position = letterPos;
            }
            else
            {
                var letter1 = Instantiate(failLetterSprites[0], CanvasManager.Instance.MainDeskUI);
                var letter2 = Instantiate(failLetterSprites[1], CanvasManager.Instance.MainDeskUI);

                letter1.GetComponent<RectTransform>().position = letterPos;
                letter2.GetComponent<RectTransform>().position = letterPos;
            }
            Destroy(gameObject);
        }
    }
}