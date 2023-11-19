using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public string sceneName = "Menu";

    public void ClickHard()
    {
        Debug.Log("로딩");
        SceneManager.LoadScene("Hard");
    }

    public void ClickSoft()
    {
        Debug.Log("로드");
    }

    public void ClickClay()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}