using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReciptInfo
{
    [SerializeField] private int speech;
    public int Speech
    {
        get { return speech; }
    }

    [SerializeField] private string questId;
    public string QuestId
    {
        get { return questId; }
    }

    private string content => contents[(int)GameData.Instance.langauage];
    public string Content
    {
        get { return content; }
    }
    private List<string> contents = new List<string>();

    [SerializeField] private bool isWrongEffect;
    public bool IsWrongEffect
    {
        get { return isWrongEffect; }
    }
    [SerializeField] private bool isWrongDuration;
    public bool IsWrongDuration
    {
        get { return isWrongDuration; }
    }

    public ReciptInfo(int speech, List<string> contents, bool isWrongEffect, bool isWrongDuration, string questId = "")
    {
        this.speech = speech;
        this.contents = contents;
        this.isWrongEffect = isWrongEffect;
        this.isWrongDuration = isWrongDuration;
        this.questId = questId;
    }

    public static ReciptInfo FromDictionary(Dictionary<string, object> data)
    {
        int speech = (int)data["Speech"];
        string questId = (string)data["QuestId"];
        bool isWrongEffect = bool.Parse(data["WrongEffect"].ToString());
        bool isWrongDuration = bool.Parse(data["WrongDuration"].ToString());
        List<string> contents = new List<string>();
        contents.Add((string)data["Content_Kor"]);
        contents.Add((string)data["Content_Eng"]);
        contents.Add((string)data["Content_Jp"]);

        return new ReciptInfo(speech, contents, isWrongEffect, isWrongDuration, questId);
    }
}