using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonData : MonoBehaviour  // MonoBehaviour 상속 추가
{
    public float SharedValue { get; private set; }
    public Vector3 velo { get; private set; }
    public bool Edge { get; private set; }


    // 데이터 설정
    public void SetData(float value, Vector3 velocity)
    {
        SharedValue = value;
        velo = velocity;
    }

    public void SetEdge(bool edge)
    {
        Edge = edge;
    }



    // 데이터 가져오기
    public float GetData()
    {
        return SharedValue;
    }

    public Vector3 GetDataVelo()
    {
        return velo;
    }


    public bool GetEdge()
    {
        return Edge;
    }
}
