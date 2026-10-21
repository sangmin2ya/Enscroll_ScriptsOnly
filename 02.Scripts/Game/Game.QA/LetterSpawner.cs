using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using Game.Request;
using TMPro;
using UnityEngine;

public class LetterSpawner : MonoBehaviour
{

    [Header("Prefabs")]
    [SerializeField]
    private GameObject memoRequestSheetPrefab;
    [SerializeField]
    private GameObject secretMemoRequestSheetPrefab;
    [SerializeField]
    private GameObject normalLetterRequestSheetPrefab;
    [SerializeField]
    private GameObject kidLetterRequestSheetPrefab;
    [SerializeField]
    private GameObject prettyLetterRequestSheetPrefab;
    [SerializeField]
    private GameObject royalLetterRequestSheetPrefab;
    [SerializeField]
    private GameObject normalNotificationRequestSheetPrefab;
    [SerializeField]
    private GameObject EmpireNotificationRequestSheetPrefab;
    [SerializeField]
    private GameObject orderSheetRequestSheetPrefab;

    [Header("영어 - 한국어 폰트")]
    [SerializeField] List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();

    [Header("일본어 폰트")]
    [SerializeField] List<TMP_FontAsset> fonts_jp = new List<TMP_FontAsset>();

    private bool isSpawned = false;
    List<NormalRequestInfo> requests = new List<NormalRequestInfo>();
    NormalRequestSheet _currentOrderSheet;

    void Awake()
    {
        List<Dictionary<string, object>> lines = CSVReader.Read("CSV/RequestList");
        foreach (Dictionary<string, object> data in lines)
        {
            requests.Add(NormalRequestInfo.FromDictionary(data));
        }
    }


    public void SpawnLetter()
    {
        if (_currentOrderSheet != null)
            Destroy(_currentOrderSheet.gameObject);

        RectTransform orderSheet = null;

        NormalRequestInfo request = requests[0];

        GameObject prefab = GetRequestPrefab(request);

        if (prefab != null)
        {
            orderSheet = Instantiate(prefab, FindObjectOfType<Canvas>().transform).GetComponent<RectTransform>();
            if (orderSheet.GetComponent<DragableObject>() != null)
                Destroy(orderSheet.GetComponent<DragableObject>());
            _currentOrderSheet = orderSheet?.GetComponent<NormalRequestSheet>();
            _currentOrderSheet.SetData(request);
        }

        requests.RemoveAt(0);
    }

    public GameObject GetRequestPrefab(NormalRequestInfo requestInfo)
    {
        switch (requestInfo.RequestType)
        {
            case RequestType.아이편지:
                return kidLetterRequestSheetPrefab;

            case RequestType.이쁜편지:
                return prettyLetterRequestSheetPrefab;

            case RequestType.귀족편지:
                return royalLetterRequestSheetPrefab;

            case RequestType.일반편지:
                return normalLetterRequestSheetPrefab;

            case RequestType.제국공고:
                return EmpireNotificationRequestSheetPrefab;

            case RequestType.주문서:
                return orderSheetRequestSheetPrefab;

            case RequestType.메모:
                return memoRequestSheetPrefab;

            case RequestType.일반공고:
                return normalNotificationRequestSheetPrefab;

            case RequestType.비밀메모:
                return secretMemoRequestSheetPrefab;

            default:
                Debug.LogWarning($"RequestType '{requestInfo.RequestType}'에 맞는 프리팹이 없습니다.");
                return null;
        }
    }

    public TMP_FontAsset GetRequestFont(int type)
    {
        switch (GameData.Instance.language)
        {
            case Language.Kor:
            case Language.Eng:
                return fonts[type];

            case Language.Jp:
                return fonts_jp[type];

            default:
                return fonts[type];
        }
    }
}

