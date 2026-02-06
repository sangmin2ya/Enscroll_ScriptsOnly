using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Desk
{
    public enum InteractablePositionType
    {
        의뢰,
        스크랩북,
        인장,
        영수증,
        물약,
        답장,
        납부서,
        엔딩,
        기타
    }
    public struct PositionDictionary
    {
        public string MainDeskUI;
        public List<RectTransform> RightUIPositions;
    }
    public class CanvasManager : Singleton<CanvasManager>
    {
        [Header("페이드 아웃용 패널")]
        [SerializeField] private GameObject fadeOutPanel; // 페이드 아웃용 패널
        [Header("UI")]
        [SerializeField] private RectTransform mainDeskUI; // 메인 책상 UI
        public RectTransform MainDeskUI => mainDeskUI;
        [SerializeField] private RectTransform rightUI;    // 우측 UI
        public RectTransform RightUI => rightUI;
        [SerializeField] private RectTransform submitUI;    // 우측 UI
        public RectTransform SubmitUI => submitUI;
        [SerializeField] private List<RectTransform> rightUIPositions;  // 우측 UI 초기 위치
        [SerializeField] private List<RectTransform> rightPotionUIPositions;  // 우측 UI 초기 위치
        private DragableObject[] _potions = new DragableObject[3];

        public RectTransform GetUIPositionOfObject(InteractablePositionType type, DragableObject dragableObject)
        {
            switch (type)
            {
                case InteractablePositionType.의뢰:
                    return rightUIPositions[0];
                case InteractablePositionType.엔딩:
                    return rightUIPositions[0];
                case InteractablePositionType.스크랩북:
                    return rightUIPositions[1];
                case InteractablePositionType.인장:
                    return rightUIPositions[2];
                case InteractablePositionType.영수증:
                    return rightUIPositions[3];
                case InteractablePositionType.물약:
                    return rightPotionUIPositions[GetEmptyPotionIndex(dragableObject)];
                case InteractablePositionType.납부서:
                    return rightUIPositions[4];
                default:
                    return null;
            }
        }
        public void RemovePotionFromDrawer(DragableObject dragableObject)
        {
            for (int i = 0; i < _potions.Length; i++)
            {
                if (_potions[i] == dragableObject)
                {
                    _potions[i] = null;
                    return;
                }
            }
        }

        private int GetEmptyPotionIndex(DragableObject dragableObject)
        {
            for (int i = 0; i < _potions.Length; i++)
            {
                if (_potions[i] == null)
                {
                    _potions[i] = dragableObject;
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 전체 암전
        /// </summary>
        public void FadeOut()
        {
            fadeOutPanel.SetActive(true);
            fadeOutPanel.GetComponent<Image>().DOFade(1, 2f).onComplete += () =>
            {
                SceneManager.LoadScene("Ending");
            };
        }
    }
}