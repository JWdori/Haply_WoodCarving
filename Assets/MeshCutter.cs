using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    public GameObject KnifeObject;

    private Mesh mesh;
    private Vector3[] initialVertices;
    private Vector3[] currentVertices;
    private MeshCollider meshCollider;
    private Rigidbody knifeRb;  // 나이프의 Rigidbody
    private Vector3 previousKnifePosition;  // 이전 프레임에서의 나이프 위치

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        initialVertices = mesh.vertices;
        currentVertices = mesh.vertices;
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh;

        if (KnifeObject)
        {
            knifeRb = KnifeObject.GetComponent<Rigidbody>();
            previousKnifePosition = KnifeObject.transform.position;  // 초기 위치 설정
        }
    }

    void Update()
    {
        if (KnifeObject == null) return;

        Collider knifeCollider = KnifeObject.GetComponent<Collider>();

        if (!knifeCollider) return;

        // 속도 및 방향 계산
        Vector3 direction = (KnifeObject.transform.position - previousKnifePosition).normalized;
        float speed = (KnifeObject.transform.position - previousKnifePosition).magnitude / Time.deltaTime;

        // 속도 및 방향 출력
        Debug.Log("Direction: " + direction);
        Debug.Log("Speed: " + speed);

        if (meshCollider.bounds.Intersects(knifeCollider.bounds))
        {
            Vector3 knifePosition = KnifeObject.transform.position;
            Vector3[] meshVertices = mesh.vertices;

            for (int i = 0; i < meshVertices.Length; i++)
            {
                Vector3 worldVertex = transform.TransformPoint(meshVertices[i]);

                // y축과 x축 둘 다 비교, 이동 방향에 따른 조건
                if (worldVertex.y > knifePosition.y && (direction.x > 0 ? worldVertex.x > knifePosition.x : worldVertex.x < knifePosition.x))
                {
                    worldVertex.y = knifePosition.y;
                    worldVertex.x = direction.x > 0 ? knifePosition.x : worldVertex.x;

                    meshVertices[i] = transform.InverseTransformPoint(worldVertex);
                }
            }

            mesh.vertices = meshVertices;
            mesh.RecalculateBounds();

            // MeshCollider 업데이트
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        // 현재 위치를 이전 위치로 업데이트
        previousKnifePosition = KnifeObject.transform.position;
    }
}
