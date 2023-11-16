using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataPlayer : MonoBehaviour
{
    [HideInInspector]
    public GameObject targetObject; // 위치를 가져올 대상 오브젝트의 참조
    //public HapticController hapticController;
    public AdvancedPhysicsHapticEffector hapticController;

    [HideInInspector]
    public Vector3 position; // 인스펙터에서 수정 가능한 포지션 변수
    private bool startBtn = false;
    [HideInInspector]
    public TextAsset csvFile; // csv 파일을 할당받을 변수

    private string[] lines; // CSV 파일의 라인들을 저장할 배열
    private int currentLineIndex = 1; // CSV 파일을 파싱할 때 현재 처리 중인 라인의 인덱스

    [HideInInspector]
    public GameObject testtargetObject;
    [HideInInspector]
    public Vector3 testposition; // 인스펙터에서 수정 가능한 포지션 변수

    public TMP_Text UIText; //UI


    public float maxDistance = 1f; // 최대 거리
    public float maxForce = 2f; // 최대 힘
    //public AnimationCurve distanceCurve; // 거리에 따른 힘 피드백 곡선
    //이거 필요 없을듯... 보간법인데

    private float elapsedTime = 0f; // 경과시간을 저장할 변수 추가



    private void Awake()
    {
        // CSV 파일에서 데이터를 읽어와서 lines 배열에 저장
        if (csvFile != null)
            lines = csvFile.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //Debug.Log("World Scale: " + transform.lossyScale);

    }

    private void Update()
    {
        if (startBtn && lines != null)
        {
            elapsedTime += Time.deltaTime; // 경과시간 업데이트

            // 경과시간이 현재 라인의 시간과 같거나 클 때까지 반복
            while (currentLineIndex < lines.Length && elapsedTime >= float.Parse(lines[currentLineIndex].Split(',')[1]))
            {
                string line = lines[currentLineIndex];
                string[] data = line.Split(new char[] { ',' });

                // 데이터를 파싱하여 변수에 저장
                if (data.Length >= 8 &&
                    float.TryParse(data[1], out float time) &&
                    float.TryParse(data[2], out float posX) &&
                    float.TryParse(data[3], out float posY) &&
                    float.TryParse(data[4], out float posZ) &&
                    float.TryParse(data[5], out float velX) &&
                    float.TryParse(data[6], out float velY) &&
                    float.TryParse(data[7], out float velZ))
                {
                    Vector3 newPosition = new Vector3(posX, posY, posZ);
                    testtargetObject.transform.position = newPosition;
                    //Debug.Log(posX + "\n"+"현재 힘피드백은"+ hapticController.forceX+ hapticController.forceY+ hapticController.forceZ);
                    CalculateForce(newPosition, time);
                    Debug.Log(time);

                }
                else
                {
                    Debug.LogError("CSV ㄹ오류. 오류 데이터: " + line);
                }


                currentLineIndex++;
            }
        }

        // 인스펙터에서 직접 수정한 포지션 값을 실시간으로 반영
        position = targetObject.transform.position;
        testposition = testtargetObject.transform.position;
        //position = transform.position;
    }

    public void Play()
    {
        testtargetObject.SetActive(true);
        startBtn = true;
        currentLineIndex = 1; // 시작할 때 첫 번째 데이터 라인부터 처리하도록 초기화합니다.
        elapsedTime = 0f; // Play 버튼을 누를 때 경과시간 초기화

    }

    public void Stop()
    {
        startBtn = false;
        hapticController.forceX = 0;
        hapticController.forceY = 0; //기본 중력
        hapticController.forceZ = 0;
    }


    private Vector3 CalculateForce(Vector3 targetPosition, float time)
    {
        Vector3 currentPosition = targetObject.transform.position;
        Vector3 distanceVector = targetPosition - currentPosition;
        Vector3 normalizedDistanceVector = new Vector3(
            Mathf.Clamp(distanceVector.x / maxDistance, -1f, 1f),
            Mathf.Clamp(distanceVector.y / maxDistance, -1f, 1f),
            Mathf.Clamp(distanceVector.z / maxDistance, -1f, 1f)
        );

        Vector3 forceVector = maxForce * Vector3.Scale(distanceVector, (Vector3.one - normalizedDistanceVector));
        forceVector = new Vector3(
            Mathf.Abs(distanceVector.x) < maxDistance ? forceVector.x : maxForce * Mathf.Sign(distanceVector.x),
            Mathf.Abs(distanceVector.y) < maxDistance ? forceVector.y : maxForce * Mathf.Sign(distanceVector.y),
            Mathf.Abs(distanceVector.z) < maxDistance ? forceVector.z : maxForce * Mathf.Sign(distanceVector.z)
        );
        Debug.Log(forceVector);
        if (currentLineIndex >= lines.Length - 1)
        {
            // CSV 파일의 끝까지 읽었으므로 힘 피드백을 0으로 초기화
            hapticController.forceX = 0;
            hapticController.forceY = 0; //기본 중력
            hapticController.forceZ = 0;
        }
        else
        {
            hapticController.forceX = forceVector.x;
            hapticController.forceY = forceVector.y;
            hapticController.forceZ = forceVector.z;
        }


        UIText.text = time.ToString()
            + "\ndistanceX: " + distanceVector.x
            + "\ndistanceY: " + distanceVector.y
            + "\ndistanceZ: " + distanceVector.z
            + "\nforceX: " + forceVector.x
            + "\nforceY: " + forceVector.y
            + "\nforceZ: " + forceVector.z;

        return forceVector;
    }

}
