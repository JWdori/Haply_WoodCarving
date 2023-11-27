using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter))]
public class EdgeDetector : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> edges = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>(); // ������ ������ ������ ����Ʈ �߰�
    private Vector3 _previousMeshSize = Vector3.zero;
    private MainEdgeDetector mainEdgeDetector;
    private GameObject reducedMeshObject; // �߰� �޽� ������Ʈ ����

    public delegate void EdgeDetectedHandler(string objectName, List<Vector3> edges, List<Vector3> vertices); // ����: ������ ���� �߰�
    public event EdgeDetectedHandler OnEdgesDetected;

    private string objectName;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
            _previousMeshSize = mesh.bounds.size;

        mainEdgeDetector = GameObject.FindObjectOfType<MainEdgeDetector>();
        objectName = gameObject.name;

        RefreshEdges();
    }

    void Update()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null && _previousMeshSize != mesh.bounds.size)
        {
            _previousMeshSize = mesh.bounds.size;
            RefreshEdges();
        }
    }

    void DetectEdges()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] meshVertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Dictionary<Vector2, List<Quaternion>> edgeRotations = new Dictionary<Vector2, List<Quaternion>>();

        vertices.Clear(); // ������ ���� �ʱ�ȭ

        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int vertexA = triangles[i + j];
                int vertexB = triangles[i + (j + 1) % 3];
                Vector2 edge = new Vector2(vertexA, vertexB);
                Vector2 reversedEdge = new Vector2(vertexB, vertexA);

                Vector3 normal = Vector3.Cross(meshVertices[vertexB] - meshVertices[vertexA], meshVertices[triangles[i + (j + 2) % 3]] - meshVertices[vertexA]).normalized;
                Quaternion rotation = Quaternion.LookRotation(normal);

                if (edgeRotations.ContainsKey(edge))
                    edgeRotations[edge].Add(rotation);
                else if (edgeRotations.ContainsKey(reversedEdge))
                    edgeRotations[reversedEdge].Add(rotation);
                else
                    edgeRotations[edge] = new List<Quaternion> { rotation };

                // ������ ������ ����Ʈ�� �߰�
                if (!vertices.Contains(meshVertices[vertexA]))
                    vertices.Add(meshVertices[vertexA]);
                if (!vertices.Contains(meshVertices[vertexB]))
                    vertices.Add(meshVertices[vertexB]);
            }
        }

        foreach (var pair in edgeRotations)
        {
            if (pair.Value.Count == 2)
            {
                float angle = Quaternion.Angle(pair.Value[0], pair.Value[1]);
                if (angle >= 30f)
                {
                    Vector3 vertexA = meshVertices[(int)pair.Key.x];
                    Vector3 vertexB = meshVertices[(int)pair.Key.y];
                    edges.Add(vertexA);
                    edges.Add(vertexB);
                }
            }
            else if (pair.Value.Count == 1)
            {
                Vector3 vertexA = meshVertices[(int)pair.Key.x];
                Vector3 vertexB = meshVertices[(int)pair.Key.y];
                edges.Add(vertexA);
                edges.Add(vertexB);
            }
        }
    }
    void OnDrawGizmos()
    {

        //// ������ ǥ��
        //Gizmos.color = Color.red;
        //float vertexSize = 0.01f;

        //foreach (Vector3 vertex in vertices)
        //{
        //    Vector3 vertexPosition = transform.TransformPoint(vertex);
        //    Gizmos.DrawSphere(vertexPosition, vertexSize);
        //}



        //Gizmos.color = Color.blue;

        //for (int i = 0; i < edges.Count; i += 2)
        //{
        //    Vector3 edgeStart = transform.TransformPoint(edges[i]);
        //    Vector3 edgeEnd = transform.TransformPoint(edges[i + 1]);

        //    // ���� �׸���
        //    Gizmos.DrawLine(edgeStart, edgeEnd);
        //}
    }

    public void RefreshEdges()
    {
        edges.Clear();
        vertices.Clear(); // ������ ���� �ʱ�ȭ
        DetectEdges();

        //// ���ο� �޽� ���� �Ǵ� ���� �޽� ������Ʈ
        //string reducedMeshObjectName = "ReducedMeshObject";

        //// ������ ������ �̸��� ���� ��ü ã��
        //Transform existingObject = transform.Find(reducedMeshObjectName);
        //if (existingObject != null)
        //{
        //    Destroy(existingObject.gameObject); // ���� ��ü ����
        //}

        //reducedMeshObject = new GameObject(reducedMeshObjectName);
        //reducedMeshObject.transform.SetParent(transform);
        //reducedMeshObject.transform.localPosition = Vector3.zero;
        //reducedMeshObject.transform.localScale = Vector3.one * 0.99f; // ���� �޽ú��� �ణ �۰� ����

        //Mesh newMesh = CreateNewMeshBasedOnEdges();
        //MeshFilter meshFilter = reducedMeshObject.AddComponent<MeshFilter>();
        //meshFilter.mesh = newMesh;
        //MeshRenderer meshRenderer = reducedMeshObject.AddComponent<MeshRenderer>();
        //meshRenderer.material = new Material(Shader.Find("Standard"));
        //MeshCollider meshCollider = reducedMeshObject.AddComponent<MeshCollider>();
        //meshCollider.sharedMesh = newMesh;
        //meshCollider.convex = true; // convex �Ӽ��� true�� ����

        mainEdgeDetector.OnEdgesDetectedHandler(objectName, edges); // ����: ������ ���� �߰�
    }
    public List<Vector3> GetEdgePositions()
    {
        return edges;
    }

    public List<Vector3> GetVertexPositions() // ����: ������ ���� ��ȯ
    {
        return vertices;
    }



    private Mesh CreateNewMeshBasedOnEdges()
    {
        Mesh newMesh = new Mesh();

        // ���� �޽��� ������ �ﰢ�� �����͸� ����
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;

        // ��Ÿ �ʿ��� �޽� ������ ���� (�ɼ�)
        newMesh.normals = mesh.normals;
        newMesh.uv = mesh.uv;

        return newMesh;
    }
    // EdgeDetector �Ǵ� MassSpawner Ŭ���� ���� �Ǵ� ������ ���� Ŭ����




}
