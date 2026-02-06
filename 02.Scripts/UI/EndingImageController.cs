using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingImageController : MonoBehaviour
{
    [Header("퀘스트 설정")]
    [SerializeField] private string questId; // 어떤 놈의 퀘스트인가

    [Header("조건 설정")]
    [SerializeField] private bool isDefault; // 아무 조건에도 맞지 않을 때 기본적으로 활성화될 오브젝트인지 여부
    [SerializeField] private Sprite successImage;
    [SerializeField] private Sprite failureImage;
    [SerializeField] private Sprite refuseImage;

    private Image uiImage;

    void Awake()
    {
        uiImage = GetComponent<Image>();
    }

    /// <summary>
    /// 엔딩 이미지 활성화 여부를 설정.
    /// </summary>
    /// <param name="successfulSteps">성공 단계 수.</param>
    public void UpdateVisibility()
    {    
        // GameData에서 이 오브젝트와 연관된 퀘스트의 마지막 수 가져오기
        int successfulSteps = GameData.Instance.GetSuccessfulSteps(questId);

        // uiImage가 null인지 확인
        if (uiImage == null)
        {
            Debug.LogError($"UI Image가 null입니다. 퀘스트 ID: {questId}");
            return; // 더 이상 실행하지 않음
        }

                // 마지막 값에 따라 이미지를 변경
        switch (successfulSteps)
        {
            case 1: // 성공
                uiImage.sprite = successImage;
                uiImage.enabled = true;
                break;
            case 2: // 실패
                uiImage.sprite = failureImage;
                uiImage.enabled = true;
                break;
            case 0: // 거절
                uiImage.enabled = false;
                break;

            default: // 예외 상황 (값이 -1 등)
                Debug.LogWarning($"퀘스트 ID '{questId}'의 마지막 값이 유효하지 않습니다: {successfulSteps}");
                gameObject.SetActive(false); // 잘못된 데이터가 있으면 오브젝트 비활성화
                return;
        }
        // 항상 활성화
        gameObject.SetActive(true);
    }
}
