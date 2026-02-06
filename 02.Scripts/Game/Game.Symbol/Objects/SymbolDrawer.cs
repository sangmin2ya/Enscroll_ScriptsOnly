using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Element;
using Game.Scroll;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Symbols
{
    public class SymbolDrawer : Symbol
    {
        #region 심볼 그리기 정보

        [Header("모양 정보")]
        private float standardLength = 45f;
        private float thickness = 5f;
        [SerializeField] protected int segments;
        private ShapeType shape;
        //public List<Vector2> points = new List<Vector2>();

        [Header("드로우 애니메이션 설정")]
        protected float animationDuration = 0.5f;
        protected bool isAnimating = false;
        protected int currentSegment = 0;
        protected float currentAnimationTime = 0f;
        protected bool isDrawingComplete = false;

        [Header("드래그 및 마우스 처리")]
        private bool isMouseOver = false;
        private Vector3 prevMousePosition;
        private float initialRotation; // 초기 회전 각도
        private float initialMouseAngle; // 마우스를 처음 눌렀을 때의 각도
        private bool isDragging = false;
        private bool isRotating = false;

        #endregion

        #region 데이터 초기화

        public override void Init(int uniqueId, SymbolData symbolData)
        {
            this.uniqueId = uniqueId;
            this.symbolData = symbolData;
            this.originColor = color;

            //다양한 도형을 그리기 위한 정보 설정
            SetSegmentsShapeForSymbol(symbolData.Diagram);
            CalculatePoints();
            UpdateCollider();

            StartDrawAnimation();
        }

        #endregion

        #region 마우스 이벤트 및 이동, 회전

        void Update()
        {
            // 미리보기 상태일 때만 마우스 이벤트 처리
            if (isPreview)
            {
                // Collider로 처리할거라 OnDrag 쓰지 않기 + OnMosueDrag는 우클릭 감지 불가 => Input으로 처리하기
                // 좌클릭
                if (isMouseOver && Input.GetMouseButtonDown(0) && !isDragging)
                {
                    // ChangeMovingMaterial();

                    isDragging = true;
                    whileDragging = true;
                    prevMousePosition = Input.mousePosition;
                    transform.SetAsLastSibling();

                    // linkedSymbolsLists에서 이 객체가 포함된 그룹을 제거하고 갱신
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

                if (Input.GetMouseButtonUp(0) && isDragging)
                {
                    // ChangePreveiwMaterial();

                    isDragging = false;
                    whileDragging = false;
                    IsMovable = false;
                    // linkedSymbolsLists에서 이 객체가 포함된 그룹을 포함해서 갱신
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

        protected override void OnDestroy()
        {
        }
        /// <summary>
        /// 마우스 놓으면 외접원 미리보기 생성
        /// </summary>
        protected override void OnDrop()
        {
            ScrollCreationManager.Instance.DropSymbol(this);
        }

        public override void MouseEnter()
        {
            if (!isMouseOver)
            {
                isMouseOver = true;
                color = Color.blue;
            }
        }

        public override void MouseExit()
        {
            if (isMouseOver)
            {
                isMouseOver = false;
                color = originColor;
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

        public void ShowWarning()
        {
            // 외곽선 색상 변경
            color = Color.red;
            TurnOffCollider();
            DOTween.Sequence()
                .Append(rectTransform.DOShakePosition(0.5f, 10f, 20, 90f, false, true))
                .AppendCallback(() => color = originColor).OnComplete(TurnOnCollider);
        }

        #endregion

        #region 외접원 그리기 정보 설정

        /// <summary>
        /// 엄밀히 말하면 정보설정은 아니고 콜라이더 설정해주는얘
        /// </summary>
        private void UpdateCollider()
        {
            if (shapeCollider != null && points.Count > 0)
            {
                // 직접 points를 콜라이더에 적용
                List<Vector2> colliderPoints = new List<Vector2>(points);

                // 도형이 열려있다면 닫아줌
                if (colliderPoints[0] != colliderPoints[colliderPoints.Count - 1])
                {
                    colliderPoints.Add(colliderPoints[0]);
                }

                shapeCollider.points = colliderPoints.ToArray();
            }
        }
        /// <summary>
        /// 심볼의 모양에 따라 세그먼트 수와 도형 모양을 설정
        /// </summary>
        private void SetSegmentsShapeForSymbol(int symbolIndex)
        {
            switch (symbolIndex)
            {
                // 불 - 삼각형
                case 0:
                    shape = ShapeType.Triangle;
                    segments = 3;
                    break;

                // 물 - 원
                case 1:
                    shape = ShapeType.Circle;
                    segments = 36;
                    break;

                // 공기 - 긴 다이아몬드
                case 2:
                    shape = ShapeType.Diamond;
                    segments = 4;
                    break;

                // 흙 - 사각형
                case 3:
                    shape = ShapeType.Square;
                    segments = 4;
                    break;

                default:
                    Debug.Log("이상한 심볼");
                    break;
            }
        }
        /// <summary>
        /// 각 도형의 정점들을 계산해서 세팅해두기
        /// </summary>
        private void CalculatePoints()
        {
            points.Clear();
            Vector2 pivot = rectTransform.pivot;
            float centerX = -rectTransform.rect.width * (pivot.x - 0.5f);
            float centerY = -rectTransform.rect.height * (pivot.y - 0.5f);

            switch (shape)
            {
                case ShapeType.Circle:
                    points = SymbolGenerateFuncs.CalculateCirclePoints(centerX, centerY, standardLength, segments);
                    break;

                case ShapeType.Triangle:
                    points = SymbolGenerateFuncs.CalculateTrianglePoints(centerX, centerY, standardLength, segments);
                    break;

                case ShapeType.Square:
                    points = SymbolGenerateFuncs.CalculateSquarePoints(centerX, centerY, standardLength, segments);
                    break;

                case ShapeType.Diamond:
                    // 다이아몬드는 standardLength 줄이기
                    points = SymbolGenerateFuncs.CalculateDiamondPoints(centerX, centerY, standardLength, segments);
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region 확정 상태

        public override void EnsureDrawing()
        {
            // 초기화할 때 애니메이션 상태 리셋
            StartCoroutine(AnimateConfirmDrawing());
        }

        public override void SetConfirmGroup(Symbol parent, List<Symbol> gropedSymbols)
        {
            isPreview = false;

            StartCoroutine(AnimateEnchant());

            if (parent == null)
            {
                return;
            }

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
            color = originColor;
            SetMaterialDirty();
        }

        protected virtual IEnumerator AnimateConfirmDrawing()
        {
            float startAlpha = 0; // 현재 알파값을 시작값으로 사용

            float elapsedTime = 0;
            float duration = 1f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;

                // 현재 알파값에서 1까지 부드럽게 보간
                float smoothProgress = Mathf.SmoothStep(startAlpha, 1f, progress);
                color = new Color(originColor.r, originColor.g, originColor.b, smoothProgress);
                SetVerticesDirty();
                yield return null;
            }

            SetMaterialDirty();
        }

        public void StartDrawAnimation()
        {
            // 초기화할 때 애니메이션 상태 리셋
            isAnimating = false;
            isDrawingComplete = false;
            currentAnimationTime = 0f;

            if (!isAnimating)
            {
                currentSegment = 0;
                isAnimating = true;
                StartCoroutine(AnimateDrawing());
            }
        }

        protected virtual IEnumerator AnimateDrawing()
        {
            currentAnimationTime = 0f;

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
                    currentSegment = segments - 1;
                    isAnimating = false;
                    isDrawingComplete = true;
                    SetVerticesDirty(); // 마지막 업데이트 확실히 하기
                    break;
                }

                yield return null;
            }
            TurnOnCollider();
        }
        #endregion

        #region 그래픽 그리기

        private void DrawSegment(VertexHelper vh, int segmentIndex, float progress)
        {
            Vector2 point1 = points[segmentIndex];
            Vector2 point2 = points[segmentIndex + 1];

            // 진행 중인 선분의 경우 부분적으로만 그리기
            if (progress < 1f)
            {
                point2 = Vector2.Lerp(point1, point2, progress);
            }

            Vector2 dir = (point2 - point1).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

            Vector2 v1 = point1 + normal;
            Vector2 v2 = point1 - normal;
            Vector2 v3 = point2 + normal;
            Vector2 v4 = point2 - normal;

            Vector2 uv1 = new Vector2(segmentIndex / (float)segments, 0);
            Vector2 uv2 = new Vector2(segmentIndex / (float)segments, 1);
            Vector2 uv3 = new Vector2((segmentIndex + progress) / (float)segments, 0);
            Vector2 uv4 = new Vector2((segmentIndex + progress) / (float)segments, 1);

            int vertCount = vh.currentVertCount;

            vh.AddVert(v1, color, uv1);
            vh.AddVert(v2, color, uv2);
            vh.AddVert(v3, color, uv3);
            vh.AddVert(v4, color, uv4);

            vh.AddTriangle(vertCount + 0, vertCount + 1, vertCount + 2);
            vh.AddTriangle(vertCount + 1, vertCount + 3, vertCount + 2);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            // 아무것도 그리지 않아야 할 때
            if (!isAnimating && currentSegment == 0 && !isDrawingComplete)
                return;

            // 완료된 세그먼트들 그리기
            for (int i = 0; i < currentSegment; i++)
            {
                DrawSegment(vh, i, 1f);
            }

            // 현재 진행 중인 세그먼트 그리기
            if (isAnimating && currentSegment < segments)
            {
                float totalProgress = currentAnimationTime / animationDuration;
                float segmentProgress = (totalProgress * segments) % 1f;
                DrawSegment(vh, currentSegment, segmentProgress);
            }
            // 완료된 상태에서는 마지막 세그먼트까지 완전히 그리기
            else if (isDrawingComplete)
            {
                DrawSegment(vh, segments - 1, 1f);
            }

        }
        #endregion

    }
}