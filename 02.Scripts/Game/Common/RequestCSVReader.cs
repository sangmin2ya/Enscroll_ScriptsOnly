using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// CSV 파일에서 데이터를 읽어와 object 타입으로 변환하여 Dictionary 형식으로 저장하는 클래스.
/// 이 클래스는 CSV 파일을 읽어 각 행의 데이터를 자동으로 변환하여 사용할 수 있도록 준비합니다.
/// </summary>
public class RequestCSVReader : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile; // CSV 파일을 Unity 인스펙터에서 할당

    private static string SPLIT_RE = @",(?=(?:[^""@]*""[^""@]*"")*(?![^""@]*""))";
    private static string LINE_SPLIT_RE = "\\r\\n|\\n\\r|\\n|\\r"; // 줄바꿈 구분 정규식 (텍스트로 \n 유지)
    private static char[] TRIM_CHARS = { '\"' }; // 양 끝의 따옴표 제거용


    /// <summary>
    /// CSV 파일에서 데이터를 읽어와 각 행을 Dictionary<string, object>로 변환하여 리스트로 반환하는 메서드.
    /// </summary>
    /// <returns>각 행의 데이터를 저장한 Dictionary 리스트</returns>
    public List<Dictionary<string, object>> LoadRequests()
    {
        List<Dictionary<string, object>> requests = new List<Dictionary<string, object>>();

        if (csvFile == null)
        {
            Debug.LogError("CSV 파일이 할당되지 않았습니다!");
            return requests;
        }

        var lines = Regex.Split(csvFile.text, LINE_SPLIT_RE); // 줄바꿈 기준으로 CSV 파일의 텍스트를 분할

        if (lines.Length <= 1) return requests; // 데이터가 없으면 빈 리스트 반환

        // 첫 번째 줄을 헤더로 처리하여 각 열의 제목을 배열로 저장
        var header = Regex.Split(lines[0], SPLIT_RE);
        // Debug.Log("Header: " + string.Join(", ", header));
        for (int i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue; // 빈 행은 무시

            var entry = new Dictionary<string, object>(); // 현재 행을 저장할 Dictionary 생성
            List<int> correctAttributes = new List<int>(); // 행마다의 정답 속성 리스트 생성

            for (int j = 0; j < header.Length && j < values.Length; j++)
            {
                // 각 열의 값을 가져와 공백 및 따옴표를 제거
                string value = values[j].Trim(TRIM_CHARS).Replace("\\", "");

                // 값을 int, float로 변환 시도 후 변환 가능하면 해당 타입으로 저장
                object finalValue = value; // 기본적으로는 문자열로 처리
                if (int.TryParse(value, out int intValue))
                {
                    finalValue = intValue; // 변환 가능하면 정수로 저장
                }
                else if (float.TryParse(value, out float floatValue))
                {
                    finalValue = floatValue; // 변환 가능하면 실수로 저장
                }


                if (Regex.IsMatch(header[j], @"^correctAttribute\d+$") && finalValue is int attributeValue)
                {
                    correctAttributes.Add(attributeValue);
                }
                else
                {
                    entry[header[j]] = finalValue; // 헤더를 키로 하여 Dictionary에 값 저장
                }
            }

            entry["correctAttribute"] = correctAttributes;

            requests.Add(entry); // 완성된 행 데이터를 리스트에 추가
        }

        return requests; // 모든 행을 포함한 리스트 반환
    }
}
