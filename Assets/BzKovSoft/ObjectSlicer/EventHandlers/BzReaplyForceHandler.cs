using System.Threading.Tasks;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Reapplying velocity and angularVelocity from the original
	/// </summary>
	[DisallowMultipleComponent]
	public class BzReaplyForceHandler : MonoBehaviour, IBzObjectSlicedEvent
	{
		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			var task = ObjectSlicedAsync(original, result);

			foreach (var resultObject in result.resultObjects)
			{
				var slicer = resultObject.gameObject.GetComponent<IBzMeshSlicer>();
				slicer.AddTask(task);
			}
		}

        public async Task ObjectSlicedAsync(GameObject original, BzSliceTryResult result)
        {
            await Task.Yield();

            var origRigid = original.GetComponent<Rigidbody>();
            if (origRigid == null)
                return;

            // 슬라이스된 첫 번째 오브젝트를 참조점으로 사용
            GameObject firstSlicedObject = null;

            foreach (var resultObject in result.resultObjects)
            {
                var rigid = resultObject.gameObject.GetComponent<Rigidbody>();
                if (rigid == null)
                    continue;

                rigid.angularVelocity = origRigid.angularVelocity;
                rigid.velocity = origRigid.velocity;

                if (firstSlicedObject == null)
                {
                    firstSlicedObject = resultObject.gameObject;
                }
                else
                {
                    // 슬라이스된 오브젝트들 간에 FixedJoint 추가
                    var joint = resultObject.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = firstSlicedObject.GetComponent<Rigidbody>();
                }
            }
        }
        void OnDrawGizmos()
        {

        }


    }
}
