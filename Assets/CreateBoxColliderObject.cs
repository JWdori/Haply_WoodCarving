using UnityEngine;

public class CreateBoxColliderObject : MonoBehaviour
{
    // 다른 스크립트에서 호출하기 위한 메서드
    public void GenerateBoxColliderObject()
    {
        CreateObjectWithBoxCollider();
    }

    void CreateObjectWithBoxCollider()
    {
        // 원본 오브젝트의 MeshFilter와 MeshRenderer를 가져옴
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (renderer != null && meshFilter != null)
        {
            // 새로운 오브젝트 생성
            GameObject newObject = new GameObject(gameObject.name + "_boxcollider");
            newObject.transform.position = transform.position;

            // 새로운 오브젝트에 BoxCollider 컴포넌트 추가
            BoxCollider collider = newObject.AddComponent<BoxCollider>();
            collider.center = renderer.bounds.center - transform.position;
            collider.size = renderer.bounds.size;

            // 새로운 오브젝트를 현재 오브젝트의 자식으로 설정 (필요하다면 이 부분은 제거 가능)
            newObject.transform.SetParent(transform);
        }
    }
}
