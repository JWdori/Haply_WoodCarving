using UnityEngine;

public class MassCollisionHandler : MonoBehaviour
{
    public MassSpringSystem massSpringSystem;

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // �浹 ������ �׸��� ��ǥ�� ��ȯ
            Vector3 gridPoint = new Vector3(1, 1, 1);
            //Debug.Log(gridPoint);
            // ���� �� ����
            float appliedForce = -massSpringSystem.CollisionForce;

            // �׸��忡 �� ����
            massSpringSystem.ApplyForceToGrid(gridPoint, appliedForce);
        }

        // Renderer ������Ʈ�� ������ ������ ���������� ����
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }



}
