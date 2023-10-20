using UnityEngine;

public class Object_Rotation : MonoBehaviour
{
    public GameObject targetObject; // ȸ����ų ������Ʈ�� ���� �Ҵ��� ����
    public float rotationSpeed = 30.0f; // ȸ�� �ӵ� ������ ����

    void Update()
    {
        // A Ű�� ������ �Ҵ�� ������Ʈ�� �������� ȸ��
        if (Input.GetKey(KeyCode.A))
        {
            // �������� ȸ���ϴ� �ڵ�
            targetObject.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        // D Ű�� ������ �Ҵ�� ������Ʈ�� ���������� ȸ��
        if (Input.GetKey(KeyCode.D))
        {
            // ���������� ȸ���ϴ� �ڵ�
            targetObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
