using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Attribute;
using Game.Request;
using Game.Scroll;
using UnityEngine;

[Serializable]
public class SaveInfo
{
    /*
        1. 날짜(day) V
        2. 퀘스트 진행 정보 V
        3. 한번 나왔던 의뢰들 V
        4. 돈 V
        5. 독촉장(서랍에 저장) 정보
        6. 속성 해금 정보 V
        7. 속성 레벨 정보
        8. 양피지 레벨 정보 V
        9. 전날 제출 정보 V
    */
    public int day;
    public List<QuestClear> questClears = new List<QuestClear>();
    public List<RequestInfo> assignedRequests = new List<RequestInfo>();
    public List<RequestResult> prevResults = new List<RequestResult>();
    public int money;
    public int parchmentLevel;
    public List<AttributeRecipeInfo> recipeInfos = new List<AttributeRecipeInfo>();
    public int unpaidSettlement;
    public bool tutorialComplete;
    public HashSet<string> reverseInterpretations = new HashSet<string>();
    public bool isAllPass;
    public bool isNoDelay;
    public bool isNoHint;
    public List<string> questLetterIds = new List<string>();
    public List<string> questReplyIds = new List<string>();

    public SaveInfo(
        int day, List<QuestClear> questClears,
        List<RequestInfo> assignedRequests,
        List<RequestResult> prevResults,
        int money,
        int parchmentLevel,
        List<AttributeRecipeInfo> recipeInfos,
        int uncompletedSettlement,
        bool tutorialComplete,
        HashSet<string> reverseInterpretations,
        bool isAllPass,
        bool isNoDelay,
        bool isNoHint,
        List<string> questLetterIds,
        List<string> questReplyIds
        )
    {
        this.day = day;
        this.questClears = questClears;
        this.assignedRequests = assignedRequests;
        this.prevResults = prevResults;
        this.money = money;
        this.parchmentLevel = parchmentLevel;
        this.recipeInfos = recipeInfos;
        this.unpaidSettlement = uncompletedSettlement;
        this.tutorialComplete = tutorialComplete;
        this.reverseInterpretations = reverseInterpretations;
        this.isAllPass = isAllPass;
        this.isNoDelay = isNoDelay;
        this.isNoHint = isNoHint;
        this.questLetterIds = questLetterIds;
        this.questReplyIds = questReplyIds;
    }

    public static SaveInfo SaveInfoFromDictionary(Dictionary<string, object> data)
    {
        int day = (int)data["Day"];
        List<QuestClear> questClears = (List<QuestClear>)data["QuestClear"];
        List<RequestInfo> assignedRequests = (List<RequestInfo>)data["AssignedRequests"];
        List<RequestResult> prevResults = (List<RequestResult>)data["PrevResults"];
        int money = (int)data["Money"];
        int parchmentLevel = (int)data["ParchmentLevel"];
        List<AttributeRecipeInfo> recipeInfos = (List<AttributeRecipeInfo>)data["RecipeInfos"];
        int uncompletedSettlement = (int)data["UncompletedSettlement"];
        bool tutorialComplete = (bool)data["TutorialComplete"];
        HashSet<string> reverseInterpretations = (HashSet<string>)data["ReverseInterpretations"];
        bool isAllPass = (bool)data["IsAllPass"];
        bool isNoDelay = (bool)data["IsNoDelay"];
        bool isNoHint = (bool)data["IsNoHint"];
        List<string> questLetterIds = (List<string>)data["QuestLetterIds"];
        List<string> questReplyIds = (List<string>)data["QuestReplyIds"];

        return new SaveInfo
        (
            day: day,
            questClears: questClears,
            assignedRequests: assignedRequests,
            prevResults: prevResults,
            money: money,
            parchmentLevel: parchmentLevel,
            recipeInfos: recipeInfos,
            uncompletedSettlement: uncompletedSettlement,
            tutorialComplete: tutorialComplete,
            reverseInterpretations: reverseInterpretations,
            isAllPass: isAllPass,
            isNoDelay: isNoDelay,
            isNoHint: isNoHint,
            questLetterIds: questLetterIds,
            questReplyIds: questReplyIds
        );
    }
}

public class SettingSaveInfo
{
    public int language;
    public float BGMSoundRatio;
    public float SFXSoundRatio;

    public SettingSaveInfo
    (
        int language,
        float BGMSoundRatio,
        float SFXSoundRatio
    )
    {
        this.language = language;
        this.BGMSoundRatio = BGMSoundRatio;
        this.SFXSoundRatio = SFXSoundRatio;
    }

    public static SettingSaveInfo FromDictionary(Dictionary<string, object> data)
    {
        int language = (int)data["Language"];
        float BGMSoundRatio = (float)data["BGMSoundRatio"];
        float SFXSoundRatio = (float)data["SFXSoundRatio"];

        return new SettingSaveInfo
        (
            language: language,
            BGMSoundRatio: BGMSoundRatio,
            SFXSoundRatio: SFXSoundRatio
        );

    }
}
