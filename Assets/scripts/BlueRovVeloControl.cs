// using UnityEngine;
// using System;

// public class BlueRovVeloControl : MonoBehaviour {
//     public float lvx = 0.0f; 
//     public float lvy = 0.0f;
//     public float lvz = 0.0f;
//     public float avz = 0.0f;
//     public bool movementActive = false;
//     public Rigidbody rb;
    
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start() {
//         this.rb = GetComponent<Rigidbody>();
//     }
    
//     private void moveVelocityRigidbody() {
//         Vector3 movement = new Vector3(-lvx * Time.deltaTime, lvz * Time.deltaTime, lvy * Time.deltaTime);
//         transform.Translate(movement);
//         transform.Rotate(0, avz * Time.deltaTime,0);
//     }
    
//     public void moveVelocity(RosMessageTypes.Geometry.TwistMsg velocityMessage) {
//         this.lvx = (float)velocityMessage.linear.x;
//         this.lvy = (float)velocityMessage.linear.y;
//         this.lvz = (float)velocityMessage.linear.z;
//         this.avz = (float)velocityMessage.angular.z;
//         this.movementActive = true;
//     }

//     // Update is called once per frame
//     void FixedUpdate() {
//         if (movementActive) {
//             moveVelocityRigidbody();
//             movementActive = false;
//         }
//     }
// }


// using UnityEngine;
// using System;

// public class BlueRovVeloControl : MonoBehaviour {
//     public float lvx = 0.0f; 
//     public float lvy = 0.0f;
//     public float lvz = 0.0f;
//     public float avz = 0.0f;
//     public float avy = 0.0f;
//     public float avx = 0.0f;
//     public Rigidbody rb;
    
//     // Timeout to stop robot when no new commands received
//     private float commandTimeout = 1f; // Stop after t seconds of no input
//     private float lastCommandTime;
    
//     void Start() {
//         this.rb = GetComponent<Rigidbody>();
        
//         // Collision detection
//         rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
//         rb.interpolation = RigidbodyInterpolation.Interpolate;
        
//         // Underwater physics
//         rb.useGravity = false;
//         rb.linearDamping = 1f;
//         rb.angularDamping = 2f;
        
//         lastCommandTime = Time.time;
//     }
    
//     private void moveVelocityRigidbody() {
        
//         if (Time.time - lastCommandTime > commandTimeout) {
//             // Stop the robot
//             rb.linearVelocity = Vector3.zero;
//             rb.angularVelocity = Vector3.zero;
//             return;
//         }
        
//         // Apply commanded velocity
//         rb.linearVelocity = transform.TransformDirection(new Vector3(-lvx, lvz, -lvy));
//         rb.angularVelocity = transform.TransformDirection(new Vector3(avx, -avz, avy));
//     }
    
//     public void moveVelocity(RosMessageTypes.Geometry.TwistMsg velocityMessage) {
//         this.lvx = (float)velocityMessage.linear.x;
//         this.lvy = (float)velocityMessage.linear.y;
//         this.lvz = (float)velocityMessage.linear.z;
//         this.avz = (float)velocityMessage.angular.z;
//         this.avy = (float)velocityMessage.angular.y;
//         this.avx = (float)velocityMessage.angular.x;
        
//         // Update last command time
//         lastCommandTime = Time.time;
//     }

//     void FixedUpdate() {
//         moveVelocityRigidbody();
//     }
    
//     void OnCollisionEnter(Collision collision) {
//         Debug.Log("Hit " + collision.gameObject.name);
//     }
// }


using UnityEngine;
using System;

public class BlueRovVeloControl : MonoBehaviour {
    [Header("Command Inputs (from ROS)")]
    public float lvx = 0.0f; 
    public float lvy = 0.0f;
    public float lvz = 0.0f;
    public float avx = 0.0f;
    public float avy = 0.0f;
    public float avz = 0.0f;
    
    [Header("Physics Settings")]
    public float mass = 11.5f;
    public float linearDrag = 2.0f;
    public float angularDrag = 3.0f;
    public float velocitySmoothing = 0.2f; // Lower = smoother, higher = more responsive
    
    [Header("Buoyancy Settings")]
    public float waterSurfaceY = 4.4f; // Y position of water surface
    public float buoyancyForce = 10.0f; // Upward force when underwater
    public float gravityForce = 9.0f; // Downward force (should match buoyancy for neutral)
    
    [Header("Control")]
    public float commandTimeout = 0.5f;
    
    private Rigidbody rb;
    private float lastCommandTime;
    private Vector3 targetVelocity;
    private Vector3 currentVelocity;
    private Vector3 targetAngularVelocity;
    private Vector3 currentAngularVelocity;
    
    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        
        lastCommandTime = Time.time;
    }
    
    public void moveVelocity(RosMessageTypes.Geometry.TwistMsg velocityMessage) {
        // ROS to Unity coordinate conversion
        
        this.lvx = (float)velocityMessage.linear.x;  // ROS forward -> Unity forward
        this.lvy = (float)velocityMessage.linear.y;  // ROS left -> Unity left
        this.lvz = (float)velocityMessage.linear.z;  // ROS up -> Unity up
        
        this.avx = (float)velocityMessage.angular.x; // Roll
        this.avy = (float)velocityMessage.angular.y; // Pitch
        this.avz = (float)velocityMessage.angular.z; // Yaw
        
        lastCommandTime = Time.time;
    }
    
    void FixedUpdate() {
        // Check command timeout
        bool hasActiveCommand = (Time.time - lastCommandTime) < commandTimeout;
        
        if (!hasActiveCommand) {
            lvx = lvy = lvz = 0f;
            avx = avy = avz = 0f;
        }
        
        // Update target velocities
        UpdateTargetVelocities();
        
        // Apply physics forces
        ApplyBuoyancyAndGravity();
        ApplyDragForces();
        ApplyThrustForces();
    }
    
    void UpdateTargetVelocities() {
        // Convert ROS commands to Unity local space
        targetVelocity = new Vector3(
            -lvx,
            lvz,    
            -lvy            
        );
        
        // Angular velocity (radians per second)
        targetAngularVelocity = new Vector3(
            -avx,
            -avz,   
            -avy
        );
    }
    
    void ApplyBuoyancyAndGravity() {
        float rovY = transform.position.y;
        
        if (rovY > waterSurfaceY) {
            // Above water - only gravity
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Force);
        } else {
            // Underwater - buoyancy and gravity
            rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Force);
        }
    }
    
    void ApplyDragForces() {
        // Only apply water drag when underwater
        if (transform.position.y > waterSurfaceY) {
            return;
        }
        
        // Linear drag in local space (different drag in different directions)
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        Vector3 localDrag = new Vector3(
            -localVel.x * linearDrag * Mathf.Abs(localVel.x), // Quadratic drag
            -localVel.y * linearDrag * Mathf.Abs(localVel.y),
            -localVel.z * linearDrag * Mathf.Abs(localVel.z)
        );
        rb.AddRelativeForce(localDrag, ForceMode.Force);
        
        // Angular drag
        Vector3 localAngVel = transform.InverseTransformDirection(rb.angularVelocity);
        Vector3 angDrag = new Vector3(
            -localAngVel.x * angularDrag * Mathf.Abs(localAngVel.x),
            -localAngVel.y * angularDrag * Mathf.Abs(localAngVel.y),
            -localAngVel.z * angularDrag * Mathf.Abs(localAngVel.z)
        );
        rb.AddRelativeTorque(angDrag, ForceMode.Force);
    }
    
    void ApplyThrustForces() {
        // Smooth velocity transitions
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, velocitySmoothing);
        currentAngularVelocity = Vector3.Lerp(currentAngularVelocity, targetAngularVelocity, velocitySmoothing);
        
        // Get current velocities in local space
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        Vector3 localAngVel = transform.InverseTransformDirection(rb.angularVelocity);
        
        // Calculate velocity errors
        Vector3 velError = currentVelocity - localVel;
        Vector3 angVelError = currentAngularVelocity - localAngVel;
        
        // Apply corrective forces (proportional control)
        float thrustGain = 20f; // Adjust for responsiveness
        float torqueGain = 5f;
        
        Vector3 thrustForce = velError * thrustGain;
        Vector3 torqueForce = angVelError * torqueGain;
        
        rb.AddRelativeForce(thrustForce, ForceMode.Force);
        rb.AddRelativeTorque(torqueForce, ForceMode.Force);
    }
    
    void OnCollisionEnter(Collision collision) {
        Debug.Log("BlueROV hit " + collision.gameObject.name);
    }
    
}

