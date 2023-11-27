using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


namespace BzKovSoft.ObjectSlicer.EventHandlers
{
    [DisallowMultipleComponent]
    public class BzReaplyForceHandler : MonoBehaviour, IBzObjectSlicedEvent
    {

        public GameObject Knife;
        private GameObject _resultObject; // 결과 오브젝트를 저장할 변수
        private GameObject _currentResultObject; // 현재 결과 오브젝트를 추적

        void Start()
        {
            // z축으로 가장 큰 부분의 위치를 추적하는 Collider를 생성 및 설정
            StartCoroutine(SetupAndRemoveInteraction());
        }

        public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
        {
            return true;
        }

        public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
        {
            var task = ObjectSlicedAsync(original, resultObjects);
            foreach (var resultObject in resultObjects)
            {
                var slicer = resultObject.gameObject.GetComponent<IBzMeshSlicer>();
                slicer.AddTask(task);
            }
        }

        public async Task ObjectSlicedAsync(GameObject original, GameObject[] resultObjects)
        {
            await Task.Yield();

            var origRigid = original.GetComponent<Rigidbody>();
            if (origRigid == null || resultObjects.Length < 2)
                return;

            GameObject originObject = null;
            GameObject resultObject = null;
            float lowestXOrigin = float.MaxValue;
            int minVerticesResult = int.MaxValue;

            // X축에서 가장 낮은 점을 기준으로 오리진 오브젝트와 결과 오브젝트 선정
            foreach (var obj in resultObjects)
            {
                var meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    float lowestX = GetLowestXPoint(obj);

                    if (lowestX < lowestXOrigin)
                    {
                        lowestXOrigin = lowestX;
                        originObject = obj;
                    }
                }
            }

            // X축 좌표가 동일한 경우 버텍스 수로 결과 오브젝트 결정
            foreach (var obj in resultObjects)
            {
                if (obj != originObject)
                {
                    var meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        int vertexCount = meshFilter.mesh.vertexCount;
                        if (vertexCount < minVerticesResult)
                        {
                            minVerticesResult = vertexCount;
                            resultObject = obj;
                        }
                    }
                }
            }

            _currentResultObject = resultObject;
            SetupRigidbody(_currentResultObject, origRigid);
            StartCoroutine(SetupAndRemoveInteraction());
        }

        private void SetupRigidbody(GameObject obj, Rigidbody originalRigidbody)
        {
            if (obj == null)
                return;

            var rigid = obj.GetComponent<Rigidbody>();
            if (rigid == null)
            {
                rigid = obj.AddComponent<Rigidbody>();
            }

            rigid.angularVelocity = originalRigidbody.angularVelocity;
            rigid.velocity = originalRigidbody.velocity;
            rigid.useGravity = originalRigidbody.useGravity;
            rigid.isKinematic = originalRigidbody.isKinematic;

            var collider = obj.GetComponent<Collider>();
            if (collider == null)
            {
                collider = obj.AddComponent<MeshCollider>();
            }
        }


        private (GameObject, int) FindObjectWithLessVertices(GameObject[] objects)
        {
            GameObject minVertexObject = null;
            int minVertices = int.MaxValue;
            float lowestX = float.MaxValue;

            foreach (var obj in objects)
            {
                var meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    int vertexCount = meshFilter.mesh.vertexCount;
                    if (vertexCount < minVertices)
                    {
                        minVertices = vertexCount;
                        minVertexObject = obj;
                        lowestX = GetLowestXPoint(obj);
                    }
                    else if (vertexCount == minVertices)
                    {
                        float currentLowestX = GetLowestXPoint(obj);
                        if (currentLowestX < lowestX)
                        {
                            minVertexObject = obj;
                            lowestX = currentLowestX;
                        }
                    }
                }
            }

            return (minVertexObject, minVertices);
        }
        private float GetLowestXPoint(GameObject obj)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null)
                return float.MaxValue;

            float lowestX = float.MaxValue;
            foreach (var vertex in meshFilter.mesh.vertices)
            {
                Vector3 worldVertex = obj.transform.TransformPoint(vertex);
                if (worldVertex.x < lowestX)
                    lowestX = worldVertex.x;
            }

            return lowestX;
        }


        Vector3 FindMaxYPoint(Mesh mesh)
        {
            var vertices = mesh.vertices;
            Vector3 maxYPoint = Vector3.zero;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            foreach (var vertex in vertices)
            {
                if (vertex.y > maxY || (vertex.y == maxY && vertex.z > maxZ))
                {
                    maxY = vertex.y;
                    maxZ = vertex.z;
                    maxYPoint = vertex;
                }
            }

            return maxYPoint;
        }
        void OnDrawGizmos()
        {

        }
        Vector3 FindMaxZPoint(Mesh mesh)
        {
            var vertices = mesh.vertices;
            Vector3 maxZPoint = Vector3.zero;
            float maxZ = float.MinValue;

            foreach (var vertex in vertices)
            {
                if (vertex.z > maxZ)
                {
                    maxZ = vertex.z;
                    maxZPoint = vertex;
                }
            }

            return maxZPoint;
        }
        private IEnumerator SetupAndRemoveInteraction()
        {
            if (_currentResultObject == null)
                yield break;

            var meshFilter = _currentResultObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                yield break;

            // SetupAndRemoveInteraction 코루틴 내부
            // SetupAndRemoveInteraction 코루틴 내부
            Mesh mesh = _currentResultObject.GetComponent<MeshFilter>().mesh;
            Vector3 specialPoint = FindSpecialPoint(mesh);

            var interactionPoint = new GameObject("InteractionPoint");
            interactionPoint.transform.position = _currentResultObject.transform.TransformPoint(specialPoint);
            interactionPoint.transform.SetParent(_currentResultObject.transform, true);
            // 나머지 코드...

            var collider = interactionPoint.AddComponent<SphereCollider>();
            collider.radius = 0.06f;
            collider.isTrigger = true;

            interactionPoint.AddComponent<InteractionPointScript>();
            yield return new WaitForSeconds(2.5f);

            // 1초 후 상호작용 추적 중단
            Destroy(interactionPoint);
        }




        public class InteractionPointScript : MonoBehaviour
        {
            private void OnTriggerEnter(Collider other)
            {
                // Rigidbody 활성화
                var rigid = GetComponentInParent<Rigidbody>();
                if (rigid != null)
                {
                    if (other.gameObject.name == "AdvancedPhysicsEffector")
                    {
                        rigid.isKinematic = false;
                        rigid.useGravity = true;

                        Vector3 launchDirection = new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)); // 위로
                        float launchForce = 100f; // 원하는 힘
                        rigid.AddForce(launchDirection * launchForce, ForceMode.Impulse);
                        Debug.Log(gameObject.name);


                        // 3초 후 오브젝트 파괴
                        StartCoroutine(DestroyAfterDelay(rigid.gameObject, 3f));

                    }
                }
            }

            private void OnTriggerStay(Collider other)
            {
                // Rigidbody 활성화
                var rigid = GetComponentInParent<Rigidbody>();
                if (rigid != null)
                {
                    if (other.gameObject.name == "AdvancedPhysicsEffector")
                    {
                        rigid.isKinematic = false;
                        rigid.useGravity = true;

                        Vector3 launchDirection = new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)); // 위로
                        float launchForce = 200f; // 원하는 힘
                        rigid.AddForce(launchDirection * launchForce, ForceMode.Impulse);
                        Debug.Log(gameObject.name);

                        // 3초 후 오브젝트 파괴
                        StartCoroutine(DestroyAfterDelay(rigid.gameObject, 3f));

                    }
                }
            }

            private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
            {
                yield return new WaitForSeconds(delay);
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        private Vector3 FindSpecialPoint(Mesh mesh)
        {
            var vertices = mesh.vertices;
            Vector3 specialPoint = Vector3.zero;
            float minX = float.MaxValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            foreach (var vertex in vertices)
            {
                if (vertex.x < minX ||
                    (vertex.x == minX && (vertex.y > maxY ||
                    (vertex.y == maxY && vertex.z > maxZ))))
                {
                    minX = vertex.x;
                    maxY = vertex.y;
                    maxZ = vertex.z;
                    specialPoint = vertex;
                }
            }

            return specialPoint;
        }






    }
}
