using System.Collections.Generic;
using UnityEngine;

public class MainEdgeDetector : MonoBehaviour
{
    [SerializeField]
    private Dictionary<string, List<Vector3>> objectEdgesData = new Dictionary<string, List<Vector3>>();

    private void Start()
    {

    }
    // �̺�Ʈ �ڵ鷯 �Լ�
    public void OnEdgesDetectedHandler(string objectName, List<Vector3> edges)
    {
        //Debug.Log(objectName);

        // ��ųʸ��� ������Ʈ �̸��� �ش��ϴ� ��ǥ�� ����Ʈ�� ����
        if (objectEdgesData.ContainsKey(objectName))
        {
            // ���� �����Ͱ� �̹� �ִ� ���, �𼭸� �����͸� ���ο� �����ͷ� �����
            objectEdgesData[objectName].Clear();
            objectEdgesData[objectName].AddRange(edges);
        }
        else
        {
            objectEdgesData[objectName] = new List<Vector3>(edges);
        }

    }

    // ��ųʸ����� ������Ʈ �̸��� �ش��ϴ� ��ǥ�� ����Ʈ�� ��� �Լ�
    public List<Vector3> GetEdgesData(string objectName)
    {
        if (objectEdgesData.ContainsKey(objectName))
        {
            return objectEdgesData[objectName];
        }
        else
        {
            return null; // ������Ʈ �̸��� �ش��ϴ� �����Ͱ� ���� ���
        }
    }

    private void Update()
    {
        // 'A' Ű�� ������ ��� ������Ʈ�� �̸��� �𼭸� �� ������ �ֿܼ� ���
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var kvp in objectEdgesData)
            {
                string objectName = kvp.Key;
                int totalEdgesCount = kvp.Value.Count;
                Debug.Log("Object Name: " + objectName + ", Total Edges Count: " + totalEdgesCount);
            }
        }
    }
}
