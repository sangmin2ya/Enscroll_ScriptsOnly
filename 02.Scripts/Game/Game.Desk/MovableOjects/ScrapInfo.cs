using System;
using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using Game.Request;
using UnityEngine;

[Serializable]
public class ScrapInfo
{
    [SerializeField] private string questId;
    public string QuestId
    {
        get { return questId; }
    }
    private string name => names[(int)GameData.Instance.language];
    private List<string> names;
    public string Name
    {
        get { return name; }
    }
    private int summaryCount;
    public int SummaryCount
    {
        get { return summaryCount; }
    }
    private List<bool> completeReplies;
    public List<bool> CompleteReplies
    {
        get { return completeReplies; }
    }
    private List<bool> readRequests;
    public List<bool> ReadRequests
    {
        get { return readRequests; }
    }
    private List<string> summaries => summariesPerLanguage[(int)GameData.Instance.language];
    private List<List<string>> summariesPerLanguage;
    public List<string> Summaries
    {
        get { return summaries; }
    }
    private List<GameObject> replies = new List<GameObject>();
    public List<GameObject> Replies
    {
        get { return replies; }
    }
    private List<GameObject> requests = new List<GameObject>();
    public List<GameObject> Requests
    {
        get { return requests; }
    }

    public ScrapInfo(string questId, List<string> names, int summaryCount, List<List<string>> summariesPerLanguage)
    {
        this.questId = questId;
        this.names = names;
        this.summaryCount = summaryCount;
        this.summariesPerLanguage = summariesPerLanguage;
        completeReplies = new List<bool>();
        readRequests = new List<bool>();
        for (int i = 0; i < summaryCount; i++)
        {
            completeReplies.Add(false);
            readRequests.Add(false);
            replies.Add(null);
            requests.Add(null);
        }
    }

    public void AddRequest(RequestInfo requestInfo)
    {
        string path = string.Format("Prefabs/QuestLetter/Request/{0}/{1}", GameData.Instance.language, requestInfo.Id);
        readRequests[RequestCsv.ParseQuestIndex(requestInfo.Id)] = true;
        GameObject go = Resources.Load<GameObject>(path);
        requests[RequestCsv.ParseQuestIndex(requestInfo.Id)] = go;
    }

    public void AddReply(ReplySheet replySheet)
    {
        completeReplies[replySheet.questIndex] = true;
        string path = string.Format("Prefabs/QuestLetter/Reply/{0}/{1}", GameData.Instance.language, replySheet.name.Replace("(Clone)", ""));
        GameObject go = Resources.Load<GameObject>(path);
        replies[replySheet.questIndex] = go;
    }

    public static ScrapInfo FromDictionary(Dictionary<string, object> data)
    {
        string questId = (string)data["QuestId"];
        List<string> names = new List<string>();
        names.Add((string)data["Name_Kor"]);
        names.Add((string)data["Name_Eng"]);
        // names.Add((string)data["Name_Jpn"]);
        int summaryCount = (int)data["SummaryCount"];
        List<List<string>> summariesPerLanguage = new List<List<string>>();
        summariesPerLanguage.Add(new List<string>());
        summariesPerLanguage.Add(new List<string>());
        summariesPerLanguage.Add(new List<string>());
        summariesPerLanguage[(int)Language.Kor].AddRange(new List<string>
        {
            FormatString((string)data["Summary0_Kor"]),
            FormatString((string)data["Summary1_Kor"]),
            FormatString((string)data["Summary2_Kor"]),
            FormatString((string)data["Summary3_Kor"]),
            FormatString((string)data["Summary4_Kor"]),
        });
        summariesPerLanguage[(int)Language.Eng].AddRange(new List<string>
        {
            FormatString((string)data["Summary0_Eng"]),
            FormatString((string)data["Summary1_Eng"]),
            FormatString((string)data["Summary2_Eng"]),
            FormatString((string)data["Summary3_Eng"]),
            FormatString((string)data["Summary4_Eng"]),
        });
        summariesPerLanguage[(int)Language.Jp].AddRange(new List<string>
        {
            FormatString((string)data["Summary0_Jp"]),
            FormatString((string)data["Summary1_Jp"]),
            FormatString((string)data["Summary2_Jp"]),
            FormatString((string)data["Summary3_Jp"]),
            FormatString((string)data["Summary4_Jp"]),
        });

        return new ScrapInfo(questId, names, summaryCount, summariesPerLanguage);
    }

    private static string FormatString(string text)
    {
        text = text.Replace('%', '\n');
        text = text.Replace('&', ',');

        return text;
    }
}
