using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Katana : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameManager.instance.Cut(other.gameObject);
    }
}
