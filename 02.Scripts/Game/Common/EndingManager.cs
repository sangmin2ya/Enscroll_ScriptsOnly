using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> endingTexts;
    [SerializeField]
    private Button toLobby;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var endingText in endingTexts)
        {
            endingText.SetActive(false);
        }

        // toLobby.onClick.AddListener(() =>
        // {
        //     SceneManager.LoadScene("Lobby");
        // });
        if (GameData.Instance.EndingCode != -1)
        {
            Debug.Log("엔딩 코드 : " + GameData.Instance.EndingCode);
            endingTexts[GameData.Instance.EndingCode].SetActive(true);
        }

        // 0 스카우트
        // 1 사업
        // 2 광산
        switch (GameData.Instance.EndingCode)
        {
            case 0:
                // 도전과제 해금
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_16");
                break;
            case 1:
                // 도전과제 해금
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_17");
                break;
            case 2:
                // 도전과제 해금
                SteamAchievementManager.Instance.UnlockAchievement("NEW_ACHIEVEMENT_1_15");
                break;
            default:
                break;
        }

        GameData.Instance.SetEndingCode(-1);
    }
}
