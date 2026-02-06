using System.Collections.Generic;
using Game.Scroll;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Game.Desk;

namespace Game.Symbols
{
    public class RangePolygonDrawer : MaskableGraphic
    {
        private struct ShapeInfo
        {
            public List<Vector2> points;
            public List<float> segmentLengths;
            public float totalLength;
        }

        [Header("그래픽 정보")]
        public List<Vector2> lastPoints = new List<Vector2>();
        [SerializeField] private float[] radiusOffsets = new float[2];
        [SerializeField] private float[] thicknessess = new float[2];
        [SerializeField] private float[] angles = new float[2];
        private int segments = 32;

        [Header("범위 정보")]
        [SerializeField] private int currentDegree = 0;
        public int CurrentDegree
        {
            get { return currentDegree; }
        }
        [SerializeField] bool isWeakening = false;
        private int fromDegree = 0;
        private int toDegree = 0;

        [Header("범위 다각형 정보")]
        private ShapeInfo currentShapeInfo;
        private MinimumEnclosingCircle.Circle inscribedCircle;
        public OutShapeType currentShape = OutShapeType.None;
        public OutShapeType previousShape = OutShapeType.None;
        private bool isMaxStrength => currentDegree == 2;
        private bool isMinStrength => currentDegree == 0;
        // private float startAngle = 90f; // 시작 각도 (도)

        [Header("애니메이션 설정")]
        [SerializeField] private float actualTransitionDuration;
        private float animationProgress = 0f;
        [SerializeField] private bool isTransitioning = false;
        private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("확대/축소 제어")]
        [SerializeField] private bool isTransitionActive = false;

        // [Header("회전 제어")]
        // private Vector2 lastMousePosition;
        // float rotationThreshold = 25f; // 각도 임계값
        // private float accumulatedRotation = 0f; // 누적 회전량

        [Header("강도에 따른 다각형 변환")]
        private OutShapeType transitionFromShape;
        private OutShapeType transitionTargetShape;
        private bool isShapeTransitioning = false;
        private bool isShapeWeakening => ScrollCreationManager.Instance.IntensityPolygonDrawer.isWeakening;

        void Update()
        {
            // if (isTransitionActive)
            // {
            //     if (Input.GetMouseButtonDown(0))
            //     {
            //         lastMousePosition = Input.mousePosition;
            //     }
            //     else if (Input.GetMouseButton(0))
            //     {
            //         Vector2 currentMousePosition = Input.mousePosition;
            //         MagicController.Instance.ReinforceController.ReinforceEffect(Camera.main.ScreenToWorldPoint(currentMousePosition));

            //         if (Vector2.Distance(lastMousePosition, currentMousePosition) > 0.3f)
            //         {
            //             Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            //             Vector2 lastDirection = (lastMousePosition - screenCenter).normalized;
            //             Vector2 currentDirection = (currentMousePosition - screenCenter).normalized;

            //             float rotationDelta = Vector2.SignedAngle(lastDirection, currentDirection);
            //             accumulatedRotation += rotationDelta;

            //             // 현재 진행 중인 전환이 있다면 방향을 제한
            //             if (isTransitioning)
            //             {
            //                 // 지우는 중일 때는 시계방향만, 그리는 중일 때는 반시계방향만 허용
            //                 bool isValidRotation = (isWeakening && rotationDelta < 0) || (!isWeakening && rotationDelta > 0);
            //                 if (!isValidRotation)
            //                 {
            //                     lastMousePosition = currentMousePosition;
            //                     return;
            //                 }
            //                 UpdateTransition();
            //             }
            //             else if (Mathf.Abs(accumulatedRotation) >= rotationThreshold)
            //             {
            //                 // 임계값 초과시에만 전환 처리
            //                 if (accumulatedRotation > 0 && !isMaxStrength)
            //                 {
            //                     if (!isTransitioning)
            //                     {
            //                         StartNextTransition(false);
            //                     }
            //                 }
            //                 else if (accumulatedRotation < 0 && !isMinStrength)
            //                 {
            //                     if (!isTransitioning)
            //                     {
            //                         StartNextTransition(true);
            //                     }
            //                 }

            //                 accumulatedRotation = 0f;
            //             }
            //         }

            //         lastMousePosition = currentMousePosition;
            //     }
            //     else if (Input.GetMouseButtonUp(0)) // 마우스 버튼을 떼면
            //     {
            //         StopTransition();
            //         MagicController.Instance.ReinforceController.EndReinforceEffect();
            //     }
            // }
        }

        #region 마우스 회전 그리기
        public void StartTransition(bool weakening)
        {
            if (isTransitioning) return;

            isWeakening = weakening;
            if ((isWeakening && isMinStrength) || (!isWeakening && isMaxStrength))
            {
                return;
            }
    // 확장/축소 사운드 재생 ㄱㄱ
    if (isWeakening)
    {
        switch(currentDegree)
        {
            case 2: // 2단계에서 1단계로 ㄱㄱ
                SoundManager.Instance.PlaySFX("ReduceLV2");
                break;
            case 1: // 1단계에서 0단계로 ㄱㄱ
                SoundManager.Instance.PlaySFX("ReduceLV1");
                break;
        }
    }
    else
    {
        switch(currentDegree)
        {
            case 0: // 0단계에서 1단계로 ㄱㄱ 
                SoundManager.Instance.PlaySFX("ExpandLV1");
                break;
            case 1: // 1단계에서 2단계로 ㄱㄱㄱㄱㄱㄱ
                SoundManager.Instance.PlaySFX("ExpandLV2");
                break;
        }
    }
            isTransitionActive = true;
            StartNextTransition();
        }

        public void StopTransition()
        {
            if (isTransitionActive)
            {
                isTransitionActive = false;
                if (isTransitioning)
                {
                    RevertTransition();
                }
            }
        }

        public IEnumerator UpdateTransition()
        {
            float normalizedProgress = 0f;
            while (normalizedProgress < 1f)
            {
                normalizedProgress += Time.deltaTime / ScrollCreationManager.Instance.TransitionDuration;
                animationProgress = transitionCurve.Evaluate(normalizedProgress); ;
                SetVerticesDirty();

                yield return null;
            }
            CompleteTransition();
        }

        private void StartNextTransition()
        {
            GetShapeOfIntensity();

            isTransitioning = true;
            fromDegree = currentDegree;
            toDegree = isWeakening ? currentDegree - 1 : currentDegree + 1;
            animationProgress = 0;
            SetVerticesDirty();

            StartCoroutine(UpdateTransition());
        }

        private void CompleteTransition()
        {
            currentDegree = toDegree;
            isTransitioning = false;
            SetVerticesDirty();
            ScrollCreationManager.Instance.SetScrollDuration();
            MagicController.Instance.turnOnOtherControlButtons();
        }

        private void RevertTransition()
        {
            currentDegree = fromDegree;
            isTransitioning = false;
            animationProgress = 0f;
            SetVerticesDirty();
        }

        public void FinishRange()
        {
            isTransitionActive = false;
        }

        #endregion

        #region 강화에 따른 도형 변화

        /// <summary>
        /// 강화된 모양으로 외부 도형 변화
        /// </summary>
        public void StartShapeTransition(OutShapeType fromShape, OutShapeType targetShape, bool weakening)
        {
            if (isShapeTransitioning) return;

            transitionFromShape = fromShape;
            transitionTargetShape = targetShape;
            isWeakening = weakening;
            isShapeTransitioning = true;
            previousShape = currentShape;
            currentShape = fromShape; // 현재 모양을 시작 모양으로 설정

            StartNextShapeTransition();
        }

        /// <summary>
        /// 강화된 모양으로 외부 도형 변화
        /// </summary>
        private void StartNextShapeTransition()
        {
            isShapeTransitioning = true;
            animationProgress = 0;

            // actualTransitionDuration = CalculateTransitionDuration();

            SetVerticesDirty();

            StartCoroutine(UpdateShapeTransition());
        }

        public IEnumerator UpdateShapeTransition()
        {
            float normalizedProgress = 0f;
            while (normalizedProgress < 1f)
            {
                normalizedProgress += Time.deltaTime / ScrollCreationManager.Instance.TransitionDuration;
                animationProgress = transitionCurve.Evaluate(normalizedProgress); ;
                SetVerticesDirty();

                yield return null;
            }
            CompleteShapeTransition();
        }

        private void CompleteShapeTransition()
        {
            currentShape = transitionTargetShape;
            isShapeTransitioning = false;
            SetVerticesDirty();
        }

        // private float CalculateTransitionDuration()
        // {
        //     return (transitionTargetShape == OutShapeType.None) ? ScrollCreationManager.Instance.TransitionDuration : ScrollCreationManager.Instance.TransitionDuration * 2f;
        // }

        #endregion

        public void DestroyObject()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateDestroy());
        }

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
                color = new Color(color.r, color.g, color.b, smoothProgress);
                SetVerticesDirty();
                yield return null;
            }
            SetMaterialDirty();

            Destroy(gameObject);
        }


        #region 모양 얻기
        public void SetInscribedCircle(OuterCircleDrawer outerCircleDrawer)
        {
            transform.position = outerCircleDrawer.transform.position;
            inscribedCircle = outerCircleDrawer.Circle;
        }

        private void GetShapeOfIntensity()
        {
            currentShape = ScrollCreationManager.Instance.GetShapeTypeOfIntensityDrawer();
            if (currentShape == OutShapeType.None)
                currentShape = OutShapeType.Circle;

            // angles[0] = GetStartAngleFromShape();
        }


        private ShapeInfo CalculateShapeInfo(OutShapeType shape, Vector2 center, float inscribedRadius, float startAngle)
        {
            float radius = GetCircumscribedRadius(inscribedRadius, shape);
            int sides = GetSidesFromShape(shape);
            float angleStep = 2f * Mathf.PI / sides;
            float startRadian = startAngle * Mathf.Deg2Rad;

            var points = new List<Vector2>();
            var segmentLengths = new List<float>();
            float totalLength = 0f;

            for (int i = 0; i <= sides; i++)
            {
                float angle = startRadian + angleStep * i;
                Vector2 point = new Vector2(
                    center.x + radius * Mathf.Cos(angle),
                    center.y + radius * Mathf.Sin(angle)
                );
                points.Add(point);

                if (i > 0)
                {
                    float length = Vector2.Distance(points[i - 1], point);
                    segmentLengths.Add(length);
                    totalLength += length;
                }
            }

            this.lastPoints = points;

            return new ShapeInfo
            {
                points = points,
                segmentLengths = segmentLengths,
                totalLength = totalLength
            };
        }

        private float GetCircumscribedRadius(float inscribedRadius, OutShapeType shape)
        {
            switch (shape)
            {
                case OutShapeType.Square:
                    return inscribedRadius * Mathf.Sqrt(2); // 45도
                case OutShapeType.Pentagon:
                    return inscribedRadius / Mathf.Cos(Mathf.PI / 5); // 36도
                case OutShapeType.Hexagon:
                    return inscribedRadius / Mathf.Cos(Mathf.PI / 6); // 30도
                default:
                    return inscribedRadius;
            }
        }

        private int GetSidesFromShape(OutShapeType shape)
        {
            switch (shape)
            {
                case OutShapeType.Hexagon:
                    return 6;
                case OutShapeType.Pentagon:
                    return 5;
                case OutShapeType.Square:
                    return 4;
                default:
                    return segments;
            }
        }
        #endregion

        #region 그래픽 그리기

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float inscribedRadius = inscribedCircle.radius;
            Vector2 inscribedCenter = inscribedCircle.center;
            int vertexIndex = 0;

            if (!isTransitioning && !isShapeTransitioning)
            {
                DrawCurrentShape(vh, inscribedCenter, inscribedRadius, ref vertexIndex);
            }
            else if (isShapeTransitioning)
            {
                // 도형 변환 애니메이션
                DrawShapeTransition(vh, inscribedCenter, inscribedRadius, ref vertexIndex);
            }
            else
            {
                // 기존의 확장/축소 애니메이션
                if (!isWeakening)
                {
                    DrawGrowingShape(vh, inscribedCenter, inscribedRadius, ref vertexIndex);
                }
                else
                {
                    DrawErasingShape(vh, inscribedCenter, inscribedRadius, ref vertexIndex);
                }
            }
        }

        private void DrawShapeTransition(VertexHelper vh, Vector2 center, float radius, ref int vertexIndex)
        {
            if (animationProgress < 0.5f)
            {
                DrawShapeErasing(vh, center, radius, animationProgress * 2, ref vertexIndex);
            }
            else
            {
                DrawShapeGrowing(vh, center, radius, (animationProgress - 0.5f) * 2, ref vertexIndex);
            }
        }

        private void DrawShapeErasing(VertexHelper vh, Vector2 center, float radius, float progress, ref int vertexIndex)
        {
            for (int i = 0; i < currentDegree; i++)
            {
                var shapeInfo = CalculateShapeInfo(transitionFromShape, center, radius + radiusOffsets[i], angles[i]);
                float targetLength = shapeInfo.totalLength * (1 - progress);

                if (!isShapeWeakening)
                {
                    // 시계 방향으로 지우기 위해 역순 정렬
                    List<Vector2> reversedPoints = new List<Vector2>(shapeInfo.points);
                    List<float> reversedSegmentLengths = new List<float>(shapeInfo.segmentLengths);
                    reversedPoints.Reverse();
                    reversedSegmentLengths.Reverse();

                    DrawPartialShape(vh, reversedPoints, reversedSegmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                }
                else
                {
                    DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                }
            }
        }

        private void DrawShapeGrowing(VertexHelper vh, Vector2 center, float radius, float progress, ref int vertexIndex)
        {
            for (int i = 0; i < currentDegree; i++)
            {
                var shapeInfo = CalculateShapeInfo(transitionTargetShape, center, radius + radiusOffsets[i], angles[i]);
                float targetLength = shapeInfo.totalLength * progress;

                if (!isShapeWeakening)
                {
                    DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                }
                else
                {
                    // 반시계 방향으로 그리기 위해 역순 정렬
                    List<Vector2> reversedPoints = new List<Vector2>(shapeInfo.points);
                    List<float> reversedSegmentLengths = new List<float>(shapeInfo.segmentLengths);
                    reversedPoints.Reverse();
                    reversedSegmentLengths.Reverse();

                    DrawPartialShape(vh, reversedPoints, reversedSegmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                }
            }
        }

        private void DrawCurrentShape(VertexHelper vh, Vector2 center, float radius, ref int vertexIndex)
        {
            if (currentDegree == 1)
            {
                DrawShape(vh, center, radius + radiusOffsets[0], thicknessess[0], ref vertexIndex, angles[0]);
            }
            else if (currentDegree == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    DrawShape(vh, center, radius + radiusOffsets[i], thicknessess[i], ref vertexIndex, angles[i]);
                }
            }
        }

        private void DrawShape(VertexHelper vh, Vector2 center, float radius, float thickness, ref int vertexIndex, float startAngle)
        {
            var shapeInfo = CalculateShapeInfo(currentShape, center, radius, startAngle);
            var points = shapeInfo.points;
            // int vertexCount = vh.currentVertCount;

            // 외곽선 그리기
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[i + 1];

                DrawSegment(vh, current, next, thickness, color, ref vertexIndex);
            }
        }

        private void DrawGrowingShape(VertexHelper vh, Vector2 center, float radius, ref int vertexIndex)
        {
            if (fromDegree == 0 && toDegree == 1)
            {
                // 첫 번째 원이 생성되는 애니메이션
                var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[0], angles[0]);
                float targetLength = shapeInfo.totalLength * animationProgress;
                DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[0], ref vertexIndex);
            }
            else if (fromDegree == 1 && toDegree == 2)
            {
                // 첫 번째 원은 완전히 그리기
                DrawShape(vh, center, radius + radiusOffsets[0], thicknessess[0], ref vertexIndex, angles[0]);

                // 두 번째 원이 생성되는 애니메이션
                var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[1], angles[1]);
                float targetLength = shapeInfo.totalLength * animationProgress;
                DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[1], ref vertexIndex);
            }
            // 지우고 다시 그리는 애니메이션
            else if (fromDegree == toDegree)
            {
                for (int i = 0; i < fromDegree; i++)
                {
                    if (animationProgress < 0.5f)
                    {
                        var shapeInfo = CalculateShapeInfo(previousShape, center, radius + radiusOffsets[i], angles[i]);
                        float targetLength = shapeInfo.totalLength * (1 - (animationProgress * 2));
                        DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                    }
                    else
                    {
                        var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[i], angles[i]);
                        float targetLength = shapeInfo.totalLength * (animationProgress - 0.5f) * 2;
                        DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                    }
                }
            }
        }

        private void DrawErasingShape(VertexHelper vh, Vector2 center, float radius, ref int vertexIndex)
        {
            if (fromDegree == 2 && toDegree == 1)
            {
                // 첫 번째 원은 완전히 그리기
                DrawShape(vh, center, radius + radiusOffsets[0], thicknessess[0], ref vertexIndex, angles[0]);

                // 두 번째 원이 사라지는 애니메이션
                var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[1], angles[1]);
                float targetLength = shapeInfo.totalLength * (1 - animationProgress);
                DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[1], ref vertexIndex);
            }
            else if (fromDegree == 1 && toDegree == 0)
            {
                // 첫 번째 원이 사라지는 애니메이션
                var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[0], angles[0]);
                float targetLength = shapeInfo.totalLength * (1 - animationProgress);
                DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[0], ref vertexIndex);
            }
            // 지우고 다시 그리는 애니메이션
            else if (fromDegree == toDegree)
            {
                if (fromDegree == 1)
                {
                    var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[0], angles[0]);
                    float targetLength = shapeInfo.totalLength * (1 - animationProgress);
                    DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[0], ref vertexIndex);
                }
                else if (fromDegree == 2)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        var shapeInfo = CalculateShapeInfo(currentShape, center, radius + radiusOffsets[i], angles[i]);
                        float targetLength = shapeInfo.totalLength * (1 - animationProgress);
                        DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, thicknessess[i], ref vertexIndex);
                    }
                }
            }
        }

        private void DrawPartialShape(VertexHelper vh, List<Vector2> fullPoints, List<float> segmentLengths, float targetLength, float thickness, ref int vertexIndex)
        {
            float currentLength = 0;
            List<Vector2> visiblePoints = new List<Vector2>();

            for (int i = 0; i < segmentLengths.Count; i++)
            {
                if (currentLength <= targetLength)
                {
                    visiblePoints.Add(fullPoints[i]);

                    float segmentProgress = (targetLength - currentLength) / segmentLengths[i];
                    if (segmentProgress < 1)
                    {
                        Vector2 direction = fullPoints[i + 1] - fullPoints[i];
                        Vector2 lastPoint = fullPoints[i] + direction * segmentProgress;
                        visiblePoints.Add(lastPoint);
                        break;
                    }
                }
                currentLength += segmentLengths[i];
            }

            for (int i = 0; i < visiblePoints.Count - 1; i++)
            {
                DrawSegment(vh, visiblePoints[i], visiblePoints[i + 1], thickness, color, ref vertexIndex);
            }
        }
        private void DrawSegment(VertexHelper vh, Vector2 start, Vector2 end, float width, Color color, ref int vertexIndex)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            Vector2 v1 = start + perpendicular * (width / 2);
            Vector2 v2 = start - perpendicular * (width / 2);
            Vector2 v3 = end - perpendicular * (width / 2);
            Vector2 v4 = end + perpendicular * (width / 2);

            // UV 좌표 계산 - 선분의 방향을 따라 매핑
            float segmentLength = Vector2.Distance(start, end);
            Vector2 uv1 = new Vector2(0, 0);
            Vector2 uv2 = new Vector2(0, 1);
            Vector2 uv3 = new Vector2(segmentLength / width, 1);
            Vector2 uv4 = new Vector2(segmentLength / width, 0);

            vh.AddVert(v1, color, uv1);
            vh.AddVert(v2, color, uv2);
            vh.AddVert(v3, color, uv3);
            vh.AddVert(v4, color, uv4);

            vh.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);

            vertexIndex += 4;
        }
        #endregion
    }
}