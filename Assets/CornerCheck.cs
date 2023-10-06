using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
public class CornerCheck : MonoBehaviour
{
    public Color vertexColor = Color.blue;
    public Color lineColor = Color.red;
    public float vertexSize = 0.05f;
    public float lineWidth = 0.02f;

    private Mesh mesh;
    private List<GameObject> verticesObjects = new List<GameObject>();
    private List<GameObject> lineObjects = new List<GameObject>();

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        CreateVertices(mesh.vertices);
        ConnectClosestVertices(mesh.vertices);
    }

    private void Update()
    {
        UpdateLines();
    }

    private void CreateVertices(Vector3[] vertices)
    {
        foreach (var v in vertices)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localScale = Vector3.one * vertexSize;
            sphere.transform.localPosition = v;
            sphere.GetComponent<Renderer>().material.color = vertexColor;
            Destroy(sphere.GetComponent<Collider>());
            verticesObjects.Add(sphere);
        }
    }

    private void ConnectClosestVertices(Vector3[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 currentVertex = vertices[i];
            Vector3 closestVertex = Vector3.zero;
            float minDistance = float.MaxValue;

            for (int j = 0; j < vertices.Length; j++)
            {
                if (i == j) continue;

                float distance = Vector3.Distance(currentVertex, vertices[j]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestVertex = vertices[j];
                }
            }

            DrawLine(verticesObjects[i].transform, verticesObjects[Array.IndexOf(vertices, closestVertex)].transform);
        }
    }

    private void DrawLine(Transform start, Transform end)
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.transform.SetParent(transform);

        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = lineColor;
        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.SetPosition(0, start.position);
        line.SetPosition(1, end.position);
        line.useWorldSpace = false;

        lineObjects.Add(lineObj);
    }

    private void UpdateLines()
    {
        foreach (var lineObj in lineObjects)
        {
            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            line.SetPosition(0, lineObj.transform.parent.GetChild(0).position);
            line.SetPosition(1, lineObj.transform.parent.GetChild(1).position);
        }
    }
}
