using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


[BurstCompile]
public struct AdjustVerticesJob : IJobParallelFor
{
    public NativeArray<Vector3> vertices;
    public float knifeYPosition;

    public void Execute(int index)
    {
        Vector3 vertex = vertices[index];
        if (vertex.y > knifeYPosition)
        {
            vertex.y = knifeYPosition;
            vertices[index] = vertex;
        }
    }
}
