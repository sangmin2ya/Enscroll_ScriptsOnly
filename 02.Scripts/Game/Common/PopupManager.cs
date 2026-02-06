using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using Game.Request;
using Game.Scroll;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : Singleton<PopupManager>
{
    [Header("캔버스")]
    [SerializeField] Transform deskOverlayCanvas;
    [SerializeField] Transform roomCanvas;

    [Header("프리팹")]
    [SerializeField] List<GameObject> parchmentPfbs;
    [SerializeField] GameObject questSkipAlertPfb;
    [SerializeField] GameObject bedAlertPfb;
    [SerializeField] GameObject paymentDelayAlertPfb;

    [Header("속성 해금")]
    [SerializeField] UnlockAttributeUI unlockUI;

    [Header("ESC 팝업")]
    [SerializeField] ESCMenu ESCMenu;
    bool isESCMenuOpen = false;

    public void CloseESCMenu()
    {
        isESCMenuOpen = false;
        ESCMenu.Close();
    }

    public void OpenESCMenu()
    {
        isESCMenuOpen = true;
        ESCMenu.Open();
    }

    public void OpenParchmentPopup()
    {
        GameObject go = Instantiate(parchmentPfbs[ScrollCreationManager.Instance.CurrentScrollLevel - 1], Vector3.zero, Quaternion.identity, deskOverlayCanvas);
        go.transform.localPosition = Vector3.zero;

        go.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            FindObjectOfType<ScrollBook>().UpdateScrollBook();
            Destroy(go);
        });
    }

    public void OpenBedAlertPopup()
    {
        GameObject go = Instantiate(bedAlertPfb, Vector3.zero, Quaternion.identity, roomCanvas);
        go.transform.localPosition = Vector3.zero;

        go.GetComponentInChildren<Button>().onClick.AddListener(() => { Destroy(go); });
    }

    public void OpenQuestSkipAlert()
    {
        GameObject go = Instantiate(questSkipAlertPfb, Vector3.zero, Quaternion.identity, deskOverlayCanvas);
        go.transform.localPosition = Vector3.zero;

        Transform buttons = go.transform.Find("Popup").transform.Find("Grid").Find("ButtonLayout");
        buttons.Find("YesButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            RequestManager.Instance.ReturnSpecialRequest();
            Destroy(go);
        });
        buttons.Find("NoButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(go);
        });
    }
    public void OpenPaymentDelayAlert(int paymentAmount, GameObject payment)
    {
        GameObject go = Instantiate(paymentDelayAlertPfb, Vector3.zero, Quaternion.identity, deskOverlayCanvas);
        go.transform.localPosition = Vector3.zero;

        Transform buttons = go.transform.Find("Popup").transform.Find("Grid").Find("ButtonLayout");
        buttons.Find("YesButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            // 도전 과제
            GameData.Instance.isNoDelay = false;

            MoneyManager.Instance.SettlementUncomplete(paymentAmount);
            if (DayManager.Instance.day == 13)
            {
                GameData.Instance.SetFinalResultCode(1);
            }
            RequestManager.Instance.SendPayment();
            Destroy(payment);
            Destroy(go);
        });
        buttons.Find("NoButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(go);
        });
    }
    public void OpenUnlockUI(string attributeId)
    {
        unlockUI.SetString(attributeId);
        unlockUI.Open();
    }
}
