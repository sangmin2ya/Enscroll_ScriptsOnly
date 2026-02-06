using System.Collections.Generic;
using System.Numerics;

namespace Game.Symbols
{
    /// <summary>
    /// 그룹화 가능한 인터페이스
    /// </summary>
    public interface IGroupable
    {
        /// <summary>
        /// 자신 하위에 포함된 심볼들 - 기본 도형은 안쓰이고 외접원에만 사용
        /// </summary>
        List<Symbol> IncludedSymbols { get; }

        /// <summary>
        /// 연결된 심볼들이 모두 포함된 리스트. 재귀 탐색 함수에서 사용됨
        /// </summary>
        List<Symbol> LinkedSymbols { get; }
        /// <summary>
        /// 제한 사항없이 충돌된 모든 심볼들
        /// </summary>
        List<Symbol> EveryLinkedSymbols { get; }
        // 그룹화 되었을 경우 미리보기 부모값
        Symbol PreviewParent { get; set; }
        List<Symbol> PreviewChildren { get; set; }

        // 클릭 우선 순위 위한 이동 가능 여부 플래그
        bool IsMovable { get; set; }

        /// <summary>
        /// 콜라이더 자체를 꺼주는 메소드. 조합식이 완성되면 하위 자식의 콜라이더를 끄기 위해 사용
        /// </summary>
        void TurnOffCollider();
        void TurnOnCollider();
        /// <summary>
        /// 그룹화 해주는 메소드. 부모로 설정하고, 부모의 IncludedSymbols에 자신을 추가함
        /// </summary>
        void SetPreviewGroup(Symbol parent, List<Symbol> gropedSymbols);
        void ResetPreviewGroup();
        /// <summary>
        /// 확정 그룹화 해주는 메소드. 부모로 설정하고, 부모의 자식으로 들어감
        /// </summary>
        void SetConfirmGroup(Symbol parent, List<Symbol> gropedSymbols);
        /// <summary>
        /// 연결된 심볼들을 업데이트하는 메소드. 부모를 제외한 모든 연결된 심볼들을 찾아서 리스트에 저장함
        /// 항상 외접원을 그리고, 자녀로 할당 해준 다음에 실행됨
        /// </summary>
        void UpdateLinkedSymbols();
    }
}