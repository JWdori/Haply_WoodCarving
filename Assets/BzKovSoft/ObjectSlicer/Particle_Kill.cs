using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Kill : MonoBehaviour
{

    ParticleSystem ps;
    List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();
    Transform parentTransform; // �θ� ������Ʈ�� Transform

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
        // �θ� ������Ʈ�� ���� ��ġ�� ũ�� ��������
        Vector3 parentPosition = parentTransform.position;
        Vector3 parentScale = parentTransform.lossyScale;

        // ��ƼŬ �迭 ��������
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int numInside = ps.GetParticles(particles);

        for (int i = numInside - 1; i >= 0; i--) // �迭�� �������� ��ȸ�Ͽ� ����
        {
            ParticleSystem.Particle particle = particles[i];

            // ��ƼŬ�� ���� ��ǥ�� ���� ��ǥ�� ��ȯ
            Vector3 particleWorldPosition = parentTransform.TransformPoint(particle.position);

            // �θ� ������Ʈ�� ������ ������� Ȯ��
            if (!IsInsideParentBounds(particleWorldPosition, parentTransform.position, parentTransform.lossyScale))
            {
                // ��ƼŬ�� �ش� �ε������� ����
                RemoveParticleAt(particles, i, ref numInside);
            }
        }
        // ��ƼŬ �ý��� ������Ʈ
        ps.SetParticles(particles, numInside);
    }

    private void RemoveParticleAt(ParticleSystem.Particle[] particles, int index, ref int count)
    {
        // ��ƼŬ �迭���� �ش� �ε����� ��ƼŬ�� �����ϰ� ������ ��ƼŬ�� �ű�
        for (int i = index; i < count - 1; i++)
        {
            particles[i] = particles[i + 1];
        }

        // ��ƼŬ �迭�� ũ�� ����
        count--;
    }


    private bool IsInsideParentBounds(Vector3 particleWorldPosition, Vector3 parentPosition, Vector3 parentScale)
    {
        // ��ƼŬ�� �θ� ������Ʈ�� ���� ���� �ִ��� Ȯ��
        return Mathf.Abs(particleWorldPosition.x - parentPosition.x) <= (parentScale.x) / 2f &&
               Mathf.Abs(particleWorldPosition.y - parentPosition.y) <= (parentScale.y) / 2f &&
               Mathf.Abs(particleWorldPosition.z - parentPosition.z) <= (parentScale.z) / 2f;
    }






}




