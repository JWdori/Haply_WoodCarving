using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassTrigger : MonoBehaviour
{
    private int triggerCount = 0;
    private bool start = false;




    void OnTriggerEnter(Collider other)
    {

        //if (other.gameObject.name == "Force2")
        //{

        //     새로 접촉한 메쉬의 트리거를 활성화
        //    Collider collider = GetComponent<Collider>();
        //    if (collider != null)
        //    {
        //        collider.isTrigger = false;
        //    }            // Rigidbody가 있다면, 키네매틱 비활성화
        //    Rigidbody rb = GetComponent<Rigidbody>();

        //     충돌 발생 시 키네매틱 활성화
        //    if (rb != null)
        //    {
        //        rb.isKinematic = false;
        //    }

        //}

    }

    void OnTriggerExit(Collider other)
    {
        //// 트리거가 끝날 때 트리거 해제
        //Collider collider = GetComponent<Collider>();
        //if (collider != null)
        //{
        //    collider.isTrigger = true;
        //}
        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.isKinematic = true;
        //}

    }









}
