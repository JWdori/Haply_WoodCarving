using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class EdgeAngleCalculator : MonoBehaviour
{
    private Mesh mesh;
    private Dictionary<Edge, float> edgeAngles = new Dictionary<Edge, float>();

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        CalculateEdgeAngles();
    }

    void Update()
    {
        if (meshHasChanged())
        {
            edgeAngles.Clear();
            CalculateEdgeAngles();
        }
    }

    bool meshHasChanged()
    {
        return mesh.vertices.Length != GetComponent<MeshFilter>().mesh.vertices.Length;
    }

    void CalculateEdgeAngles()
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int indexA = triangles[i + j];
                int indexB = triangles[i + (j + 1) % 3];
                int indexC = triangles[i + (j + 2) % 3];

                Vector3 sideAB = vertices[indexB] - vertices[indexA];
                Vector3 sideAC = vertices[indexC] - vertices[indexA];

                float angle = Vector3.Angle(sideAB, sideAC);

                Edge edge = new Edge(indexA, indexB);
                if (!edgeAngles.ContainsKey(edge))
                {
                    edgeAngles[edge] = angle;
                    Debug.Log($"Edge({indexA}, {indexB}) Angle: {angle}¡Æ");
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (mesh == null)
            return;

        Gizmos.color = Color.blue;

        foreach (var kvp in edgeAngles)
        {
            Edge edge = kvp.Key;

            Vector3 pointA = transform.TransformPoint(mesh.vertices[edge.vertex1]);
            Vector3 pointB = transform.TransformPoint(mesh.vertices[edge.vertex2]);

            Gizmos.DrawSphere(pointA, 0.02f);
            Gizmos.DrawSphere(pointB, 0.02f);
        }
    }

    struct Edge
    {
        public int vertex1;
        public int vertex2;

        public Edge(int v1, int v2)
        {
            vertex1 = v1;
            vertex2 = v2;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge edge &&
                   ((vertex1 == edge.vertex1 && vertex2 == edge.vertex2) ||
                    (vertex1 == edge.vertex2 && vertex2 == edge.vertex1));
        }

        public override int GetHashCode()
        {
            return vertex1 ^ vertex2;
        }
    }
}
