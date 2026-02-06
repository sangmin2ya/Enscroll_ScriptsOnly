using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField] private Image loadingImg;
    [SerializeField] private TextMeshProUGUI loadingText1;
    [SerializeField] private TextMeshProUGUI loadingText2;
    private TextMeshProUGUI _loadingText;
    void Start()
    {
        loadingImg.DOFade(0f, 0f);
        loadingText1.DOFade(0f, 0f);
        loadingText2.DOFade(0f, 0f);
    }
    void OnEnable()
    {
        //첫째날이면
        if (TitleManager.Instance.ChooseDataUI.GetComponent<ChooseDataUI>().clickedDay == 0)
        {
            _loadingText = loadingText1;
        }
        else
        {
            _loadingText = loadingText2;
        }
        loadingImg.DOFade(1f, 0.5f);
        _loadingText.DOFade(1f, 0.5f).OnComplete(() => LoadingAnim());
    }
    IEnumerator LoadingText()
    {
        float time = 0;
        while (time < 5)
        {
            time += Time.deltaTime;
            yield return new WaitForSeconds(0.8f);
            _loadingText.text = _loadingText.text + ".";
        }
        yield return null;
    }
    public void Disable()
    {
        gameObject.SetActive(false);
    }
    private void LoadingAnim()
    {
        StartCoroutine(LoadingText());
        // 로딩 이미지 회전 애니메이션
    }
}