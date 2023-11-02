using FruitSlicer.Code.MeshSlice;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
struct MeshSliceJob : IJobFor
{
    [ReadOnly] public NativeArray<Vector3> Vertices;
    [ReadOnly] public NativeArray<Vector2> Uv;
    [ReadOnly] public NativeArray<ushort> Indices;

    public NativeList<Vector3> Intersections;

    public NativeList<Vector3> Points;
    public NativeList<Vector3> Points1;
    public NativeList<Vector2> Uv1;
    public NativeList<Vector3> Points2;
    public NativeList<Vector2> Uv2;
    
    [WriteOnly] public NativeList<Vector3> LeftPartVertices;
    [WriteOnly] public NativeList<Vector2> LeftPartUv;
    [WriteOnly] public NativeList<int> LeftPartIndices;

    [WriteOnly] public NativeList<Vector3> RightPartVertices;
    [WriteOnly] public NativeList<Vector2> RightPartUv;
    [WriteOnly] public NativeList<int> RightPartIndices;
    
    [WriteOnly] public NativeList<int> LeftCapIndices;
    [WriteOnly] public NativeList<int> RightCapIndices;
    
    [ReadOnly] public Matrix4x4 LocalToWorldMatrix;

    [ReadOnly] public Plane SlicePlane;

    private int _leftPartIndexCnt;
    private int _rightPartIndexCnt;

    private void AddTriangle(int part, Triangle tri)
    {
        if (part == 0)
        {
            LeftPartVertices.Add(tri.V1);
            LeftPartVertices.Add(tri.V2);
            LeftPartVertices.Add(tri.V3);
            LeftPartUv.Add(tri.Uv1);
            LeftPartUv.Add(tri.Uv2);
            LeftPartUv.Add(tri.Uv3);
            LeftPartIndices.Add(_leftPartIndexCnt++);
            LeftPartIndices.Add(_leftPartIndexCnt++);
            LeftPartIndices.Add(_leftPartIndexCnt++);
        }
        else if (part == 1)
        {
            RightPartVertices.Add(tri.V1);
            RightPartVertices.Add(tri.V2);
            RightPartVertices.Add(tri.V3);
            RightPartUv.Add(tri.Uv1);
            RightPartUv.Add(tri.Uv2);
            RightPartUv.Add(tri.Uv3);
            RightPartIndices.Add(_rightPartIndexCnt++);
            RightPartIndices.Add(_rightPartIndexCnt++);
            RightPartIndices.Add(_rightPartIndexCnt++);
        }
    }
    
    public void Execute(int i)
    {
        Points.Clear();
        
        int index = i * 3;

        var i1 = Indices[index];
        var i2 = Indices[index + 1];
        var i3 = Indices[index + 2];
        Vector3 v1 = LocalToWorldMatrix.MultiplyPoint(Vertices[i1]);
        Vector3 v2 = LocalToWorldMatrix.MultiplyPoint(Vertices[i2]);
        Vector3 v3 = LocalToWorldMatrix.MultiplyPoint(Vertices[i3]);
        Vector3 uv1 = LocalToWorldMatrix.MultiplyPoint(Uv[i1]);
        Vector3 uv2 = LocalToWorldMatrix.MultiplyPoint(Uv[i2]);
        Vector3 uv3 = LocalToWorldMatrix.MultiplyPoint(Uv[i3]);

        var norm = Vector3.Cross(v1 - v2, v1 - v3);

        float enter;
        
        var dir = v2 - v1;
        if (SlicePlane.Raycast(new Ray(v1, dir), out enter) && enter <= dir.magnitude)
        {
            Vector3 intersection = v1 + enter * dir.normalized;
            Intersections.Add(intersection);
            Points.Add(intersection);
        }

        dir = v3 - v2;
        if (SlicePlane.Raycast(new Ray(v2, dir), out enter) && enter <= dir.magnitude)
        {
            Vector3 intersection = v2 + enter * dir.normalized;
            Intersections.Add(intersection);
            Points.Add(intersection);
        }

        dir = v3 - v1;
        if (SlicePlane.Raycast(new Ray(v1, dir), out enter) && enter <= dir.magnitude)
        {
            Vector3 intersection = v1 + enter * dir.normalized;
            Intersections.Add(intersection);
            Points.Add(intersection);
        }

        if (Points.Length > 0)
        {
            Debug.Assert(Points.Length == 2);
            Points1.Clear();
            Points2.Clear();
            Uv1.Clear();
            Uv2.Clear();
            Points1.AddRange(Points.AsArray());
            Points2.AddRange(Points.AsArray());
            Uv1.AddRange(Uv);
            Uv2.AddRange(Uv);
            
            if (SlicePlane.GetSide(v1))
            {
                Points1.Add(v1);
                Uv1.Add(uv1);
            }
            else
            {
                Points2.Add(v1);
                Uv2.Add(uv1);
            }

            if (SlicePlane.GetSide(v2))
            {
                Points1.Add(v2);
                Uv1.Add(uv2);
            }
            else
            {
                Points2.Add(v2);
                Uv2.Add(uv2);
            }

            if (SlicePlane.GetSide(v3))
            {
                Points1.Add(v3);
                Uv1.Add(uv3);
            }
            else
            {
                Points2.Add(v3);
                Uv2.Add(uv3);
            }

            if (Points1.Length == 3)
            {
                AddTriangle(0, new Triangle(Points1[1], Points1[0], Points1[2], Uv1[1], Uv1[0], Uv1[2], norm));
            }
            else
            {
                Debug.Assert(Points1.Length == 4);
                if (Vector3.Dot(Points1[0] - Points1[1], Points1[2] - Points1[3]) >= 0f)
                {
                    AddTriangle(0, new Triangle(Points1[0], Points1[2], Points1[3], Uv1[0], Uv1[2], Uv1[3], norm));
                    AddTriangle(0, new Triangle(Points1[0], Points1[3], Points1[1], Uv1[0], Uv1[3], Uv1[1], norm));
                }
                else
                {
                    AddTriangle(0, new Triangle(Points1[0], Points1[3], Points1[2], Uv1[0], Uv1[3], Uv1[2], norm));
                    AddTriangle(0, new Triangle(Points1[0], Points1[2], Points1[1], Uv1[0], Uv1[2], Uv1[1], norm));
                }
            }

            if (Points2.Length == 3)
            {
                AddTriangle(1, new Triangle(Points2[1], Points2[0], Points2[2], Uv1[1], Uv1[0], Uv1[2], norm));
            }
            else
            {
                Debug.Assert(Points2.Length == 4);
                if (Vector3.Dot(Points2[0] - Points2[1], Points2[2] - Points2[3]) >= 0f)
                {
                    AddTriangle(1, new Triangle(Points2[0], Points2[2], Points2[3], Uv1[0], Uv1[2], Uv1[3], norm));
                    AddTriangle(1, new Triangle(Points2[0], Points2[3], Points2[1], Uv1[0], Uv1[3], Uv1[1], norm));
                }
                else
                {
                    AddTriangle(1, new Triangle(Points2[0], Points2[3], Points2[2], Uv1[0], Uv1[3], Uv1[2], norm));
                    AddTriangle(1, new Triangle(Points2[0], Points2[2], Points2[1], Uv1[0], Uv1[2], Uv1[1], norm));
                }
            }
        }
        else
        {
            if (SlicePlane.GetSide(v1))
            {
                AddTriangle(0, new Triangle(v1, v2, v3, uv1, uv2, uv3));
            }
            else
            {
                AddTriangle(1, new Triangle(v1, v2, v3, uv1, uv2, uv3));
            }
        }

        if (Intersections.Length > 1 && i == Indices.Length / 3 - 1)
        {
            Vector3 center = Vector3.zero;
            foreach (var point in Intersections)
            {
                center += point;
            }

            center /= Intersections.Length;

            for (int n = 0; n < Intersections.Length; n++)
            {
                var tri = new Triangle(Intersections[n], center,
                    n + 1 == Intersections.Length ? Intersections[0] : Intersections[n + 1],
                    Vector2.zero, Vector2.zero, Vector2.zero, -SlicePlane.normal);
                
                LeftPartVertices.Add(tri.V1);
                LeftPartVertices.Add(tri.V2);
                LeftPartVertices.Add(tri.V3);
                LeftPartUv.Add(tri.Uv1);
                LeftPartUv.Add(tri.Uv2);
                LeftPartUv.Add(tri.Uv3);
                LeftCapIndices.Add(_leftPartIndexCnt++);
                LeftCapIndices.Add(_leftPartIndexCnt++);
                LeftCapIndices.Add(_leftPartIndexCnt++);
            }
            
            for (int n = 0; n < Intersections.Length; n++)
            {
                var tri = new Triangle(Intersections[n], center,
                    n + 1 == Intersections.Length ? Intersections[0] : Intersections[n + 1],
                    Vector2.zero, Vector2.zero, Vector2.zero, SlicePlane.normal);
                
                RightPartVertices.Add(tri.V1);
                RightPartVertices.Add(tri.V2);
                RightPartVertices.Add(tri.V3);
                RightPartUv.Add(tri.Uv1);
                RightPartUv.Add(tri.Uv2);
                RightPartUv.Add(tri.Uv3);
                RightCapIndices.Add(_rightPartIndexCnt++);
                RightCapIndices.Add(_rightPartIndexCnt++);
                RightCapIndices.Add(_rightPartIndexCnt++);
            }
        }
    }
}