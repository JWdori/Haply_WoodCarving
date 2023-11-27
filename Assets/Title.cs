using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public string sceneName = "Menu";

    public void ClickHard()
    {
        Debug.Log("·Îµù");
        SceneManager.LoadScene("Hard");
    }

    public void ClickSoft()
    {
        SceneManager.LoadScene("Soft");
    }

    public void ClickClay()
    {
        SceneManager.LoadScene("Clay");
    }
}