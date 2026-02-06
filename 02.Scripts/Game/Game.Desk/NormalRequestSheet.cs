using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using Game.Request;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NormalRequestSheet : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> contents;

    public void SetData(NormalRequestInfo request)
    {
        // 포맷팅
        string content = request.Content.Replace("%", "\n");
        content = content.Replace("&", ",");
        content = ProcessStyledText(content);

        TMP_FontAsset font = DeskManager.Instance.GetRequestFont(request.Font);

        string[] strings = content.Split('@');
        for (int i = 0; i < strings.Length; i++)
        {
            if (contents[i].GetComponent<TextMeshProUGUI>() != null)
            {
                TextMeshProUGUI text = contents[i].GetComponent<TextMeshProUGUI>();
                text.font = font;
                text.SetText(strings[i]);
            }
            else if (contents[i].GetComponent<Image>() != null)
            {
                contents[i].GetComponent<Image>().sprite = null;
            }
        }

    }
    private string ProcessStyledText(string input)
    {
        string pattern = @"\$(.*?)\$";
        return System.Text.RegularExpressions.Regex.Replace(input, pattern, match =>
        {
            // $$ 사이의 텍스트를 추출
            string textToStyle = match.Groups[1].Value;

            // 볼드체와 색상 태그 적용
            // TextMeshPro의 Rich Text 태그 사용
            return $"<color=red>{textToStyle}</color>";
        });
    }


}
