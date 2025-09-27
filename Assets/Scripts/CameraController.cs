using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 0.1f;
    public float minZoom = 1.0f;
    public float maxZoom = 2.0f;

    public float cameraSpeed = 1.0f;

    public float snapiness = 5.0f;

    public float testXOffset = 0.5f;

    private float currentZoom = 1.0f;
    private float targetZoom = 1.0f;
    private float targetXAngle = 35.0f;
    private float targetYAngle = 0.0f;
    private float currentXAngle = 35.0f;
    private float currentYAngle = 0.0f;
    private Vector3 oldMousePosition = Vector3.zero;
    private bool rightPressed = false;

    public void SetTargetAt(float x, float y)
    {
        targetXAngle = 2.0f * (y - 0.5f) * 90.0f;
        if (targetXAngle > 89.0f)
        {
            targetXAngle = 89.0f;
        }
        if (targetXAngle < -89.0f)
        {
            targetXAngle = -89.0f;
        }
        targetYAngle = 180.0f - (2.0f * (x + 0.75f) * 180.0f);
        if (targetYAngle > 360.0f)
        {
            targetYAngle -= 360.0f;
        }
        if (targetYAngle < 0.0f)
        {
            targetYAngle += 360.0f;
        }
    }

    void Start()
    {
        targetZoom = minZoom + 0.5f*(maxZoom - minZoom);
        currentZoom = targetZoom;
        oldMousePosition = Input.mousePosition;
    }

    void Update()
    {
        if(Input.GetMouseButton(1) == true)
        {
            if (rightPressed == false)
            {
                rightPressed = true;
                oldMousePosition = Input.mousePosition;
            }
            else
            {

                Vector3 newMousePosition = Input.mousePosition;
                Vector3 diff = newMousePosition - oldMousePosition;
                targetYAngle += cameraSpeed*diff.x;
                if (targetYAngle > 360.0f)
                {
                    targetYAngle -= 360.0f;
                }
                targetXAngle -= cameraSpeed * diff.y;
                if (targetXAngle > 89.0f)
                {
                    targetXAngle = 89.0f;
                }
                if (targetXAngle < -89.0f)
                {
                    targetXAngle = -89.0f;
                }
                oldMousePosition = newMousePosition;
            }
        }
        else
        {
            rightPressed = false;
        }

        if(Input.mouseScrollDelta.y != 0)
        {
            float delta = Input.mouseScrollDelta.y;
            targetZoom -= delta * zoomSpeed;
            if(targetZoom < minZoom)
            {
                targetZoom = minZoom;
            }
            if(targetZoom > maxZoom)
            {
                targetZoom = maxZoom;
            }
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, snapiness * Time.deltaTime);

        currentXAngle = Mathf.Lerp(currentXAngle, targetXAngle, snapiness * Time.deltaTime);
        if(targetYAngle - currentYAngle > 180.0f)
        {
            targetYAngle -= 360.0f;
        }
        else if(targetYAngle - currentYAngle < -180.0f)
        {
            targetYAngle += 360.0f;
        }
        currentYAngle = Mathf.Lerp(currentYAngle, targetYAngle, snapiness * Time.deltaTime);

        Vector3 currentVector = new Vector3(0.0f, 
            -Mathf.Sin(Mathf.Deg2Rad * currentXAngle), 
            Mathf.Cos(Mathf.Deg2Rad * currentXAngle));
        currentVector = new Vector3(Mathf.Sin(Mathf.Deg2Rad * currentYAngle) * currentVector.z,
            currentVector.y,
            Mathf.Cos(Mathf.Deg2Rad * currentYAngle)* currentVector.z);

        transform.position = currentZoom * -currentVector;
        transform.rotation = Quaternion.LookRotation(currentVector.normalized);
    }
}
