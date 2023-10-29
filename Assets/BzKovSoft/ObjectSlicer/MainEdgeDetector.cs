using System.Collections.Generic;
using UnityEngine;

public class MainEdgeDetector : MonoBehaviour
{
    [SerializeField]
    private Dictionary<string, List<Vector3>> objectEdgesData = new Dictionary<string, List<Vector3>>();

    private void Start()
    {

    }
    // 이벤트 핸들러 함수
    public void OnEdgesDetectedHandler(string objectName, List<Vector3> edges)
    {
        //Debug.Log(objectName);

        // 딕셔너리에 오브젝트 이름과 해당하는 좌표값 리스트를 저장
        if (objectEdgesData.ContainsKey(objectName))
        {
            // 기존 데이터가 이미 있는 경우, 모서리 데이터를 새로운 데이터로 덮어쓰기
            objectEdgesData[objectName].Clear();
            objectEdgesData[objectName].AddRange(edges);
        }
        else
        {
            objectEdgesData[objectName] = new List<Vector3>(edges);
        }

    }

    // 딕셔너리에서 오브젝트 이름에 해당하는 좌표값 리스트를 얻는 함수
    public List<Vector3> GetEdgesData(string objectName)
    {
        if (objectEdgesData.ContainsKey(objectName))
        {
            return objectEdgesData[objectName];
        }
        else
        {
            return null; // 오브젝트 이름에 해당하는 데이터가 없을 경우
        }
    }

    private void Update()
    {
        // 'A' 키를 누르면 모든 오브젝트의 이름과 모서리 총 개수를 콘솔에 출력
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
