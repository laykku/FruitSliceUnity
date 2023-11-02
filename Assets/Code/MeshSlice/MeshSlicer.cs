using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FruitSlicer.Code.MeshSlice
{
    public class MeshSlicer : MonoBehaviour
    {
        public static GameObject[] Slice(GameObject objToSlice, Plane slicePlane, Material objMat, Material sliceMat)
        {
            GameObject[] parts = null;

            var mesh = objToSlice.GetComponent<MeshFilter>().mesh;

            var vertexCount = mesh.vertexCount;
            
            using (var dataArray = Mesh.AcquireReadOnlyMeshData(mesh))
            {
                var data = dataArray[0];
                var vertices = new NativeArray<Vector3>(mesh.vertexCount, Allocator.TempJob);
                data.GetVertices(vertices);
                var uv = new NativeArray<Vector2>(mesh.vertexCount, Allocator.TempJob);
                data.GetUVs(0, uv);
                var indices = data.GetIndexData<ushort>();

                var intersections = new NativeList<Vector3>(vertexCount, Allocator.TempJob);
                var points = new NativeList<Vector3>(vertexCount, Allocator.TempJob);
                var points1 = new NativeList<Vector3>(vertexCount, Allocator.TempJob);
                var points2 = new NativeList<Vector3>(vertexCount, Allocator.TempJob);
                var uv1 = new NativeList<Vector2>(vertexCount, Allocator.TempJob);
                var uv2 = new NativeList<Vector2>(vertexCount, Allocator.TempJob);

                var leftPartVertices = new NativeList<Vector3>(vertexCount, Allocator.TempJob);
                var leftPartUv = new NativeList<Vector2>(vertexCount, Allocator.TempJob);
                var leftPartIndices = new NativeList<int>(vertexCount, Allocator.TempJob);

                var rightPartVertices = new NativeList<Vector3>(vertexCount, Allocator.TempJob);
                var rightPartUv = new NativeList<Vector2>(vertexCount, Allocator.TempJob);
                var rightPartIndices = new NativeList<int>(vertexCount, Allocator.TempJob);

                var leftCapIndices = new NativeList<int>(vertexCount, Allocator.TempJob);
                var rightCapIndices = new NativeList<int>(vertexCount, Allocator.TempJob);
                
                var job = new MeshSliceJob
                {
                    Vertices = vertices,
                    Indices = indices,
                    Uv = uv,
                    Intersections = intersections,
                    Points = points,
                    Points1 = points1,
                    Points2 = points2,
                    Uv1 = uv1,
                    Uv2 = uv2,

                    LeftPartVertices = leftPartVertices,
                    LeftPartUv = leftPartUv,
                    LeftPartIndices = leftPartIndices,

                    RightPartVertices = rightPartVertices,
                    RightPartUv = rightPartUv,
                    RightPartIndices = rightPartIndices,
                    
                    LeftCapIndices = leftCapIndices,
                    RightCapIndices = rightCapIndices,

                    LocalToWorldMatrix = objToSlice.transform.localToWorldMatrix,
                    SlicePlane = slicePlane,
                };

                var jobDependency = new JobHandle();
                var handle = job.Schedule(indices.Length / 3, jobDependency);

                handle.Complete();

                if (intersections.Length > 0)
                {
                    parts = new[]
                    {
                        CreateObj(leftPartVertices, leftPartUv, leftPartIndices, leftCapIndices, objMat, sliceMat),
                        CreateObj(rightPartVertices, rightPartUv, rightPartIndices, rightCapIndices, objMat, sliceMat),
                    };
                }

                vertices.Dispose();
                uv.Dispose();
                indices.Dispose();
                intersections.Dispose();
                points.Dispose();
                points1.Dispose();
                points2.Dispose();
                uv1.Dispose();
                uv2.Dispose();
                leftPartVertices.Dispose();
                leftPartUv.Dispose();
                leftPartIndices.Dispose();
                rightPartVertices.Dispose();
                rightPartUv.Dispose();
                rightPartIndices.Dispose();
                leftCapIndices.Dispose();
                rightCapIndices.Dispose();
            }
            
            return parts;
        }

        private static GameObject CreateObj(NativeList<Vector3> verticesList, NativeList<Vector2> uvList,
            NativeList<int> indicesList, NativeList<int> capIndicesList, Material objMat, Material sliceMat)
        {
            Vector3[] vertices = new Vector3[verticesList.Length];
            var nav = new NativeArray<Vector3>(verticesList.AsArray(), Allocator.Temp);
            nav.CopyTo(vertices);

            Vector2[] uv = new Vector2[uvList.Length];
            var nau = new NativeArray<Vector2>(uvList.AsArray(), Allocator.Temp);
            nau.CopyTo(uv);
            
            int[] indices = new int[indicesList.Length];
            var nai = new NativeArray<int>(indicesList.AsArray(), Allocator.Temp);
            nai.CopyTo(indices);

            int[] capIndices = new int[capIndicesList.Length];
            var cnai = new NativeArray<int>(capIndicesList.AsArray(), Allocator.Temp);
            cnai.CopyTo(capIndices);
            
            var mesh = new Mesh();
            mesh.subMeshCount = 2;
            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.SetIndices(capIndices, MeshTopology.Triangles, 1);
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var obj = new GameObject();
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterials = new []
            {
                objMat,
                sliceMat
            };
            obj.AddComponent<SphereCollider>();
            
            nav.Dispose();
            nai.Dispose();

            return obj;
        }
    }
}