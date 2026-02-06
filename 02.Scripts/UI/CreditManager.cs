using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CreditManager : MonoBehaviour
{

    [SerializeField] private Animator creditAnimator; // 크레딧 패널의 Animator
    [SerializeField] private string mainMenuSceneName = "Lobby"; // 메인 메뉴 씬 이름
    [SerializeField] private GameObject button;
    private bool isAnimationEnded = false; // 애니메이션이 끝났는지 확인

    [Header("엔딩 이미지 오브젝트")]
    [SerializeField] private List<EndingImageController> endingImageControllers; // 엔딩 이미지 컨트롤러 리스트


    void Start()
    {
        // 각 EndingImageController가 자신의 퀘스트 ID를 사용해 업데이트하도록 호출
        foreach (var controller in endingImageControllers)
        {
            controller.UpdateVisibility();
        }

        SoundManager.Instance.CrossfadeMusic("TitleBGM");

        if(GameData.Instance.EndingCode == -1)
        {
            PlayAnimation();
        }
    }

    void Update()
    {
        // ESC 키를 눌렀을 때 즉시 스킵
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipToMainMenu();
        }
        if (Input.GetMouseButton(0))
        {
            creditAnimator.speed = 3;
        }
        else
        {
            creditAnimator.speed = 1;
        }
    }

    public void PlayAnimation()
    {
        creditAnimator.SetTrigger("moveDown");
        button.gameObject.SetActive(false);
    }

    // 애니메이션이 끝났을 때 호출되는 메서드
    public void OnCreditAnimationEnd()
    {
        if (!isAnimationEnded)
        {
            isAnimationEnded = true;
            LoadMainMenu();
        }
    }

    // 메인 메뉴 씬으로 전환
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ESC 키로 스킵
    private void SkipToMainMenu()
    {
        // 바로 애니메이션을 종료하고 씬 전환
        if (!isAnimationEnded)
        {
            isAnimationEnded = true;
            LoadMainMenu();
        }
    }
}
