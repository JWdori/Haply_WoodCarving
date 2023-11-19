using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonData : MonoBehaviour  // MonoBehaviour ��� �߰�
{
    public float SharedValue { get; private set; }
    public Vector3 velo { get; private set; }
    public bool Edge { get; private set; }


    // ������ ����
    public void SetData(float value, Vector3 velocity)
    {
        SharedValue = value;
        velo = velocity;
    }

    public void SetEdge(bool edge)
    {
        Edge = edge;
    }



    // ������ ��������
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
