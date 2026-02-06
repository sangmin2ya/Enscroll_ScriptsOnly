using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Scroll;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Linq;
using Game.Magic;

namespace Game.Symbols
{
    /// <summary>
    /// 상징의 기본적인 기능을 정의한 추상 클래스
    /// </summary>
    public abstract class Symbol : MaskableGraphic, IGroupable
    {
        #region 인터페이스 정의
        public List<Symbol> IncludedSymbols { get; protected set; } = new List<Symbol>();
        public List<Symbol> EveryLinkedSymbols { get; protected set; } = new List<Symbol>();
        public List<Symbol> LinkedSymbols { get; protected set; } = new List<Symbol>();
        public Symbol PreviewParent { get; set; }
        public List<Symbol> PreviewChildren { get; set; }
        public bool IsMovable { get; set; } = true;
        public bool ableToCollisionWithOther = true;
        public bool whileDragging = false;

        public void TurnOffCollider()
        {
            shapeCollider.enabled = false;
        }
        public void TurnOnCollider()
        {
            shapeCollider.enabled = true;
        }
        /// <summary>
        /// 부모로 설정하고, 부모의 IncludedSymbols에 자신을 추가함
        /// 그리고 자기 LinkedSymbol에 부모를 제거, 부모의 LinkedSymbol에도 자기를 제거함
        /// 그래서 모두 그려지고 충돌이 난 뒤에 실행해야됨.
        /// </summary>
        public void SetPreviewGroup(Symbol parent, List<Symbol> groupedSymbols)
        {
            //Debug.Log("미리보기 그룹끼리 LinkedSymbol 제거" + parent.name);
            if (parent != null)
                PreviewParent = parent;
            if (groupedSymbols != null)
                PreviewChildren = groupedSymbols;
        }
        public void ResetPreviewGroup()
        {
            PreviewParent = null;
            //PreviewChildren = null;
        }
        /// <summary>
        /// 재귀 내부에서 실행되며, 해당 상징의 값만 확정 심볼로 변경해주는 함수
        /// </summary>
        public virtual void SetConfirmGroup(Symbol parent, List<Symbol> gropedSymbols)
        {
            isPreview = false;
            // color = new Color(originColor.r, originColor.g, originColor.b, 1);

            if (parent == null)
            {
                return;
            }

            GetComponent<RectTransform>().SetParent(parent.GetComponent<RectTransform>());
            TurnOffCollider(); // 콜라이더를 끔
            symbolData.Type = SymbolType.Confirm;
        }
        /// <summary>
        /// 연결된 심볼들을 업데이트하는 메소드. 부모를 제외한 모든 연결된 심볼들을 찾아서 리스트에 저장함
        /// </summary>
        /// <param name="parent">제외할 심볼 객체(=부모, 자식)</param>
        public void UpdateLinkedSymbols()
        {
            LinkedSymbols.Clear();
            //그룹 내부의 심볼들 싹다 추가
            List<Symbol> groupedSymbols = new List<Symbol>();
            if (PreviewParent != null)
            {
                groupedSymbols.Add(PreviewParent);
            }
            if (PreviewChildren != null)
            {
                foreach (Symbol child in PreviewChildren)
                {
                    groupedSymbols.Add(child);
                }
            }
            //그룹 심볼들 제외하고 리스트 생성
            foreach (Symbol symbol in EveryLinkedSymbols)
            {
                if (groupedSymbols.Contains(symbol) == false)
                {
                    LinkedSymbols.Add(symbol);
                }
            }
        }
        #endregion

        #region 문양 데이터 정보

        [Header("문양 데이터")]
        [SerializeField]
        protected int uniqueId;
        public int UniqueId => uniqueId;
        [SerializeField]
        protected SymbolData symbolData;
        public SymbolData SymbolData => symbolData;
        [SerializeField]
        protected List<Vector2> points;
        public List<Vector2> Points => points;
        [SerializeField]
        protected Color originColor;
        public Color OriginColor => originColor;
        [Header("문양 버튼")]
        private MagicSpell magicSpell;
        public MagicSpell MagicSpell { get => magicSpell; set => magicSpell = value; }
        /// <summary>
        /// 문양 정보 초기화 함수
        /// </summary>
        public virtual void Init(int uniqueId, SymbolData symbolData)
        {
            this.uniqueId = uniqueId;
            this.symbolData = symbolData; ;
        }
        /// <summary>
        /// 양쪽에서 다 필요한 좌표 변환 함수
        /// </summary>
        public List<Vector2> ConvertPointsToWorld(List<Vector2> points)
        {
            List<Vector2> worldPoints = new List<Vector2>();

            foreach (Vector2 point in points)
            {
                Vector3 localPoint3D = new Vector3(point.x, point.y, 0);
                Vector3 worldPoint3D = transform.TransformPoint(localPoint3D);
                worldPoints.Add(new Vector2(worldPoint3D.x, worldPoint3D.y));
            }

            return worldPoints;
        }

        /// <summary>
        /// 확정시 호출하는 함수
        /// </summary>
        public virtual void EnsureDrawing() { }


        /// <summary>
        /// 해당 심볼을 확정하는 함수
        /// </summary>
        public virtual void ConfrimSymbol()
        {
            //SetConfirmGroup(PreviewParent, PreviewChildren);

            if (PreviewChildren == null)
            {
                return;
            }
            // 부모로 설정하고, 부모의 자식으로 들어감
            foreach (Symbol symbol in PreviewChildren)
            {
                symbol.SetConfirmGroup(this, null);
            }
            //PreviewChildren.Clear();
            isPreview = false;
        }

        public virtual void DestroyObject() { }
        #endregion

        #region 이동과 회전
        [Header("미리보기, 부여모드 상태 확인")]
        public bool isEnchant = false;
        public bool isPreview = true;
        /// <summary>
        /// 이동과 회전을 위한 메소드
        /// </summary>
        protected abstract void Move();
        protected abstract void Rotate();

        protected float GetCanvasScaleFactor()
        {
            return ScrollCreationManager.Instance.Canvas ? ScrollCreationManager.Instance.Canvas.scaleFactor : 1f;
        }


        #endregion

        #region 충돌 감지
        [Header("콜라이더")]
        [SerializeField] protected PolygonCollider2D shapeCollider;
        private bool _isFirstSpawned = true;
        /// <summary>
        /// 마우스 감지를 위한 메소드들
        /// MouseController.cs에서 호출
        /// </summary>
        public abstract void MouseEnter();
        public abstract void MouseExit();
        protected bool IsPointerOverUIExceptSelf()
        {
            var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // 자기 자신을 제외한 UI 요소가 있는지 확인
            return results.Any(r => r.gameObject != this.gameObject);
        }
        /// <summary>
        /// 마우스를 놓았을때 업데이트 해주기 위한 함수
        /// </summary>
        //OnDestroy() 도 추가해야함
        protected abstract void OnDrop();
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Symbol>() != null)
            {
                Symbol symbol = other.GetComponent<Symbol>();
                if (symbol != null)
                {
                    EveryLinkedSymbols.Add(symbol);
                    UpdateLinkedSymbols();
                    if (ableToCollisionWithOther && !symbol.whileDragging)
                    {
                        if (GetComponent<OuterCircleDrawer>() != null)
                        {
                            ScrollCreationManager.Instance.GeneratePreviewCircleOnCollision();
                            // Debug.Log("충돌");
                            //ableToCollisionWithOther = false;
                        }
                        if (GetComponent<SymbolDrawer>() != null)
                        {
                            _isFirstSpawned = false;
                            ScrollCreationManager.Instance.GeneratePreviewCircleAtOnce(this);
                            // Debug.Log("충돌");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 재귀를 통해 모든 자식 심볼들을 찾아주는 메소드
        /// </summary>
        /// <returns></returns>
        public List<Symbol> GetEveryChildSymbols()
        {
            List<Symbol> everyChildSymbols = new List<Symbol>();
            if (PreviewChildren == null)
            {
                return everyChildSymbols;
            }
            foreach (Symbol symbol in PreviewChildren)
            {
                everyChildSymbols.Add(symbol);
                everyChildSymbols.AddRange(symbol.GetEveryChildSymbols());
            }
            return everyChildSymbols;
        }
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<Symbol>() != null)
            {
                Symbol symbol = other.GetComponent<Symbol>();
                if (symbol != null)
                {
                    EveryLinkedSymbols.Remove(symbol);
                    UpdateLinkedSymbols();
                }
            }
        }
        #endregion
    }
}