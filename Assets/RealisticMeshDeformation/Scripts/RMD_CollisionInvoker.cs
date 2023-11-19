//----------------------------------------------
//        Realistic Mesh Deformation
//
// Copyright ?2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
// Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("BoneCracker Games/Realistic Mesh Deformation/RMD Collision Invoker")]
public class RMD_CollisionInvoker : MonoBehaviour {

    private RMD_Deformation deformationParent;

    private void Awake() {

        CheckParent();

    }

    public void OnCollisionStay(Collision collision)
    {

        deformationParent.OnCollisionStay(collision);

    }

    public void OnCollisionEnter(Collision collision)
    {

        deformationParent.OnCollisionEnter(collision);
        Debug.Log("gd");

    }


    private void CheckParent() {

        deformationParent = GetComponentInParent<RMD_Deformation>();

        if (!deformationParent)
            Destroy(this);

    }

}
