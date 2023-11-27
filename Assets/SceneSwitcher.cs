using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // 'Z' 키가 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // "Menu" 씬으로 전환
            SceneManager.LoadScene("Menu");
        }
    }
}
