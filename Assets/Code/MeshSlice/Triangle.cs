using UnityEngine;

namespace FruitSlicer.Code.MeshSlice
{
    struct Triangle
    {
        public readonly Vector3 V1;
        public readonly Vector3 V2;
        public readonly Vector3 V3;
        public readonly Vector3 Uv1;
        public readonly Vector3 Uv2;
        public readonly Vector3 Uv3;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Uv1 = uv1;
            Uv2 = uv2;
            Uv3 = uv3;
        }
    
        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 normal)
        {
            if (Vector3.Dot(Vector3.Cross(v1 - v2, v1 - v3).normalized, normal) <= 0f)
            {
                (v1, v3) = (v3, v1);
                (uv1, uv3) = (uv3, uv1);
            }
        
            V1 = v1;
            V2 = v2;
            V3 = v3;
            Uv1 = uv1;
            Uv2 = uv2;
            Uv3 = uv3;
        }
    }
}