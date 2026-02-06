using System.Collections;
using DG.Tweening;
using Game.Tutorial;
using UnityEngine;
using UnityEngine.UI;
public class ScreenManager : MonoBehaviour
{
    [SerializeField] private RectTransform[] screens; // 각 화면 패널 배열
    private int currentScreenIndex = 0; // 현재 활성화된 화면 배열 번호
    private Vector2 offScreenPos = new Vector2(2000f, 0); // 화면 밖 위치

    [Header("룸-데스크 전환 용 패널")]
    [SerializeField] RectTransform transitionPanel;
    [SerializeField] Image transitionImage;
    [Header("퀘스트 보드")]
    [SerializeField] private RectTransform questBoard;



    private void Start()
    {
        StartScreen();
    }

    /// <summary>
    /// 메서드 실행 시 초기화면(침대씬)으로 초기화하는 메서드
    /// </summary>
    public void StartScreen()
    {
        // 모든 화면 시작 시 화면 밖으로 설정
        foreach (RectTransform screen in screens)
        {
            screen.anchoredPosition = offScreenPos; // 배열 안에 모든 스크린 위치 고정
        }

        screens[currentScreenIndex].anchoredPosition = Vector2.zero; // 첫번째 화면[0]은 화면 중앙에 표시

        transitionImage.DOFade(0f, 1f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            transitionImage.gameObject.SetActive(false);
            //TutorialManager.Instance.StartTutorial();
        });
    }

    /// <summary>
    /// 장사 화면으로 이동하는 메서드(왼쪽에서 오른쪽으로)
    /// </summary>
    public void GoToNextScreen()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false && TutorialManager.Instance.CurrentStepIndex == 8)
        {
            TutorialManager.Instance.NextStep();
        }

        if (currentScreenIndex < screens.Length - 1) // 마지막 화면이 아닌 경우
        {
            SoundManager.Instance.PlaySFX("ChairSliding");

            transitionImage.gameObject.SetActive(true);
            Color originColor = transitionImage.color;
            Color targetColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 1f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionImage.DOColor(targetColor, 0.5f)).SetEase(Ease.InQuad);
            sequence.AppendCallback(() =>
            {
                MoveScreenToLeft();
            });
            sequence.AppendInterval(0.2f);
            sequence.Append(transitionImage.DOColor(originColor, 0.5f)).SetEase(Ease.InQuad);
            sequence.OnComplete(() =>
            {
                transitionImage.color = originColor;
                transitionImage.gameObject.SetActive(false);
            });
            sequence.Play();
        }
    }

    private void MoveScreenToLeft()
    {
        // 오른쪽에서 왼쪽으로 이동하는 방향
        // StartCoroutine(SlideTransition(1));
        Vector2 startPos = screens[currentScreenIndex].anchoredPosition; // 현재 화면 시작 위치
        Vector2 targetPos = -offScreenPos;

        screens[currentScreenIndex].anchoredPosition = targetPos;
        screens[currentScreenIndex + 1].anchoredPosition = Vector2.zero;
        screens[currentScreenIndex].DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
        // screens[currentScreenIndex + 1]
        //     .DOAnchorPos(Vector2.zero, 0.2f)
        //     .SetEase(Ease.OutQuad)
        //     .OnStart(() =>
        //     {
        //         // 애니메이션 시작과 동시에 효과음 재생
        //         SoundManager.Instance.PlaySFX("ChairSliding");
        //     });
        currentScreenIndex++;

        SoundManager.Instance.CrossfadeMusic("Table", 1.0f);
    }

    /// <summary>
    /// 침대 화면으로 이동하는 메서드(오른쪽에서 왼쪽으로)
    /// </summary>
    public void GoToPerviousScreen()
    {
        if (currentScreenIndex > 0) // 첫 화면이 아닌 경우
        {
            SoundManager.Instance.PlaySFX("ChairSliding");

            transitionImage.gameObject.SetActive(true);
            Color originColor = transitionImage.color;
            Color targetColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 1f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transitionImage.DOColor(targetColor, 0.5f)).SetEase(Ease.InQuad);
            sequence.AppendCallback(() =>
            {
                MoveScreenToRight();
            });
            sequence.AppendInterval(0.2f);
            sequence.Append(transitionImage.DOColor(originColor, 0.5f)).SetEase(Ease.InQuad);
            sequence.OnComplete(() =>
            {
                transitionImage.color = originColor;
                transitionImage.gameObject.SetActive(false);
            });
            sequence.Play();
        }

        // transitionPanel.gameObject.SetActive(true);
        // Color originColor = transitionPanel.color;
        // Color targetColor = new Color(transitionPanel.color.r, transitionPanel.color.g, transitionPanel.color.b, 1f);
        // Sequence sequence = DOTween.Sequence();
        // sequence.Append(transitionPanel.DOColor(targetColor, 0.2f)).SetEase(Ease.OutQuad);
        // sequence.AppendInterval(0.5f);
        // sequence.Append(transitionPanel.DOColor(originColor, 0.2f)).SetEase(Ease.OutQuad);
        // sequence.OnComplete(() =>
        // {
        //     transitionPanel.gameObject.SetActive(false);
        // });
        // sequence.Play();

    }

    private void MoveScreenToRight()
    {
        // // 왼쪽에서 오른쪽으로 이동하는 방향
        // StartCoroutine(SlideTransition(-1));
        Vector2 startPos = screens[currentScreenIndex].anchoredPosition; // 현재 화면 시작 위치
        Vector2 targetPos = offScreenPos;

        screens[currentScreenIndex].anchoredPosition = targetPos;
        screens[currentScreenIndex - 1].anchoredPosition = Vector2.zero;

        // screens[currentScreenIndex].DOAnchorPos(targetPos, 0.7f).SetEase(Ease.OutQuad);
        // screens[currentScreenIndex - 1]
        //     .DOAnchorPos(Vector2.zero, 0.7f)
        //     .SetEase(Ease.OutQuad)
        //     .OnStart(() =>
        //     {
        //         // 애니메이션 시작과 동시에 효과음 재생
        //     });
        currentScreenIndex--;

        SoundManager.Instance.CrossfadeMusic("Bed", 1.0f);
    }

    public void ToggleScreen()
    {
        if (currentScreenIndex == 0)
        {
            GoToNextScreen();
        }
        else
        {
            GoToPerviousScreen();
        }
    }
    /// <summary>
    /// 캔버스 줌인하는 메서드
    /// </summary>
    public void ZoomInBoard()
    {
        transitionImage.gameObject.SetActive(true);
        Color originColor = transitionImage.color;
        Color targetColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 1f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transitionImage.DOColor(targetColor, 0.5f)).SetEase(Ease.InQuad);
        sequence.AppendCallback(() =>
        {
            questBoard.gameObject.SetActive(true);
        });
        sequence.AppendInterval(0.2f);
        sequence.Append(transitionImage.DOColor(originColor, 0.5f)).SetEase(Ease.InQuad);
        sequence.OnComplete(() =>
        {
            transitionImage.color = originColor;
            transitionImage.gameObject.SetActive(false);
        });
        sequence.Play();
    }

    public void ZoomOutBoard()
    {
        transitionImage.gameObject.SetActive(true);
        Color originColor = transitionImage.color;
        Color targetColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 1f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transitionImage.DOColor(targetColor, 0.5f)).SetEase(Ease.InQuad);
        sequence.AppendCallback(() =>
        {
            questBoard.gameObject.SetActive(false);
        });
        sequence.AppendInterval(0.2f);
        sequence.Append(transitionImage.DOColor(originColor, 0.5f)).SetEase(Ease.InQuad);
        sequence.OnComplete(() =>
        {
            transitionImage.color = originColor;
            transitionImage.gameObject.SetActive(false);
        });
        sequence.Play();
    }

}
