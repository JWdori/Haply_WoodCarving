using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // 'Z' Ű�� ���ȴ��� Ȯ��
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // "Menu" ������ ��ȯ
            SceneManager.LoadScene("Menu");
        }
    }
}
