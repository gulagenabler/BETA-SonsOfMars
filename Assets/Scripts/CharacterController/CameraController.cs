using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("SETTINGS")]
    public float mouseSensitivity = 4;
    public float distance = 3;
    public Vector2 pitchClampValues = new Vector3(-17, 80);
    public float rotationSmoothingFactor = 1.5f;

    
    Vector3 rotationSmoothingVelocity;
    Vector3 currentRotation;
    float yaw, pitch;
    Transform target;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("CameraTarget").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchClampValues.x, pitchClampValues.y); //Clamp pitch rotation to minimum and maximum

        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothingVelocity, rotationSmoothingFactor/30); //Smoothen the rotation
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * distance;
    }
}
