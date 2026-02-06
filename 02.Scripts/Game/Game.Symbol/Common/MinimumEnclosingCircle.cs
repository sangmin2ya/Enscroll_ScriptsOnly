using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Game.Symbols
{
    public class MinimumEnclosingCircle
    {
        private const float EPSILON = 1e-6f;

        public class Circle
        {
            public Vector2 center;
            public float radius;

            public Circle(Vector2 center, float radius)
            {
                this.center = center;
                this.radius = radius;
            }
        }

        // 메인 함수
        public static Circle FindMinimumEnclosingCircle(List<Vector2> points)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Points list cannot be null or empty");

            // 랜덤으로 점들의 순서를 섞어 더 안정적인 결과를 얻습니다
            List<Vector2> shuffledPoints = points.OrderBy(x => UnityEngine.Random.value).ToList();
            return WelzlAlgorithm(shuffledPoints, 0, new List<Vector2>());
        }

        // Welzl's 알고리즘 구현
        private static Circle WelzlAlgorithm(List<Vector2> points, int index, List<Vector2> boundary)
        {
            if (index >= points.Count || boundary.Count == 3)
            {
                return MakeCircleFromBoundary(boundary);
            }

            Circle circle = WelzlAlgorithm(points, index + 1, boundary);

            if (circle == null || !IsPointInCircle(points[index], circle, EPSILON))
            {
                boundary.Add(points[index]);
                circle = WelzlAlgorithm(points, index + 1, boundary);
                boundary.RemoveAt(boundary.Count - 1);
            }

            return circle;
        }

        // 경계점들로부터 원 생성
        private static Circle MakeCircleFromBoundary(List<Vector2> boundary)
        {
            if (boundary.Count == 0)
                return new Circle(Vector2.zero, 0);

            if (boundary.Count == 1)
                return new Circle(boundary[0], 0);

            if (boundary.Count == 2)
                return MakeCircleFromTwoPoints(boundary[0], boundary[1]);

            return MakeCircleFromThreePoints(boundary[0], boundary[1], boundary[2]);
        }

        // 두 점으로부터 원 생성
        private static Circle MakeCircleFromTwoPoints(Vector2 p1, Vector2 p2)
        {
            Vector2 center = (p1 + p2) * 0.5f;
            float radius = Vector2.Distance(p1, center);
            return new Circle(center, radius);
        }

        // 세 점으로부터 원 생성 (개선된 버전)
        private static Circle MakeCircleFromThreePoints(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // 세 점이 일직선 상에 있는지 확인
            float d = (p1.x * (p2.y - p3.y) + p2.x * (p3.y - p1.y) + p3.x * (p1.y - p2.y));
            if (Mathf.Abs(d) < EPSILON)
            {
                // 일직선상에 있는 경우, 가장 멀리 떨어진 두 점으로 원 생성
                float d1 = Vector2.Distance(p1, p2);
                float d2 = Vector2.Distance(p2, p3);
                float d3 = Vector2.Distance(p3, p1);

                if (d1 >= d2 && d1 >= d3)
                    return MakeCircleFromTwoPoints(p1, p2);
                else if (d2 >= d1 && d2 >= d3)
                    return MakeCircleFromTwoPoints(p2, p3);
                else
                    return MakeCircleFromTwoPoints(p3, p1);
            }

            // 외심 계산
            float a = p1.x * p1.x + p1.y * p1.y;
            float b = p2.x * p2.x + p2.y * p2.y;
            float c = p3.x * p3.x + p3.y * p3.y;

            float x = (a * (p2.y - p3.y) + b * (p3.y - p1.y) + c * (p1.y - p2.y)) / (2 * d);
            float y = (a * (p3.x - p2.x) + b * (p1.x - p3.x) + c * (p2.x - p1.x)) / (2 * d);

            Vector2 center = new Vector2(x, y);
            float radius = Vector2.Distance(center, p1);

            return new Circle(center, radius);
        }

        // 점이 원 안에 있는지 확인 (개선된 버전)
        private static bool IsPointInCircle(Vector2 point, Circle circle, float epsilon = EPSILON)
        {
            float distance = Vector2.Distance(point, circle.center);
            return distance <= circle.radius + epsilon;
        }


    }
}