using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Game.Request;
using TMPro;
using System;
using System.Linq;

public class Recipt : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private ReciptBlock reciptBlockPfb;

    [Header("Grid")]
    [SerializeField] Transform grid;
    [Header("총 수입")]
    [SerializeField] private TextMeshProUGUI totalEarnText;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;

    private bool isMouseOver;

    private Canvas _canvas;
    private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;

    [Header("영수증 테이블")]
    [SerializeField] private List<ReciptInfo> reciptInfoTable = new List<ReciptInfo>();

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = _rectTransform.parent as RectTransform;

        LoadReciptInfos();
    }

    private void LoadReciptInfos()
    {
        List<Dictionary<string, object>> lines = CSVReader.Read("CSV/ReciptVariations");
        foreach (Dictionary<string, object> line in lines)
        {
            reciptInfoTable.Add(ReciptInfo.FromDictionary(line));
        }
    }

    public void CreateRecipt()
    {
        for (int i = 0; i < grid.childCount; i++)
        {
            Destroy(grid.GetChild(i).gameObject);
        }

        if (RequestManager.Instance.GetPreviousRequestResult().Count == 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }

        int totalEarn = 0;

        // 전날 결과
        foreach (RequestResult result in RequestManager.Instance.GetPreviousRequestResult())
        {

            if (result.requestInfo is NormalRequestInfo)
            {
                if (result is CorrectRequest correctRequest)
                {

                    ReciptBlock rb = Instantiate(reciptBlockPfb, grid);
                    ReciptInfo reciptInfo = reciptInfoTable.Where(e =>
                    (
                        e.Speech == (result.requestInfo as NormalRequestInfo).Speech &&
                        !e.IsWrongDuration &&
                        !e.IsWrongEffect
                    )).ToList()[UnityEngine.Random.Range(0, 3)];
                    rb.SetData(reciptInfo, correctRequest.money);

                    totalEarn += correctRequest.money;

                    rb.SetLinkedResult(correctRequest); // 결과 연결. 스크롤 정보도 쓸거임
                }
                else if (result is IncorrectRequest incorrectRequest)
                {
                    ReciptBlock rb = Instantiate(reciptBlockPfb, grid);
                    if (incorrectRequest.isWrongBoth)
                    {
                        ReciptInfo reciptInfo = reciptInfoTable.Where(e =>
                        (
                            e.Speech == (result.requestInfo as NormalRequestInfo).Speech &&
                            e.IsWrongDuration &&
                            e.IsWrongEffect
                        )).ToList()[UnityEngine.Random.Range(0, 3)];
                        rb.SetData(reciptInfo, 0);
                    }
                    else if (incorrectRequest.isWrongEffect)
                    {
                        ReciptInfo reciptInfo = reciptInfoTable.Where(e =>
                        (
                            e.Speech == (result.requestInfo as NormalRequestInfo).Speech &&
                            !e.IsWrongDuration &&
                            e.IsWrongEffect
                        )).ToList()[UnityEngine.Random.Range(0, 3)];
                        rb.SetData(reciptInfo, 0);
                    }
                    else
                    {
                        ReciptInfo reciptInfo = reciptInfoTable.Where(e =>
                        (
                            e.Speech == (result.requestInfo as NormalRequestInfo).Speech &&
                            e.IsWrongDuration &&
                            !e.IsWrongEffect
                        )).ToList()[UnityEngine.Random.Range(0, 3)];
                        rb.SetData(reciptInfo, 0);
                    }
                    rb.SetLinkedResult(incorrectRequest);   // 결과 연결. 스크롤 정보도 쓸거임.
                }
            }
            // 특수 퀘
            else
            {
                if (result is CorrectRequest correctRequest)
                {
                    ReciptBlock rb = Instantiate(reciptBlockPfb, grid);
                    ReciptInfo reciptInfo = reciptInfoTable.Find(e =>
                    (
                        e.QuestId == (result.requestInfo as SpecialRequestInfo).QuestId &&
                        !e.IsWrongDuration &&
                        !e.IsWrongEffect
                    ));
                    rb.SetData(reciptInfo, correctRequest.money);

                    totalEarn += correctRequest.money;

                    rb.SetLinkedResult(correctRequest); // 결과 연결. 스크롤 정보도 쓸거임
                }
                else if (result is IncorrectRequest incorrectRequest)
                {
                    ReciptBlock rb = Instantiate(reciptBlockPfb, grid);
                    if (incorrectRequest.isWrongBoth)
                    {
                        ReciptInfo reciptInfo = reciptInfoTable.Find(e =>
                        (
                            e.QuestId == (result.requestInfo as SpecialRequestInfo).QuestId &&
                            e.IsWrongDuration &&
                            e.IsWrongEffect
                        ));
                        rb.SetData(reciptInfo, 0);
                    }
                    else if (incorrectRequest.isWrongEffect)
                    {
                        ReciptInfo reciptInfo = reciptInfoTable.Find(e =>
                        (
                            e.QuestId == (result.requestInfo as SpecialRequestInfo).QuestId &&
                            !e.IsWrongDuration &&
                            e.IsWrongEffect
                        ));
                        rb.SetData(reciptInfo, 0);
                    }
                    else
                    {
                        ReciptInfo reciptInfo = reciptInfoTable.Find(e =>
                        (
                            e.QuestId == (result.requestInfo as SpecialRequestInfo).QuestId &&
                            e.IsWrongDuration &&
                            !e.IsWrongEffect
                        ));
                        rb.SetData(reciptInfo, 0);
                    }
                    rb.SetLinkedResult(incorrectRequest);   // 결과 연결. 스크롤 정보도 쓸거임.
                }
            }
        }
        // if (DayManager.Instance.day == 6 || DayManager.Instance.day == 9 || DayManager.Instance.day == 12 || DayManager.Instance.day == 16)
        // {
        //     // 적금
        //     Instantiate(savingsBlockPfb, grid).transform.Find("청구금").GetComponent<TextMeshProUGUI>().SetText(MoneyManager.Instance.GetSettlementOfDay(DayManager.Instance.day - 1).ToString());
        //     totalEarn -= MoneyManager.Instance.GetSettlementOfDay(DayManager.Instance.day - 1);
        // }
        totalEarnText.text = totalEarn.ToString();
        totalEarnText.color = totalEarn >= 0 ? positiveColor : negativeColor;
        MoneyManager.Instance.AddMoney(totalEarn);
    }
}

