using UnityEngine;

namespace MarchingCubesProject
{
    public static class TriangleAreaCalculator
    {
        public static float CalculateTriangleArea(Vector3 A, Vector3 B, Vector3 C)
        {
            float a = Vector3.Distance(B, C);
            float b = Vector3.Distance(C, A);
            float c = Vector3.Distance(A, B);
            float p = (a + b + c) / 2.0f;

            float area = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));

            return area;
        }
    }

}
