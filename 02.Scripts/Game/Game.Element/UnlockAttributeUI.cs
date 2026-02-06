using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Attribute;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UnlockAttributeUI : MonoBehaviour
{
    TextMeshProUGUI text;
    RectTransform rect;
    ScrollBook scrollBook;

    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
        scrollBook = FindObjectOfType<ScrollBook>();
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    // 기본적인 문자열 포맷팅
    public string GetFormattedString(string tableReference, string key, params object[] args)
    {
        var localizedString = new LocalizedString(tableReference, key);
        return string.Format(localizedString.GetLocalizedString(), args);
    }

    public void SetString(string attributeId)
    {
        string name = AttributeManager.Instance.FindNameById(attributeId);
        string result = GetFormattedString("Localizing", "ObtainAttribute", name);
        text.SetText(result);
        scrollBook.ShowCheckOfAttribute(attributeId);

    }

    public void Open()
    {
        SoundManager.Instance.PlaySFX("percussivedrum");
        gameObject.SetActive(true);

        rect.localPosition = new Vector3(-210, 700, 0);

        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(3f);
        sequence.OnComplete(() =>
        {
            GetComponent<Animator>().SetTrigger("Close");
        });

        // 트윈 애니메이션을 Sequence에 추가
        rect.DOLocalMove(new Vector3(-210, 400, 0), 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            GetComponent<Animator>().SetTrigger("Open");
            // Sequence 실행
            sequence.Play();
        });

        // 트윈 애니메이션이 완료된 후 콜백 함수 실행
    }

    public void Close()
    {
        rect.DOLocalMove(new Vector3(-210, 700, 0), 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
