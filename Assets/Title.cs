using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public string sceneName = "Menu";

    public void ClickHard()
    {
        Debug.Log("�ε�");
        SceneManager.LoadScene("Hard");
    }

    public void ClickSoft()
    {
        Debug.Log("�ε�");
    }

    public void ClickClay()
    {
        Debug.Log("���� ����");
        Application.Quit();
    }
}