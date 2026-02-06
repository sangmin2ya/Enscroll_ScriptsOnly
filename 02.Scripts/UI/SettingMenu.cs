using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    [Header("UIs")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button languageBtn;
    [SerializeField] GameObject languagePanel;
    [SerializeField] Button resolution;
    [SerializeField] TextMeshProUGUI resolutionText;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] GameObject scroll;
    [SerializeField] Animator animator;

    [Header("언어")]
    int nowLanguage;

    private bool isFullScreeen = true;
    // 전체화면/창모드 상태에 따른 키 값
    private const string FULLSCREEN_KEY = "전체 화면";
    private const string WINDOWED_KEY = "창 모드";

    void Awake()
    {
        nowLanguage = (int)GameData.Instance.langauage;

        languageBtn.onClick.AddListener(ToggleLanguages);
        resolution.onClick.AddListener(ChangeWindowMode);
        resolutionText.GetComponentInChildren<TextMeshProUGUI>();
        closeBtn.onClick.AddListener(Close);

        animator = GetComponent<Animator>();

        // 슬라이더 값 변경 이벤트 리스너
        sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
        bgmVolumeSlider?.onValueChanged.AddListener(OnBGMVolumeChanged);

        // 슬라이더 초기값 설정
        if (SoundManager.Instance != null)
        {
            sfxVolumeSlider.value = SoundManager.Instance.GetSFXVolume(); // SFX 초기 볼륨 설정
            bgmVolumeSlider.value = SoundManager.Instance.GetBGMVolume(); // BGM 초기 볼륨 설정
            bgmVolumeText.text = Mathf.Ceil(SoundManager.Instance.GetBGMVolume() * 100).ToString();
            sfxVolumeText.text = Mathf.Ceil(SoundManager.Instance.GetSFXVolume() * 100).ToString();
        }
    }

    void Update()
    {
        UpdateDisplayText();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        scroll.GetComponent<RectTransform>().DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // animator.SetTrigger("Open");
            animator.Play("OpenESCMenu");
        });
    }

    public void Close()
    {
        languagePanel.SetActive(false);
        animator.Play("CloseESCMenu");
    }

    void DestroyObject()
    {
        scroll.GetComponent<RectTransform>().DOAnchorPosY(1000, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance?.SetSFXVolume(value); // SFX 볼륨 값을 SoundManager에 전달
        sfxVolumeText.text = Mathf.Ceil(value * 100).ToString();
        SaveManager.SaveSetting();
    }

    private void OnBGMVolumeChanged(float value)
    {
        SoundManager.Instance?.SetBGMVolume(value); // BGM 볼륨 값을 SoundManager에 전달
        bgmVolumeText.text = Mathf.Ceil(value * 100).ToString();
        SaveManager.SaveSetting();
    }

    void UpdateDisplayText()
    {
        // 현재 화면 모드에 따라 적절한 키 선택
        string currentKey = Screen.fullScreen ? FULLSCREEN_KEY : WINDOWED_KEY;

        // 선택된 키로 현재 설정된 언어의 텍스트 가져오기
        var localizedString = new LocalizedString(tableReference: "Localizing", entryReference: currentKey);
        resolutionText.text = localizedString.GetLocalizedString();
    }

    private void ChangeWindowMode()
    {
        if (isFullScreeen)
        {
            isFullScreeen = false;
            Screen.fullScreen = false;

            Vector2Int resolutionValue = new Vector2Int(1280, 720);  // HD
            Screen.SetResolution(resolutionValue.x, resolutionValue.y, isFullScreeen);
        }
        else
        {
            isFullScreeen = true;
            Screen.fullScreen = true;

            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
            Screen.SetResolution(width, height, true);
        }
    }

    private void ShowLanguages()
    {
        languagePanel.SetActive(true);
    }
    private void HideLanguages()
    {
        languagePanel.SetActive(false);
    }
    private void ToggleLanguages()
    {
        languagePanel.SetActive(!languagePanel.activeSelf);
    }

    public void ChangeLanaguage(int index)
    {
        nowLanguage = index;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[nowLanguage];
        GameData.Instance.langauage = (Langauage)nowLanguage;

        languagePanel.SetActive(false);

        SaveManager.SaveSetting();
    }
}
