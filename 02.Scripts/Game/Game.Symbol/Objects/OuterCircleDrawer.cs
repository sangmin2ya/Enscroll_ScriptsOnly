using System.Collections;
using System.Collections.Generic;
using Game.Scroll;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Symbols
{
    public class OuterCircleDrawer : Symbol
    {
        #region 외접원 그리기 정보

        [Header("모양 정보")]
        private float thickness = 5f;
        private int segments = 48; //원은 불러오지않고 고정

        [Header("최소 외접원")]
        protected MinimumEnclosingCircle.Circle minimumEnclosingCircle;
        public MinimumEnclosingCircle.Circle Circle { get { return minimumEnclosingCircle; } }

        [Header("드로우 애니메이션 설정")]
        protected float currentAnimationTime = 0f;
        // private float animationDuration = 0.5f; // 애니메이션 지속 시간 (초)
        private int currentSegment = 0;
        private Coroutine animationCoroutine;

        [Header("드래그 및 마우스 처리")]
        private Vector3 prevMousePosition;
        // private Vector3 offset; // 마우스와 오브젝트 간 오프셋
        private bool isMouseOver = false;
        private bool isDragging = false;
        private bool isRotating = false;
        private float initialRotation; // 초기 회전 각도
        private float initialMouseAngle; // 마우스를 처음 눌렀을 때의 각도

        [Header("메테리얼")]
        private bool isFixed = false;

        #endregion

        #region 데이터 초기화

        /// <summary>
        /// 외접원의 데이터 초기화 항상 먼저 SetIncludedSymbols를 통해 자식 심볼들을 받아와야함!!!
        /// </summary>
        public override void Init(int uniqueId, SymbolData symbolData)
        {
            // 초기화할 때 애니메이션 상태 리셋
            currentSegment = 0;

            this.uniqueId = uniqueId;
            this.symbolData = symbolData;
            this.originColor = color;

            DrawPreviewCircle();
        }
        public void ConfirmOuterCircle()
        {
            SetCenter(IncludedSymbols);
            base.ConfrimSymbol();
            foreach (var subSymbol in base.IncludedSymbols)
            {
                subSymbol.color = subSymbol.OriginColor;
            }
            //TurnOnCollider();
            //StartCoroutine(AnimateEnchant());
        }

        #endregion

        #region 마우스 이벤트 및 이동
        void Update()
        {
            //미리보기 상태가 아니고, 부여모드일 때만 마우스 이벤트 처리
            if (!isPreview && isEnchant)
            {
                // 좌클릭으로 드래그 시작
                if (isMouseOver && Input.GetMouseButtonDown(0) && !isDragging)
                {
                    // ChangeMovingMaterial();

                    isDragging = true;
                    prevMousePosition = Input.mousePosition;
                    // offset = transform.position - GetMouseWorldPosition();

                    ScrollCreationManager.Instance.DragSymbol(this);
                }
                // 우클릭
                else if (isMouseOver && Input.GetMouseButtonDown(1) && !isRotating)
                {
                    // ChangeMovingMaterial();

                    isRotating = true;
                    whileDragging = true;
                    // 현재 오브젝트의 회전 각도 저장
                    initialRotation = transform.rotation.eulerAngles.z;

                    // 마우스의 초기 각도 계산
                    Vector3 mousePosition = Input.mousePosition;
                    Vector3 centerPoint = Camera.main.WorldToScreenPoint(transform.position);
                    Vector3 direction = mousePosition - centerPoint;
                    initialMouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // linkedSymbolsLists에서 이 객체가 포함된 그룹을 제거하고 갱신
                    ScrollCreationManager.Instance.DragSymbol(this);
                }

                // 마우스 버튼이 떼어질 때 드래그와 회전을 멈춤
                if (Input.GetMouseButtonUp(0) && isDragging)
                {
                    // ChangePreveiwMaterial();

                    isDragging = false;
                    IsMovable = false;

                    OnDrop();
                }
                else if (Input.GetMouseButtonUp(1) && isRotating)
                {
                    // ChangePreveiwMaterial();

                    isRotating = false;
                    whileDragging = false;
                    IsMovable = false;
                    // linkedSymbolsLists에서 이 객체가 포함된 그룹을 포함해서 갱신
                    OnDrop();
                }

                // 드래그 중에는 부모 오브젝트 이동
                if (isDragging)
                {
                    Move();
                }

                if (isRotating)
                {
                    Rotate();
                }
            }
        }

        protected override void Move()
        {
            // 현재 마우스 위치
            Vector3 currentMousePosition = Input.mousePosition;

            // 이전 프레임과의 마우스 위치 차이 계산
            Vector3 delta = currentMousePosition - prevMousePosition;

            // UI 캔버스의 스케일 팩터 고려
            float scaleFactor = GetCanvasScaleFactor();
            delta /= scaleFactor;

            // RectTransform의 현재 위치에 delta 더하기
            RectTransform parentRect = (RectTransform)transform.parent;
            Vector2 newPosition = rectTransform.anchoredPosition + (Vector2)delta;

            // Collider의 bounds를 UI 좌표계로 변환
            Bounds colliderBounds = shapeCollider.bounds;
            Vector3 colliderExtents = colliderBounds.extents; // bounds의 절반 크기

            // UI 좌표계에서의 Collider 크기 계산
            Vector3 worldToUIScale = new Vector3(
                1f / transform.lossyScale.x,
                1f / transform.lossyScale.y,
                1f
            );
            Vector2 colliderSize = new Vector2(
                colliderExtents.x * 2f * worldToUIScale.x,
                colliderExtents.y * 2f * worldToUIScale.y
            );

            // 부모 UI 영역의 경계 계산
            Vector2 parentPivot = parentRect.pivot;
            float parentWidth = parentRect.rect.width;
            float parentHeight = parentRect.rect.height;

            // Collider 크기를 고려한 이동 제한 경계 계산
            float minX = -parentWidth * parentPivot.x + colliderSize.x / 2;
            float maxX = parentWidth * (1 - parentPivot.x) - colliderSize.x / 2;
            float minY = -parentHeight * parentPivot.y + colliderSize.y / 2;
            float maxY = parentHeight * (1 - parentPivot.y) - colliderSize.y / 2;

            // 새로운 위치 제한
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            // 위치 적용
            rectTransform.anchoredPosition = newPosition;

            // 현재 마우스 위치를 다음 프레임의 이전 위치로 저장
            prevMousePosition = currentMousePosition;
        }
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            return Camera.main.ScreenToWorldPoint(mouseScreenPos);
        }
        protected override void Rotate()
        {
            // 현재 마우스 각도 계산
            Vector3 mousePosition = Input.mousePosition;
            Vector3 centerPoint = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 direction = mousePosition - centerPoint;
            float currentMouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 마우스 회전 각도의 차이 계산
            float deltaAngle = currentMouseAngle - initialMouseAngle;

            // 최종 회전 각도 = 초기 오브젝트 각도 + 마우스 회전 각도
            float newRotation = initialRotation + deltaAngle;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, newRotation));
        }
        public override void MouseEnter()
        {
            if (!isMouseOver && isEnchant)
            {
                isMouseOver = true;
                color = Color.blue;
            }
        }
        public override void MouseExit()
        {
            if (isMouseOver && isEnchant)
            {
                isMouseOver = false;
                color = originColor;
            }
        }
        /// <summary>
        /// 마우스 놓으면 외접원 미리보기 생성
        /// </summary>
        protected override void OnDrop()
        {
            ScrollCreationManager.Instance.DropSymbol(this);
        }
        // private void OnDestroy()
        // {

        // }

        #endregion

        #region 외접원 그리기 정보 설정
        /// <summary>
        /// 외접원 그릴때 하위 2개의 심볼을 받아와서 저장해주고, 자녀 심볼들의 정점을 저장
        /// </summary>
        public void SetIncludedSymbols(List<Symbol> includedSymbols)
        {
            base.IncludedSymbols = new List<Symbol>(includedSymbols);
            base.PreviewChildren = new List<Symbol>(includedSymbols);

            // 위치 지정
            SetCenter(includedSymbols);

            // 자녀 그룹들의 미리보기 그룹을 설정해줌
            points = GetIncludedSymbolsVertices(IncludedSymbols);
        }

        public void SetCenter(List<Symbol> includedSymbols)
        {
            List<Vector2> worldPoints = new List<Vector2>();

            foreach (var groupableSymbol in includedSymbols)
            {
                foreach (Vector2 vertex in groupableSymbol.ConvertPointsToWorld(groupableSymbol.Points))
                {
                    worldPoints.Add(vertex);
                }
            }

            transform.position = MinimumEnclosingCircle.FindMinimumEnclosingCircle(worldPoints).center;
        }
        /// <summary>
        /// 외접원을 그리는데 필요한 모든 자식들의 정점리스트를 반환
        /// </summary>
        public List<Vector2> GetIncludedSymbolsVertices(List<Symbol> includedSymbols)
        {
            List<Vector2> results = new List<Vector2>();

            foreach (var groupableSymbol in includedSymbols)
            {
                foreach (Vector2 vertex in groupableSymbol.ConvertPointsToWorld(groupableSymbol.Points))
                {
                    // 최소 외접원의 로컬 좌표로 변환
                    Vector3 localPose = transform.InverseTransformPoint(vertex);

                    results.Add(localPose);
                }
            }
            return results;
        }

        protected void UpdateCollider()
        {
            if (shapeCollider != null && minimumEnclosingCircle.radius > 0)
            {
                points.Clear();
                Vector2 center = minimumEnclosingCircle.center;
                float radius = minimumEnclosingCircle.radius;

                // 원형 콜라이더를 위한 포인트 생성
                float angleStep = 2f * Mathf.PI / segments;
                for (int i = 0; i <= segments; i++) // <= 를 사용하여 원을 완전히 닫음
                {
                    float angle = i * angleStep;
                    Vector2 point = new Vector2(
                        center.x + radius * Mathf.Cos(angle),
                        center.y + radius * Mathf.Sin(angle)
                    );
                    points.Add(point);
                }

                shapeCollider.points = points.ToArray();
            }
        }
        #endregion

        #region 확정 상태

        public override void EnsureDrawing()
        {
            DrawConfirmCircle();
        }

        public override void SetConfirmGroup(Symbol parent, List<Symbol> gropedSymbols)
        {
            isPreview = false;

            if (parent == null)
            {
                return;
            }

            if (!isFixed)
                StartCoroutine(AnimateEnchant());

            GetComponent<RectTransform>().SetParent(parent.GetComponent<RectTransform>());
            TurnOffCollider(); // 콜라이더를 끔
            symbolData.Type = SymbolType.Confirm;
        }

        public override void DestroyObject()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateDestroy());
        }

        #endregion

        #region 외접원 그리기
        private IEnumerator AnimateDestroy()
        {
            float startAlpha = color.a; // 현재 알파값을 시작값으로 사용

            float elapsedTime = 0;
            float duration = 0.5f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;

                // 현재 알파값에서 1까지 부드럽게 보간
                float smoothProgress = Mathf.SmoothStep(startAlpha, 0f, progress);
                color = new Color(originColor.r, originColor.g, originColor.b, smoothProgress);
                SetVerticesDirty();
                yield return null;
            }
            SetMaterialDirty();

            Destroy(gameObject);
        }

        protected void DrawPreviewCircle()
        {
            //StoreChildVertecies();  ScrollCreationManager에서 적용하고 실행
            if (points.Count > 0)
            {
                minimumEnclosingCircle = MinimumEnclosingCircle.FindMinimumEnclosingCircle(points);

                // currentSegment = segments; // 모든 세그먼트를 완성 상태로 설정
                // SetVerticesDirty(); // 즉시 외접원 그래픽을 그리도록 업데이트

                // SetInscribedCircle(minimumEnclosingCircle); 강화, 확장용이라 안씀
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }

                animationCoroutine = StartCoroutine(AnimateCircle());
            }

            //UpdateCollider();

            // 미리보기 외접원의 자식 오브젝트를 LinkedList에서 제거.
            base.SetPreviewGroup(null, IncludedSymbols);
            foreach (var subSymbol in base.IncludedSymbols)
            {
                subSymbol.SetPreviewGroup(this, null);
            }

        }

        protected void DrawConfirmCircle()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateCircle());
        }

        #endregion

        #region 그래픽 그리기  

        private IEnumerator AnimateEnchant()
        {
            float progress = 0;
            float stampDuration = 0.7f;

            float startAlpha = color.a; // 현재 알파값을 시작값으로 사용

            while (progress < 1f)
            {
                progress += Time.deltaTime / stampDuration;
                // 현재 알파값에서 1까지 부드럽게 보간
                float smoothProgress = Mathf.SmoothStep(startAlpha, 1f, progress);
                color = new Color(originColor.r, originColor.g, originColor.b, smoothProgress);

                SetMaterialDirty(); // UI 머티리얼 업데이트를 위해 필요
                yield return null;
            }

            // 스탬프 효과 완료 후 최종 상태 설정
            originColor = new Color(color.r, color.g, color.b, 1f);
            // color = originColor;
            SetMaterialDirty();

            isFixed = true;
        }

        protected IEnumerator AnimateCircle()
        {
            float animationDuration = ScrollCreationManager.Instance.CombinationDuration;

            while (currentSegment < segments)
            {
                currentAnimationTime += Time.deltaTime;
                float overallProgress = currentAnimationTime / animationDuration;

                // 전체 진행도를 기반으로 현재 세그먼트 계산
                currentSegment = Mathf.Min(Mathf.FloorToInt(overallProgress * segments), segments - 1);

                // 현재 세그먼트의 진행도 계산
                float segmentProgress = (overallProgress * segments) - currentSegment;
                segmentProgress = Mathf.Clamp01(segmentProgress);

                SetVerticesDirty();

                // 애니메이션 완료 체크
                if (overallProgress >= 1f)
                {
                    currentSegment = segments;
                    // isAnimating = false;
                    // isDrawingComplete = true;
                    SetVerticesDirty(); // 마지막 업데이트 확실히 하기
                    break;
                }

                yield return null;
            }
            SetMaterialDirty();
            UpdateCollider();
            TurnOnCollider();
            ScrollCreationManager.Instance.confirmCertainSymbol(this);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (points.Count == 0 && minimumEnclosingCircle.Equals(default)) return;

            float inscribedRadius = minimumEnclosingCircle.radius;
            Vector2 center = minimumEnclosingCircle.center;
            int vertexIndex = 0;

            float startAngle = 90;
            float angleStep = 2f * Mathf.PI / segments;
            float currentAngle = startAngle * Mathf.Deg2Rad;

            DrawShape(vh, center, inscribedRadius, ref vertexIndex);

        }

        private void DrawShape(VertexHelper vh, Vector2 center, float inscribedRadius, ref int vertexIndex)
        {
            float startAngle = 90;
            float angleStep = 2f * Mathf.PI / segments;
            float currentAngle = startAngle * Mathf.Deg2Rad;

            for (int i = 0; i < currentSegment; i++)
            {
                float angle1 = currentAngle;
                float angle2 = currentAngle + angleStep;

                Vector2 start = new Vector2(
                    center.x + inscribedRadius * Mathf.Cos(angle1),
                    center.y + inscribedRadius * Mathf.Sin(angle1)
                );

                Vector2 end = new Vector2(
                    center.x + inscribedRadius * Mathf.Cos(angle2),
                    center.y + inscribedRadius * Mathf.Sin(angle2)
                );

                // progress 값을 각 세그먼트의 위치에 따라 계산하여 전달
                float progress = (float)i / segments;
                DrawSegment(vh, start, end, thickness, color, ref vertexIndex, progress);

                currentAngle += angleStep;
            }
        }

        private void DrawSegment(VertexHelper vh, Vector2 start, Vector2 end, float width, Color color, ref int vertexIndex, float progress = 0f)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            Vector2 v1 = start + perpendicular * (width / 2);
            Vector2 v2 = start - perpendicular * (width / 2);
            Vector2 v3 = end - perpendicular * (width / 2);
            Vector2 v4 = end + perpendicular * (width / 2);

            // UV 좌표 계산 추가 (쉐이더의 진행도를 위한 설정)
            Vector2 uv1 = new Vector2(progress, 0);
            Vector2 uv2 = new Vector2(progress, 1);
            Vector2 uv3 = new Vector2(progress + (1.0f / segments), 1);
            Vector2 uv4 = new Vector2(progress + (1.0f / segments), 0);

            vh.AddVert(new Vector3(v1.x, v1.y, 0), color, uv1);
            vh.AddVert(new Vector3(v2.x, v2.y, 0), color, uv2);
            vh.AddVert(new Vector3(v3.x, v3.y, 0), color, uv3);
            vh.AddVert(new Vector3(v4.x, v4.y, 0), color, uv4);

            vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);

            vertexIndex += 4;
        }
        #endregion
    }

}
