using UnityEngine;

public class Decrease : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // �������� �浹�� ������Ʈ ������ ���� ���͸� ����մϴ�.
        Vector3 directionToOther = other.transform.position - transform.position;
        StartCoroutine(ShrinkObject(other.gameObject, directionToOther));
    }

    System.Collections.IEnumerator ShrinkObject(GameObject obj, Vector3 direction)
    {
        direction = direction.normalized;

        Vector3 originalScale = obj.transform.localScale;
        Vector3 targetScale = originalScale;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            targetScale.x *= 0.9f;
        }
        else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) && Mathf.Abs(direction.y) > Mathf.Abs(direction.z))
        {
            targetScale.y *= 0.9f;
        }
        else
        {
            targetScale.z *= 0.9f;
        }

        float elapsedTime = 0;
        float shrinkTime = 1.0f; // 1�� ���� �پ��ϴ�.

        while (elapsedTime < shrinkTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 newScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / shrinkTime);
            obj.transform.localScale = newScale;

            yield return null;
        }
    }
}
