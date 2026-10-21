namespace Game.Desk
{
    /// <summary>
    /// 책상 위 오브젝트별 동작. 공통 드래그는 DragableObject가 처리하고,
    /// 오브젝트마다 다른 반응은 이 인터페이스로 위임한다.
    /// </summary>
    public interface IDeskItem
    {
        // 메인 책상 위에 놓여 있는지 여부
        bool IsPlacedOnDesk { get; set; }

        // 지금 제출 영역에 넣을 수 있는지
        bool CanSubmit { get; }

        // 제출 영역에 놓였을 때
        void OnSubmitted();

        // 우측 서랍으로 되돌아갔을 때
        void OnReturnedToDrawer();
    }
}
