using UnityEngine;

public class CreateBoxColliderObject : MonoBehaviour
{
    // �ٸ� ��ũ��Ʈ���� ȣ���ϱ� ���� �޼���
    public void GenerateBoxColliderObject()
    {
        CreateObjectWithBoxCollider();
    }

    void CreateObjectWithBoxCollider()
    {
        // ���� ������Ʈ�� MeshFilter�� MeshRenderer�� ������
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (renderer != null && meshFilter != null)
        {
            // ���ο� ������Ʈ ����
            GameObject newObject = new GameObject(gameObject.name + "_boxcollider");
            newObject.transform.position = transform.position;

            // ���ο� ������Ʈ�� BoxCollider ������Ʈ �߰�
            BoxCollider collider = newObject.AddComponent<BoxCollider>();
            collider.center = renderer.bounds.center - transform.position;
            collider.size = renderer.bounds.size;

            // ���ο� ������Ʈ�� ���� ������Ʈ�� �ڽ����� ���� (�ʿ��ϴٸ� �� �κ��� ���� ����)
            newObject.transform.SetParent(transform);
        }
    }
}
