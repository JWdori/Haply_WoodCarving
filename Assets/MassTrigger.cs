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

        //     ���� ������ �޽��� Ʈ���Ÿ� Ȱ��ȭ
        //    Collider collider = GetComponent<Collider>();
        //    if (collider != null)
        //    {
        //        collider.isTrigger = false;
        //    }            // Rigidbody�� �ִٸ�, Ű�׸�ƽ ��Ȱ��ȭ
        //    Rigidbody rb = GetComponent<Rigidbody>();

        //     �浹 �߻� �� Ű�׸�ƽ Ȱ��ȭ
        //    if (rb != null)
        //    {
        //        rb.isKinematic = false;
        //    }

        //}

    }

    void OnTriggerExit(Collider other)
    {
        //// Ʈ���Ű� ���� �� Ʈ���� ����
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
