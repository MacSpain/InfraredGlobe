using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ProceduralSphere))]
public class ProceduralSphereEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ProceduralSphere myTarget = (ProceduralSphere)target;
        if (GUILayout.Button("Create Mesh"))
        {
            myTarget.CreateMesh();
        }
    }
}
#endif

public class ProceduralSphere : MonoBehaviour
{
    public const int vertexDimCount = 720;

    public const float rotateDT = (1.0f / 60.0f) * 360.0f;
    private float rotateTime = 0.0f;
    private float[] times;

    public int targetFramerate = 24;
    public int cycleInSeconds = 60;

    private bool rotating = true;

    public void CreateMesh()
    {

        var rand = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);

        int vertexAttributeCount = 4;
        int vertexCount = vertexDimCount * vertexDimCount;
        int triangleIndexCount = 6 * (vertexDimCount - 1) * (vertexDimCount);

        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3, stream: 1
        );
        vertexAttributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4, 2
        );
        vertexAttributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 3
        );

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        NativeArray<float3> positions = meshData.GetVertexData<float3>(0);
        NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        NativeArray<half4> tangents = meshData.GetVertexData<half4>(2);
        NativeArray<half2> texCoords = meshData.GetVertexData<half2>(3);


        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt32);
        NativeArray<uint> triangleIndices = meshData.GetIndexData<uint>();

        NativeArray<float3> tmpPositions = new NativeArray<float3>(positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        NativeArray<float3> tmpNormals = new NativeArray<float3>(normals.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        NativeArray<half4> tmpTangents = new NativeArray<half4>(tangents.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        NativeArray<half2> tmpUVs = new NativeArray<half2>(texCoords.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);



        var createJob = new CreateMeshJob()
        {
            Positions = tmpPositions,
            Normals = tmpNormals,
            Tangents = tmpTangents,
            UVs = tmpUVs,
        };
        var indicesJob = new AssignIndicesJob()
        {
            indices = triangleIndices,
        };


        var createJobHandle = createJob.Schedule(vertexCount, vertexCount >> 6);

        createJobHandle.Complete();
        var indicesJobHandle = indicesJob.Schedule(triangleIndexCount, triangleIndexCount >> 6);
        indicesJobHandle.Complete();
        int i = 0;

        for (int y = 0; y < vertexDimCount; ++y)
        {
            for (int x = 0; x < vertexDimCount; ++x)
            {
                positions[i] = tmpPositions[i];
                normals[i] = tmpNormals[i];
                tangents[i] = tmpTangents[i];
                texCoords[i] = tmpUVs[i];
                ++i;
            }
        }

        tmpPositions.Dispose();
        tmpNormals.Dispose();
        tmpTangents.Dispose();
        tmpUVs.Dispose();



        var bounds = new Bounds(Vector3.zero, new Vector3(1.0f, 1.0f, 1.0f));


        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Procedural Mesh"
        };
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

        GetComponent<MeshFilter>().mesh = mesh;

    }

    public void ToggleRotating()
    {
        rotating = !rotating;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ToggleRotating();
        }

        if (rotating == true)
        {
            transform.rotation = Quaternion.Euler(0.0f, times[Time.frameCount % (targetFramerate * cycleInSeconds)], 0.0f);
        }
        //transform.rotation = Quaternion.Euler(0.0f, 170.0f, 0.0f);
    }

    private void Start()
    {
        times = new float[targetFramerate * cycleInSeconds];
        float dT = 360.0f / (float)(targetFramerate * cycleInSeconds);
        float t = 0.0f;
        for(int i = 0; i < targetFramerate * cycleInSeconds; ++i)
        {
            times[i] = t;

            t += dT;
        }
    }

}

[BurstCompile]
public partial struct CreateMeshJob : IJobParallelFor
{
    public NativeArray<float3> Positions;
    public NativeArray<float3> Normals;
    public NativeArray<half4> Tangents;
    public NativeArray<half2> UVs;

    public void Execute(int i)
    {
        int vertexDimCount = ProceduralSphere.vertexDimCount;

        float polarDiv = Mathf.PI;
        float azimuthDiv = (2.0f* Mathf.PI) / ((float)(vertexDimCount) - 1.0f);
        int widthCount = (ProceduralSphere.vertexDimCount);
        int y = i / (widthCount);
        int x = i % widthCount;

        float fy = (float)y / (float)(vertexDimCount - 1);
        if(fy > 0.5f)
        {
            fy = (1.0f - (2.0f*(fy - 0.5f)));
            fy *= fy;
            fy = 1.0f - fy;
            fy = 0.5f*fy + 0.5f;
        }
        else
        {
            fy *= 2.0f;
            fy *= fy;
            fy *= 0.5f;
        }

        float currPolar = fy * polarDiv;

        float currAzimuth = x*azimuthDiv;

        half2 newUV = new half2((half)((float)x / (float)(vertexDimCount - 1)), (half)(1.0f - (float)fy));
        UVs[i] = newUV;


        Vector3 position = 0.5f*new Vector3(math.sin(currPolar)*math.cos(currAzimuth), math.cos(currPolar), math.sin(currPolar) * math.sin(currAzimuth));
        Positions[i] = position;
        Vector3 normal = position.normalized;
        Normals[i] = normal;
        Vector3 tangent = new Vector3(position.y, position.z, position.x);
        tangent = Vector3.Cross(normal, tangent).normalized;
        Tangents[i] = new half4((half)tangent.x, (half)tangent.y, (half)tangent.z, (half)0);


    }
}

[BurstCompile]
public partial struct AssignIndicesJob : IJobParallelFor
{
    public NativeArray<uint> indices;

    public void Execute(int i)
    {
        int widthCount = (ProceduralSphere.vertexDimCount);
        int dimCount = ProceduralSphere.vertexDimCount;
        int y = (i / 6) / (widthCount);
        int x = (i / 6) % widthCount;
        int nextX = (x + 1) % widthCount;
        int index = i % 6;

        if (y < dimCount - 1)
        {
            switch (index)
            {
                case 0:
                    {
                        indices[i] = (uint)(y * widthCount + x);
                    }
                    break;
                case 1:
                    {
                        indices[i] = (uint)(y * widthCount + nextX); 

                    }
                    break;
                case 2:
                    {
                        indices[i] = (uint)((y + 1) * widthCount + x);

                    }
                    break;
                case 3:
                    {
                        indices[i] = (uint)((y + 1) * widthCount + x);

                    }
                    break;
                case 4:
                    {
                        indices[i] = (uint)(y * widthCount + nextX); 

                    }
                    break;
                case 5:
                    {
                        indices[i] = (uint)((y + 1) * widthCount + nextX);

                    }
                    break;
            }
        }
    }
}