using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Game.Desk;
using System;
using DG.Tweening;
/// <summary>
/// 게임 내 재화 관리 클래스: 총 재화 관리, 수익 관리, 적금 관리
/// </summary>
public class MoneyManager : Singleton<MoneyManager>
{
    [SerializeField] public int totalBalance = 0; // 총 잔고
    [SerializeField] private int dailyEarnings = 0; // 일일 수입 기록
    [SerializeField] private int savingBalance = 0; // 보유 적금량
    [SerializeField] private int debt = 100000; // 초기 빚
    //[SerializeField] private TMP_Text myMoneyOnBed;
    [SerializeField] private TMP_Text myMoneyOnDesk;
    [SerializeField] private TMP_Text moneyChange;
    [SerializeField] private TextMeshProUGUI goalMoneyOnDesk;
    [Header("코인 프리팹")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private RectTransform recite;

    [Header("정산금 금액")]
    private int settlementIndex = 0;
    private int currentSettlementDay = 3;
    [SerializeField]
    Dictionary<int, int> settlementGoals = new Dictionary<int, int>()
    {
        {4, 100},
        {7, 250},
        {10, 700},
        {13, 1200}
    };
    Dictionary<int, Tuple<int, int>> nextSettlementGoals = new Dictionary<int, Tuple<int, int>>() { };
    [Header("못다한 정산금")]
    [SerializeField] private int uncompletedSettlement = 0;
    public int UncompletedSettlement => uncompletedSettlement;

    void Awake()
    {
        // 저장 데이터
        totalBalance = GameData.SaveInfo?.money ?? totalBalance;
        uncompletedSettlement = GameData.SaveInfo?.unpaidSettlement ?? uncompletedSettlement;

        SetSettlementGoalDictionary();
    }

    void Start()
    {
        GetBalance();
    }

    ///<summary>
    ///현재 잔고 보유량 설정
    ///</summary>
    public void GetBalance()
    {
        //현재 잔고
        int currentValue = myMoneyOnDesk.text == "" ? 0 : int.Parse(myMoneyOnDesk.text);
        //숫자 바뀌는 애니메이션
        if (currentValue != totalBalance)
        {

            SoundManager.Instance.PlaySFX("Coin_Money");
            DOTween.To(() => currentValue, x => currentValue = x, totalBalance, 1f).OnUpdate(() =>
            {
                myMoneyOnDesk.text = currentValue.ToString();
                //myMoneyOnBed.text = $"{currentValue} G";
            }).onComplete += () =>
            {
                myMoneyOnDesk.text = $"{totalBalance}";
                //myMoneyOnBed.text = $"{totalBalance} G";
            };
        }
    }

    // public void SetSettlementGoalText()
    // {
    //     goalMoneyOnDesk.SetText(string.Format("D-{0} / {1} G", currentSettlementDay - DayManager.Instance.day, settlementGoals[currentSettlementDay]));
    // }

    private void SetSettlementGoalDictionary()
    {
        List<int> keys = settlementGoals.Keys.ToList();
        //settlementGoals에 key가 있는경우, 딕셔너리 key+1, 해당 key의 딕셔너리의 다음 인덱스의 value를 하나의 value로 묶어서 nextSettlementGoals에 추가
        nextSettlementGoals.Add(2, new Tuple<int, int>(keys[0], settlementGoals[keys[0]]));
        for (int i = 0; i < keys.Count - 1; i++)
        {
            nextSettlementGoals.Add(keys[i] + 1, new Tuple<int, int>(keys[i + 1], settlementGoals[keys[i + 1]]));
        }
    }
    ///<summary>
    ///재화 획득하고 일일 수익 반영
    ///</summary>
    public void AddMoney(int amount)
    {
        if (amount > 0)
        {
            moneyChange.text = $"+{amount}";
            moneyChange.DOFade(1, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                moneyChange.DOFade(1, 0.5f).SetEase(Ease.OutQuad).onComplete += () =>
                {
                    moneyChange.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
                };
            });
            totalBalance += amount; // 총 수익에 획득한 돈 추가
            dailyEarnings += amount; // 일일 수익에 획득한 돈 추가
        }
        GetBalance();
    }

    ///<summary>
    ///재화를 감소, 돈을 쓸 때 쓸 메서드
    ///</summary>
    public bool SpendMoney(int amount)
    {
        if (amount > 0 && totalBalance >= amount) // 지불할 돈이 있고, 지불해야할 돈이 전체 돈보다 작을 때
        {
            moneyChange.text = $"-{amount}";
            moneyChange.DOFade(1, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                moneyChange.DOFade(1, 0.5f).SetEase(Ease.OutQuad).onComplete += () =>
                {
                    moneyChange.DOFade(0, 0.5f).SetEase(Ease.OutQuad);
                };
            });
            totalBalance -= amount; // 재화가 깎이고
            GetBalance();
            return true; // 살 수 있는 상태임을 알려줌
        }
        else
        {
            return false; // 살 수 없는 상태이다
        }
    }

    ///<summary>
    ///빚 상환하기, 적금 넣기
    ///</summary>
    // public bool DepositToSavings()
    // {
    //     if (totalBalance >= settlementGoals[DayManager.Instance.day]) // 빚을 갚는다.
    //     {
    //         totalBalance -= settlementGoals[DayManager.Instance.day];
    //         // settlementGoals.Remove(DayManager.Instance.day);
    //         settlementIndex++;
    //         currentSettlementDay = settlementGoals.ElementAt(settlementIndex).Key;

    //         GetBalance();
    //         SetSettlementGoalText();

    //         return true;
    //     }

    //     else
    //     {
    //         return false; // 잔액이 부족해서 빚을 갚을 수 없습니다
    //     }
    // }

    public int GetSettlementOfDay(int day)
    {
        // 정산일이면 목표 금액 반환
        if (settlementGoals.ContainsKey(day))
        {
            return settlementGoals[day];
        }
        // 정산일이 아니라면 -1 반환
        else
        {
            return -1;
        }
    }
    public Tuple<int, int> GetNextSettlementOfDay(int day)
    {
        if (nextSettlementGoals.ContainsKey(day))
        {
            return nextSettlementGoals[day];
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 날짜별 랜덤 보상 생성
    /// </summary>
    public int GetPaymentValue(int day)
    {
        if (day <= 3)
        {
            return 10 + UnityEngine.Random.Range(-2, 3);
        }
        if (day <= 6)
        {
            return 30 + UnityEngine.Random.Range(-3, 7);
        }
        if (day <= 9)
        {
            return 50 + UnityEngine.Random.Range(-10, 11);
        }
        return 100 + UnityEngine.Random.Range(-10, 21);
    }
    public int GetSpecialPaymentValue(int day)
    {
        if(day <= 1)
        {
            return 20 + UnityEngine.Random.Range(0, 5);
        }
        if (day <= 3)
        {
            return 30 + UnityEngine.Random.Range(-6, 7);
        }
        if (day <= 6)
        {
            return 50 + UnityEngine.Random.Range(-8, 13);
        }
        if (day <= 9)
        {
            return 100 + UnityEngine.Random.Range(-20, 21);
        }
        return 180 + UnityEngine.Random.Range(-20, 31);
    }
    public int GetAdditionalPaymentValue()
    {
        List<int> keys = nextSettlementGoals.Keys.ToList();
        // 2, 5, 8, 11
        int day = DayManager.Instance.day;
        if (day < keys[1]) //2 일차
        {
            return 0;
        }
        if (day < keys[2])
        {
            return 50;
        }
        if (day < keys[3])
        {
            return 100;
        }
        if (day <= 13)
        {
            return 150;
        }
        return 0;
    }
    /// <summary>
    /// 상환성공
    /// </summary>
    public void SettlementComplete()
    {
        uncompletedSettlement = 0;
    }
    /// <summary>
    /// 상환실패
    /// </summary>
    /// <param name="amount">실패한 양</param>
    public void SettlementUncomplete(int amount)
    {
        uncompletedSettlement = amount;
    }
    ///<summary>
    ///현재 일일 수익 초기화, 날이 넘어갈 때 쓰면 됨
    ///</summary>
    public void ResetDailyEarnings()
    {
        dailyEarnings = 0;
    }

    /// <summary>
    /// 현재 일일 수익 반환
    /// </summary>
    public int GetDailyEarnings()
    {
        return dailyEarnings;
    }

    /// <summary>
    /// 현재 빚 반환
    /// </summary>
    public int GetDebt()
    {
        return debt;
    }
}
