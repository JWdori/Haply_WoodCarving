using Haply.HardwareAPI.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class HapticController : MonoBehaviour
{
    private HapticThread m_hapticThread;
    private struct AdditionalData
    {
        public Vector3 physicsCursorPosition;
        public bool isTouching;
    }
    public bool forceEnabled = true;

    [Range(1, 10000)]
    [Tooltip("Adjust Fixed Timestep directly from here to compare with Haptics Thread frequency")]
    public int physicsFrequency = 1000;
    [Range(0, 1000)]
    public float stiffness = 600f;
    [Range(0, 10)]
    public float damping = 1;
    public Text hapticsFrequencyText;
    public Text physicsFrequencyText;
    [Range(-2, 2)]
    public float forceX;
    [Range(-2, 2)]
    public float forceY;
    [Range(-2, 2)]
    public float forceZ;
    [Header("Collision detection")]
    [Tooltip("Apply force only when a collision is detected (prevent air friction feeling)")]
    public bool collisionDetection;
    public List<Collider> touched = new();
    private void Start()
    {
        // find the HapticThread object
        m_hapticThread = FindObjectOfType<HapticThread>();
        Debug.Log(m_hapticThread.avatar.gameObject.name + "ㅎㅇ");

        // run the haptic loop with given function
        //m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run( ForceCalculation ));
        // Run haptic loop with AdditionalData method to get initial values
        if (m_hapticThread.isInitialized)
        {
            m_hapticThread.Run(ForceCalculation, GetAdditionalData());
        }
        else
        {
            m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, GetAdditionalData()));

        }
    }
    private void FixedUpdate() =>
    // Update AdditionalData 
    m_hapticThread.SetAdditionalData(GetAdditionalData());


    void Update()
    {

        //StoreTransformInfos();
        Time.fixedDeltaTime = 1f / physicsFrequency;

        if (m_hapticThread != null && hapticsFrequencyText != null)
            hapticsFrequencyText.text = $"haptics : {m_hapticThread.actualFrequency}Hz";
        if (physicsFrequencyText != null)
            physicsFrequencyText.text = $"physics : {physicsFrequency}Hz";

        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }

    private AdditionalData GetAdditionalData()
    {
        AdditionalData additionalData;
        additionalData.physicsCursorPosition = transform.localPosition;
        additionalData.isTouching = collisionDetection && touched.Count > 0;
        return additionalData;
    }

    private Vector3 ForceCalculation(in Vector3 position, in Vector3 velocity, in AdditionalData additionalData)
    {
        var force = additionalData.physicsCursorPosition - position;
        force *= stiffness;
        force -= velocity * damping;
        if (!forceEnabled || (collisionDetection && !additionalData.isTouching))
        {
            // Don't compute forces if there are no collisions which prevents feeling drag/friction while moving through air. 
            force = new Vector3(forceX, forceY, forceZ);
        }
        force += Gravitiy();
        //Debug.Log(force);
        return force;
    }
    public Vector3 Gravitiy()
    {
        float weight = -(0.1391f + 0.1f); // 물체의 무게 뉴턴으로 변환한 값

        Vector3 gravityForce = Vector3.down * (weight);
        //Debug.Log(gravityForce);

        return gravityForce;
    }

}