using UnityEngine;
using TMPro;

namespace Game.Board
{
    public class QuestController : MonoBehaviour
    {
        void Awake()
        {
            QuestManager.Instance.InitQuestBoard();
            gameObject.SetActive(false);
        }
    }
}