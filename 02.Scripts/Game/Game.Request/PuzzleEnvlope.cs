using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PuzzleEnvlope : MonoBehaviour
{
    [Header("생성될 편지 오브젝트")]
    [SerializeField] private List<DraggablePuzzle> puzzles = new List<DraggablePuzzle>();
    [SerializeField] private Button openButton;

    private RectTransform rectTransform;

    [SerializeField] Image envlope;
    [SerializeField] Image envlopeCover;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        rectTransform.DOLocalMoveY(0, 1f).SetEase(Ease.OutQuad);
        rectTransform.DORotate(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            openButton.onClick.AddListener(OpenEnvelope);
            openButton.interactable = true;
        });
    }

    public void OpenEnvelope()
    {
        GetComponent<Animator>().SetTrigger("Open");
        openButton.interactable = false;
    }

    public void SpawnLetters()
    {
        Vector3 letterPos = GetComponent<RectTransform>().position;
        foreach (var puzzle in puzzles)
        {
            var letter = SpawnInsideArea(puzzle);
            letter.GetComponent<RectTransform>().position = letterPos;
        }

        Destroy(gameObject);
    }

    private GameObject SpawnInsideArea(DraggablePuzzle puzzle)
    {
        return Instantiate(puzzle, CanvasManager.Instance.MainDeskUI).gameObject;
    }

    public void DestroyObject()
    {
        DG.Tweening.Sequence sequence = DOTween.Sequence();

        sequence.Append(envlope.DOColor(new Color(envlope.color.r, envlope.color.g, envlope.color.g, 0), 0.7f));
        sequence.Join(envlopeCover.DOColor(new Color(envlope.color.r, envlope.color.g, envlope.color.g, 0), 0.7f));
        sequence.OnComplete(() => { Destroy(gameObject); });

        sequence.Play();
    }
}
