using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Attribute;
using Game.Desk;
using Game.Request;
using Game.Scroll;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QAManager : MonoBehaviour
{
    [Header("QA 패널")]
    [SerializeField] Button QAPanelBtn;
    [SerializeField] GameObject QAPanel;
    private bool isOpenPanel = false;

    [Header("하루 스킵")]
    [SerializeField] Button daySkip;

    [Header("특정 날짜로 가기")]
    [SerializeField] TMP_InputField dateInputField;
    [SerializeField] Button dateDecideBtn;

    [Header("무조건 정답 처리")]
    [SerializeField] Button successBtn;

    [Header("돈 무한")]
    [SerializeField] Button moneyButton;

    [Header("퀘스트 상태")]
    [SerializeField] List<TMP_InputField> erinie;
    [SerializeField] List<TMP_InputField> dwarf;
    [SerializeField] List<TMP_InputField> myosithar;
    [SerializeField] List<TMP_InputField> westmark;

    [Header("모든 속성 해금 상태")]
    [SerializeField] Button unlockAttributesBtn;

    [Header("저장 데이터 삭제")]
    [SerializeField] Button deleteDataBtn;

    [Header("강화/확장 속도")]
    [SerializeField] Slider intensitySlider;

    [Header("외접원 속도")]
    [SerializeField] Slider circlSlider;

    [Header("팝업창 닫기")]
    [SerializeField] Button popupButton;

    [Header("편지 사라짐 버튼")]
    [SerializeField] Button lostLtterBtn;
    [SerializeField] GameObject letterSubmitArea;

    void Awake()
    {
        QAPanelBtn.onClick.AddListener(TogglePanel);

        daySkip.onClick.AddListener(SkipDay);
        dateDecideBtn.onClick.AddListener(GoDate);

        successBtn.onClick.AddListener(SuccessRequest);

        moneyButton.onClick.AddListener(AddMoneyInfinite);

        for (int i = 0; i < erinie.Count; i++)
        {
            int index = i; // 현재 i 값을 복사
            TMP_InputField inputField = erinie[index];
            inputField.onEndEdit.AddListener((value) => ChangeComplete("Quest0", index, value));
        }
        for (int i = 0; i < dwarf.Count; i++)
        {
            int index = i; // 현재 i 값을 복사
            TMP_InputField inputField = dwarf[index];
            inputField.onEndEdit.AddListener((value) => ChangeComplete("Quest1", index, value));
        }
        for (int i = 0; i < myosithar.Count; i++)
        {
            int index = i; // 현재 i 값을 복사
            TMP_InputField inputField = myosithar[index];
            inputField.onEndEdit.AddListener((value) => ChangeComplete("Quest2", index, value));
        }
        for (int i = 0; i < westmark.Count; i++)
        {
            int index = i; // 현재 i 값을 복사
            TMP_InputField inputField = westmark[i];
            inputField.onEndEdit.AddListener((value) => ChangeComplete("Quest3", index, value));
        }

        unlockAttributesBtn.onClick.AddListener(UnlockAllAtrribute);

        deleteDataBtn.onClick.AddListener(DeleteData);

        intensitySlider.maxValue = ScrollCreationManager.Instance.TransitionDuration;
        intensitySlider.minValue = intensitySlider.maxValue * 0.5f;
        intensitySlider.value = intensitySlider.maxValue;
        intensitySlider.onValueChanged.AddListener((value) => ModifyIntensitySpeed(value));

        circlSlider.maxValue = ScrollCreationManager.Instance.CombinationDuration;
        circlSlider.minValue = circlSlider.maxValue * 0.5f;
        circlSlider.value = circlSlider.maxValue;
        circlSlider.onValueChanged.AddListener((value) => ModifyCircleSpeed(value));

        popupButton.onClick.AddListener(CloseAllPopups);

        letterSubmitArea = GameObject.Find("제출칸");
        lostLtterBtn.onClick.AddListener(LostLettr);
    }

    private void TogglePanel()
    {
        if (!isOpenPanel)
        {
            isOpenPanel = true;
            QAPanel.SetActive(true);
        }
        else
        {
            isOpenPanel = false;
            QAPanel.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        if (!isOpenPanel)
        {
            isOpenPanel = true;
            QAPanel.SetActive(true);
        }
    }

    public void ClsoePanel()
    {
        if (isOpenPanel)
        {
            isOpenPanel = false;
            QAPanel.SetActive(false);
        }
    }

    #region 하루 스킵
    public void SkipDay()
    {
        GameManager.Instance.ResetDaily();
    }
    #endregion


    #region 특정 날짜로 가기
    public void GoDate()
    {
        int goDay;
        if (int.TryParse(dateInputField.text, out goDay))
        {
            if (goDay > 0 && goDay < 14)
                DayManager.Instance.day = goDay - 2;

            GameManager.Instance.ResetDaily();
            dateInputField.text = "";
        }
    }
    #endregion

    #region 무조건 정답 처리
    public void SuccessRequest()
    {
        if (RequestManager.Instance.GetCurrentRequest() != null && DeskManager.Instance.CurrentOrderSheet != null)
        {
            RequestInfo request = RequestManager.Instance.GetCurrentRequest();
            string attributeId = AttributeManager.Instance.FindIdByName(request.AnswerIds[0].Substring(0, request.AnswerIds[0].Length - 1));
            int power = int.Parse(request.AnswerIds[0][request.AnswerIds[0].Length - 1].ToString());
            int duration = request.RequiredDurations[0];
            int money;

            if (request is SpecialRequestInfo)
                money = MoneyManager.Instance.GetSpecialPaymentValue(DayManager.Instance.day);
            else money = MoneyManager.Instance.GetPaymentValue(DayManager.Instance.day);

            string answer = string.Format("{0}{1}", AttributeManager.Instance.FindKoreanNameById(attributeId), power);

            ScrollInfo scrollInfo = new ScrollInfo(attributeId, power, duration, money, answer);

            // RequestManager.Instance.CheckResponse(scrollInfo);

            RequestManager.Instance.SendRequest(scrollInfo);
        }
    }
    #endregion

    #region 퀘스트 상태
    public void ChangeComplete(string questId, int index, string result)
    {
        if (result.Equals("0") || result.Equals("1") || result.Equals("2"))
            GameData.Instance.CompleteQuest(questId, index, int.Parse(result));
    }
    #endregion

    #region 돈 무한
    private void AddMoneyInfinite()
    {
        MoneyManager.Instance.AddMoney(10000);
    }
    #endregion

    #region 속성 전부 해금
    private void UnlockAllAtrribute()
    {
        ScrollCreationManager.Instance.UpgradeScroll(2, false);
        FindObjectOfType<ScrollBook>().UpdateScrollBook();
        ScrollCreationManager.Instance.UpgradeScroll(3, false);
        FindObjectOfType<ScrollBook>().UpdateScrollBook();
        ScrollCreationManager.Instance.UpgradeScroll(4, false);
        FindObjectOfType<ScrollBook>().UpdateScrollBook();
        foreach (AttributeInfo ai in AttributeManager.Instance.attributeTable)
        {
            AttributeManager.Instance.OnAttributeObtained(ai);
        }
    }
    #endregion

    #region 저장 데이터 삭제
    private void DeleteData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "saveTestData.json");

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"File deleted: {savePath}");
        }
    }
    #endregion

    #region 강화/확장 속도 조절
    private void ModifyIntensitySpeed(float value)
    {
        ScrollCreationManager.Instance.SetTransitionDuration(value);
    }

    private void ModifyCircleSpeed(float value)
    {
        ScrollCreationManager.Instance.SetCombinationDuration(value);
    }
    #endregion

    #region 팝업창 닫기
    private void CloseAllPopups()
    {
        GameObject[] popups = GameObject.FindObjectsOfType<GameObject>()
            .Where(obj => obj.name.Contains("Popup"))
            .ToArray();

        foreach (GameObject go in popups)
            Destroy(go);
    }
    #endregion

    #region 사라진 편지
    private void LostLettr()
    {
        letterSubmitArea.SetActive(true);
    }
    #endregion

}
