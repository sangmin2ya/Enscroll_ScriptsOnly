using System.Collections.Generic;
using UnityEngine;

public enum ShapeType
{
    Circle,
    Triangle,
    Square,
    Diamond
}

namespace Game.Symbols
{
    public class SymbolGenerateFuncs
    {
        public static List<Vector2> CalculateCirclePoints(float centerX, float centerY, float radius, int segments)
        {
            List<Vector2> points = new List<Vector2>();

            float anglePerSegment = 2f * Mathf.PI / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * anglePerSegment;
                Vector2 point = new Vector2(
                    centerX + radius * Mathf.Cos(angle),
                    centerY + radius * Mathf.Sin(angle)
                );
                points.Add(point);
            }
            return points;
        }

        public static List<Vector2> CalculateTrianglePoints(float centerX, float centerY, float radius, int segments)
        {
            List<Vector2> points = new List<Vector2>();

            float angleOffset = Mathf.PI / 2;
            for (int i = 0; i <= 3; i++)
            {
                float angle = (i * 2f * Mathf.PI / 3) + angleOffset;
                Vector2 point = new Vector2(
                    centerX + radius * Mathf.Cos(angle),
                    centerY + radius * Mathf.Sin(angle)
                );
                points.Add(point);
            }

            return points;
        }

        public static List<Vector2> CalculateSquarePoints(float centerX, float centerY, float radius, int segments)
        {
            List<Vector2> points = new List<Vector2>();

            float size = radius * Mathf.Sqrt(2);
            points.Add(new Vector2(centerX - size / 2, centerY - size / 2));
            points.Add(new Vector2(centerX + size / 2, centerY - size / 2));
            points.Add(new Vector2(centerX + size / 2, centerY + size / 2));
            points.Add(new Vector2(centerX - size / 2, centerY + size / 2));
            points.Add(new Vector2(centerX - size / 2, centerY - size / 2));

            return points;
        }

        public static List<Vector2> CalculateDiamondPoints(float centerX, float centerY, float radius, int segments)
        {
            List<Vector2> points = new List<Vector2>();

            float diamondVerticalRadius = radius;
            float diamondHorizontalRadius = radius * 0.5f;

            // 세로로 늘어난 마름모 생성
            points.Add(new Vector2(centerX, centerY + diamondVerticalRadius));  // 상단
            points.Add(new Vector2(centerX + diamondHorizontalRadius, centerY));                  // 우측
            points.Add(new Vector2(centerX, centerY - diamondVerticalRadius));  // 하단
            points.Add(new Vector2(centerX - diamondHorizontalRadius, centerY));                  // 좌측
            points.Add(new Vector2(centerX, centerY + diamondVerticalRadius));  // 닫힌 도형을 위해 시작점 추가

            return points;
        }
    }


}
