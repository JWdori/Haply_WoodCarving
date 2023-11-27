using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter))]
public class EdgeDetector : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> edges = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>(); // 꼭짓점 정보를 저장할 리스트 추가
    private Vector3 _previousMeshSize = Vector3.zero;
    private MainEdgeDetector mainEdgeDetector;
    private GameObject reducedMeshObject; // 추가 메쉬 오브젝트 참조

    public delegate void EdgeDetectedHandler(string objectName, List<Vector3> edges, List<Vector3> vertices); // 수정: 꼭짓점 정보 추가
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

        vertices.Clear(); // 꼭짓점 정보 초기화

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

                // 꼭짓점 정보를 리스트에 추가
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

        //// 꼭짓점 표시
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

        //    // 라인 그리기
        //    Gizmos.DrawLine(edgeStart, edgeEnd);
        //}
    }

    public void RefreshEdges()
    {
        edges.Clear();
        vertices.Clear(); // 꼭짓점 정보 초기화
        DetectEdges();

        //// 새로운 메시 생성 또는 기존 메시 업데이트
        //string reducedMeshObjectName = "ReducedMeshObject";

        //// 기존의 동일한 이름을 가진 객체 찾기
        //Transform existingObject = transform.Find(reducedMeshObjectName);
        //if (existingObject != null)
        //{
        //    Destroy(existingObject.gameObject); // 기존 객체 제거
        //}

        //reducedMeshObject = new GameObject(reducedMeshObjectName);
        //reducedMeshObject.transform.SetParent(transform);
        //reducedMeshObject.transform.localPosition = Vector3.zero;
        //reducedMeshObject.transform.localScale = Vector3.one * 0.99f; // 원본 메시보다 약간 작게 설정

        //Mesh newMesh = CreateNewMeshBasedOnEdges();
        //MeshFilter meshFilter = reducedMeshObject.AddComponent<MeshFilter>();
        //meshFilter.mesh = newMesh;
        //MeshRenderer meshRenderer = reducedMeshObject.AddComponent<MeshRenderer>();
        //meshRenderer.material = new Material(Shader.Find("Standard"));
        //MeshCollider meshCollider = reducedMeshObject.AddComponent<MeshCollider>();
        //meshCollider.sharedMesh = newMesh;
        //meshCollider.convex = true; // convex 속성을 true로 설정

        mainEdgeDetector.OnEdgesDetectedHandler(objectName, edges); // 수정: 꼭짓점 정보 추가
    }
    public List<Vector3> GetEdgePositions()
    {
        return edges;
    }

    public List<Vector3> GetVertexPositions() // 수정: 꼭짓점 정보 반환
    {
        return vertices;
    }



    private Mesh CreateNewMeshBasedOnEdges()
    {
        Mesh newMesh = new Mesh();

        // 기존 메시의 정점과 삼각형 데이터를 복사
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;

        // 기타 필요한 메시 데이터 복사 (옵션)
        newMesh.normals = mesh.normals;
        newMesh.uv = mesh.uv;

        return newMesh;
    }
    // EdgeDetector 또는 MassSpawner 클래스 내부 또는 별도의 관리 클래스




}
