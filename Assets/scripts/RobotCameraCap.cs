using UnityEngine;
using System;
using System.Collections;

public class BlueRovCameraCapture : MonoBehaviour
{
    private Camera _camera;
    public int resWidth = 640;
    public int resHeight = 480;

    // Request an async JPEG capture; result delivered via callback.
    public void CaptureJpegAsync(Action<byte[]> onJpegReady)
    {
        StartCoroutine(GetJpgCoroutine(onJpegReady));
    }

    private IEnumerator GetJpgCoroutine(Action<byte[]> onJpegReady)
    {
        yield return new WaitForEndOfFrame();  // Ensure camera rendered

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        _camera.targetTexture = rt;
        RenderTexture.active = rt;

        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        _camera.Render();
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        _camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        onJpegReady?.Invoke(bytes);
    }

    void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
            Debug.LogError("BlueRovCameraCapture requires a Camera component!");
    }
}
