using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;




namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// The script must be attached to a GameObject that have collider marked as a "IsTrigger".
	/// </summary>
	public class BzKnife : MonoBehaviour
	{
		Vector3 _prevPos;
		Vector3 _pos;

		[SerializeField]
		private Vector3 _origin = Vector3.down;

		[SerializeField]
		private Vector3 _direction = Vector3.up;
		private GameObject other2;

		private void Update()
		{
			_prevPos = _pos;
			_pos = transform.position;

			if (other2 != null)
			{
				EdgeDetector edgeDetector = other2.GetComponentInParent<EdgeDetector>();
				if (edgeDetector != null)
				{
					List<Vector3> edges = edgeDetector.GetEdgePositions();
					// 모서리 그리기
					for (int i = 0; i < edges.Count; i += 2)
					{
						Vector3 edgeStart = other2.transform.TransformPoint(edges[i]);
						Vector3 edgeEnd = other2.transform.TransformPoint(edges[i + 1]);

						// 라인 그리기
						Debug.DrawLine(edgeStart, edgeEnd, Color.blue);

						// 모서리 정보 출력
						Debug.Log($"Edge {i / 2 + 1}: Start {edgeStart}, End {edgeEnd}");
					}
				}
			}
		}

		/// <summary>
		/// Origin of the knife
		/// </summary>
		public Vector3 Origin
		{
			get
			{
				Vector3 localShifted = transform.InverseTransformPoint(transform.position) + _origin;
				return transform.TransformPoint(localShifted);
			}
		}

		/// <summary>
		/// The direction the knife is pointed to
		/// </summary>
		public Vector3 BladeDirection { get { return transform.rotation * _direction.normalized; } }
		/// <summary>
		/// The nnife moving direction
		/// </summary>
		public Vector3 MoveDirection { get { return (_pos - _prevPos).normalized; } }

		void OnDrawGizmosSelected()
		{
			var color = Gizmos.color;
			var direction = transform.rotation * _direction;
			var from = Origin;
			var to = Origin + direction;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(from, to);
			Gizmos.DrawSphere(from, 0.06f);
			DrawArrowEnd(from, direction);
			Gizmos.color = color;
		}

		private static void DrawArrowEnd(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
		{
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
			Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
			Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
			right *= arrowHeadLength;
			left *= arrowHeadLength;
			up *= arrowHeadLength;
			down *= arrowHeadLength;

			Vector3 center = pos + direction;
			Gizmos.DrawRay(center, right);
			Gizmos.DrawRay(center, left);
			Gizmos.DrawRay(center, up);
			Gizmos.DrawRay(center, down);
			
			Gizmos.DrawLine(center + right, center + left);
			Gizmos.DrawLine(center + up, center + down);
		}

		async void OnTriggerEnter(Collider other)
		{
			var slicer = other.GetComponentInParent<IBzMeshSlicer>();
			if (slicer == null)
			{
				return;
			}
			GameObject otherGameObject = other.gameObject;
			other2 = otherGameObject;
			EdgeDetector edgeDetector = other.GetComponentInParent<EdgeDetector>();
			// EdgeDetector가 있고 모서리 데이터가 있는지 확인
			if (edgeDetector != null)
			{
				List<Vector3> edges = edgeDetector.GetEdgePositions();
				// 모서리 그리기
				for (int i = 0; i < edges.Count; i += 2)
				{
					Vector3 edgeStart = other.transform.TransformPoint(edges[i]);
					Vector3 edgeEnd = other.transform.TransformPoint(edges[i + 1]);

					// 라인 그리기
					Debug.DrawLine(edgeStart, edgeEnd, Color.blue);

					// 모서리 정보 출력
					Debug.Log($"Edge {i / 2 + 1}: Start {edgeStart}, End {edgeEnd}");
				}


				// 현재 충돌한 오브젝트의 위치를 가져옵니다.
				Vector3 collisionPoint = GetCollisionPoint();

				// 충돌한 위치와 모서리 위치 간의 거리를 계산하여 모서리 데이터와 관련 있는지 확인
				float thresholdDistance = 1f; // 모서리와 충돌한 위치 간의 임계값 설정
				foreach (Vector3 edgePosition in edges)
				{
					float distance = Vector3.Distance(collisionPoint, edgePosition);
					//Debug.Log(distance);
					if (distance < thresholdDistance)
					{
						Vector3 point = GetCollisionPoint();
						Vector3 normal = Vector3.Cross(MoveDirection, BladeDirection);
						Plane plane = new Plane(normal, point);
						await slicer.SliceAsync(plane);
					}

				}
			}

		}

		private Vector3 GetCollisionPoint()
		{
			Vector3 distToObject = transform.position - Origin;
			Vector3 proj = Vector3.Project(distToObject, BladeDirection);

			Vector3 collisionPoint = Origin + proj;
			return collisionPoint;
		}
	}
}
