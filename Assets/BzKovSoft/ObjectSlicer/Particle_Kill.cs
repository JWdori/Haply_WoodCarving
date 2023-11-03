using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Kill : MonoBehaviour
{

    ParticleSystem ps;
    List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
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
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        int numInside = ps.GetParticles(particles);

        // 부모 오브젝트의 스케일을 가져옵니다.
        Vector3 parentScale = transform.parent.localScale;


        for (int i = 0; i < numInside; i++)
        {
            ParticleSystem.Particle particle = particles[i];

            Vector3 particlePosition = transform.parent.TransformPoint(particle.position);
            Debug.Log(particlePosition);

            if (Mathf.Abs(particlePosition.x) > parentScale.x / 2f ||
                Mathf.Abs(particlePosition.y) > parentScale.y / 2f ||
                Mathf.Abs(particlePosition.z) > parentScale.z / 2f)
            {
                ps.Emit(1);  /
            }
        }
    }






}




