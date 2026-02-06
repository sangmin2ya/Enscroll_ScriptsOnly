using System.Collections;
using UnityEngine;
using Game.Request;
using Game.Desk;
using Game.Scroll;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Tutorial;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic; //추가했음!

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject fadePanel; // fadeInOut 패널
    private CanvasRenderer panelRenderer;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private RequestManager requestManager;
    [SerializeField] private Button calanderCloseButton;

    [SerializeField] TextMeshProUGUI saveText;
    //[SerializeField] private TextMeshProUGUI sleepMessageText; // "클릭하면 다음 날짜로 넘어갑니다" 텍스트 UI

    private enum GameState // 게임 상태들의 종류
    {
        Morning
    }

    private GameState currentState; // 게임의 현재 상태

    void Start()
    {
        panelRenderer = fadePanel.GetComponent<CanvasRenderer>(); // 패널 투명도 값 쓸 수 있게 컴포넌트 가져오기
        panelRenderer.SetAlpha(0f); // 패널 투명도 0
        StartNewDay();
        //spellManager.LoadSpell();
    }

    /// <summary>
    /// 새로운 날 시작하는 메서드. 카메라를 침실로 이동, 날짜 증가, 게임 상태 초기화(순서 초기화)
    /// </summary>
    private void StartNewDay() // 새로운 날을 시작하는 메서드
    {
        currentState = GameState.Morning; // 하루 시작 시 게임 상태 초기화
        // 카메라를 침실로 이동시켜야함
        // dayManager.AdvanceDay();
    }

    /*
    public void GoToSleep()
    {
        if (RequestManager.Instance.todayRequests.Count == 0 && dayManager.day < 13)
        {
            StartCoroutine(DayAfterFadeOut());
        }
        else
        {
            PopupManager.Instance.OpenBedAlertPopup();
        }
    }
    */

    public void GoToSleep()
    {
        if (RequestManager.Instance.todayRequests.Count == 0 && dayManager.day < 13)
        {
            StartCoroutine(SleepSequence());
        }
        else
        {
            PopupManager.Instance.OpenBedAlertPopup();
        }
    }

    /*
    private IEnumerator SleepSequence()
    {
       bool sequenceSkipped = false;
       float sleepDuration = 12.0f; // 대기 시간

       fadePanel.SetActive(true);
       panelRenderer.SetAlpha(0f);
       //어두워지는 효과 넣기 ㅇㅇ
       yield return StartCoroutine(Fade(1.0f));
        //낭만있는 음악 틀어버리기
       List<string> sleepSounds = new List<string> { "Sleep", "SweetDreams", "NightAmbient" };
       SoundManager.Instance.CrossfadeMusicForSleep(sleepSounds, "Bed", sleepDuration);

        // "클릭하면 다음 날짜로 넘어갑니다" 텍스트 표시
        //sleepMessageText.gameObject.SetActive(true);

       float elapsedTime = 0f;
       while (elapsedTime < sleepDuration && !sequenceSkipped)
       {
           if (Input.GetMouseButtonDown(0)) 
           {
               sequenceSkipped = true;
               SoundManager.Instance.CleanupSleepMusic(0.5f);
               SoundManager.Instance.CrossfadeMusic("Bed", 0.5f);
               yield return new WaitForSeconds(0.5f);
               break;
           }
           elapsedTime += Time.deltaTime;
           yield return null;
       }
       //sleepMessageText.gameObject.SetActive(false);
           // 바로 DayAfterFadeOut으로 넘어감 (페이드 아웃 생략)
        StartCoroutine(DayAfterFadeOut(skipFadeOut: true));
    }
    */
    private IEnumerator SleepSequence()
    {
        bool sequenceSkipped = false;
        float sleepDuration = 200.0f; // 음악 재생 시간 (200초)

        // 화면을 검게 만들기 위해 페이드 패널 활성화
        fadePanel.SetActive(true);
        //panelRenderer.SetAlpha(1f); // 처음부터 화면이 검게 보이도록 설정함.
        panelRenderer.SetAlpha(0f);
        yield return StartCoroutine(Fade(1f)); // 화면 검게 보이게 하는 Fade 효과 || 1f = 완전 검정 낮을수록 불투명

        // 먼저 수면 음악들 재생 시작
        List<string> sleepSounds = new List<string> { "Sleep", "SweetDreams", "NightAmbient" };
        SoundManager.Instance.CrossfadeMusicForSleep(sleepSounds, "Bed", sleepDuration);

        // 달력을 표시하기 전에 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 달력을 표시
        dayManager.ToggleCalendar(true);
        SoundManager.Instance.PlaySFX("MagazineAndNewpaper");
        StartCoroutine(DayAfterFadeOut(skipFadeOut: true)); // 날짜 넘어가는 효과만 실행 || 이거 False 하면 하얘지는 버그

        // 사용자가 클릭하면 음악을 건너뛰고, 바로 날짜가 넘어감
        float elapsedTime = 0f;
        while (elapsedTime < sleepDuration && !sequenceSkipped)
        {
            /*
            if (Input.GetMouseButtonDown(0))
            {
            */
            if (!dayManager.IsCalendarActive())
            {
                sequenceSkipped = true;
                SoundManager.Instance.CleanupSleepMusic(0.5f); // 음악 정리
                SoundManager.Instance.CrossfadeMusic("Bed", 0.5f); // 새로운 음악 시작
                yield return new WaitForSeconds(0.5f); // 잠깐 대기
                break; // 클릭 시 바로 넘어감
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 매일매일 초기화되는 것들 초기화 시키는 메서드 
    /// </summary>
    public void ResetDaily()
    {
        // 하루 넘어가면서 해야되는 것들
        dayManager.AdvanceDay();

        // 레벨
        dayManager.SetPlayerLevelForToday();

        // 힌트 초기화
        MagicController.Instance.ResetHint();

        // 전날 남아있는 스크롤 지우기
        if (DeskManager.Instance.CurrentScroll != null)
        {
            ScrollCreationManager.Instance.ResetScroll();
            // 전날 남아있는 EnchantPanel 지우기
            MagicController.Instance.ResetElementSpell();
        }
        // 전날 남아있는 ordersheet 지우기
        DeskManager.Instance.EraseOrderSheet();

        // 책상 정리
        DeskManager.Instance.OrganizeDesk();

        if (dayManager.day == 13)
        {
            // 도전 과제
            if (GameData.Instance.isAllPass)
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_5");
            if (GameData.Instance.isNoHint)
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_13");
        }

        // 저장 정보 관련
        SaveManager.Save();

        DeskManager.Instance.LoadPrevResult();


        if (DayManager.Instance.day != 13)
        {
            DeskManager.Instance.CreateNewScroll();
            requestManager.AssignRequestForToday();
            UpgradeScroll();
        }
        else
        {
            requestManager.ResetOwlFor13thDay();
        }

        if (DayManager.Instance.day == 1 && TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == false)
        {
            TutorialManager.Instance.DayOneTutoReset();
        }
    }

    IEnumerator SaveBuffering()
    {
        saveText.gameObject.SetActive(true);
        var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Localizing", "저장중");
        if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
        {
            saveText.SetText(stringOperation.Result);
        }

        yield return new WaitForSeconds(1f);

        var stringOperation2 = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Localizing", "저장 완료");
        if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
        {
            saveText.SetText(stringOperation2.Result);
        }

        yield return new WaitForSeconds(0.5f);

    }

    /// <summary>
    /// 바로 날짜 시작해도 되도록 ScrollCreation에서 호출할 것
    /// </summary>
    public void UpgradeScroll()
    {
        if (DayManager.Instance.day == 1)
        {
            if (TutorialManager.Instance == null || (TutorialManager.Instance != null && TutorialManager.Instance.TutorialComplete == true))
                ScrollCreationManager.Instance.UpgradeScroll(2);
        }
        else if (DayManager.Instance.day == 4)
        {
            ScrollCreationManager.Instance.UpgradeScroll(3);
        }
        else if (DayManager.Instance.day == 7)
        {
            ScrollCreationManager.Instance.UpgradeScroll(4);
        }
    }

    /// <summary>
    /// 저장 데이터 로드할 때
    /// </summary>
    public void ReloadDay()
    {
        GameData.Instance.LoadSaveInfo(dayManager.day);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    /// <summary>
    /// 자면 화면 검게 바뀌고 업데이트 하고 다시 밝아짐
    /// </summary>
    /// <returns></returns>

    /*
    public IEnumerator DayAfterFadeOut(bool skipFadeOut = false)
    {
        calanderCloseButton.gameObject.SetActive(false);
        // Debug.Log("In Fade");
        //페이드 아웃
    if (!skipFadeOut) 
    {
        fadePanel.SetActive(true);
        panelRenderer.SetAlpha(0f);
        yield return StartCoroutine(Fade(1f)); // 화면 검게 시작
    }

        dayManager.ToggleCalendar(true);
        yield return new WaitForSeconds(1.5f); // 대기

        ResetDaily(); // 하루 넘어갈 때 처리하는 놈들

        saveText.gameObject.SetActive(true);
        var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Localizing", "저장중");
        if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
        {
            saveText.SetText(stringOperation.Result);
        }

        yield return new WaitForSeconds(0.5f);

        var stringOperation2 = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Localizing", "저장 완료");
        if (stringOperation2.IsDone && stringOperation2.Status == AsyncOperationStatus.Succeeded)
        {
            saveText.SetText(stringOperation2.Result);
        }

        yield return new WaitForSeconds(0.5f);

        calanderCloseButton.gameObject.SetActive(true);
        saveText.gameObject.SetActive(false);

        // 달력이 닫힐 때까지 대기
        yield return new WaitUntil(() => !dayManager.IsCalendarActive());

        // //페이드 인
        // yield return StartCoroutine(Fade(0f));
        fadePanel.SetActive(false);
    }
    */
    public IEnumerator DayAfterFadeOut(bool skipFadeOut = false)
    {
        calanderCloseButton.gameObject.SetActive(false);

        // 페이드 아웃을 건너뛰어야 하는 경우
        if (!skipFadeOut)
        {
            fadePanel.SetActive(true);
            panelRenderer.SetAlpha(0f);
            yield return StartCoroutine(Fade(1f)); // 화면 검게 시작
        }

        // 다음 날로 넘어가는 작업 처리
        dayManager.ToggleCalendar(true);
        yield return new WaitForSeconds(2.0f); // 대기

        ResetDaily(); // 하루 넘어갈 때 처리하는 놈들

        saveText.gameObject.SetActive(true);
        var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Localizing", "저장중");
        if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
        {
            saveText.SetText(stringOperation.Result);
        }

        yield return new WaitForSeconds(0.8f);

        var stringOperation2 = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Localizing", "저장 완료");
        if (stringOperation2.IsDone && stringOperation2.Status == AsyncOperationStatus.Succeeded)
        {
            saveText.SetText(stringOperation2.Result);
        }

        yield return new WaitForSeconds(0.8f);

        calanderCloseButton.gameObject.SetActive(true);
        saveText.gameObject.SetActive(false);

        // 달력이 닫힐 때까지 대기
        yield return new WaitUntil(() => !dayManager.IsCalendarActive());

        // 페이드 인 효과 없이 바로 넘어감
        fadePanel.SetActive(false);
    }

    /// <summary>
    /// 페이드 효과 적용 코루틴
    /// </summary>
    /// <param name="targetAlpha">목표 알파값 0 투명 / 1 기본</param>
    /// <returns></returns>
    private IEnumerator Fade(float targetAlpha)
    {
        float duration = 1f; // 페이드 시간
        float startAlpha = panelRenderer.GetAlpha(); // 현재 알파값 가져오기
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            panelRenderer.SetAlpha(newAlpha);
            yield return null;
        }

        panelRenderer.SetAlpha(targetAlpha);
    }

    private void TransitionToNextState(GameState nextState) // GameManager에서 게임 상태를 다음 상태로 만들어주는 메서드
    {
        currentState = nextState;
    }
}