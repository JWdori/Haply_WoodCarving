using Haply.HardwareAPI.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GroundForce: MonoBehaviour
{

    private struct AdditionalData
    {
        public Vector3 physicsCursorPosition;
        public bool isTouching;
    }

    [Range(1, 10000)]
    [Tooltip("Adjust Fixed Timestep directly from here to compare with Haptics Thread frequency")]
    public int physicsFrequency = 1000;
    [Range(0, 1000)]
    public float stiffness = 600f;
    [Range(0, 10)]
    public float damping = 1;
    
    public Transform ground;


    private HapticThread m_hapticThread;
    
    private float m_groundHeight;
    private float m_cursorRadius;


    public Text hapticsFrequencyText;
    public Text physicsFrequencyText;

    // Cursor Offset
    private float m_cursorOffsetScale = 1;
    private float m_cursorOffsetHeight;
    public bool forceEnabled = true;

    [Range(-4, 4)]
    public float forceX;
    [Range(-4, 4)]
    public float forceY;
    [Range(-4, 4)]
    public float forceZ;



    // PHYSICS
    [Header("Physics")]
    [Tooltip("Use it to enable friction and mass force feeling")]
    public bool complexJoint;
    public float drag = 20f;
    public float linearLimit = 0.001f;
    public float limitSpring = 500000f;
    public float limitDamper = 10000f;
    public GameObject handle;

    private ConfigurableJoint m_joint;
    private Rigidbody m_rigidbody;

    private const float MinimumReconfigureDelta = 0.5f;
    private bool needConfigure =>
        (complexJoint && m_joint.zMotion != ConfigurableJointMotion.Limited)
        || Mathf.Abs(m_joint.linearLimit.limit - linearLimit) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.spring - limitSpring) > MinimumReconfigureDelta
        || Mathf.Abs(m_joint.linearLimitSpring.damper - limitDamper) > MinimumReconfigureDelta
        || Mathf.Abs(m_rigidbody.drag - drag) > MinimumReconfigureDelta;

    [Header("Collision detection")]
    [Tooltip("Apply force only when a collision is detected (prevent air friction feeling)")]
    public bool collisionDetection;
    public List<Collider> touched = new();



    private void Start()
    {
        // find the HapticThread object
        m_hapticThread = FindObjectOfType<HapticThread>();
        AttachCursor(m_hapticThread.avatar.gameObject);
        SetupCollisionDetection();
        Debug.Log(m_hapticThread.avatar.gameObject.name + "ㅎㅇ");

        // run the haptic loop with given function
        //m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run( ForceCalculation ));
        // Run haptic loop with AdditionalData method to get initial values
        if (m_hapticThread.isInitialized)
        {
            m_hapticThread.Run(ForceCalculation, GetAdditionalData());
        }
        else{
            m_hapticThread.onInitialized.AddListener(() => m_hapticThread.Run(ForceCalculation, GetAdditionalData()));

        }
    }
    private void FixedUpdate() =>
    // Update AdditionalData 
    m_hapticThread.SetAdditionalData(GetAdditionalData());


    void Update()
    {
        if (needConfigure)
        {
            ConfigureJoint();
        }
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


    /// <param name="cursor">Cursor to attach with</param>
    private void AttachCursor(GameObject cursor)
    {
        // Add kinematic rigidbody to cursor
        var rbCursor = cursor.GetComponent<Rigidbody>();
        if (!rbCursor)
        {
            rbCursor = cursor.AddComponent<Rigidbody>();
            rbCursor.useGravity = false;
            rbCursor.isKinematic = true;
        }

        // Add non-kinematic rigidbody to self
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!m_rigidbody)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.useGravity = false;
            m_rigidbody.isKinematic = false;
            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Connect with cursor rigidbody with a spring/damper joint and locked rotation
        m_joint = gameObject.GetComponent<ConfigurableJoint>();
        if (!m_joint)
        {
            m_joint = gameObject.AddComponent<ConfigurableJoint>();
            m_joint.connectedBody = rbCursor;
            m_joint.autoConfigureConnectedAnchor = false;
            m_joint.anchor = m_joint.connectedAnchor = Vector3.zero;
            m_joint.axis = m_joint.secondaryAxis = Vector3.zero;
        }

        ConfigureJoint();

    }
    private void ConfigureJoint()
    {
        if (!complexJoint)
        {
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Locked;
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;

            m_rigidbody.drag = 0;
        }
        else
        {
            // limited linear movements
            m_joint.xMotion = m_joint.yMotion = m_joint.zMotion = ConfigurableJointMotion.Limited;

            // lock rotation to avoid sphere roll caused by physics material friction instead of feel it
            m_joint.angularXMotion = m_joint.angularYMotion = m_joint.angularZMotion = ConfigurableJointMotion.Locked;

            // configure limit, spring and damper
            m_joint.linearLimit = new SoftJointLimit()
            {
                limit = linearLimit
            };
            m_joint.linearLimitSpring = new SoftJointLimitSpring()
            {
                spring = limitSpring,
                damper = limitDamper
            };

            // stabilize spring connection 
            m_rigidbody.drag = drag;
        }
    }

    private AdditionalData GetAdditionalData()
    {
        AdditionalData additionalData;
        additionalData.physicsCursorPosition = transform.localPosition;
        additionalData.isTouching = collisionDetection && touched.Count > 0;
        return additionalData;
    }

    /// <summary>
    /// Calculate the force to apply based on the cursor position and the scene data
    /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
    /// </summary>
    /// <param name="position">cursor position</param>
    /// <param name="velocity">cursor velocity</param>
    /// <param name="additionalData">additional scene data synchronized by <see cref="GetAdditionalData"/> method</param>
    /// <returns>Force to apply</returns>
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

    


   // Display collision infos
   // ----------------------------------
   /*private void OnGUI()
   {
       if (advancedEffector.gameObject.activeSelf && advancedEffector.touched.Count > 0)
       {
           display touched physic material infos on screen
          var physicMaterial = advancedEffector.touched[0].GetComponent<Collider>().material;
           var text = $"PhysicsMaterial: {physicMaterial.name.Replace("(Instance)", "")} \n" +
                      $"dynamic friction: {physicMaterial.dynamicFriction}, static friction: {physicMaterial.staticFriction}\n";

           display touched rigidbody infos on screen
           var rb = advancedEffector.touched[0].GetComponent<Rigidbody>();
           if (rb)
           {
               text += $"mass: {rb.mass}, drag: {rb.drag}, angular drag: {rb.angularDrag}\n";
           }

           GUI.Label(new Rect(20, 40, 800f, 200f), text);
       }
   }*/



    /// <summary>
    /// store all transform information which cannot be acceded in haptic tread loop
    ///
    /// <remarks>Do not use this method for dynamic objects in Update() or FixedUpdate() except for debug in editor
    /// (prefer <see cref="HapticThread.GetAdditionalData{T}"/>)</remarks>
    /// </summary>
    private void StoreTransformInfos ()
    {
        m_groundHeight = ground.transform.position.y;
        m_cursorRadius = m_hapticThread.avatar.lossyScale.y / 2;
        
        var cursorOffset = m_hapticThread.avatar.parent;
        if ( cursorOffset )
        {
            m_cursorOffsetScale = cursorOffset.lossyScale.y;
            m_cursorOffsetHeight = cursorOffset.position.y;
        }
    }


    /// <summary>
    /// Calculate force to apply when the cursor hit the ground.
    /// <para>This method is called once per haptic frame (~1000Hz) and needs to be efficient</para>
    /// </summary>
    /// <param name="position">cursor position</param>
    /// <param name="velocity">cursor velocity (optional)</param>
    /// <returns>Force to apply</returns>
    /*private Vector3 ForceCalculation ( in Vector3 position, in Vector3 velocity )
    {
        var force = Vector3.zero;

        // Contact point scaled by parent offset
        var contactPoint = (position.y * m_cursorOffsetScale) + m_cursorOffsetHeight - m_cursorRadius;
        
        var penetration = m_groundHeight - contactPoint;
        if ( penetration > 0 && stiffness != 0 )
        {
            force.y = penetration * stiffness - velocity.y * damping;
            
            // invert the offset scale to avoid stiffness relative to it
            force.y /= m_cursorOffsetScale;
        }
        else
        {

            force = new Vector3(forceX, forceY, forceZ);
        }
        force += Gravitiy();
        Debug.Log(force);
        return force;
    }
    */

    /// </summary>
    /// <param name="position">cursor position</param>
    /// <param name="velocity">cursor velocity</param>
    /// <param name="additionalData">additional scene data synchronized by <see cref="GetAdditionalData"/> method</param>
    /// <returns>Force to apply</returns>



    public Vector3 Gravitiy()
    {
        float weight = -(0.1391f + 0.1f); // 물체의 무게 뉴턴으로 변환한 값

        Vector3 gravityForce = Vector3.down * (weight);
        //Debug.Log(gravityForce);

        return gravityForce;
    }







    // COLLISION DETECTION
    #region Collision Detection

    private void SetupCollisionDetection()
    {
        // Add collider if not exists
        var col = gameObject.GetComponent<Collider>();
        if (!col)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }

        // Neutral PhysicMaterial to interact with others 
        if (!col.material)
        {
            col.material = new PhysicMaterial { dynamicFriction = 0, staticFriction = 0 };
        }

        collisionDetection = true;
    }

    /// <summary>
    /// Called when effector touch other game object
    /// </summary>
    /// <param name="collision">collision information</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (forceEnabled && collisionDetection && !touched.Contains(collision.collider))
        {
            // store touched object
            touched.Add(collision.collider);
        }
    }

    /// <summary>
    /// Called when effector move away from another game object 
    /// </summary>
    /// <param name="collision">collision information</param>
    private void OnCollisionExit(Collision collision)
    {
        if (forceEnabled && collisionDetection && touched.Contains(collision.collider))
        {
            touched.Remove(collision.collider);
        }
    }

    #endregion






}


