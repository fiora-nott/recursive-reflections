using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFree : MonoBehaviour
{
    public bool lockCursor = true;
    public bool lockRotation = false;
    public bool lockMovement = false;
    [Range(2.0f, 30.0f)]
    public float cameraMoveSpeed;
    [Range(2.0f, 30.0f)]
    public float cameraLookSensitivity;

    private float rotationY;

    private void Update()
    {

        if (!lockMovement)
        {
            Vector3 cameraPosition = transform.position;
            float speedAdj = cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
                speedAdj *= 3;
            cameraPosition += transform.forward * Input.GetAxis("Vertical") * speedAdj;
            cameraPosition += transform.right * Input.GetAxis("Horizontal") * speedAdj;
            if (Input.GetKey(KeyCode.Space))
            {
                cameraPosition.y += 1 * speedAdj;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                cameraPosition.y -= 1 * speedAdj;
            }
            transform.position = cameraPosition;
        }

        if (!lockRotation)
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * cameraLookSensitivity;
            rotationY = rotationY - Input.GetAxis("Mouse Y") * cameraLookSensitivity;
            rotationY = Mathf.Clamp(rotationY, -89.99f, 89.99f);
            //newRotationY = Mathf.Clamp(newRotationY, -89, 89);
            transform.localEulerAngles = new Vector3(rotationY, newRotationX, 0f);
        }
    }
    private void OnEnable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public Vector3 GetRotation()
    {
        return transform.eulerAngles;
    }
    public Vector3 GetForward()
    {
        return transform.forward;
    }
    public Vector3 GetRight()
    {
        return transform.right;
    }
    public Vector3 GetUp()
    {
        return transform.up;
    }
}
