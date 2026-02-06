using UnityEngine;
using TMPro;
using System.Collections;
using ntw.CurvedTextMeshPro;

public class TypingEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent; // 텍스트를 표시할 TextMeshPro GUI
    [SerializeField] private float typingSpeed = 0.05f; // 글자 타이핑 속도

    private string fullText; // 전체 텍스트 저장
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI 컴포넌트가 연결되지 않았습니다.");
            return;
        }

        fullText = textComponent.text; // 초기 텍스트 저장
        textComponent.text = ""; // 화면에 표시할 텍스트를 초기화
    }

    private void OnEnable()
    {
        StartTypingEffect();
    }

    // 타이핑 효과 시작
    public void StartTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypingCoroutine());
    }

    private IEnumerator TypingCoroutine()
    {
        textComponent.text = "";
        foreach (char letter in fullText)
        {
            textComponent.SetText(textComponent.text + letter); // 한 글자씩 추가하고 SetText로 업데이트
            yield return new WaitForSeconds(typingSpeed); // 타이핑 속도만큼 대기
        }
    }

}
