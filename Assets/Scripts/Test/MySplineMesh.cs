using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Splines;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Profiling;

public static class MySplineMesh
{
    public interface ISplineVertexData
    {
        Vector3 position { get; set; }

        Vector3 normal { get; set; }

        Vector2 texture { get; set; }
    }

    struct VertexData : ISplineVertexData
    {
        public Vector3 position { get; set; }

        public Vector3 normal { get; set; }

        public Vector2 texture { get; set; }
    }

    struct Settings
    {
        public int sides { get; private set; }

        public int segments { get; private set; }

        public bool capped { get; private set; }

        public bool closed { get; private set; }

        public float2 range { get; private set; }

        public float radius { get; private set; }

        public Settings(int sides, int segments, bool capped, bool closed, float2 range, float radius)
        {
            this.sides = math.clamp(sides, 3, 2084);
            this.segments = math.clamp(segments, 2, 4096);
            this.range = new float2(math.min(range.x, range.y), math.max(range.x, range.y));
            this.closed = math.abs(1f - (this.range.y - this.range.x)) < float.Epsilon && closed;
            this.capped = capped && !this.closed;
            this.radius = math.clamp(radius, 1E-05f, 10000f);
        }
    }

    static readonly VertexAttributeDescriptor[] k_PipeVertexAttribs = new VertexAttributeDescriptor[3]
    {
        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
        new VertexAttributeDescriptor(VertexAttribute.Normal),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
    };

    static void ExtrudeRing<T, K>(T spline, float t, NativeArray<K> data, int start, int count, float radius) where T : ISpline where K : struct, ISplineVertexData
    {
        float num = math.clamp(t, 0f, 1f);
        Profiler.BeginSample("Test");
        spline.Evaluate(num, out var position, out var tangent, out var upVector);
        Profiler.EndSample();
        
        float num2 = math.lengthsq(tangent);
        if (num2 == 0f || float.IsNaN(num2))
        {
            float t2 = math.clamp(num + 0.0001f * ((t < 1f) ? 1f : (-1f)), 0f, 1f);
            spline.Evaluate(t2, out var _, out tangent, out upVector);
        }

        tangent = math.normalize(tangent);
        quaternion q = quaternion.LookRotationSafe(tangent, upVector);
        float num3 = math.radians(360f / count);
        for (int i = 0; i < count; i++)
        {
            K value = new K();
            float3 v = new float3(math.cos(i * num3), math.sin(i * num3), 0f) * radius;
            value.position = position + math.rotate(q, v);
            value.normal = (value.position - (Vector3)position).normalized;
            float num4 = i / ((float)count + (count % 2));
            float x = math.abs(num4 - math.floor(num4 + 0.5f)) * 2f;
            value.texture = new Vector2(x, t * spline.GetLength());
            data[start + i] = value;
        }
    }

    static void GetVertexAndIndexCount(Settings settings, out int vertexCount, out int indexCount)
    {
        vertexCount = settings.sides * (settings.segments + (settings.capped ? 2 : 0));
        indexCount = settings.sides * 6 * (settings.segments - ((!settings.closed) ? 1 : 0)) + (settings.capped ? ((settings.sides - 2) * 3 * 2) : 0);
    }

    public static async UniTask ExtrudeAsync<T>(IReadOnlyList<T> splines, Mesh mesh, float radius, int sides, float segmentsPerUnit, bool capped, float2 range, CancellationToken token) where T : ISpline
    {
        mesh.Clear();
        if (splines == null)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Trying to extrude a spline mesh with no valid splines.");
            }
            return;
        }

        Mesh.MeshDataArray data = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = data[0];
        meshData.subMeshCount = 1;
        int num = 0;
        int num2 = 0;
        Settings[] array = new Settings[splines.Count];
        float num3 = Mathf.Abs(range.y - range.x);
        (int, int)[] array2 = new (int, int)[splines.Count];
        for (int i = 0; i < splines.Count; i++)
        {
            T val = splines[i];
            int segments = Mathf.Max((int)Mathf.Ceil(val.GetLength() * num3 * segmentsPerUnit), 1);
            array[i] = new Settings(sides, segments, capped, val.Closed, range, radius);
            GetVertexAndIndexCount(array[i], out var vertexCount, out var indexCount);
            array2[i] = (num2, num);
            num += vertexCount;
            num2 += indexCount;
        }

        IndexFormat indexFormat = (num >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16;
        meshData.SetIndexBufferParams(num2, indexFormat);
        meshData.SetVertexBufferParams(num, k_PipeVertexAttribs);
        NativeArray<VertexData> vertexData = meshData.GetVertexData<VertexData>();
        if (indexFormat == IndexFormat.UInt16)
        {
            NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
            for (int j = 0; j < splines.Count; j++)
            {
                await ExtrudeAsync(splines[j], vertexData, indexData, array[j], array2[j].Item2, array2[j].Item1, token);
            }
        }
        else
        {
            NativeArray<uint> indexData2 = meshData.GetIndexData<uint>();
            for (int k = 0; k < splines.Count; k++)
            {
                await ExtrudeAsync(splines[k], vertexData, indexData2, array[k], array2[k].Item2, array2[k].Item1, token);
            }
        }

        meshData.SetSubMesh(0, new SubMeshDescriptor(0, num2));
        Mesh.ApplyAndDisposeWritableMeshData(data, mesh);
        mesh.RecalculateBounds();
    }

    static async UniTask ExtrudeAsync<TSplineType, TVertexType, TIndexType>(TSplineType spline, NativeArray<TVertexType> vertices, NativeArray<TIndexType> indices, Settings settings, int vertexArrayOffset = 0, int indicesArrayOffset = 0, CancellationToken token = default) where TSplineType : ISpline where TVertexType : struct, ISplineVertexData where TIndexType : struct
    {
        float radius = settings.radius;
        int sides = settings.sides;
        int segments = settings.segments;
        float2 range = settings.range;
        bool capped = settings.capped;
        GetVertexAndIndexCount(settings, out var vertexCount, out var indexCount);
        if (sides < 3)
        {
            throw new ArgumentOutOfRangeException("sides", "Sides must be greater than 3");
        }

        if (segments < 2)
        {
            throw new ArgumentOutOfRangeException("segments", "Segments must be greater than 2");
        }

        if (vertices.Length < vertexCount)
        {
            throw new ArgumentOutOfRangeException($"Vertex array is incorrect size. Expected {vertexCount} or more, but received {vertices.Length}.");
        }

        if (indices.Length < indexCount)
        {
            throw new ArgumentOutOfRangeException($"Index array is incorrect size. Expected {indexCount} or more, but received {indices.Length}.");
        }

        if (typeof(TIndexType) == typeof(ushort))
        {
            await WindTrisAsync(indices.Reinterpret<ushort>(), settings, vertexArrayOffset, indicesArrayOffset, token);
        }
        else
        {
            if (!(typeof(TIndexType) == typeof(uint)))
            {
                throw new ArgumentException("Indices must be UInt16 or UInt32", "indices");
            }

            WindTrisB(indices.Reinterpret<uint>(), settings, vertexArrayOffset, indicesArrayOffset);
        }
        await UniTask.Yield(token);

        for (int i = 0; i < segments; i++)
        {
            ExtrudeRing(spline, math.lerp(range.x, range.y, (float)i / ((float)segments - 1f)), vertices, vertexArrayOffset + i * sides, sides, radius);
            if(i % 20 == 0)
            {
                await UniTask.Yield(token);
            }
        }

        if (capped)
        {
            int num = vertexArrayOffset + segments * sides;
            int num2 = vertexArrayOffset + (segments + 1) * sides;
            float2 @float = (spline.Closed ? math.frac(range) : math.clamp(range, 0f, 1f));
            ExtrudeRing(spline, @float.x, vertices, num, sides, radius);
            ExtrudeRing(spline, @float.y, vertices, num2, sides, radius);
            float3 float2 = math.normalize(spline.EvaluateTangent(@float.x));
            float num3 = math.lengthsq(float2);
            if (num3 == 0f || float.IsNaN(num3))
            {
                float2 = math.normalize(spline.EvaluateTangent(@float.x + 0.0001f));
            }

            float3 float3 = math.normalize(spline.EvaluateTangent(@float.y));
            num3 = math.lengthsq(float3);
            if (num3 == 0f || float.IsNaN(num3))
            {
                float3 = math.normalize(spline.EvaluateTangent(@float.y - 0.0001f));
            }

            float num4 = math.radians(360f / (float)sides);
            float2 float4 = new float2(0.5f, 0.5f);
            for (int j = 0; j < sides; j++)
            {
                TVertexType value = vertices[num + j];
                TVertexType value2 = vertices[num2 + j];
                value.normal = -float2;
                value.texture = float4 + new float2(math.cos((float)j * num4), math.sin((float)j * num4)) * 0.5f;
                value2.normal = float3;
                value2.texture = float4 + new float2(0f - math.cos((float)j * num4), math.sin((float)j * num4)) * 0.5f;
                vertices[num + j] = value;
                vertices[num2 + j] = value2;
            }
        }
    }

    static async UniTask WindTrisAsync(NativeArray<ushort> indices, Settings settings, int vertexArrayOffset = 0, int indexArrayOffset = 0, CancellationToken token = default)
    {
        bool closed = settings.closed;
        int segments = settings.segments;
        int sides = settings.sides;
        bool capped = settings.capped;
        for (int i = 0; i < (closed ? segments : (segments - 1)); i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int num = vertexArrayOffset + i * sides + j;
                int num2 = vertexArrayOffset + i * sides + (j + 1) % sides;
                int num3 = vertexArrayOffset + (i + 1) % segments * sides + j;
                int num4 = vertexArrayOffset + (i + 1) % segments * sides + (j + 1) % sides;
                indices[indexArrayOffset + i * sides * 6 + j * 6] = (ushort)num;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 1] = (ushort)num2;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 2] = (ushort)num3;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 3] = (ushort)num2;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 4] = (ushort)num4;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 5] = (ushort)num3;
            }
            if(i % 400 == 0)
            {
                await UniTask.Yield(token);
            }
        }

        if (capped)
        {
            int num5 = vertexArrayOffset + segments * sides;
            int num6 = indexArrayOffset + sides * 6 * (segments - 1);
            int num7 = vertexArrayOffset + (segments + 1) * sides;
            int num8 = indexArrayOffset + (segments - 1) * 6 * sides + (sides - 2) * 3;
            for (ushort num9 = 0; num9 < sides - 2; num9++)
            {
                indices[num6 + num9 * 3] = (ushort)num5;
                indices[num6 + num9 * 3 + 1] = (ushort)(num5 + num9 + 2);
                indices[num6 + num9 * 3 + 2] = (ushort)(num5 + num9 + 1);
                indices[num8 + num9 * 3] = (ushort)num7;
                indices[num8 + num9 * 3 + 1] = (ushort)(num7 + num9 + 1);
                indices[num8 + num9 * 3 + 2] = (ushort)(num7 + num9 + 2);
            }
        }
    }

    static void WindTrisB(NativeArray<uint> indices, Settings settings, int vertexArrayOffset = 0, int indexArrayOffset = 0)
    {
        bool closed = settings.closed;
        int segments = settings.segments;
        int sides = settings.sides;
        bool capped = settings.capped;
        for (int i = 0; i < (closed ? segments : (segments - 1)); i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int value = vertexArrayOffset + i * sides + j;
                int value2 = vertexArrayOffset + i * sides + (j + 1) % sides;
                int value3 = vertexArrayOffset + (i + 1) % segments * sides + j;
                int value4 = vertexArrayOffset + (i + 1) % segments * sides + (j + 1) % sides;
                indices[indexArrayOffset + i * sides * 6 + j * 6] = (uint)value;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 1] = (uint)value2;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 2] = (uint)value3;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 3] = (uint)value2;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 4] = (uint)value4;
                indices[indexArrayOffset + i * sides * 6 + j * 6 + 5] = (uint)value3;
            }
        }

        if (capped)
        {
            int num = vertexArrayOffset + segments * sides;
            int num2 = indexArrayOffset + sides * 6 * (segments - 1);
            int num3 = vertexArrayOffset + (segments + 1) * sides;
            int num4 = indexArrayOffset + (segments - 1) * 6 * sides + (sides - 2) * 3;
            for (ushort num5 = 0; num5 < sides - 2; num5++)
            {
                indices[num2 + num5 * 3] = (uint)num;
                indices[num2 + num5 * 3 + 1] = (uint)(num + num5 + 2);
                indices[num2 + num5 * 3 + 2] = (uint)(num + num5 + 1);
                indices[num4 + num5 * 3] = (uint)num3;
                indices[num4 + num5 * 3 + 1] = (uint)(num3 + num5 + 1);
                indices[num4 + num5 * 3 + 2] = (uint)(num3 + num5 + 2);
            }
        }
    }
}