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

        // �ʱ� ���ؽ� �迭 ����
        initialVertices = meshFilter.mesh.vertices.Clone() as Vector3[];
        currentVertices = meshFilter.mesh.vertices;
    }

    void Update()
    {
        // MeshCollider�� ���� ���, �޼��� ����
        if (meshCollider == null) return;

        currentVertices = meshFilter.mesh.vertices; // ���� ���ؽ� ������ ����

        // �޽� ���� ����
        if (HasMeshDeformed())
        {
            // �޽� �ݶ��̴� ������Ʈ
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

        // ������Ʈ�� �޽��� ���ؽ� �迭 ����
        initialVertices = meshFilter.mesh.vertices.Clone() as Vector3[];
    }
}
