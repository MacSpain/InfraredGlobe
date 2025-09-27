using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CountryClicker : MonoBehaviour
{
    public enum Mode
    {
        Visual,
        Data,
        Game
    }

    public Texture2D countryTexture;
    public Color[] texturePixels;
    public int width;
    public int height;
    public Country[] countries;
    public Renderer earthRenderer;
    public Transform playerMarker;


    private LightingSettings lightingSettings;
    private ReadMeanTemperatureFiles reader;
    private DataUIManager dataUIManager;
    private DataGatherer dataGatherer;
    private Mode currentMode;
    private int dataChoice;
    public int currentChosenCountry;

    void Start()
    {
        currentChosenCountry = -1;
        lightingSettings = FindObjectOfType<LightingSettings>(true);
        reader = FindObjectOfType<ReadMeanTemperatureFiles>();
        dataUIManager = FindObjectOfType<DataUIManager>(true);
        dataGatherer = FindObjectOfType<DataGatherer>(true);
        currentMode = Mode.Visual;
        texturePixels = countryTexture.GetPixels();
        width = countryTexture.width;
        height = countryTexture.height;
        earthRenderer.material.SetFloat("_DataStrength", 0.0f);
        earthRenderer.material.SetFloat("_DataSaturation", 1.0f);
        earthRenderer.material.SetFloat("_CloudsOpacity", 0.5f);
        earthRenderer.material.SetFloat("_LightsStrength", 1.0f);
        earthRenderer.material.SetFloat("_Choice", 0.0f);
    }

    public void SetMode(int index)
    {
        if (lightingSettings == null)
        {
            lightingSettings = FindObjectOfType<LightingSettings>(true);
        }
        currentMode = (Mode)(index);
        switch(currentMode)
        {
            case Mode.Visual:
                {
                    lightingSettings.SetLighting(true);
                    earthRenderer.material.SetFloat("_DataStrength", 0.0f);
                    earthRenderer.material.SetFloat("_DataSaturation", 1.0f);
                    earthRenderer.material.SetFloat("_CloudsOpacity", 0.5f);
                    earthRenderer.material.SetFloat("_LightsStrength", 1.0f);
                }
                break;
            case Mode.Data:
                {
                    lightingSettings.SetLighting(false);
                    earthRenderer.material.SetFloat("_DataStrength", 1.0f);
                    earthRenderer.material.SetFloat("_DataSaturation", 0.0f);
                    earthRenderer.material.SetFloat("_CloudsOpacity", 0.0f);
                    earthRenderer.material.SetFloat("_LightsStrength", 0.0f);
                }
                break;
            case Mode.Game:
                {
                    lightingSettings.SetLighting(false);
                    earthRenderer.material.SetFloat("_DataStrength", 1.0f);
                    earthRenderer.material.SetFloat("_DataSaturation", 0.0f);
                    earthRenderer.material.SetFloat("_CloudsOpacity", 0.0f);
                    earthRenderer.material.SetFloat("_LightsStrength", 0.0f);
                }
                break;
        }
    }

    public void ChooseCountry(int index)
    {
        if(dataUIManager == null)
        {
            dataUIManager = FindObjectOfType<DataUIManager>(true);

        }
        earthRenderer.material.SetFloat("_CountryIndex", (float)(index)/255.0f);
        if(index == 0)
        {

            dataUIManager.RegionSelected(-1, "");
        }
        else
        {

            dataUIManager.RegionSelected(index-1, countries[index-1].name);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            if(currentMode == Mode.Visual)
            {
                currentMode = Mode.Data;
                lightingSettings.SetLighting(false);
                earthRenderer.material.SetFloat("_DataStrength", 1.0f);
                earthRenderer.material.SetFloat("_DataSaturation", 0.0f);
                earthRenderer.material.SetFloat("_CloudsOpacity", 0.0f);
                earthRenderer.material.SetFloat("_LightsStrength", 0.0f);
            }
            else if(currentMode == Mode.Data)
            {
                currentMode = Mode.Visual;
                lightingSettings.SetLighting(true);
                earthRenderer.material.SetFloat("_DataStrength", 0.0f);
                earthRenderer.material.SetFloat("_DataSaturation", 1.0f);
                earthRenderer.material.SetFloat("_CloudsOpacity", 0.5f);
                earthRenderer.material.SetFloat("_LightsStrength", 1.0f);

            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) == true)
        {
            reader.SetIndexLower();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) == true)
        {
            reader.SetIndexHigher();
        }
        if (currentMode == Mode.Data)
        {
            if (Input.GetMouseButtonDown(0) == true)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (EventSystem.current.IsPointerOverGameObject() == false && Physics.Raycast(ray, out RaycastHit hit, 100000.0f, -1) == true)
                {

                    Vector3 hitPoint = hit.point.normalized;
                    float longitude = Mathf.Atan2(hitPoint.z, hitPoint.x);
                    if (longitude < 0)
                    {
                        longitude = 2.0f * Mathf.PI + longitude;
                    }
                    float latitude = Mathf.PI - Mathf.Acos(hitPoint.y);

                    int pointX = (int)((longitude / (2.0f * Mathf.PI)) * width);
                    int pointY = (int)((latitude / (Mathf.PI)) * height);
                    float redValue = texturePixels[pointY * width + pointX].r;
                    if (dataUIManager.guessObject.activeSelf == false)
                    {
                        if (redValue > 0.0f)
                        {
                            float val = 255.0f * redValue - 1.0f;

                            int countryIndex = (int)val;
                            currentChosenCountry = countryIndex;

                        }
                        else
                        {

                            currentChosenCountry = -1;
                        }

                        dataGatherer.FillDataAtClickedPoint(pointX, pointY);
                        playerMarker.position = hit.point;
                        playerMarker.localRotation = Quaternion.FromToRotation(Vector3.up, hit.point.normalized);
                        earthRenderer.material.SetFloat("_CountryIndex", redValue);

                        if (currentChosenCountry == -1)
                        {

                            dataUIManager.RegionSelected(-1, "");
                        }
                        else
                        {
                            dataUIManager.RegionSelected(currentChosenCountry, countries[currentChosenCountry].name);
                        }
                    }
                    else if(dataUIManager.guessButtonObject.activeSelf == true)
                    {

                        if (redValue > 0.0f)
                        {
                            float val = 255.0f * redValue - 1.0f;

                            int countryIndex = (int)val;
                            currentChosenCountry = countryIndex;

                        }
                        else
                        {

                            currentChosenCountry = -1;
                        }
                        if (dataUIManager.guessRegionSelected == currentChosenCountry)
                        {

                            dataGatherer.FillDataAtClickedPoint(pointX, pointY);
                            playerMarker.position = hit.point;
                            playerMarker.localRotation = Quaternion.FromToRotation(Vector3.up, hit.point.normalized);
                            earthRenderer.material.SetFloat("_CountryIndex", redValue);

                            dataUIManager.RegionSelected(currentChosenCountry, countries[currentChosenCountry].name);
                        }
                    }

                }
            }
        }
    }
}
