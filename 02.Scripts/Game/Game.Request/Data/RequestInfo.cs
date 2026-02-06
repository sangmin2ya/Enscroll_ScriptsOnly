using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

// ---- CSV 데이터를 객체로 변환하는 스크립트 ----- //

namespace Game.Request
{
    [Serializable]
    public class RequestInfo
    {
        [SerializeField]
        protected string id; // 의뢰 번호ID
        public string Id
        {
            get { return id; }
        }
        [SerializeField]
        protected List<int> requiredDurations; // 의뢰 지속 시간  
        public List<int> RequiredDurations
        {
            get { return requiredDurations; }
        }
        [SerializeField]
        protected List<string> answerIds;
        public List<string> AnswerIds
        {
            get { return answerIds; }
        }
        [SerializeField]
        protected HashSet<string> answerAttributes;
        public HashSet<string> AnswerAttributes
        {
            get { return answerAttributes; }
        }
    }

    [Serializable]
    public class NormalRequestInfo : RequestInfo
    {
        [SerializeField]
        private int level; // 의뢰의 조합레벨
        public int Level
        {
            get { return level; }
        }
        [SerializeField]
        private RequestType requestType; // 의뢰 타입
        public RequestType RequestType
        {
            get { return requestType; }
        }
        [SerializeField]
        private int font;
        public int Font
        {
            get { return font; }
        }

        [SerializeField]
        private int speech;
        public int Speech
        {
            get { return speech; }
        }
        [SerializeField] private string content => contents[(int)GameData.Instance.langauage];
        public string Content
        {
            get { return content; }
        }
        [JsonProperty][SerializeField] private List<string> contents = new List<string>();

        public NormalRequestInfo(string id, int level, int font, RequestType requestType, List<int> requiredDurations, List<string> answerIds, int speech, List<string> contents)
        {
            this.id = id;
            this.level = level;
            this.requestType = requestType;
            this.requiredDurations = requiredDurations;
            this.font = font;
            this.answerIds = answerIds;
            this.answerAttributes = new HashSet<string>();
            foreach (string answerId in answerIds)
            {
                this.AnswerAttributes.Add(answerId.Substring(0, answerId.Length - 1));
            }
            this.speech = speech;
            this.contents = contents;
        }

        /// <summary>
        /// Dictionary 데이터를 받아 Request 객체를 생성하는 정적 메서드
        /// </summary>
        /// <param name="data">CSV에서 불러온 Dictionary 형태의 데이터</param>
        /// <returns>생성된 Request 객체</returns>
        public static NormalRequestInfo FromDictionary(Dictionary<string, object> data)
        {
            // 필수 필드를 Dictionary에서 가져와 변수에 저장, CSV 파일 이름 그대로
            string id = (string)data["Id"]; // 의뢰 ID
            int level = (int)data["Level"]; // 의뢰 레벨
            RequestType requestType = GetRequestType((string)data["RequestType"]); // 의뢰 타입
            int font = (int)data["Font"]; // 폰트
            List<int> requiredDurations = data["RequiredDuration"].ToString().Split('&').ToList().ConvertAll(int.Parse); // 의뢰 지속 시간
            List<string> answerIds = data["AnswerIds"].ToString().Split('&').ToList();
            int speech = (int)data["Speech"];
            List<string> contents = new List<string>();
            contents.Add("");
            contents.Add("");
            contents.Add("");
            contents[(int)Langauage.Kor] = (string)data["Content_Kor"]; // 의뢰 설명
            contents[(int)Langauage.Eng] = (string)data["Content_Eng"]; // 의뢰 설명
            contents[(int)Langauage.Jp] = (string)data["Content_Jp"]; // 의뢰 설명

            // 수집된 데이터를 사용하여 Request 객체 생성 및 반환
            return new NormalRequestInfo(id, level, font, requestType, requiredDurations, answerIds, speech, contents);
        }

        private static RequestType GetRequestType(string requestType)
        {
            if (Enum.TryParse(requestType, ignoreCase: true, out RequestType result))
            {
                return result;
            }

            Debug.LogError("이상한 타입의 주문서 들어옴");
            return RequestType.주문서;
        }
    }

    [Serializable]
    public class SpecialRequestInfo : RequestInfo
    {
        [SerializeField]
        private string questId;
        public string QuestId
        {
            get { return questId; }
        }
        [SerializeField]
        private int questIndex;
        public int QuestIndex
        {
            get { return questIndex; }
        }
        [SerializeField]
        private int day;
        public int Day
        {
            get { return day; }
        }
        [SerializeField]
        private string rightReply;
        public string RightReply
        {
            get { return rightReply; }
        }
        [SerializeField]
        private string wrongReply;
        public string WrongReply
        {
            get { return wrongReply; }
        }
        [SerializeField]
        private bool firstOrder;
        public bool FirstOrder
        {
            get { return firstOrder; }
        }

        public SpecialRequestInfo(string id, int day, List<int> requiredDurations, List<string> answerIds, string rightReply, string wrongReply, bool firstOrder)
        {
            this.id = id;
            questId = id.Split('_')[0];
            questIndex = int.Parse(id.Split("_")[1].Substring(0, 1));
            this.day = day;
            this.requiredDurations = requiredDurations;
            this.answerIds = answerIds;
            this.answerAttributes = new HashSet<string>();
            foreach (string answerId in answerIds)
            {
                this.AnswerAttributes.Add(answerId.Substring(0, answerId.Length - 1));
            }
            this.rightReply = rightReply;
            this.wrongReply = wrongReply;
            this.firstOrder = firstOrder;
        }

        /// <summary>
        /// Dictionary 데이터를 받아 Request 객체를 생성하는 정적 메서드
        /// </summary>
        /// <param name="data">CSV에서 불러온 Dictionary 형태의 데이터</param>
        /// <returns>생성된 Request 객체</returns>
        public static SpecialRequestInfo FromDictionary(Dictionary<string, object> data)
        {
            // 필수 필드를 Dictionary에서 가져와 변수에 저장, CSV 파일 이름 그대로
            string id = (string)data["Id"]; // 의뢰 ID
            List<int> requiredDurations = data["RequiredDuration"].ToString().Split('&').ToList().ConvertAll(int.Parse); // 의뢰 지속 시간
            List<string> answerIds = data["AnswerIds"].ToString().Split('&').ToList();
            int day = (int)data["Day"];
            string rightReply = (string)data["RightReply_Id"];
            string wrongReply = (string)data["WrongReply_Id"];
            bool firstOrder = bool.Parse(data["FirstOrder"].ToString());

            // 수집된 데이터를 사용하여 Request 객체 생성 및 반환
            return new SpecialRequestInfo(id, day, requiredDurations, answerIds, rightReply, wrongReply, firstOrder);
        }
    }


}