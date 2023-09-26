using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    public GameObject KnifeObject;

    private Mesh mesh;
    private Vector3[] initialVertices;
    private Vector3[] currentVertices;
    private MeshCollider meshCollider;
    private Rigidbody knifeRb;  // �������� Rigidbody
    private Vector3 previousKnifePosition;  // ���� �����ӿ����� ������ ��ġ

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
            previousKnifePosition = KnifeObject.transform.position;  // �ʱ� ��ġ ����
        }
    }

    void Update()
    {
        if (KnifeObject == null) return;

        Collider knifeCollider = KnifeObject.GetComponent<Collider>();

        if (!knifeCollider) return;

        // �ӵ� �� ���� ���
        Vector3 direction = (KnifeObject.transform.position - previousKnifePosition).normalized;
        float speed = (KnifeObject.transform.position - previousKnifePosition).magnitude / Time.deltaTime;

        // �ӵ� �� ���� ���
        Debug.Log("Direction: " + direction);
        Debug.Log("Speed: " + speed);

        if (meshCollider.bounds.Intersects(knifeCollider.bounds))
        {
            Vector3 knifePosition = KnifeObject.transform.position;
            Vector3[] meshVertices = mesh.vertices;

            for (int i = 0; i < meshVertices.Length; i++)
            {
                Vector3 worldVertex = transform.TransformPoint(meshVertices[i]);

                // y��� x�� �� �� ��, �̵� ���⿡ ���� ����
                if (worldVertex.y > knifePosition.y && (direction.x > 0 ? worldVertex.x > knifePosition.x : worldVertex.x < knifePosition.x))
                {
                    worldVertex.y = knifePosition.y;
                    worldVertex.x = direction.x > 0 ? knifePosition.x : worldVertex.x;

                    meshVertices[i] = transform.InverseTransformPoint(worldVertex);
                }
            }

            mesh.vertices = meshVertices;
            mesh.RecalculateBounds();

            // MeshCollider ������Ʈ
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        // ���� ��ġ�� ���� ��ġ�� ������Ʈ
        previousKnifePosition = KnifeObject.transform.position;
    }
}
