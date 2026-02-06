using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChooseDataUI : MonoBehaviour
{
    [SerializeField] List<GameObject> days = new List<GameObject>();

    [SerializeField] Button closeBtn;
    [SerializeField] Button dataDeleteBtn;
    [SerializeField] GameObject dayPassAlert;
    [SerializeField] GameObject deleteAlert;

    [SerializeField] List<Sprite> dateChecks = new List<Sprite>();

    public int clickedDay = -1;

    void OnEnable()
    {
        ButtonLoad();
    }

    private void ButtonLoad()
    {
        int lastDay = 0;
        for (int i = 1; i < 14; i++)
        {
            string savePath = Path.Combine(Application.persistentDataPath, string.Format("Day{0}.json", i));
            days[i].transform.Find("동그라미").gameObject.SetActive(false);

            if (File.Exists(savePath))
            {
                int index = i;
                lastDay = index;
                days[index].SetActive(true);
                //days[index].GetComponentInChildren<Image>().sprite = dateChecks[0];
                days[index].GetComponentInChildren<Button>().interactable = true;
                days[index].GetComponentInChildren<Button>().GetComponent<Image>().raycastTarget = true;
                days[index].GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    clickedDay = index;
                    GameData.Instance.LoadSaveInfo(i);

                    if (deleteAlert.activeSelf)
                        deleteAlert.SetActive(false);
                    dayPassAlert.SetActive(true);
                });
            }
            else
            {
                days[i].GetComponentInChildren<Button>().interactable = false;
                days[i].GetComponentInChildren<Button>().GetComponent<Image>().raycastTarget = false;
            }
        }

        //days[0].GetComponentInChildren<Image>().sprite = dateChecks[0];
        days[0].GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (lastDay == 0)
            {
                clickedDay = 0;
                TitleManager.Instance.StartGame();
            }
            else
            {
                if (deleteAlert.activeSelf)
                    deleteAlert.SetActive(false);
                dayPassAlert.SetActive(true);
                clickedDay = 0;
            }
        });

        //days[lastDay].GetComponentInChildren<Image>().sprite = dateChecks[1];
        days[lastDay].transform.Find("동그라미").gameObject.SetActive(true);
        days[lastDay].GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        days[lastDay].GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            clickedDay = lastDay;
            GameData.Instance.LoadSaveInfo(lastDay);
            TitleManager.Instance.StartGame();
        });
    }

    public void DeleteData()
    {
        for (int i = 1; i < 14; i++)
        {
            string savePath = Path.Combine(Application.persistentDataPath, string.Format("Day{0}.json", i));

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"File deleted: {savePath}");
            }
        }

        ButtonLoad();

        deleteAlert.SetActive(false);
    }

    public void Close()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public void OpenDeleteAlert()
    {
        if (dayPassAlert.activeSelf)
            dayPassAlert.SetActive(false);
        if (!deleteAlert.activeSelf)
            deleteAlert.SetActive(true);
    }

    public void CloseDayAlert()
    {
        dayPassAlert.SetActive(false);
    }
    public void CloseDeleteAlert()
    {
        deleteAlert.SetActive(false);
    }

    public void DoDayPass()
    {
        for (int i = 14; i > clickedDay; i--)
        {
            string savePath = Path.Combine(Application.persistentDataPath, string.Format("Day{0}.json", i));

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"File deleted: {savePath}");
            }
        }
        GameData.Instance.LoadSaveInfo(clickedDay);
        TitleManager.Instance.StartGame();
    }
}
