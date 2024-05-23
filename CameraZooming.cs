using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZooming : MonoBehaviour
{
    // The minimum and maximum orthographic size values to limit zooming
    public float minSize = 2f;
    public float maxSize = 20f;
    // The sensitivity of the zoom, higher value means faster zooming
    public float sensitivity = 10f;
    // The speed at which the zooming happens
    public float zoomSpeed = 5f;

    // The target orthographic size to smoothly zoom to
    private float targetSize;

    void Start()
    {
        // Initialize the target size to the current orthographic size of the camera
        targetSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        // Get the scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Adjust the target orthographic size based on the scroll input
        targetSize -= scroll * sensitivity;

        // Clamp the target orthographic size to make sure it stays within the min and max limits
        targetSize = Mathf.Clamp(targetSize, minSize, maxSize);

        // Smoothly interpolate the camera's orthographic size towards the target size
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
        Vector3 cameraPosition = Camera.main.transform.position;
        cameraPosition.x = 1.76f*Camera.main.orthographicSize - 3;
        cameraPosition.y = Camera.main.orthographicSize - 4;
        Camera.main.transform.position = cameraPosition;
    }
}
