using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class MeshDeformationWatcher : MonoBehaviour
{
    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private Vector3[] initialVertices;
    private Vector3[] currentVertices;

    void Start()
    {
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();

        // 초기 버텍스 배열 복사
        initialVertices = meshFilter.mesh.vertices.Clone() as Vector3[];
        currentVertices = meshFilter.mesh.vertices;
    }

    void Update()
    {
        // MeshCollider가 없는 경우, 메서드 종료
        if (meshCollider == null) return;

        currentVertices = meshFilter.mesh.vertices; // 현재 버텍스 데이터 갱신

        // 메쉬 변형 감지
        if (HasMeshDeformed())
        {
            // 메쉬 콜라이더 업데이트
            UpdateMeshCollider();
        }
    }

    private bool HasMeshDeformed()
    {
        for (int i = 0; i < currentVertices.Length; i++)
        {
            if (currentVertices[i] != initialVertices[i])
                return true;
        }
        return false;
    }

    private void UpdateMeshCollider()
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = meshFilter.mesh.vertices;
        newMesh.triangles = meshFilter.mesh.triangles;
        newMesh.normals = meshFilter.mesh.normals;
        meshCollider.sharedMesh = newMesh;

        // 업데이트된 메쉬의 버텍스 배열 복사
        initialVertices = meshFilter.mesh.vertices.Clone() as Vector3[];
    }
}
