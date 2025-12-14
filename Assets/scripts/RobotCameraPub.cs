using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class BlueRovCameraPublisher : MonoBehaviour
{
    public string topicName = "bluerov1/camera/image/compressed";
    public float publishRateHz = 5f;
    private float timeSinceLastPublish = 0f;

    private ROSConnection ros;
    private BlueRovCameraCapture cameraCapture;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        cameraCapture = GetComponent<BlueRovCameraCapture>();

        if (cameraCapture == null)
            Debug.LogError("BlueRovCameraCapture missing on GameObject!");

        // Register your publisher before the first publish
        ros.RegisterPublisher<CompressedImageMsg>(topicName);
    }

    void Update()
    {
        if (cameraCapture == null) return;

        timeSinceLastPublish += Time.deltaTime;

        if (timeSinceLastPublish >= 1f / publishRateHz)
        {
            cameraCapture.CaptureJpegAsync(OnImageCaptured);
            timeSinceLastPublish = 0f;
        }
    }

    private void OnImageCaptured(byte[] jpgData)
    {
        if (jpgData == null)
        {
            Debug.LogWarning("Captured JPEG is null");
            return;
        }

        CompressedImageMsg imageMsg = new CompressedImageMsg();
        imageMsg.format = "png";
        imageMsg.data = jpgData;

        ros.Publish(topicName, imageMsg);
        Debug.Log($"Published image to {topicName} ({jpgData.Length} bytes)");
    }
}
