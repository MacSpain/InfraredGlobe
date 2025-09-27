using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField]
    private Slider timeSlider;
    [SerializeField]
    private Button visualButton;
    [SerializeField]
    private Button dataButton;
    [SerializeField]
    private Button gameButton;
    [SerializeField]
    private VisualUIManager visualUI;

    [SerializeField]
    private DataUIManager dataUI;

    [SerializeField]
    private GameUIManager gameUI;
    [SerializeField]
    private Renderer earthRenderer;
    [SerializeField]
    private RawImage legendImage;
    [SerializeField]
    private RawImage guessLegendImage;
    [SerializeField]
    private Toggle rotatingButton;

    private CountryClicker countryClicker;
    private Button currentMenuButton;
    private Button currentDataNormButton;

    private void Start()
    {
        countryClicker = FindObjectOfType<CountryClicker>(true);
        visualButton.onClick.Invoke();
        
    }


    public void SetCurrentMenuButton(Button button)
    {
        if(currentMenuButton != null)
        {
            currentMenuButton.GetComponent<Image>().color = Color.grey;
        }
        currentMenuButton = button;
        currentMenuButton.GetComponent<Image>().color = Color.white;
    }
    public void SetCurrentDataNormButton(Button button)
    {
        if (currentDataNormButton != null)
        {
            currentDataNormButton.GetComponent<Image>().color = Color.grey;
        }
        currentDataNormButton = button;
        currentDataNormButton.GetComponent<Image>().color = Color.white;
    }

    public void SetVisual()
    {
        visualUI.gameObject.SetActive(true);
        dataUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
        countryClicker.SetMode(0);
        SetCurrentMenuButton(visualButton);
    }
    public void SetData()
    {
        visualUI.gameObject.SetActive(false);
        dataUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
        countryClicker.SetMode(1);
        SetCurrentMenuButton(dataButton);
    }
    public void SetGame()
    {
        visualUI.gameObject.SetActive(false);
        dataUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        countryClicker.SetMode(2);

        SetCurrentMenuButton(gameButton);
    }
    private void Update()
    {
        earthRenderer.material.SetFloat("_CheckedTime", timeSlider.value);
        legendImage.material.SetFloat("_CheckedTime", timeSlider.value);
        guessLegendImage.material.SetFloat("_CheckedTime", timeSlider.value);
    }
}
