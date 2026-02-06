using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using UnityEngine;

public class PuzzleRequest : MonoBehaviour
{
    private RectTransform mainDeskUI;

    [Header("퍼즐 조각")]
    [SerializeField] PuzzleEnvlope puzzleEnvlope;

    void Awake()
    {
        mainDeskUI = CanvasManager.Instance.MainDeskUI;
    }

    void Start()
    {
        Instantiate(puzzleEnvlope, mainDeskUI).transform.position = this.transform.position;
        transform.SetAsLastSibling();
    }

    IEnumerator spawnEnvlope()
    {
        yield return new WaitForSeconds(1f);
    }

    void OnDestroy()
    {
        foreach (DraggablePuzzle dp in FindObjectsOfType<DraggablePuzzle>())
        {
            dp.DestroyObject();
        }

        FindObjectOfType<PuzzleEnvlope>()?.DestroyObject();
    }

}
