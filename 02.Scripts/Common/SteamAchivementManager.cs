using Steamworks;
using UnityEngine;

public class SteamAchievementManager : Singleton<SteamAchievementManager>
{
    private bool initialized = false;

    private void Awake()
    {
        if (Instance == this)
        {
            InitializeSteam();
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeSteam()
    {
        try
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("Steam API 초기화 실패!");
                return;
            }

            initialized = true;
            //            Debug.Log("Steam API 초기화 성공");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Steam 초기화 중 오류 발생: {e.Message}");
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!initialized)
        {
            Debug.LogWarning("Steam이 초기화되지 않았습니다!");
            return;
        }

        try
        {
            bool achieved = false;
            SteamUserStats.GetAchievement(achievementId, out achieved);

            if (!achieved)
            {
                SteamUserStats.SetAchievement(achievementId);
                SteamUserStats.StoreStats();
                //                Debug.Log($"업적 달성: {achievementId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업적 해제 중 오류 발생: {e.Message}");
        }
    }

    public void ResetAchievement(string achievementId)
    {
        if (!initialized) return;

        try
        {
            SteamUserStats.ClearAchievement(achievementId);
            SteamUserStats.StoreStats();
            //            Debug.Log($"업적 초기화: {achievementId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업적 초기화 중 오류 발생: {e.Message}");
        }
    }

    public bool GetAchievementStatus(string achievementId)
    {
        if (!initialized) return false;

        bool achieved = false;
        try
        {
            SteamUserStats.GetAchievement(achievementId, out achieved);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업적 상태 확인 중 오류 발생: {e.Message}");
        }
        return achieved;
    }

    private void OnApplicationQuit()
    {
        if (initialized)
        {
            SteamAPI.Shutdown();
        }
    }
}
