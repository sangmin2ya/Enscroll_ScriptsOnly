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
        [SerializeField] private string content => contents[(int)GameData.Instance.language];
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
            string id = RequestCsv.GetString(data, "Id", "?"); // 의뢰 ID
            int level = RequestCsv.GetInt(data, "Level", id); // 의뢰 레벨
            RequestType requestType = GetRequestType(RequestCsv.GetString(data, "RequestType", id), id); // 의뢰 타입
            int font = RequestCsv.GetInt(data, "Font", id); // 폰트
            List<int> requiredDurations = RequestCsv.GetIntList(data, "RequiredDuration", id); // 의뢰 지속 시간
            List<string> answerIds = RequestCsv.GetStringList(data, "AnswerIds", id);
            int speech = RequestCsv.GetInt(data, "Speech", id);
            List<string> contents = RequestCsv.GetLocalized(data, "Content", id); // 의뢰 설명 (Content_Kor, Content_Eng, ...)

            // 수집된 데이터를 사용하여 Request 객체 생성 및 반환
            return new NormalRequestInfo(id, level, font, requestType, requiredDurations, answerIds, speech, contents);
        }

        private static RequestType GetRequestType(string requestType, string rowId)
        {
            if (Enum.TryParse(requestType, ignoreCase: true, out RequestType result) && Enum.IsDefined(typeof(RequestType), result))
            {
                return result;
            }

            Debug.LogError($"[{rowId}] 'RequestType' 값이 올바르지 않습니다: {requestType}");
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
            questIndex = RequestCsv.ParseQuestIndex(id);
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
            string id = RequestCsv.GetString(data, "Id", "?"); // 의뢰 ID
            List<int> requiredDurations = RequestCsv.GetIntList(data, "RequiredDuration", id); // 의뢰 지속 시간
            List<string> answerIds = RequestCsv.GetStringList(data, "AnswerIds", id);
            int day = RequestCsv.GetInt(data, "Day", id);
            string rightReply = RequestCsv.GetString(data, "RightReply_Id", id);
            string wrongReply = RequestCsv.GetString(data, "WrongReply_Id", id);
            bool firstOrder = RequestCsv.GetBool(data, "FirstOrder", id);

            // 수집된 데이터를 사용하여 Request 객체 생성 및 반환
            return new SpecialRequestInfo(id, day, requiredDurations, answerIds, rightReply, wrongReply, firstOrder);
        }
    }

    /// <summary>
    /// 의뢰 CSV 행 파싱 도우미. 값이 없거나 형식이 다르면 행 ID와 컬럼명을 로그로 남기고 기본값을 사용
    /// </summary>
    public static class RequestCsv
    {
        public static string GetString(Dictionary<string, object> data, string column, string rowId)
        {
            // CSVReader가 숫자처럼 보이는 값은 int/float로 바꾸므로 캐스팅 대신 ToString 사용
            if (data.TryGetValue(column, out object value) && value != null)
            {
                return value.ToString();
            }
            Debug.LogError($"[{rowId}] '{column}' 컬럼이 없습니다.");
            return "";
        }

        public static int GetInt(Dictionary<string, object> data, string column, string rowId)
        {
            return ParseInt(GetString(data, column, rowId), column, rowId);
        }

        public static bool GetBool(Dictionary<string, object> data, string column, string rowId)
        {
            string value = GetString(data, column, rowId);
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            Debug.LogError($"[{rowId}] '{column}' 값을 bool로 읽을 수 없습니다: {value}");
            return false;
        }

        // '&'로 구분된 값 목록
        public static List<string> GetStringList(Dictionary<string, object> data, string column, string rowId)
        {
            return GetString(data, column, rowId).Split('&').ToList();
        }

        public static List<int> GetIntList(Dictionary<string, object> data, string column, string rowId)
        {
            return GetStringList(data, column, rowId).ConvertAll(value => ParseInt(value, column, rowId));
        }

        /// <summary>
        /// "{prefix}_{언어}" 컬럼을 언어 enum 순서대로 읽음 (Content_Kor, Content_Eng, Content_Jp ...)
        /// 언어를 추가할 때 파서는 수정하지 않아도 됨
        /// </summary>
        public static List<string> GetLocalized(Dictionary<string, object> data, string prefix, string rowId)
        {
            // Language 값이 0부터 연속이므로 리스트 인덱스 = (int)Language
            List<string> values = new List<string>();
            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                values.Add(GetString(data, $"{prefix}_{language}", rowId));
            }
            return values;
        }

        /// <summary>
        /// ID의 '_' 뒤 숫자 부분을 퀘스트 인덱스로 읽음 (예: "Q3_12A" -> 12). 두 자리 이상도 지원
        /// </summary>
        public static int ParseQuestIndex(string id)
        {
            string[] parts = id.Split('_');
            if (parts.Length > 1)
            {
                string digits = new string(parts[1].TakeWhile(ch => ch >= '0' && ch <= '9').ToArray());
                if (int.TryParse(digits, out int index))
                {
                    return index;
                }
            }
            Debug.LogError($"퀘스트 인덱스를 읽을 수 없는 ID: {id}");
            return 0;
        }

        private static int ParseInt(string value, string column, string rowId)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            Debug.LogError($"[{rowId}] '{column}' 값을 정수로 읽을 수 없습니다: {value}");
            return 0;
        }
    }


}