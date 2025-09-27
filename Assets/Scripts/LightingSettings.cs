using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightingSettings : MonoBehaviour
{
    [SerializeField]
    private Light sun;
    [SerializeField]
    private float sunSpeed;
    [SerializeField, Range(-60.0f, 60.0f)]
    private float sunHeight;
    [SerializeField]
    private GameObject postProcessingVolume;

    private bool naturalLighting = true;
    private float currentAngle;
    private Transform sunTransform;


    public void SetSunHeight(float height)
    {
        sunHeight = height;
    }
    public void SetSunSpeed(float speed)
    {
        sunSpeed = speed;
    }
    public void SetSunlightPower(float power)
    {
        sun.intensity = power;
    }
    public void SetSunlightColor(float temperature)
    {
        sun.colorTemperature = temperature;
    }

    private void Start()
    {
        currentAngle = 0.0f;
        naturalLighting = true;
        sun.enabled = true;
        postProcessingVolume.SetActive(true);
        sunTransform = sun.gameObject.transform;
        RenderSettings.ambientLight = Color.Lerp(Color.Lerp(Color.grey, Color.black, 0.5f), Color.blue, 0.15f);
    }

    public void SetLighting(bool natural)
    {
        naturalLighting = natural;

        if (naturalLighting == true)
        {

            postProcessingVolume.SetActive(true);
            sun.enabled = true;
            RenderSettings.ambientLight = Color.Lerp(Color.Lerp(Color.grey, Color.black, 0.25f), Color.blue, 0.15f);
        }
        else
        {

            postProcessingVolume.SetActive(false);
            sun.enabled = false;
            RenderSettings.ambientLight = Color.white;
        }
    }

    void Update()
    {
        currentAngle += sunSpeed * Time.deltaTime;
        if(currentAngle > 360.0f)
        {
            currentAngle -= 360.0f;
        }
        Vector3 currentSunEulers = sunTransform.localRotation.eulerAngles;
        currentSunEulers.y = currentAngle;
        currentSunEulers.x = sunHeight;
        sunTransform.localRotation = Quaternion.Euler(currentSunEulers);
    }
}
