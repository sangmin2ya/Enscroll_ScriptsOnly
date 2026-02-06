using System;
using System.Collections;
using DG.Tweening;
using Game.Scroll;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : Singleton<TitleManager>
{
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject mainCanvas; // 기존 Canvas

    [SerializeField] private ChooseDataUI chooseDataUI;
    public ChooseDataUI ChooseDataUI => chooseDataUI;
    [SerializeField] private GameObject exitAlert;
    [SerializeField] private Image fadeInImage;

    [SerializeField] int nowLanguage;
    [SerializeField] int leastLoadTime = 3;

    // [Header("데이터 선택 UI")]
    // [SerializeField] Game

    void Awake()
    {
        // nowLanguage = (int)GameData.Instance.langauage;
        // LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[nowLanguage];
        fadeInImage.DOFade(0, 1.5f).SetEase(Ease.InQuart).OnComplete(() => fadeInImage.gameObject.SetActive(false));
    }

    public void OpenChooseDateUI()
    {
        chooseDataUI.gameObject.SetActive(true);
    }

    public void OpenExitAlert()
    {
        exitAlert.SetActive(true);
    }

    public void CloseExitAlert()
    {
        exitAlert.SetActive(false);
    }


    public void StartGame()
    {
        StartCoroutine(LoadSceneWithLoadingScreen());
    }

    private IEnumerator LoadSceneWithLoadingScreen()
    {
        // 기존 Canvas 비활성화
        // mainCanvas.SetActive(false);

        // 로딩창 활성화
        loadingCanvas.SetActive(true);

        // 씬 비동기 로드 시작 
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        SoundManager.Instance.CrossfadeMusic("Bed");


        // 씬이 로드되더라도 바로 넘어가지 않도록 로딩 중단
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;
        //const float minimumLoadTime = 1f; // 최소 대기 시간 설정 (1초)


        // 씬 로딩 완료될 때까지 대기
        while (!operation.isDone)
        {
            // 로딩 진행률 (0 ~ 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // 1초 대기 시간 누적
            elapsedTime += Time.deltaTime;

            // 로딩이 완료되고 최소 대기 시간이 지난 경우 씬 전환
            if (operation.progress >= 0.9f && elapsedTime >= leastLoadTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 로딩 완료 후 로딩창 비활성화 (필요하면 유지 가능)
        loadingCanvas.GetComponent<Loading>().Disable();
    }


    public void ChangeLanaguage()
    {
        if (nowLanguage == 0)
            nowLanguage = 1;
        else nowLanguage = 0;
        // LocalizationSettings.InitializationOperation;
        // Setting the locale using the singleton instance
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[nowLanguage];
        GameData.Instance.langauage = (Langauage)nowLanguage;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void Credit()
    {
        SceneManager.LoadScene("Ending");
    }
}
