using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Button _button;
    void Start()
    {
        // 버튼 컴포넌트를 가져옴
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();
        if (_button == null)
        {
            return;
        }

        // EventTrigger 컴포넌트가 없으면 추가
        EventTrigger eventTrigger = _button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = _button.gameObject.AddComponent<EventTrigger>();
        }

        // PointerDown 이벤트 추가
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((eventData) => OnPointerDown((PointerEventData)eventData));
        eventTrigger.triggers.Add(pointerDownEntry);

        // PointerUp 이벤트 추가
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((eventData) => OnPointerUp((PointerEventData)eventData));
        eventTrigger.triggers.Add(pointerUpEntry);

        SetPivotToCenter(_rectTransform);
    }
    private void OnPointerDown(PointerEventData eventData)
    {
        _rectTransform.DOKill();
        _rectTransform.DOScale(new Vector3(0.95f, 0.95f, 0.95f), 0.1f);
        // 버튼 클릭 효과음
        SoundManager.Instance.PlaySFX("Clip");
    }

    private void OnPointerUp(PointerEventData eventData)
    {
        _rectTransform.DOKill();
        _rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
    }
    public void SetPivotToCenter(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is null!");
            return;
        }

        // 현재 피벗에서 중심으로 이동할 오프셋 계산
        Vector2 pivotOffset = rectTransform.pivot - new Vector2(0.5f, 0.5f);
        Vector2 deltaPosition = new Vector2(
            pivotOffset.x * rectTransform.rect.width * rectTransform.localScale.x,
            pivotOffset.y * rectTransform.rect.height * rectTransform.localScale.y
        );

        // 피벗을 중심으로 설정
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 위치 보정
        rectTransform.localPosition -= new Vector3(deltaPosition.x, deltaPosition.y, 0);
    }
}