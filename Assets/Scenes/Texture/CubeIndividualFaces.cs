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
        // ... 정점 위치를 계산하는 로직 ...
    }

    private void CreateTriangles()
    {
        int[] trianglesZ = new int[(gridSize * gridSize) * 12];
        int[] trianglesX = new int[(gridSize * gridSize) * 12];
        int[] trianglesY = new int[(gridSize * gridSize) * 12];
        // ... 삼각형을 계산하는 로직 ...
        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);
    }

    // 여기에 OnDrawGizmos를 이용하여 Gizmo를 그리는 코드를 추가할 수 있습니다.
}
