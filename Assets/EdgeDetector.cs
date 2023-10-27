using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class EdgeDetector : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> edges = new List<Vector3>();
    private Vector3 _previousMeshSize = Vector3.zero; // ���� �����ӿ����� �޽� ũ�� ����
    private MainEdgeDetector mainEdgeDetector; // MainEdgeDetector ��ũ��Ʈ ���� �߰�

    public delegate void EdgeDetectedHandler(string objectName, List<Vector3> edges);
    public event EdgeDetectedHandler OnEdgesDetected;

    private string objectName; // ������Ʈ�� �̸� ����

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
            _previousMeshSize = mesh.bounds.size;

        // MainEdgeDetector ���� ������Ʈ�� ã�Ƽ� ����
        mainEdgeDetector = GameObject.FindObjectOfType<MainEdgeDetector>();

        // ������Ʈ �̸��� ����
        objectName = gameObject.name;

        // �����͸� MainEdgeDetector�� ����
        RefreshEdges();
     }

    void Update()
    {

        mesh = GetComponent<MeshFilter>().mesh; // ���⼭ �Ź� �޽��� �ֽ� ���·� ������Ʈ
        // �޽��� �Ҵ�Ǿ���, ũ�Ⱑ ���ߴٸ� RefreshEdges ȣ��
        if (mesh != null && _previousMeshSize != mesh.bounds.size)
        {
            RefreshEdges();
            _previousMeshSize = mesh.bounds.size; // ���� ũ�� ������Ʈ
        }
    }
    void DetectEdges()
    {
        mesh = GetComponent<MeshFilter>().mesh; // ���⼭ �Ź� �޽��� �ֽ� ���·� ������Ʈ
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Dictionary<Vector2, List<Quaternion>> edgeRotations = new Dictionary<Vector2, List<Quaternion>>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int vertexA = triangles[i + j];
                int vertexB = triangles[i + (j + 1) % 3];
                Vector2 edge = new Vector2(vertexA, vertexB);
                Vector2 reversedEdge = new Vector2(vertexB, vertexA);

                Vector3 normal = Vector3.Cross(vertices[vertexB] - vertices[vertexA], vertices[triangles[i + (j + 2) % 3]] - vertices[vertexA]).normalized;
                Quaternion rotation = Quaternion.LookRotation(normal);

                if (edgeRotations.ContainsKey(edge))
                    edgeRotations[edge].Add(rotation);
                else if (edgeRotations.ContainsKey(reversedEdge))
                    edgeRotations[reversedEdge].Add(rotation);
                else
                    edgeRotations[edge] = new List<Quaternion> { rotation };
            }
        }

        foreach (var pair in edgeRotations)
        {
            if (pair.Value.Count == 2)
            {
                float angle = Quaternion.Angle(pair.Value[0], pair.Value[1]);
                if (angle >= 30f)
                {
                    Vector3 vertexA = vertices[(int)pair.Key.x];
                    Vector3 vertexB = vertices[(int)pair.Key.y];
                    edges.Add(vertexA);
                    edges.Add(vertexB);
                    //Debug.Log($"�𼭸� ��ġ: {vertexA} - {vertexB}, ����: {angle}��");
                }
            }
            else if (pair.Value.Count == 1)
            {
                Vector3 vertexA = vertices[(int)pair.Key.x];
                Vector3 vertexB = vertices[(int)pair.Key.y];
                edges.Add(vertexA);
                edges.Add(vertexB);
                //Debug.Log($"�𼭸� ��ġ: {vertexA} - {vertexB}, �ܺ� �𼭸�");
            }

        }
    }


    bool IsSharedEdge(int[] triangles, int triangleIndex, int vertexA, int vertexB)
    {
        int count = 0;

        for (int i = 0; i < 3; i++)
        {
            int vertex = triangles[triangleIndex + i];
            if (vertex == vertexA || vertex == vertexB)
                count++;
        }

        return count == 2;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        float boxSize = 0.005f;

        for (int i = 0; i < edges.Count; i += 2)
        {
            Vector3 edgeStart = transform.TransformPoint(edges[i]);
            Vector3 edgeEnd = transform.TransformPoint(edges[i + 1]);
            Vector3 edgeCenter = (edgeStart + edgeEnd) / 2;
            Vector3 edgeDirection = (edgeEnd - edgeStart).normalized;
            float edgeLength = Vector3.Distance(edgeStart, edgeEnd);

            // �ڽ��� ũ�⸦ �����մϴ�.
            Vector3 boxDimensions = new Vector3(boxSize, boxSize, edgeLength);

            // �ڽ��� ������ �𼭸��� �������� �����ϱ� ���� ȸ�� ���� ����մϴ�.
            Quaternion boxRotation = Quaternion.LookRotation(edgeDirection, Vector3.up);

            // �ڽ��� �׸��ϴ�.
            Gizmos.matrix = Matrix4x4.TRS(edgeCenter, boxRotation, boxDimensions);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;  // Gizmos�� ��ȯ ����� �⺻������ �����մϴ�.
        }
    }
    public void RefreshEdges()
    {
        edges.Clear();  // ���� ������ �𼭸� ����� �ʱ�ȭ
        DetectEdges();  // �𼭸� �ٽ� ����
        mainEdgeDetector.OnEdgesDetectedHandler(objectName, edges);
        Debug.Log(gameObject.name + " : " + edges.Count);

    }

    public List<Vector3> GetEdgePositions()
    {
        return edges;
    }
}

