//----------------------------------------------
//        Realistic Mesh Deformation
//
// Copyright © 2014 - 2023 BoneCracker Games
// http://www.bonecrackergames.com
// Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("BoneCracker Games/Realistic Mesh Deformation/RMD Deformation")]
public class RMD_Deformation : MonoBehaviour {

    [Space()]
    public MeshFilter[] meshFilters = new MeshFilter[0];    //  Collected mesh filters.
    float force;
    bool EdgeCheck;
    public DeformationMode deformationMode = DeformationMode.Fast;
    [Range(1, 100)] public int deformResolution = 100;
    public LayerMask deformFilter = -1;     // LayerMask filter. Damage will be taken from the objects with these layers.
    [Range(0f, 5f)] public float randomizeVertices = 0f;        // Randomize verticies on collisions for more complex deforms.
    public float deformRadius = 1f;        // Verticies in this radius will be effected on collisions.
    public float deformMultiplier = 1f;     // Damage multiplier.
    public float maximumDeformation = 1f;       // Maximum vert distance for limiting the damage. 0 value will disable the limit.
    public float minimumDeformationImpulse = 2f;       // Minimum collision force.
    private readonly float minimumVertDistanceForDamagedMesh = .002f;        // Comparing original vertex positions between the last vertex positions to decide mesh is repaired or not.

    private struct MeshVertexStructure { public Vector3[] meshVerts; }     // Struct for original mesh verticies positions.

    private MeshVertexStructure[] originalMeshData;        // Array for struct above.
    private MeshVertexStructure[] damagedMeshData;     // Array for struct above.

    public enum DeformationMode { Smooth, Fast }

    // Meshes are currently repairing, or repaired?
    public enum RepairState { Repairing, Repaired }
    public RepairState repairState = RepairState.Repaired;

    // Meshes are currently deforming, or deformed?
    public enum DeformState { Deforming, Deformed }
    public DeformState deformState = DeformState.Deformed;

    public Transform collisionDirection;        //  Collision direction.

    [Space()]
    public bool recalculateNormals = true;      //  Recalculate normals while deforming / restoring the mesh.
    public bool recalculateBounds = true;       //  Recalculate bounds while deforming / restoring the mesh.

    private ContactPoint contactPoint = new ContactPoint();

    public float overallDeform = 0f;
    public int latestProcessedVertexCount = 0;
    bool particleOpen = false;
    public GameObject angleObject; // 각도를 구할 오브젝트

    [SerializeField] 
    private ParticleSystem woodFx;
    private ParticleSystem.EmissionModule woodFxEmission;

    float angle;

    private float lastForce; // 마지막 충돌의 힘을 저장할 변수



    public void Awake() {

        collisionDirection = new GameObject("CollisionDirection").transform;
        collisionDirection.SetParent(transform, false);
        woodFxEmission = woodFx.emission;
    }

    private void OnEnable() {

        if (meshFilters == null || meshFilters.Length < 1)
            AddAllMeshes();

        for (int i = 0; i < meshFilters.Length; i++) {

            if (meshFilters[i] && meshFilters[i].transform != transform) {

                if (meshFilters[i].gameObject.GetComponent<RMD_CollisionInvoker>() == null)
                    meshFilters[i].gameObject.AddComponent<RMD_CollisionInvoker>();

            }

        }

    }

    public void AddAllMeshes() {

        MeshFilter[] allMeshFilters = GetComponentsInChildren<MeshFilter>(true);
        List<MeshFilter> properMeshFilters = new List<MeshFilter>();

        // Model import must be readable. If it's not readable, inform the developer. We don't wanna deform wheel meshes. Exclude any meshes belongts to the wheels.
        foreach (MeshFilter mf in allMeshFilters) {

            if (mf.sharedMesh != null)
                properMeshFilters.Add(mf);

        }

        allMeshFilters = properMeshFilters.ToArray();
        meshFilters = allMeshFilters;

    }

    /// <summary>
    /// We will be using two structs for deformed sections. Original part struction, and deformed part struction. 
    /// All damaged meshes and wheel transforms will be using these structs. At this section, we're creating them with original struction.
    /// </summary>
    private void CheckMeshData() {

        originalMeshData = new MeshVertexStructure[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
            originalMeshData[i].meshVerts = meshFilters[i].mesh.vertices;

        damagedMeshData = new MeshVertexStructure[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
            damagedMeshData[i].meshVerts = meshFilters[i].mesh.vertices;

    }

    private void Update() {

        //  If vehicle is not deformed completely, and deforming is enabled, deform all meshes to their damaged structions.
        if (deformState == DeformState.Deforming)
            UpdateDeformation();

        //  If vehicle is not repaired completely, and repairNow is enabled, restore all deformed meshes to their original structions.
        if (repairState == RepairState.Repairing)
            UpdateRepair();
        CommonData commonData = GetComponent<CommonData>();

        if (commonData != null)
        {
            force = commonData.GetData();
            EdgeCheck = commonData.GetEdge();
        }
        //Debug.Log(force+5);
        //EdgeDetector edgeDetector = GetComponent<EdgeDetector>();

        //if (edgeDetector != null)
        //{

        //    List<Vector3> edges = edgeDetector.GetEdgePositions();
        //        // 모서리 그리기
        //        for (int i = 0; i < edges.Count; i += 2)
        //        {
        //            Vector3 edgeStart = gameObject.transform.TransformPoint(edges[i]);
        //            Vector3 edgeEnd = gameObject.transform.TransformPoint(edges[i + 1]);

        //        // 라인 그리기
        //        Debug.DrawLine(edgeStart, edgeEnd, Color.red); // 위치 인자 두 개와 색상 인자 하나를 받음

        //    }
        //}


        // 오브젝트의 forward 벡터
        Vector3 objectForward = angleObject.gameObject.transform.forward;

        // z축 벡터 (Unity의 전역 좌표계에서 z축은 Vector3.forward로 표현됩니다)
        Vector3 zAxis = Vector3.forward;

        // 두 벡터 사이의 각도 계산
        angle = Vector3.Angle(objectForward, zAxis);

        // 각도 출력
        //Debug.Log(angle);

    }



    void OnGUI()
    {
        // 화면의 오른쪽 상단에 위치시키기 위한 설정
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        int boxWidth = 200;
        int boxHeight = 50;

        // 상자의 위치를 설정
        Rect boxRect = new Rect(screenWidth - boxWidth - 10, 10, boxWidth, boxHeight);

        // 상자와 레이블을 그리기
        GUI.Box(boxRect, "Info");
        GUI.Label(new Rect(screenWidth - boxWidth, 30, boxWidth, 20), $"Angle: {angle}");
        GUI.Label(new Rect(screenWidth - boxWidth, 50, boxWidth, 20), $"Force: {force}");
    }


    void OnDrawGizmos()
    {
        if (meshFilters != null)
        {
            Gizmos.color = Color.red; // 빨간색으로 설정
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf != null && mf.mesh != null)
                {
                    // 각 메쉬의 모든 버텍스에 대해 반복
                    foreach (Vector3 vertex in mf.mesh.vertices)
                    {
                        // 버텍스를 월드 좌표로 변환
                        Vector3 worldVertex = mf.transform.TransformPoint(vertex);
                        // 해당 위치에 구체 그리기
                        Gizmos.DrawSphere(worldVertex, 0.01f); // 구체의 크기는 0.01 단위로 설정
                    }
                }
            }
        }
    }



    /// <summary>
    /// Moving deformed vertices to their original positions while repairing.
    /// </summary>
    public void UpdateRepair() {

        if (originalMeshData == null || originalMeshData.Length < 1)
            CheckMeshData();

        int k;
        bool repaired = true;

        //  If deformable mesh is still exists, get all verticies of the mesh first. And then move all single verticies to the original positions. If verticies are close enough to the original
        //  position, repaired = true;
        for (k = 0; k < meshFilters.Length; k++) {

            if (meshFilters[k] != null && meshFilters[k].mesh != null) {

                //  Get all verticies of the mesh first.
                Vector3[] vertices = meshFilters[k].mesh.vertices;

                for (int i = 0; i < vertices.Length; i++) {

                    //  And then move all single verticies to the original positions
                    if (deformationMode == DeformationMode.Smooth)
                        vertices[i] += (originalMeshData[k].meshVerts[i] - vertices[i]) * (Time.deltaTime * 5f);
                    else
                        vertices[i] += (originalMeshData[k].meshVerts[i] - vertices[i]);

                    //  If verticies are close enough to their original positions, repaired = true;
                    if ((originalMeshData[k].meshVerts[i] - vertices[i]).magnitude >= minimumVertDistanceForDamagedMesh)
                        repaired = false;

                }

                //  We were using the variable named "vertices" above, therefore we need to set the new verticies to the damaged mesh data.
                //  Damaged mesh data also restored while repairing with this proccess.
                damagedMeshData[k].meshVerts = vertices;

                //  Setting new verticies to the all meshes. Recalculating normals and bounds, and then optimizing. This proccess can be heavy for high poly meshes.
                //  You may want to disable last three lines.
                meshFilters[k].mesh.SetVertices(vertices);

                if (recalculateNormals)
                    meshFilters[k].mesh.RecalculateNormals();

                if (recalculateBounds)
                    meshFilters[k].mesh.RecalculateBounds();

            }

        }

        //  If all meshes are completely restored, make sure repairing now is false.
        if (repaired)
            repairState = RepairState.Repaired;

    }

    /// <summary>
    /// Moving vertices of the collided meshes to the damaged positions while deforming.
    /// </summary>
    public void UpdateDeformation() {

        if (originalMeshData == null || originalMeshData.Length < 1)
            CheckMeshData();

        int k;
        bool deformed = true;

        //  If deformable mesh is still exists, get all verticies of the mesh first. And then move all single verticies to the damaged positions. If verticies are close enough to the original
        //  position, deformed = true;
        for (k = 0; k < meshFilters.Length; k++) {

            if (meshFilters[k] != null && meshFilters[k].mesh != null) {

                //  Get all verticies of the mesh first.
                Vector3[] vertices = meshFilters[k].mesh.vertices;

                //  And then move all single verticies to the damaged positions.
                for (int i = 0; i < vertices.Length; i++) {

                    //  Getting closest point to the mesh. Distance value will be set to closest point of the mesh - contact point.
                    float distance = (damagedMeshData[k].meshVerts[i] - vertices[i]).magnitude;

                    if (distance > .05f) {

                        if (Vector3.Distance(vertices[i], damagedMeshData[k].meshVerts[i]) > minimumVertDistanceForDamagedMesh) {

                            deformed = false;

                            if (deformationMode == DeformationMode.Smooth)
                                vertices[i] += (damagedMeshData[k].meshVerts[i] - vertices[i]) * (Time.deltaTime * 20f);
                            else
                                vertices[i] += (damagedMeshData[k].meshVerts[i] - vertices[i]);

                        }

                        latestProcessedVertexCount++;

                    }

                }

                //  Setting new verticies to the all meshes. Recalculating normals and bounds, and then optimizing. This proccess can be heavy for high poly meshes.
                meshFilters[k].mesh.SetVertices(vertices);

                if (recalculateNormals)
                    meshFilters[k].mesh.RecalculateNormals();

                if (recalculateBounds)
                    meshFilters[k].mesh.RecalculateBounds();

            }

        }

        //  If all meshes are completely deformed, make sure deforming is false.
        if (deformed)
            deformState = DeformState.Deformed;

    }

    /// <summary>
    /// Deforming meshes.
    /// </summary>
    /// <param name="collision"></param>
    /// <param name="impulse"></param>
    private void DamageMesh(float impulse) {

        if (originalMeshData == null || originalMeshData.Length < 1)
            CheckMeshData();

        float res = (deformResolution / 10f);
        res = (int)Mathf.Lerp(11f, 1f, res / 11f);

        //  We will be checking all mesh filters with these contact points. If contact point is close enough to the mesh, deformation will be applied.
        for (int i = 0; i < meshFilters.Length; i++) {

            //  If mesh filter is not null, enabled, and has a valid mesh data...
            if (meshFilters[i] != null && meshFilters[i].mesh != null && meshFilters[i].gameObject.activeSelf) {

                //  Getting closest point to the mesh. Distance value will be set to closest point of the mesh - contact point.
                float distance = Vector3.Distance(NearestVertex(meshFilters[i].transform, meshFilters[i], contactPoint.point), contactPoint.point);

                Quaternion collisionNormal = Quaternion.FromToRotation(Vector3.forward, contactPoint.normal);
                collisionDirection.rotation = collisionNormal;

                //  If distance between contact point and closest point of the mesh is in range...
                if (distance <= deformRadius) {

                    //  All vertices of the mesh.
                    Vector3[] vertices = damagedMeshData[i].meshVerts;

                    //  Contact point is a world space unit. We need to transform to the local space unit with mesh origin. Verticies are local space units.
                    Vector3 point = meshFilters[i].transform.InverseTransformPoint(contactPoint.point);

                    for (int k = 0; k < vertices.Length; k += (int)res) {

                        //  Distance between vertex and contact point.
                        float distanceToVert = (point - vertices[k]).magnitude;

                        //  If distance between vertex and contact point is in range...
                        if (distanceToVert <= deformRadius) {

                            //  Default impulse of the collision.
                            float damage = impulse;

                            // The damage should decrease with distance from the contact point.
                            damage -= damage * Mathf.Clamp01(distanceToVert / deformRadius);

                            //  Randomizing vectors.
                            Vector3 randomizedVector = new Vector3(UnityEngine.Random.Range(-randomizeVertices, randomizeVertices), UnityEngine.Random.Range(-randomizeVertices, randomizeVertices), UnityEngine.Random.Range(-randomizeVertices, randomizeVertices));

                            collisionDirection.position = meshFilters[i].transform.TransformPoint(vertices[k]);

                            if (randomizeVertices > 0)
                                collisionDirection.localRotation *= Quaternion.Euler(randomizedVector);

                            collisionDirection.position += (collisionDirection.forward * damage * (deformMultiplier / 10f));
                            vertices[k] = meshFilters[i].transform.InverseTransformPoint(collisionDirection.position);

                            //  If distance between original vertex position and deformed vertex position exceeds limits, make sure they are in the limits.
                            if (maximumDeformation > 0 && ((vertices[k] - originalMeshData[i].meshVerts[k]).magnitude) > maximumDeformation)
                                vertices[k] = originalMeshData[i].meshVerts[k] + (vertices[k] - originalMeshData[i].meshVerts[k]).normalized * (maximumDeformation);

                            overallDeform += (damage * (deformMultiplier)) / vertices.Length;
                        }

                    }

                }
            }

        }

    }


    bool IsPointOnEdge(Vector3 edgeStart, Vector3 edgeEnd, Vector3 point, float error)
    {
        // 모서리의 길이
        float edgeLength = Vector3.Distance(edgeStart, edgeEnd);
        // 모서리 시작점과 점 사이의 거리
        float distanceFromStart = Vector3.Distance(edgeStart, point);
        // 모서리 끝점과 점 사이의 거리
        float distanceFromEnd = Vector3.Distance(edgeEnd, point);
        // 점이 모서리 위에 있으면 true 반환
        // (두 거리의 합이 모서리 길이와 거의 같을 때로 판단)
        return Mathf.Abs(distanceFromStart + distanceFromEnd - edgeLength) < error;
    }


    /// <summary>
    /// Raises the collision enter event.
    /// </summary>
    /// <param name="collision">Collision.</param>
    public void OnCollisionStay(Collision collision) {

        if (gameObject.scene.name == "Hard")
        {
            EdgeDetector edgeDetector = GetComponent<EdgeDetector>();
            float distance = 0.005f;
            if (edgeDetector != null)
            {
                List<Vector3> edges = edgeDetector.GetEdgePositions();
                // 모서리 그리기
                for (int i = 0; i < edges.Count; i += 2)
                {
                    Vector3 edgeStart = transform.TransformPoint(edges[i]);
                    Vector3 edgeEnd = transform.TransformPoint(edges[i + 1]);
                    //CalculateContactAngle(angleObject, otherGameObject);
                    Vector3 planePoint = collision.gameObject.transform.position; // angleObject의 중심 좌표
                    if (IsPointOnEdge(edgeStart, edgeEnd, planePoint, distance))
                    {
                        if (((1 << collision.gameObject.layer) & deformFilter) != 0)
                        {

                            if (angle >= 5 && angle <= 20)
                            {
                                if (force < 10)
                                    force = 0f;

                                if (force > 10f)
                                {
                                    lastForce = force;
                                    force = 10f;
                                }

                                if (force > 0f)
                                {

                                    repairState = RepairState.Repaired;
                                    deformState = DeformState.Deforming;

                                    contactPoint = new ContactPoint();
                                    contactPoint = collision.GetContact(0);

                                    if (meshFilters != null && meshFilters.Length >= 1)
                                        DamageMesh(force);

                                    latestProcessedVertexCount = 0;

                                }
                            }
                            else
                            {
                                if (force < 20)
                                    force = 0f;

                                if (force > 20f)
                                {
                                    lastForce = force;
                                    force = 10f;
                                }

                                if (force > 0f)
                                {

                                    repairState = RepairState.Repaired;
                                    deformState = DeformState.Deforming;

                                    contactPoint = new ContactPoint();
                                    contactPoint = collision.GetContact(0);

                                    if (meshFilters != null && meshFilters.Length >= 1)
                                        DamageMesh(force);

                                    latestProcessedVertexCount = 0;

                                }
                            }

                        }
                    }



                }

            }
        }else if(gameObject.scene.name == "Clay")
        {


            if (((1 << collision.gameObject.layer) & deformFilter) != 0)
            {

                if (force < minimumDeformationImpulse)
                {
                    force = 0f;
                }
                if (force > 0f && force < 0.6f)
                {
                    force *= 2.5f;
                }
                else if (force > 0.6f && force < 1.2f)
                {
                    force *= 1.8f;
                }
                else if (force > 3f)
                {
                    force *= 1f;
                }
                if (force > 0f)
                {

                    repairState = RepairState.Repaired;
                    deformState = DeformState.Deforming;

                    contactPoint = new ContactPoint();
                    contactPoint = collision.GetContact(0);

                    if (meshFilters != null && meshFilters.Length >= 1)
                        DamageMesh(force);

                    latestProcessedVertexCount = 0;
                }

            }
            

        }




    }
    public void OnCollisionExit(Collision collision)
    {
        if (gameObject.scene.name == "Hard") {
            if (lastForce > 9)
            {
                Debug.Log(lastForce);
                woodFxEmission.enabled = true;
                woodFx.Play();
                lastForce = 0;
            }
        }

    }

    public void OnCollisionEnter(Collision collision)
    {

        if (gameObject.scene.name == "Hard")
        {
            EdgeDetector edgeDetector = GetComponent<EdgeDetector>();

            float distance = 0.005f;
            if (edgeDetector != null)
            {
                List<Vector3> edges = edgeDetector.GetEdgePositions();
                // 모서리 그리기
                for (int i = 0; i < edges.Count; i += 2)
                {
                    Vector3 edgeStart = transform.TransformPoint(edges[i]);
                    Vector3 edgeEnd = transform.TransformPoint(edges[i + 1]);
                    //CalculateContactAngle(angleObject, otherGameObject);
                    Vector3 planePoint = collision.gameObject.transform.position; // angleObject의 중심 좌표





                    if (IsPointOnEdge(edgeStart, edgeEnd, planePoint, distance))
                    {
                        if (((1 << collision.gameObject.layer) & deformFilter) != 0)
                        {
                            if (angle >= 5 && angle <= 20)
                            {
                                if (force < 10)
                                    force = 0f;

                                if (force > 10f)
                                {
                                    lastForce = force;
                                    force = 10f;
                                }

                                if (force > 0f)
                                {

                                    repairState = RepairState.Repaired;
                                    deformState = DeformState.Deforming;

                                    contactPoint = new ContactPoint();
                                    contactPoint = collision.GetContact(0);

                                    if (meshFilters != null && meshFilters.Length >= 1)
                                        DamageMesh(force);

                                    latestProcessedVertexCount = 0;

                                }
                            }
                            else
                            {
                                if (force < 20)
                                    force = 0f;

                                if (force > 20f)
                                {
                                    lastForce = force;
                                    force = 10f;
                                }

                                if (force > 0f)
                                {

                                    repairState = RepairState.Repaired;
                                    deformState = DeformState.Deforming;

                                    contactPoint = new ContactPoint();
                                    contactPoint = collision.GetContact(0);

                                    if (meshFilters != null && meshFilters.Length >= 1)
                                        DamageMesh(force);

                                    latestProcessedVertexCount = 0;

                                }
                            }

                        }
                    }



                }

            }
        }
        else if (gameObject.scene.name == "Clay")
        {

            if (((1 << collision.gameObject.layer) & deformFilter) != 0)
            {


                if (force < minimumDeformationImpulse)
                {
                    force = 0f;
                }
                if (force > 0f && force < 0.6f)
                {
                    force *= 1.5f;
                }
                else if (force > 0.6f && force < 1.2f)
                {
                    force *= 1.25f;
                }
                else if (force > 3f)
                {
                    force *= 0.875f;
                }
                if (force > 0f)
                {

                    repairState = RepairState.Repaired;
                    deformState = DeformState.Deforming;

                    contactPoint = new ContactPoint();
                    contactPoint = collision.GetContact(0);

                    if (meshFilters != null && meshFilters.Length >= 1)
                        DamageMesh(force);

                    latestProcessedVertexCount = 0;
                }

            }


        }
    }

    /// <summary>
    /// Finds closest vertex to the target point.
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="mf"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Vector3 NearestVertex(Transform trans, MeshFilter mf, Vector3 point) {

        // Convert point to local space.
        point = trans.InverseTransformPoint(point);

        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestVertex = Vector3.zero;

        // Check all vertices to find nearest.
        foreach (Vector3 vertex in mf.mesh.vertices) {

            Vector3 diff = point - vertex;
            float distSqr = diff.sqrMagnitude;

            if (distSqr < minDistanceSqr) {

                minDistanceSqr = distSqr;
                nearestVertex = vertex;

            }

        }

        // Convert nearest vertex back to the world space.
        return trans.TransformPoint(nearestVertex);

    }

    public void Repair() {

        deformState = DeformState.Deformed;
        repairState = RepairState.Repairing;

    }

    private void Reset() {

        AddAllMeshes();

    }

}
