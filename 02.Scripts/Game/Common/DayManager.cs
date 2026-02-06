using System.Collections;
using System.Collections.Generic;
using Game.Scroll;
using Game.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayManager : Singleton<DayManager>
{
    public int day = 0; // Day
    public int todayRequestCount { get; private set; } // 오늘의 의뢰 수
    [SerializeField] private TMP_Text dateText; // 날짜 표시할 UI
    [SerializeField] private TMP_Text dateText1; // 침대씬에서 날짜 표시UI
    [SerializeField] private TMP_Text ddayText; //  D-day 표시UI
    [SerializeField] private GameObject[] dayMakers; // 달력에 적금 성공이면 X 표시할 오브젝트
    [SerializeField] private GameObject[] daySmallMakers; // 작은 달력에 적금 성공이면 X 표시할 오브젝트
    [SerializeField] private GameObject[] faildayMakers; // 달력에 적금 실패이면 표시할 오브젝트
    [SerializeField] public GameObject calendarPanel; // 달력 UI

    [SerializeField] public List<int> currentPlayerLevels = new List<int>(); // 오늘의 레벨 목록

    private bool isGoingToEnding = false;

    void Awake()
    {
        day = GameData.SaveInfo?.day ?? day;
        SetPlayerLevelForToday();
    }

    void Start()
    {
        SetDateCompleteIcon();
        UpdateDate(); // 날짜 한 번 표시
    }

    public void SetDateCompleteIcon()
    {
        for (int i = 0; i < day; i++)
        {
            dayMakers[i].SetActive(true);
        }
        for (int i = 0; i < day; i++)
        {
            daySmallMakers[i].SetActive(true);
        }
    }

    /// <summary>
    /// 현재 날짜에 맞춰서 레벨 설정 
    /// </summary>
    public void SetPlayerLevelForToday()
    {
        currentPlayerLevels.Clear(); // 레벨 리스트 초기화

        switch (day)
        {
            case 0:
                //currentPlayerLevels.Add(-1);
                currentPlayerLevels.Add(1);
                todayRequestCount = 2;
                break;
            case 1: // 이때까지 튜토리얼
                currentPlayerLevels.Add(1); // 8-1 레벨 1
                todayRequestCount = 3;
                break;
            case 2:
                currentPlayerLevels.Add(1); // 8-2 레벨 1,2
                currentPlayerLevels.Add(2);
                todayRequestCount = 3;
                break;
            case 3:
                currentPlayerLevels.Add(2);
                todayRequestCount = 3;
                break;
            case 4:
                currentPlayerLevels.Add(3);
                todayRequestCount = 3;
                break;
            case 5:
                currentPlayerLevels.Add(3);
                currentPlayerLevels.Add(4);
                todayRequestCount = 3;
                break;
            case 6:
                currentPlayerLevels.Add(3);
                currentPlayerLevels.Add(4);
                todayRequestCount = 3;
                break;
            case 7:
                currentPlayerLevels.Add(4);
                todayRequestCount = 4;
                break;
            case 8:
                currentPlayerLevels.Add(5);
                todayRequestCount = 4;
                break;
            case 9:
                currentPlayerLevels.Add(5);
                currentPlayerLevels.Add(6);
                todayRequestCount = 4;
                break;
            case 10:
                currentPlayerLevels.Add(5);
                currentPlayerLevels.Add(6);
                todayRequestCount = 4;
                break;
            case 11:
                currentPlayerLevels.Add(6);
                todayRequestCount = 4;
                break;
            case 12:
                currentPlayerLevels.Add(6);
                todayRequestCount = 4;
                break;
            case 13:
                currentPlayerLevels.Add(0);
                todayRequestCount = 0;
                break;
        }
    }

    /// <summary>
    /// 날짜를 증가시키고 달력에 표시해주는 메서드
    /// </summary>
    public void AdvanceDay() // 표시 날짜를 증가시킴
    {
        int markerIndex = day; // 인덱스 보정
        if (markerIndex < dayMakers.Length)
        {
            dayMakers[markerIndex].SetActive(true);

            day++;
            UpdateDate();
            SetDateCompleteIcon();
        }
    }


    /// <summary>
    /// 현재 날짜를 UI에 표시해주는 메서드
    /// </summary>
    public void UpdateDate()
    {
        if (day == 0)
        {
            dateText.text = "1";
            dateText1.text = "1";
            return;
        }

        dateText.text = $"{day + 1}";
        dateText1.text = $"{day + 1}";
        ddayText.text = day == 13 ? "Day" : $"{13 - day}";
    }

    ///<summary>
    /// 달력 패널을 활성화 하는 메서드
    /// </summary>
    public void ToggleCalendar(bool isActive)
    {
        if (isGoingToEnding)
        {
            SceneManager.LoadScene("Ending");
        }

        calendarPanel.SetActive(isActive);
    }

    /// <summary>
    /// 현재 달력이 켜젔나 안켜젔나 확인하는 메서드
    /// </summary>
    /// <returns></returns>
    public bool IsCalendarActive()
    {
        return calendarPanel.activeSelf;
    }

    ///<summary>
    ///버튼 클릭 시 메서드 호출
    ///</summary>
    public void OnOpenCalendarButton()
    {
        ToggleCalendar(true);
    }
}
