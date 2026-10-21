
using System;
using System.Collections.Generic;
using Game.Request;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;
using UnityEngine.Localization.Settings;
using Game.Board;

[Serializable]
public struct QuestClear
{
    public string Id;
    public List<int> clears;
}

public class GameData : Singleton<GameData>
{
    [Header("저장 정보")]
    public static SaveInfo SaveInfo = null;
    public static SettingSaveInfo SettingInfo = null;

    void Awake()
    {
        if (Instance == this)
        {
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject); // Destroy if another instance already exists
            return;
        }

        if (DayManager.Instance != null)
            LoadSaveInfo(DayManager.Instance.day);

        LoadSetting();
    }

    [Header("언어")]
    [FormerlySerializedAs("langauage")]
    public Language language = Language.Eng;

    [Header("퀘스트 라인")]
    // 1 = 성공, 2 = 실패, 0 = 아직 안함, 
    [SerializeField] List<QuestClear> questComlitions = new List<QuestClear>();
    public List<QuestClear> QuestComlitions => questComlitions;
    [SerializeField] private List<string> questLetterIds = new List<string>();
    public List<string> QuestLetterIds => questLetterIds;
    [SerializeField] private List<string> questReplyIds = new List<string>();
    public List<string> QuestReplyIds => questReplyIds;

    [Header("엔딩")]
    // 0 = 상환 성공 / 1 = 상환 실패 / 2 - 잘못된 값
    [SerializeField] private int finalResultCode = 0;
    public int FinalResultCode
    {
        get { return finalResultCode; }
    }
    [SerializeField] private int endingCode = -1;
    public int EndingCode
    {
        get { return endingCode; }
    }

    [Header("도전 과제")]
    public bool isAllPass = true;
    public bool isNoDelay = true;
    public bool isNoHint = true;

    public void CompleteQuest(string id, int index, int result)
    {
        // if (QuestComlitions.Find(e => e.Id == id).clears[index] == 0)
        QuestComlitions.Find(e => e.Id == id).clears[index] = result;
    }

    public int GetResultOfQuest(string id, int index)
    {
        return QuestComlitions.Find(e => e.Id == id).clears[index];
    }

    public void AddQuestLetterId(string id)
    {
        questLetterIds.Add(id);
    }

    public void AddQuestReplyId(string id)
    {
        questReplyIds.Add(id);
    }

    /// <summary>
    /// 마지막 상환 성공 여부를 설정.
    /// </summary>
    /// <param name="value"> 0: 성공, 1: 실패</param>
    public void SetFinalResultCode(int value)
    {
        finalResultCode = value;
    }
    /// <summary>
    /// 엔딩 코드 설정.
    /// </summary>
    /// <param name="value">0: 스카우트, 1: 자영업, 2: 압류</param>
    public void SetEndingCode(int value)
    {
        endingCode = value;
    }

    #region 저장 데이터

    public void LoadQuestClears()
    {
        foreach (var quest in QuestComlitions)
        {
            for (int i = 0; i < quest.clears.Count; i++)
            {
                quest.clears[i] = 0;
            }
        }
        questLetterIds.Clear();
        questReplyIds.Clear();

        questComlitions = SaveInfo?.questClears ?? questComlitions;
        questLetterIds = SaveInfo?.questLetterIds ?? questLetterIds;
        questReplyIds = SaveInfo?.questReplyIds ?? questReplyIds;
    }

    public void LoadChallenges()
    {
        isAllPass = SaveInfo?.isAllPass ?? true;
        isNoDelay = SaveInfo?.isNoDelay ?? true;
        isNoHint = SaveInfo?.isNoHint ?? true;
    }
    public void LoadQuestBoard()
    {
        foreach (var quest in questLetterIds)
        {
            QuestManager.Instance.ShowQuestRecord(quest);
        }
        foreach (var reply in questReplyIds)
        {
            QuestManager.Instance.ShowReplyRecord(reply);
        }
        QuestManager.Instance.TurnOffAlarm();
    }

    public void LoadSaveInfo(int day)
    {
        SaveInfo = SaveManager.Load(day);

        LoadQuestClears();
        LoadChallenges();
    }

    public void LoadSetting()
    {
        SettingInfo = SaveManager.LoadSetting();

        if (SettingInfo != null)
            language = (Language)SettingInfo.language;
        else
        {
            SetLanguageAsApplication();
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int)language];
    }
    private void SetLanguageAsApplication()
    {
        if (Application.systemLanguage == SystemLanguage.Korean)
        {
            language = Language.Kor;
        }
        else if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            language = Language.Jp;
        }
        else
        {
            language = Language.Eng;
        }
    }
    #endregion

    /// <summary>
    /// 특정 퀘스트의 마지막 단계 값을 반환.
    /// </summary>
    /// <param name="id">퀘스트 ID.</param>
    /// <returns>성공한 단계 수.</returns>
    public int GetSuccessfulSteps(string id)
    {
        var quest = QuestComlitions.Find(q => q.Id == id);

        if (quest.Id == null)
        {
            Debug.LogWarning($"퀘스트 ID '{id}'를 찾을 수 없습니다.");
            return -1; // 퀘스트를 찾을 수 없으면 0 반환
        }

        // 리스트의 마지막 요소 반환
        return quest.clears[quest.clears.Count - 1];
    }

    #region 도전 과제
    #endregion
}
