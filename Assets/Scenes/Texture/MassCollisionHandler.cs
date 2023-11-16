using UnityEngine;

public class MassCollisionHandler : MonoBehaviour
{
    public MassSpringSystem massSpringSystem;

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // 충돌 지점을 그리드 좌표로 변환
            Vector3 gridPoint = new Vector3(1, 1, 1);
            //Debug.Log(gridPoint);
            // 음수 힘 적용
            float appliedForce = -massSpringSystem.CollisionForce;

            // 그리드에 힘 적용
            massSpringSystem.ApplyForceToGrid(gridPoint, appliedForce);
        }

        // Renderer 컴포넌트가 있으면 색상을 빨간색으로 변경
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }



}
