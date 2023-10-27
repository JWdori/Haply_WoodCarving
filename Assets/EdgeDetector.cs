using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class EdgeDetector : MonoBehaviour
{
    private Mesh mesh;

    private List<Vector3> edges = new List<Vector3>();
    private Vector3 _previousMeshSize = Vector3.zero; // 이전 프레임에서의 메쉬 크기 저장
    private MainEdgeDetector mainEdgeDetector; // MainEdgeDetector 스크립트 참조 추가

    public delegate void EdgeDetectedHandler(string objectName, List<Vector3> edges);
    public event EdgeDetectedHandler OnEdgesDetected;

    private string objectName; // 오브젝트의 이름 저장

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        if (mesh != null)
            _previousMeshSize = mesh.bounds.size;

        // MainEdgeDetector 게임 오브젝트를 찾아서 참조
        mainEdgeDetector = GameObject.FindObjectOfType<MainEdgeDetector>();

        // 오브젝트 이름을 설정
        objectName = gameObject.name;

        // 데이터를 MainEdgeDetector로 전달
        RefreshEdges();
     }

    void Update()
    {

        mesh = GetComponent<MeshFilter>().mesh; // 여기서 매번 메쉬를 최신 상태로 업데이트
        // 메쉬가 할당되었고, 크기가 변했다면 RefreshEdges 호출
        if (mesh != null && _previousMeshSize != mesh.bounds.size)
        {
            RefreshEdges();
            _previousMeshSize = mesh.bounds.size; // 이전 크기 업데이트
        }
    }
    void DetectEdges()
    {
        mesh = GetComponent<MeshFilter>().mesh; // 여기서 매번 메쉬를 최신 상태로 업데이트
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
                    //Debug.Log($"모서리 위치: {vertexA} - {vertexB}, 각도: {angle}도");
                }
            }
            else if (pair.Value.Count == 1)
            {
                Vector3 vertexA = vertices[(int)pair.Key.x];
                Vector3 vertexB = vertices[(int)pair.Key.y];
                edges.Add(vertexA);
                edges.Add(vertexB);
                //Debug.Log($"모서리 위치: {vertexA} - {vertexB}, 외부 모서리");
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

            // 박스의 크기를 설정합니다.
            Vector3 boxDimensions = new Vector3(boxSize, boxSize, edgeLength);

            // 박스의 방향을 모서리의 방향으로 설정하기 위해 회전 값을 계산합니다.
            Quaternion boxRotation = Quaternion.LookRotation(edgeDirection, Vector3.up);

            // 박스를 그립니다.
            Gizmos.matrix = Matrix4x4.TRS(edgeCenter, boxRotation, boxDimensions);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;  // Gizmos의 변환 행렬을 기본값으로 복원합니다.
        }
    }
    public void RefreshEdges()
    {
        edges.Clear();  // 현재 감지된 모서리 목록을 초기화
        DetectEdges();  // 모서리 다시 감지
        mainEdgeDetector.OnEdgesDetectedHandler(objectName, edges);
        Debug.Log(gameObject.name + " : " + edges.Count);

    }

    public List<Vector3> GetEdgePositions()
    {
        return edges;
    }
}

