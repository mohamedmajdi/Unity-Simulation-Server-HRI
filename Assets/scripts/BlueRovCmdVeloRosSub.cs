using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class BlueRovCmdVeloRosSub : MonoBehaviour
{
    public string topic = "bluerov1/cmd_vel";
    public BlueRovVeloControl blueRovVeloControl;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.blueRovVeloControl = GetComponent<BlueRovVeloControl>();
        ROSConnection.GetOrCreateInstance().Subscribe<RosMessageTypes.Geometry.TwistMsg>(topic, VelocityChange);
    }

    void VelocityChange(RosMessageTypes.Geometry.TwistMsg velocityMessage)
    {
        Debug.Log("" + velocityMessage);
        this.blueRovVeloControl.moveVelocity(velocityMessage);
    }
}
