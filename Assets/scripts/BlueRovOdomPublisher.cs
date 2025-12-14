using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class BlueRovOdomPublisher : MonoBehaviour
{
    public string topic = "/bluerov/navigator/odometry";
    public string frameId = "odom";
    public string childFrameId = "base_link";
    public float publishFrequency = 50f; // Hz
    
    private ROSConnection ros;
    private Rigidbody rb;
    private float publishTimer = 0f;
    private float publishInterval;
    
    // Store previous values for velocity calculation
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float lastTime;
    private bool firstUpdate = true;
    
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<OdometryMsg>(topic);
        
        rb = GetComponent<Rigidbody>();
        publishInterval = 1f / publishFrequency;
        
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastTime = Time.time;
    }
    
    void FixedUpdate()
    {
        publishTimer += Time.fixedDeltaTime;
        
        if (publishTimer >= publishInterval)
        {
            PublishOdometry();
            publishTimer = 0f;
        }
    }
    
    void PublishOdometry()
    {
        OdometryMsg odomMsg = new OdometryMsg();
        
        // Header
        odomMsg.header = new HeaderMsg
        {
            stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
            {
                sec = (int)Time.time,
                nanosec = (uint)((Time.time - (int)Time.time) * 1e9)
            },
            frame_id = frameId
        };
        
        odomMsg.child_frame_id = childFrameId;
        
        // Position (Unity coordinates)
        odomMsg.pose.pose.position = new PointMsg
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z
        };
        
        // Orientation (Unity quaternion)
        odomMsg.pose.pose.orientation = new QuaternionMsg
        {
            x = transform.rotation.x,
            y = transform.rotation.y,
            z = transform.rotation.z,
            w = transform.rotation.w
        };
        
        // Calculate velocities
        Vector3 linearVelocity;
        Vector3 angularVelocity;
        
        if (rb != null)
        {
            // Use Rigidbody velocities if available (more accurate)
            linearVelocity = rb.linearVelocity;
            angularVelocity = rb.angularVelocity;
        }
        else
        {
            // Calculate from position/rotation changes
            if (firstUpdate)
            {
                linearVelocity = Vector3.zero;
                angularVelocity = Vector3.zero;
                firstUpdate = false;
            }
            else
            {
                float deltaTime = Time.time - lastTime;
                
                // Linear velocity
                linearVelocity = (transform.position - lastPosition) / deltaTime;
                
                // Angular velocity (approximate)
                Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
                angularVelocity = new Vector3(
                    deltaRotation.x * 2f / deltaTime,
                    deltaRotation.y * 2f / deltaTime,
                    deltaRotation.z * 2f / deltaTime
                );
            }
        }
        
        // Twist - Linear velocity
        odomMsg.twist.twist.linear = new Vector3Msg
        {
            x = linearVelocity.x,
            y = linearVelocity.y,
            z = linearVelocity.z
        };
        
        // Twist - Angular velocity
        odomMsg.twist.twist.angular = new Vector3Msg
        {
            x = angularVelocity.x,
            y = angularVelocity.y,
            z = angularVelocity.z
        };
        
        // Publish
        ros.Publish(topic, odomMsg);
        
        // Update last values
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastTime = Time.time;
    }
}