using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualUIManager : MonoBehaviour
{
    [SerializeField]
    private Slider sunHeightSlider;
    [SerializeField]
    private Slider sunSpeedSlider;
    [SerializeField]
    private Slider sunlightColorSlider;
    [SerializeField]
    private Slider sunlightPowerSlider;
    private LightingSettings lightingSettings;

    private void OnEnable()
    {
        if(lightingSettings == null)
        {
            lightingSettings = FindObjectOfType<LightingSettings>(true); 
        }

        lightingSettings.SetSunHeight(sunHeightSlider.value);
        lightingSettings.SetSunlightColor(sunlightColorSlider.value);
        lightingSettings.SetSunlightPower(sunlightPowerSlider.value);
        lightingSettings.SetSunSpeed(sunSpeedSlider.value);
    
    }

}
