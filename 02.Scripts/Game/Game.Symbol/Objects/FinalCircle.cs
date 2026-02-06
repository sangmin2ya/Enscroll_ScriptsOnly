using System.Collections;
using System.Collections.Generic;
using Game.Symbols;
using UnityEngine;
using UnityEngine.UI;

public class FinalCircle : MaskableGraphic
{
    [Header("모양 정보")]
    private float thickness = 5f;
    private int segments = 48; //원은 불러오지않고 고정

    [Header("최소 외접원")]
    private int currentSegment = 0;
    protected MinimumEnclosingCircle.Circle minimumEnclosingCircle;
    protected List<Vector2> points = new List<Vector2>();

    [Header("드로우 애니메이션 설정")]
    protected float currentAnimationTime = 0f;
    private Coroutine animationCoroutine;

    public void Init(List<Vector2> points)
    {
        currentSegment = 0;

        this.points = points;
        minimumEnclosingCircle = MinimumEnclosingCircle.FindMinimumEnclosingCircle(points);

        // DrawCircle();
    }

    public void SetCircle(List<Vector2> points)
    {
        this.points = points;
        minimumEnclosingCircle = MinimumEnclosingCircle.FindMinimumEnclosingCircle(points);

        currentSegment = 0;
    }

    public MinimumEnclosingCircle.Circle GetCircle()
    {
        return minimumEnclosingCircle;
    }


    #region 원그리기

    public void DrawCircle()
    {

        if (points.Count > 0)
        {
            minimumEnclosingCircle = MinimumEnclosingCircle.FindMinimumEnclosingCircle(points);

            // SetInscribedCircle(minimumEnclosingCircle); 강화, 확장용이라 안씀
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateCircle());
        }
    }

    public void ClearCirclePoints()
    {
        currentSegment = 0;
        currentAnimationTime = 0f;  // currentAnimationTime도 초기화
        points.Clear();
        SetVerticesDirty();
    }

    protected IEnumerator AnimateCircle()
    {
        while (currentSegment < segments)
        {
            currentAnimationTime += Time.deltaTime;
            float overallProgress = currentAnimationTime / 1f;

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
    }
    #endregion

    #region 그래픽 그리기
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count == 0) return;

        float inscribedRadius = minimumEnclosingCircle.radius;
        Vector2 center = minimumEnclosingCircle.center;
        int vertexIndex = 0;

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
