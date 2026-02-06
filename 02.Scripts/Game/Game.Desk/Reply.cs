using System.Collections;
using System.Collections.Generic;
using Game.Board;
using Game.Desk;
using Game.Request;
using Game.Tutorial;
using UnityEngine;

public class Reply : MonoBehaviour
{
    [SerializeField]
    private GameObject replyEnvelopePrefab;
    private Canvas canvas;
    private RectTransform rectTransform;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void CreateReply()
    {
        // 첫날이 아나리면(현재는 전날 처리한 의뢰가 없다면)    // 튜토리얼 0, 1일차 특수퀘 답장 안나오게 하려고 이렇게 함.
        if (RequestManager.Instance.GetPreviousRequestResult().Count > 0 && DayManager.Instance.day != 1 && DayManager.Instance.day != 2)
        {
            List<RequestResult> replyList = new List<RequestResult>();
            int replyCode = 0;
            foreach (RequestResult rr in RequestManager.Instance.GetPreviousRequestResult())
            {
                if (rr.requestInfo is SpecialRequestInfo specialRequestResult)
                {
                    switch (rr.replyId)
                    {
                        //이리니 마지막 답장이라면
                        case "Reply0_4R":
                            replyCode = 1;
                            break;
                        //엑스뱌른 마지막 답장이라면
                        case "Reply1_3R":
                            replyCode = 2;
                            break;
                        //마이오시타르 마지막 답장이라면
                        case "Reply2_4R":
                            replyCode = 3;
                            break;
                        //웨스트마크 마지막 답장이라면
                        case "Reply3_3R":
                            replyCode = 4;
                            break;
                        default:
                            break;
                    }

                    List<string> exceptQuestIds = new List<string>
                    {
                        "Quest5",
                        "Quest6",
                        "Quest7"
                    };

                    if (!exceptQuestIds.Contains(specialRequestResult.QuestId))
                        replyList.Add(rr);
                }
            }
            if (replyList.Count > 0)
            {
                GameObject replyEnvelope = Instantiate(replyEnvelopePrefab, CanvasManager.Instance.MainDeskUI);
                ReplyEnvelop replyEnvelop = replyEnvelope.GetComponent<ReplyEnvelop>();

                replyEnvelop.SetReply(replyList, replyCode);
            }
        }
    }
}
