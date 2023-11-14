using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeIndividualFaces : MonoBehaviour
{

    public int gridSize;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";

        CreateVertices();
        CreateTriangles();
    }

    private void CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
        int faceVertices = (
            (gridSize - 1) * (gridSize - 1) +
            (gridSize - 1) * (gridSize - 1) +
            (gridSize - 1) * (gridSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        // ... ���� ��ġ�� ����ϴ� ���� ...
    }

    private void CreateTriangles()
    {
        int[] trianglesZ = new int[(gridSize * gridSize) * 12];
        int[] trianglesX = new int[(gridSize * gridSize) * 12];
        int[] trianglesY = new int[(gridSize * gridSize) * 12];
        // ... �ﰢ���� ����ϴ� ���� ...
        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);
    }

    // ���⿡ OnDrawGizmos�� �̿��Ͽ� Gizmo�� �׸��� �ڵ带 �߰��� �� �ֽ��ϴ�.
}
