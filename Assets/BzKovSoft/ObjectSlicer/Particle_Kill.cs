using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Kill : MonoBehaviour
{

    ParticleSystem ps;
    List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();
    Transform parentTransform; // 부모 오브젝트의 Transform

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        CheckParticleBounds();
    }

    private void OnParticleTrigger()
    {
        ps.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, inside);

        foreach (var v in inside)
        {
            Debug.Log("Effect Trigger2");
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log($"Effect Collision : {other.name}");
    }

    private void Update()
    {
        CheckParticleBounds();
    }

    private void CheckParticleBounds()
    {
        parentTransform = transform.parent;
        // 부모 오브젝트의 현재 위치와 크기 가져오기
        Vector3 parentPosition = parentTransform.position;
        Vector3 parentScale = parentTransform.lossyScale;

        // 파티클 배열 가져오기
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int numInside = ps.GetParticles(particles);

        for (int i = numInside - 1; i >= 0; i--) // 배열을 역순으로 순회하여 제거
        {
            ParticleSystem.Particle particle = particles[i];

            // 파티클의 로컬 좌표를 월드 좌표로 변환
            Vector3 particleWorldPosition = parentTransform.TransformPoint(particle.position);

            // 부모 오브젝트의 범위를 벗어나는지 확인
            if (!IsInsideParentBounds(particleWorldPosition, parentTransform.position, parentTransform.lossyScale))
            {
                // 파티클을 해당 인덱스에서 제거
                RemoveParticleAt(particles, i, ref numInside);
            }
        }
        // 파티클 시스템 업데이트
        ps.SetParticles(particles, numInside);
    }

    private void RemoveParticleAt(ParticleSystem.Particle[] particles, int index, ref int count)
    {
        // 파티클 배열에서 해당 인덱스의 파티클을 제거하고 나머지 파티클을 옮김
        for (int i = index; i < count - 1; i++)
        {
            particles[i] = particles[i + 1];
        }

        // 파티클 배열의 크기 감소
        count--;
    }


    private bool IsInsideParentBounds(Vector3 particleWorldPosition, Vector3 parentPosition, Vector3 parentScale)
    {
        // 파티클이 부모 오브젝트의 범위 내에 있는지 확인
        return Mathf.Abs(particleWorldPosition.x - parentPosition.x) <= (parentScale.x) / 2f &&
               Mathf.Abs(particleWorldPosition.y - parentPosition.y) <= (parentScale.y) / 2f &&
               Mathf.Abs(particleWorldPosition.z - parentPosition.z) <= (parentScale.z) / 2f;
    }






}




