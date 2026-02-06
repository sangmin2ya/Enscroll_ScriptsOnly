using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Request;

public class ReciptBlock : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI idText;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] TextMeshProUGUI reviewText;
    [SerializeField] private Button detailButton;   // 상세 의뢰 버튼

    private RequestResult linkedResult; // 연결된 결과 데이터

    private void Awake()
    {
        if (detailButton != null)
        {
            detailButton.onClick.AddListener(OnDetailButtonClick);
        }
    }

    public void SetData(ReciptInfo reciptInfo, int price)
    {
        // idText.SetText(id);
        priceText.SetText(price.ToString());
        reviewText.SetText(reciptInfo.Content.Replace('&', ','));
    }


    // public void SetData(string id, int price)
    // {
    //     idText.SetText(id);
    //     priceText.SetText(price.ToString());
    //     // reviewText.SetText(review);
    // }

    // 상세 의뢰 정보 패널에 쓸 데이터를 설정하는 메서드
    public void SetLinkedResult(RequestResult result)
    {
        linkedResult = result;
    }

    private void OnDetailButtonClick()
    {
        if (DeskManager.Instance != null && linkedResult != null)
        {
            if (linkedResult is CorrectRequest correctRequest)
            {
                DeskManager.Instance.OpenDetail(correctRequest.requestInfo, correctRequest.submittedScroll);
            }
            else if (linkedResult is IncorrectRequest incorrectRequest)
            {
                DeskManager.Instance.OpenDetail(incorrectRequest.requestInfo, incorrectRequest.submittedScroll);
            }
        }
    }
}
