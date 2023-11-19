using UnityEngine;

public class Katana : MonoBehaviour
{
    private async void OnTriggerEnter(Collider other)
    {
        await GameManager.instance.Cut(other.gameObject);
    }
}
