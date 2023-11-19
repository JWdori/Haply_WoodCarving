
//===========================================================================================
// Summary
//===========================================================================================

/**
 * This class maintains an array of game objects and periodically updates their positions
 * in the FixedUpdate function. It can be used to spawn a collection of objects that are
 * clones of the public 'MassPrefab' GameObject member variable.
 * 
 * The positions of the objects can be set externally using the public UpdatePositions 
 * function. The FixedUpdate function then assigns the latest positions maintained in the
 * positions array to each object in the collection.
 * 
 * This class is designed to act as a game world spawner for a Mass Spring system that is
 * based around a Y=up coordinate system. Input positions in the SpawnPrimitives and 
 * UpdatePositions functions are therefore translated from the Mass Spring system
 * coordㄹinates to Unity world coordinates by swapping Y and Z values. 
 */
using System.Collections.Generic; 
using UnityEngine;
using System.Collections;

public class MassSpawner : MonoBehaviour
{
    public GameObject MassPrefab;

    private float MassUnitSize;
    private List<GameObject> Primitives = new List<GameObject>();
    private Vector3[] positions;
    private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();






        // 나머지 코드...
    



    //===========================================================================================
    //Overrides
    //===========================================================================================
    void FixedUpdate()
    {
        float distanceThreshold = 0.05f; // 오브젝트가 이 거리 이상 벗어나면 파괴

        for (int i = 0; i < Primitives.Count; ++i)
        {
            GameObject currentPrimitive = Primitives[i];
            Vector3 currentPosition = currentPrimitive.transform.position; // 현재 오브젝트의 실제 위치

            // 초기 위치와 현재 위치 사이의 거리가 임계값을 초과하면 파괴
            if (initialPositions.ContainsKey(currentPrimitive) &&
                Vector3.Distance(initialPositions[currentPrimitive], currentPosition) > distanceThreshold)
            {
                Destroy(currentPrimitive);
                Primitives.RemoveAt(i);
                initialPositions.Remove(currentPrimitive);
                i--; // 인덱스 조정
            }
        }
    }


    //===========================================================================================
    // Setter Functions
    //===========================================================================================
    public void SetMassUnitSize(float length) { MassUnitSize = length; }

    //===========================================================================================
    // Initialisation
    //===========================================================================================

    public void SpawnPrimitives(Vector3[] p)
    {
        GameObject woodParent = GameObject.Find("WOOD") ?? new GameObject("WOOD");

        foreach (GameObject obj in Primitives)
            Destroy(obj.gameObject);
        Primitives.Clear();

        positions = p; //이거 개수임.
        int numPositions = positions.Length;
        Primitives.Clear();
        foreach (Vector3 massPosition in positions)
        {
            //translate y to z so we can use Unity's in-built gravity on the y axis.
            Vector3 worldPosition = TranslateToUnityWorldSpace(massPosition);

            Object springUnit = Instantiate(MassPrefab, worldPosition, Quaternion.identity);
            GameObject springMassObject = (GameObject)springUnit;
            springMassObject.transform.SetParent(woodParent.transform);

            springMassObject.transform.localScale = Vector3.one * MassUnitSize;

            Rigidbody rb = springMassObject.AddComponent<Rigidbody>();
            rb.mass = 100000.0f; // 기본 mass 값 설정
            rb.drag = Mathf.Infinity; // 이론적으로 무한대 값을 설정할 수 있으나, 실제로는 매우 큰 값이 됩니다.
            rb.angularDrag = Mathf.Infinity; // 회전 저항도 무한대로 설정
            rb.useGravity = false;
            rb.isKinematic = true;


            Primitives.Add(springMassObject); // springMassObject는 이미 GameObject 타입입니다.
            initialPositions[springMassObject] = worldPosition; // 초기 위치 저장

        }
    }

    //===========================================================================================
    // Position Updating
    //===========================================================================================

    public void UpdatePositions(Vector3[] p)
    {
        positions = p;
    }

    //===========================================================================================
    // Helper Functions
    //===========================================================================================

    private Vector3 TranslateToUnityWorldSpace(Vector3 gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.z, gridPosition.y);
    }



}


