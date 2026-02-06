using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Attribute;
using Game.Request;
using Game.Scroll;
using Game.Tutorial;
using Newtonsoft.Json;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{

    public static void Save()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, // Auto 대신 All 사용
            NullValueHandling = NullValueHandling.Include,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects // 참조 관계 보존
        };

        Dictionary<string, object> data = new Dictionary<string, object>();

        int day = DayManager.Instance.day;
        data["Day"] = day;
        List<QuestClear> questClears = GameData.Instance.QuestComlitions;
        data["QuestClear"] = questClears;
        List<RequestInfo> assignedRequests = RequestManager.Instance.AssignedRequests;
        data["AssignedRequests"] = assignedRequests;
        List<RequestResult> prevResults = RequestManager.Instance.GetPreviousRequestResult();
        data["PrevResults"] = prevResults;
        int money = MoneyManager.Instance.totalBalance;
        data["Money"] = money;
        int parchmentLevel = ScrollCreationManager.Instance.CurrentScrollLevel;
        data["ParchmentLevel"] = parchmentLevel;
        List<AttributeRecipeInfo> recipeInfos = AttributeManager.Instance.AttributeRecipeInfos;
        data["RecipeInfos"] = recipeInfos;
        int uncompletedSettlement = MoneyManager.Instance.UncompletedSettlement;
        data["UncompletedSettlement"] = uncompletedSettlement;
        bool tutorialComplete = false;
        if (TutorialManager.Instance != null)
            tutorialComplete = TutorialManager.Instance.TutorialComplete;
        data["TutorialComplete"] = tutorialComplete;
        data["ReverseInterpretations"] = RequestManager.Instance.ReverseInterpretations;
        data["IsAllPass"] = GameData.Instance.isAllPass;
        data["IsNoDelay"] = GameData.Instance.isNoDelay;
        data["IsNoHint"] = GameData.Instance.isNoHint;
        data["QuestLetterIds"] = GameData.Instance.QuestLetterIds;
        data["QuestReplyIds"] = GameData.Instance.QuestReplyIds;

        SaveInfo saveInfo = SaveInfo.SaveInfoFromDictionary(data);

        var result = JsonConvert.SerializeObject(saveInfo, settings);
        // Debug.Log(result);

        string saveDayPath = Path.Combine(Application.persistentDataPath, string.Format("Day{0}.json", day));
        File.WriteAllText(saveDayPath, result);
    }

    public static SaveInfo Load(int day)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, // Auto 대신 All 사용
            NullValueHandling = NullValueHandling.Include,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects // 참조 관계 보존
        };

        // string loadPath = Path.Combine(Application.persistentDataPath, "saveTestData.json");
        string loadPath = Path.Combine(Application.persistentDataPath, string.Format("Day{0}.json", day));

        if (File.Exists(loadPath))
        {
            string jsonData = File.ReadAllText(loadPath);
            SaveInfo loadedData = JsonConvert.DeserializeObject<SaveInfo>(jsonData, settings);

            // Debug.Log(jsonData);

            return loadedData;
        }
        else return null;
    }

    public static void SaveSetting()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, // Auto 대신 All 사용
            NullValueHandling = NullValueHandling.Include,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects // 참조 관계 보존
        };

        Dictionary<string, object> data = new Dictionary<string, object>();

        int langauage = (int)GameData.Instance.langauage;
        data["Language"] = langauage;
        float BGMSoundRatio = SoundManager.Instance.GetBGMVolume();
        data["BGMSoundRatio"] = BGMSoundRatio;
        float SFXSoundRatio = SoundManager.Instance.GetSFXVolume();
        data["SFXSoundRatio"] = SFXSoundRatio;


        SettingSaveInfo settingInfo = SettingSaveInfo.FromDictionary(data);

        var result = JsonConvert.SerializeObject(settingInfo, settings);
        // Debug.Log(result);

        string savePath = Path.Combine(Application.persistentDataPath, "Setting.json");
        File.WriteAllText(savePath, result);
    }

    public static SettingSaveInfo LoadSetting()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, // Auto 대신 All 사용
            NullValueHandling = NullValueHandling.Include,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects // 참조 관계 보존
        };

        // string loadPath = Path.Combine(Application.persistentDataPath, "saveTestData.json");
        string loadPath = Path.Combine(Application.persistentDataPath, "Setting.json");

        if (File.Exists(loadPath))
        {
            string jsonData = File.ReadAllText(loadPath);
            SettingSaveInfo loadedData = JsonConvert.DeserializeObject<SettingSaveInfo>(jsonData, settings);

            // Debug.Log(jsonData);

            return loadedData;
        }
        else return null;
    }
}
