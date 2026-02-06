using System;
using System.Collections;
using System.Collections.Generic;
using Game.Desk;
using Game.Scroll;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Symbols
{
    public class IntensityPolygonDrawer : MaskableGraphic
    {
        private struct ShapeInfo
        {
            public List<Vector2> points;
            public List<float> segmentLengths;
            public float totalLength;
        }

        [Header("그래픽 정보")]
        public List<Vector2> points = new List<Vector2>();
        [SerializeField] private bool isDrawn = false;
        private float thickness = 5f;
        private int segments;

        [Tooltip("강도 정보")]
        [SerializeField]
        public int CurrentDegree
        {
            get
            {
                switch (currentShape)
                {
                    case OutShapeType.None:
                        return 0;
                    case OutShapeType.Pentagon:
                        return 1;
                    case OutShapeType.Square:
                        return 2;
                    default:
                        return 0;
                }
            }
        }

        [Header("강도 다각형 정보")]
        private MinimumEnclosingCircle.Circle inscribedCircle;
        private bool isMaxStrength => currentShape == OutShapeType.Square;
        private bool isMinStrength => !isDrawn;
        private float startAngle = 90f; // 시작 각도 (도)
        public OutShapeType currentShape = OutShapeType.None;
        [Header("확장 제어 오브젝트")]
        [SerializeField] private RangePolygonDrawer rangePolygonDrawer;
        public RangePolygonDrawer RangePolygonDrawer
        {
            get => rangePolygonDrawer;
            set => rangePolygonDrawer = value;
        }

        [Header("도형 변환 애니메이션")]
        public bool isWeakening = false;
        [SerializeField] private bool isTransitioning = false;
        private OutShapeType fromShape = OutShapeType.None;
        public OutShapeType targetShape = OutShapeType.None;
        private float transitionProgress = 0f;
        [SerializeField] private float actualTransitionDuration;
        private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("강화/약화 제어")]
        [SerializeField] private bool isTransitionActive = false;

        // [Header("회전 제어")]
        // private Vector2 lastMousePosition; private
        // float rotationThreshold = 25f; // 각도 임계값
        // private float accumulatedRotation = 0f; // 누적 회전량

        private void Update()
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

            //t

            isWeakening = weakening;
            if ((isWeakening && isMinStrength) || (!isWeakening && isMaxStrength))
            {
                return;
            }

    // 강화/약화 사운드 재생 ㄱㄱ
    if (isWeakening)
    {
        switch(CurrentDegree)
        {
            case 2: // 2단계에서 1단계로 ㄱㄱ 
                SoundManager.Instance.PlaySFX("WeakenLV2");
                break;
            case 1: // 1단계에서 0단계로 ㄱㄱ
                SoundManager.Instance.PlaySFX("WeakenLV1");
                break;
        }
    }
    else
    {
        switch(CurrentDegree)
        {
            case 0: // 0단계에서 1단계로 ㄱㄱ
                SoundManager.Instance.PlaySFX("StrengthenLV1");
                break;
            case 1: // 1단계에서 2단계로 ㄱㄱ
                SoundManager.Instance.PlaySFX("StrengthenLV2");
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
                transitionProgress = transitionCurve.Evaluate(normalizedProgress);
                SetVerticesDirty();

                yield return null;
            }
            CompleteTransition();
        }

        private void StartNextTransition()
        {
            // 전환 준비
            fromShape = currentShape;
            targetShape = isWeakening ? GetNextWeakShape(currentShape) : GetNextStrongShape(currentShape);

            // actualTransitionDuration = CalculateTransitionDuration();

            // 전환 시작
            isTransitioning = true;
            transitionProgress = 0f;
            SetVerticesDirty();

            StartCoroutine(UpdateTransition());
            rangePolygonDrawer.StartShapeTransition(fromShape, targetShape, isWeakening);
        }

        private void CompleteTransition()
        {
            currentShape = targetShape;
            isDrawn = currentShape != OutShapeType.None;
            isTransitioning = false;

            SetVerticesDirty();
            ScrollCreationManager.Instance.SetScrollEffect();
            MagicController.Instance.turnOnOtherControlButtons();
        }

        private void RevertTransition()
        {
            currentShape = fromShape;
            isTransitioning = false;
            transitionProgress = 0f;
            SetVerticesDirty();
        }

        public void FinishIntensity()
        {
            isTransitionActive = false;
        }


        #endregion

        public void SetInscribedCircle(OuterCircleDrawer outerCircleDrawer)
        {
            transform.position = outerCircleDrawer.transform.position;
            inscribedCircle = outerCircleDrawer.Circle;
        }

        private ShapeInfo CalculateShapeInfo(OutShapeType shape, Vector2 center, float inscribedRadius)
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

            this.points = points;

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

        private OutShapeType GetNextStrongShape(OutShapeType current)
        {
            switch (current)
            {
                case OutShapeType.None:
                    // return OutShapeType.Hexagon;
                    return OutShapeType.Pentagon;
                // case OutShapeType.Hexagon:
                //     return OutShapeType.Pentagon;
                case OutShapeType.Pentagon:
                    return OutShapeType.Square;
                default:
                    return OutShapeType.Square;
            }
        }

        private OutShapeType GetNextWeakShape(OutShapeType current)
        {
            switch (current)
            {
                case OutShapeType.Square:
                    return OutShapeType.Pentagon;
                case OutShapeType.Pentagon:
                    // return OutShapeType.Hexagon;
                    return OutShapeType.None;
                // case OutShapeType.Hexagon:
                //     return OutShapeType.None;
                default:
                    return OutShapeType.None;
            }
        }

        #region 그래픽 그리기
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (inscribedCircle.radius <= 0) return;

            float inscribedRadius = inscribedCircle.radius;
            Vector2 center = inscribedCircle.center;
            int vertexIndex = 0;

            if (!isTransitioning)
            {
                // 전환 중이 아닐 때는 현재 도형을 완전히 그림
                DrawCurrentShape(vh, center, inscribedRadius, ref vertexIndex);
            }
            else
            {
                DrawTransitionShape(vh, center, inscribedRadius, ref vertexIndex);
            }
        }

        private void DrawCurrentShape(VertexHelper vh, Vector2 center, float inscribedRadius, ref int vertexIndex)
        {
            DrawCompleteShape(vh, currentShape, center, inscribedRadius, ref vertexIndex);
        }

        private void DrawCompleteShape(VertexHelper vh, OutShapeType shape, Vector2 center, float inscribedRadius, ref int vertexIndex)
        {
            // 그려져 있지 않다면, None을 그리고 있어야 함
            if (!isDrawn)
            {
                var shapeInfo = CalculateShapeInfo(OutShapeType.None, center, inscribedRadius);
                for (int i = 0; i < shapeInfo.points.Count - 1; i++)
                {
                    DrawSegment(vh, shapeInfo.points[i], shapeInfo.points[i + 1], thickness, color, ref vertexIndex);
                }
            }
            else
            {
                var shapeInfo = CalculateShapeInfo(shape, center, inscribedRadius);
                for (int i = 0; i < shapeInfo.points.Count - 1; i++)
                {
                    DrawSegment(vh, shapeInfo.points[i], shapeInfo.points[i + 1], thickness, color, ref vertexIndex);
                }
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

        #region 다각형 그리기
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

        // private float CalculateTransitionDuration()
        // {
        //     return (!isDrawn || targetShape == OutShapeType.None) ? ScrollCreationManager.Instance.TransitionDuration : ScrollCreationManager.Instance.TransitionDuration * 2f;
        // }

        private void DrawTransitionShape(VertexHelper vh, Vector2 center, float inscribedRadius, ref int vertexIndex)
        {
            if (!isDrawn)
            {
                DrawGrowingShape(vh, targetShape, center, inscribedRadius, transitionProgress, ref vertexIndex);
            }
            else if (targetShape == OutShapeType.None)
            {
                DrawErasingShape(vh, fromShape, center, inscribedRadius, transitionProgress, ref vertexIndex);
            }
            else
            {
                if (transitionProgress < 0.5f)
                {
                    float eraseProgress = transitionProgress * 2f;
                    DrawErasingShape(vh, fromShape, center, inscribedRadius, eraseProgress, ref vertexIndex);
                }
                else
                {
                    float drawProgress = (transitionProgress - 0.5f) * 2f;
                    DrawGrowingShape(vh, targetShape, center, inscribedRadius, drawProgress, ref vertexIndex);
                }
            }
        }

        private void DrawErasingShape(VertexHelper vh, OutShapeType shape, Vector2 center, float inscribedRadius, float progress, ref int vertexIndex)
        {
            if (!isWeakening)
            {
                var shapeInfo = CalculateShapeInfo(shape, center, inscribedRadius);
                float remainingLength = shapeInfo.totalLength * (1 - progress);

                // 시계 방향으로 지우기 위해 역순 정렬
                List<Vector2> reversedPoints = new List<Vector2>(shapeInfo.points);
                List<float> reversedSegmentLengths = new List<float>(shapeInfo.segmentLengths);
                reversedPoints.Reverse();
                reversedSegmentLengths.Reverse();

                DrawPartialShape(vh, reversedPoints, reversedSegmentLengths, remainingLength, ref vertexIndex);
            }
            else
            {
                var shapeInfo = CalculateShapeInfo(shape, center, inscribedRadius);
                float remainingLength = shapeInfo.totalLength * (1 - progress);

                DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, remainingLength, ref vertexIndex);
            }
        }

        private void DrawGrowingShape(VertexHelper vh, OutShapeType shape, Vector2 center, float inscribedRadius, float progress, ref int vertexIndex)
        {
            if (!isWeakening)
            {
                var shapeInfo = CalculateShapeInfo(shape, center, inscribedRadius);
                float targetLength = shapeInfo.totalLength * progress;
                DrawPartialShape(vh, shapeInfo.points, shapeInfo.segmentLengths, targetLength, ref vertexIndex);
            }
            else
            {
                var shapeInfo = CalculateShapeInfo(shape, center, inscribedRadius);
                float targetLength = shapeInfo.totalLength * progress;

                // 반시계 방향으로 지우기 위해 역순 정렬
                List<Vector2> reversedPoints = new List<Vector2>(shapeInfo.points);
                List<float> reversedSegmentLengths = new List<float>(shapeInfo.segmentLengths);
                reversedPoints.Reverse();
                reversedSegmentLengths.Reverse();

                DrawPartialShape(vh, reversedPoints, reversedSegmentLengths, targetLength, ref vertexIndex);
            }
        }

        private void DrawPartialShape(VertexHelper vh, List<Vector2> fullPoints, List<float> segmentLengths, float targetLength, ref int vertexIndex)
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
        #endregion
    }
}