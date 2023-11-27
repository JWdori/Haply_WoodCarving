using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;




namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// The script must be attached to a GameObject that have collider marked as a "IsTrigger".
	/// </summary>
    /// 
	public class BzKnife : MonoBehaviour
	{
		Vector3 _prevPos;
		Vector3 _pos;
		public GameObject angleObject; // 각도를 구할 오브젝트
		public float rotationSpeed = 360f;

		[SerializeField]
		private Vector3 _origin = Vector3.down;

		[SerializeField]
		private Vector3 _direction = Vector3.up;
		private GameObject other2;
		[SerializeField]
		public CommonData commonData;
		float force;
		bool EdgeCheck;

		private Vector3 _planePointToDraw = Vector3.zero; // Gizmos에서 그릴 평면의 점

		private Vector3 previousVelocity = Vector3.zero;
		private Vector3 currentAcceleration = Vector3.zero;

		public List<AudioClip> audioClips; // 오디오 클립들을 저장할 리스트

		private AudioSource collisionStaySound; // 충돌 시 재생될 오디오 클립
		private AudioSource collisionEnterSound;  // 오디오 소스 컴포넌트
												 //woodFxEmission.enabled = true;
												 //          woodFx.transform.position = collision.contacts[0].point;
		public AudioClip collisionStayClip;
		public AudioClip collisionEnterClip;         //          coll.HitCollider(hitDamage);
													 //          wood.Hit(coll.index, hitDamage);



		[SerializeField] private ParticleSystem woodFx;

		private ParticleSystem.EmissionModule woodFxEmission;


		float angle;
		private float lastForce; // 마지막 충돌의 힘을 저장할 변수




		// 오디오 볼륨을 인스펙터에서 조절할 수 있도록 합니다.
		[Range(0f, 1f)]
		public float collisionStayVolume = 1.0f;
		[Range(0f, 1f)]
		public float collisionEnterVolume = 1.0f;



		private void Start()
		{

			// 각각의 AudioSource 컴포넌트 생성 또는 할당
			collisionStaySound = gameObject.AddComponent<AudioSource>();
			collisionEnterSound = gameObject.AddComponent<AudioSource>();

			// 클립과 볼륨 설정
			collisionStaySound.clip = collisionStayClip;
			collisionStaySound.volume = collisionStayVolume;

			collisionEnterSound.clip = collisionEnterClip;
			collisionEnterSound.volume = collisionEnterVolume;


			woodFxEmission = woodFx.emission;

		}
		private void FixedUpdate()
        {


			Vector3 currentVelocity = commonData.GetDataVelo();
			//currentAcceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
			force = commonData.GetData();
			//Debug.Log(force);
			// 현재 속도를 저장
			//previousVelocity = currentVelocity;
		}
		private void Update()
		{


			// 오브젝트의 forward 벡터
			Vector3 objectForward = transform.forward;
			// z축 벡터 (Unity의 전역 좌표계에서 z축은 Vector3.forward로 표현됩니다)
			Vector3 zAxis = Vector3.forward;

			// 두 벡터 사이의 각도 계산
			angle = Vector3.Angle(objectForward, zAxis);

			// 각도 출력
			//Debug.Log(angle);



			_prevPos = _pos;
			_pos = transform.position;
			//지울 부분
			if (other2 != null)
			{
				EdgeDetector edgeDetector = other2.GetComponent<EdgeDetector>();
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

					}
				}
			}

			if (commonData != null)
			{

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

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(_planePointToDraw, 0.02f); // 반지름 0.02의 파란색 구
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

		async void OnCollisionStay(Collision collision)
        {



			GameObject otherGameObject = collision.gameObject;
			other2 = otherGameObject;




			//
			//

			//var rb = other.gameObject.GetComponent<Rigidbody>();
			//float mass2 = rb.mass;
			//float f = mass2 * currentAcceleration.magnitude;
			//Debug.Log(f);

			Vector3 planePoint = angleObject.transform.position; // angleObject의 중심 좌표
			Vector3 planeNormal = angleObject.transform.up; // angleObject의 상방 방향 (기울임에 따라 바뀜)
			Plane plane = new Plane(planeNormal, planePoint);

			if (collision.gameObject.scene.name == "Clay")
			{
				if (!collisionEnterSound.isPlaying && !collisionStaySound.isPlaying)
					collisionStaySound.Play();
			}
			else if (collision.gameObject.scene.name == "Soft")
            {
				var slicer = collision.gameObject.GetComponentInParent<IBzMeshSlicer>();
				if (slicer == null)
				{
					return;
				}

				EdgeDetector edgeDetector = collision.gameObject.GetComponentInParent<EdgeDetector>();
				float distance = 0.004f;
				// EdgeDetector가 있고 모서리 데이터가 있는지 확인
				if (edgeDetector != null)
				{
					List<Vector3> edges = edgeDetector.GetEdgePositions();
					// 모서리 그리기
					for (int i = 0; i < edges.Count; i += 2)
					{
						Vector3 edgeStart = otherGameObject.transform.TransformPoint(edges[i]);
						Vector3 edgeEnd = otherGameObject.transform.TransformPoint(edges[i + 1]);
						_planePointToDraw = planePoint; // 평면 점 위치 업데이트

						if (IsPointOnEdge(edgeStart, edgeEnd, planePoint, distance))
						{

							if (angle >= 1f && angle < 20f)
							{
								if (force > 2f)
								{
									woodFxEmission.enabled = true;
									await slicer.SliceAsync(plane);
                                }
                                else
                                {
									woodFxEmission.enabled = false;

								}
							}
							else if (angle >= 20f && angle < 32f)
							{
								if (force > 3f)
								{
									woodFxEmission.enabled = true;
									await slicer.SliceAsync(plane);
                                }
                                else
                                {
									woodFxEmission.enabled = false;

								}
							}
							else if (angle >= 32f && angle < 40f)
							{
								if (force > 5f)
								{
									lastForce = force;
									woodFxEmission.enabled = true;
									woodFx.Play();
									await slicer.SliceAsync(plane);

                                }
                                else
                                {
									woodFxEmission.enabled = false;

								}

							}
                        }
                        else
                        {
							if (force > 8f)
							{
								lastForce = force;
							}

						}


					} 

				}
			}

			else if (collision.gameObject.scene.name == "Hard") 
			{
				lastForce = force;



			}


		}

		async void OnCollisionExit(Collision collision)
        {
			woodFxEmission.enabled = false;
			if (collision.gameObject.scene.name == "Hard")
            {
				if (lastForce > 8)
				{
					if (!collisionEnterSound.isPlaying)
					{
						Debug.Log(lastForce);
						collisionEnterSound.Play();
						lastForce = 0;
					}
				}
			}
			else if  (collision.gameObject.scene.name == "Soft")
            {
				if (lastForce > 4)
				{
					if (!collisionEnterSound.isPlaying) {
					Debug.Log(lastForce);
					collisionEnterSound.Play();
						lastForce = 0;
					}
				}
			}else if (gameObject.scene.name == "Clay")
            {
				
					if (!collisionEnterSound.isPlaying)
					{
						collisionStaySound.Stop();
						collisionEnterSound.Play();
	
					}
				
			}
		}





		async void OnCollisionEnter(Collision collision)
		{

			var slicer = collision.gameObject.GetComponentInParent<IBzMeshSlicer>();
			if (slicer == null)
			{
				return;
			}

			GameObject otherGameObject = collision.gameObject;
			other2 = otherGameObject;




			//
			//

			//var rb = other.gameObject.GetComponent<Rigidbody>();
			//float mass2 = rb.mass;
			//float f = mass2 * currentAcceleration.magnitude;
			//Debug.Log(f);

			Vector3 planePoint = angleObject.transform.position; // angleObject의 중심 좌표
			Vector3 planeNormal = angleObject.transform.up; // angleObject의 상방 방향 (기울임에 따라 바뀜)
			Plane plane = new Plane(planeNormal, planePoint);

			if (gameObject.scene.name== "Soft")
			{

				EdgeDetector edgeDetector = collision.gameObject.GetComponentInParent<EdgeDetector>();
				float distance = 0.004f;


				// EdgeDetector가 있고 모서리 데이터가 있는지 확인
				if (edgeDetector != null)
				{
					List<Vector3> edges = edgeDetector.GetEdgePositions();
					// 모서리 그리기
					for (int i = 0; i < edges.Count; i += 2)
					{
						Vector3 edgeStart = otherGameObject.transform.TransformPoint(edges[i]);
						Vector3 edgeEnd = otherGameObject.transform.TransformPoint(edges[i + 1]);
						_planePointToDraw = planePoint; // 평면 점 위치 업데이트

						if (IsPointOnEdge(edgeStart, edgeEnd, planePoint, distance))
						{

							if (angle >= 1f && angle < 20f)
                            {
								if(force > 2f)
                                {
									await slicer.SliceAsync(plane);
								}
							}
							else if(angle >= 20f && angle < 32f)
                            {
								if (force > 3f)
								{
									await slicer.SliceAsync(plane);
								}
							}
							else if (angle >= 32f && angle < 40f)
                            {
								if (force > 5f)
								{
									await slicer.SliceAsync(plane);
								}

                            }

						}
						else
						{
							if (force > 8f)
							{

							}
						}

					}

				}
			}else if (gameObject.scene.name == "Hard")
            {

            }

		

		}



		void OnGUI()
		{
			if (gameObject.scene.name == "Soft") {
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
		}

		private Vector3 GetCollisionPoint()
		{
			Vector3 distToObject = transform.position - Origin;
			Vector3 proj = Vector3.Project(distToObject, BladeDirection);

			Vector3 collisionPoint = Origin + proj;
			return collisionPoint;
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




		////이제 필요 없음
		//private void Awake()
		//{
		//	ps = GameObject.Find("PWood").GetComponent<ParticleSystem>();
		//}

		//private void OnParticleTrigger()
		//{
		//	Debug.Log("Cube Trigger");
		//	ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);

		//	foreach (var v in inside)
		//	{
		//		Debug.Log("CWube Trigger2");
		//	}
		//}

		//private void OnParticleCollision(GameObject other)
		//{
		//	Debug.Log($"Cube Collision : {other.name}");
		//}



	}



}
