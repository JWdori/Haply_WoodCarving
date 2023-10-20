using UnityEngine;

public class Object_Rotation : MonoBehaviour
{
    public GameObject targetObject; // 회전시킬 오브젝트를 직접 할당할 변수
    public float rotationSpeed = 30.0f; // 회전 속도 조절용 변수

    void Update()
    {
        // A 키를 누르면 할당된 오브젝트를 왼쪽으로 회전
        if (Input.GetKey(KeyCode.A))
        {
            // 왼쪽으로 회전하는 코드
            targetObject.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        // D 키를 누르면 할당된 오브젝트를 오른쪽으로 회전
        if (Input.GetKey(KeyCode.D))
        {
            // 오른쪽으로 회전하는 코드
            targetObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
