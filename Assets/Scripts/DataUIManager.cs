using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataUIManager : MonoBehaviour
{
    public enum DataMode
    {
        OldNorm,
        NewNorm,
        NormDifference,
    }

    [SerializeField]
    private Button oldClimateNormButton;
    [SerializeField]
    private Button newClimateNormButton;
    [SerializeField]
    private Button normDifferenceButton;
    [SerializeField]
    private TMP_Text currentRegionSelectionText;
    [SerializeField]
    private TMP_Text currentDataSelectionText;
    [SerializeField]
    private RawImage legendImage;
    [SerializeField]
    private TMP_Text legendMinValueText;
    [SerializeField]
    private TMP_Text legendMaxValueText;
    [SerializeField]
    private string[] dataTypesNames;
    [SerializeField]
    private Renderer earthRenderer;
    [SerializeField]
    private GameObject playGuessButtonObject;
    [SerializeField]
    public GameObject guessObject;
    [SerializeField]
    private TMP_Text guessCountry;
    [SerializeField]
    private RawImage guessLegendImage;
    [SerializeField]
    private GameObject guessDistanceObject;
    [SerializeField]
    private TMP_Text guessDistance;
    [SerializeField]
    private TMP_Text guessScore;
    [SerializeField]
    public GameObject guessButtonObject;
    [SerializeField]
    private GameObject guessTargetMarker;

    private DataMode currentDataMode = DataMode.OldNorm;
    private int currentDataType;
    private int currentRegionSelected;
    public int guessRegionSelected;
    private CountryClicker countryClicker;
    private ReadMeanTemperatureFiles reader;
    private DataGatherer gatherer;
    private MainUIManager mainUIManager;

    public void Guess()
    {
        guessDistanceObject.SetActive(true);
        guessTargetMarker.SetActive(true);
        guessButtonObject.SetActive(false);
        float distance = gatherer.GetCurrentClickedDistance();
        guessDistance.text = distance.ToString("F2");
        float score = gatherer.GetCurrentClickedScore();
        guessScore.text = score.ToString("F2");
    }
    public void PrepareGuess()
    {
        gatherer.ChoosePointInCountry();
        guessObject.SetActive(true);
        guessButtonObject.SetActive(true);
        playGuessButtonObject.SetActive(false);
        guessDistanceObject.SetActive(false);
        guessTargetMarker.SetActive(false);


    }
    public void CloseGuess()
    {
        guessObject.SetActive(false);
        guessButtonObject.SetActive(false);
        playGuessButtonObject.SetActive(true);
        guessDistanceObject.SetActive(false);
        guessTargetMarker.SetActive(false);
    }

    public void GuessRegionSelected(int index, string region)
    {
        guessRegionSelected = index;
        if (index == -1)
        {
            guessCountry.text = "Earth";
        }
        else
        {
            guessCountry.text = region;
            gatherer.ResetClickedData();
            countryClicker.ChooseCountry(guessRegionSelected + 1);
        }
        ChooseLegend(currentDataMode, currentDataType);
    }

    public void RegionSelected(int index, string region)
    {
        if(currentRegionSelected != index)
        {
            gatherer.PointCameraAtCountry(index);
        }
        currentRegionSelected = index;
        if (index == -1)
        {
            currentRegionSelectionText.text = "Earth";
        }
        else
        {
            currentRegionSelectionText.text = region;
        }
        ChooseLegend(currentDataMode, currentDataType);
    }


    public void SetLegend(Texture2D legendTexture, double legendMinValue, double legendMaxValue, float[] values)
    {
        legendMinValueText.text = legendMinValue.ToString("F2");
        legendMaxValueText.text = legendMaxValue.ToString("F2");
        legendImage.material.SetTexture("_LegendTexture", legendTexture);
        if (values != null)
        {
            legendImage.material.SetFloat("_Value1", values[0]);
            legendImage.material.SetFloat("_Value2", values[1]);
            legendImage.material.SetFloat("_Value3", values[2]);
            legendImage.material.SetFloat("_Value4", values[3]);
            legendImage.material.SetFloat("_Value5", values[4]);
            legendImage.material.SetFloat("_Value6", values[5]);
            legendImage.material.SetFloat("_Value7", values[6]);
            legendImage.material.SetFloat("_Value8", values[7]);
            legendImage.material.SetFloat("_Value9", values[8]);
            legendImage.material.SetFloat("_Value10", values[9]);
            legendImage.material.SetFloat("_Value11", values[10]);
            legendImage.material.SetFloat("_Value12", values[11]);
            legendImage.material.SetFloat("_Value12", values[11]);
        }
        else
        {

            legendImage.material.SetFloat("_Value1", 0.0f);
            legendImage.material.SetFloat("_Value2", 0.0f);
            legendImage.material.SetFloat("_Value3", 0.0f);
            legendImage.material.SetFloat("_Value4", 0.0f);
            legendImage.material.SetFloat("_Value5", 0.0f);
            legendImage.material.SetFloat("_Value6", 0.0f);
            legendImage.material.SetFloat("_Value7", 0.0f);
            legendImage.material.SetFloat("_Value8", 0.0f);
            legendImage.material.SetFloat("_Value9", 0.0f);
            legendImage.material.SetFloat("_Value10", 0.0f);
            legendImage.material.SetFloat("_Value11", 0.0f);
            legendImage.material.SetFloat("_Value12", 0.0f);
        }
    }

    public void SetGuessLegend(Texture2D legendTexture,  float[] values)
    {
        guessLegendImage.material.SetTexture("_LegendTexture", legendTexture);
        if (values != null)
        {
            guessLegendImage.material.SetFloat("_Value1", values[0]);
            guessLegendImage.material.SetFloat("_Value2", values[1]);
            guessLegendImage.material.SetFloat("_Value3", values[2]);
            guessLegendImage.material.SetFloat("_Value4", values[3]);
            guessLegendImage.material.SetFloat("_Value5", values[4]);
            guessLegendImage.material.SetFloat("_Value6", values[5]);
            guessLegendImage.material.SetFloat("_Value7", values[6]);
            guessLegendImage.material.SetFloat("_Value8", values[7]);
            guessLegendImage.material.SetFloat("_Value9", values[8]);
            guessLegendImage.material.SetFloat("_Value10", values[9]);
            guessLegendImage.material.SetFloat("_Value11", values[10]);
            guessLegendImage.material.SetFloat("_Value12", values[11]);
            guessLegendImage.material.SetFloat("_Value12", values[11]);
        }
        else
        {

            guessLegendImage.material.SetFloat("_Value1", 0.0f);
            guessLegendImage.material.SetFloat("_Value2", 0.0f);
            guessLegendImage.material.SetFloat("_Value3", 0.0f);
            guessLegendImage.material.SetFloat("_Value4", 0.0f);
            guessLegendImage.material.SetFloat("_Value5", 0.0f);
            guessLegendImage.material.SetFloat("_Value6", 0.0f);
            guessLegendImage.material.SetFloat("_Value7", 0.0f);
            guessLegendImage.material.SetFloat("_Value8", 0.0f);
            guessLegendImage.material.SetFloat("_Value9", 0.0f);
            guessLegendImage.material.SetFloat("_Value10", 0.0f);
            guessLegendImage.material.SetFloat("_Value11", 0.0f);
            guessLegendImage.material.SetFloat("_Value12", 0.0f);
        }
    }

    public void ChooseLegend(DataMode dataMode, int dataType)
    {
        Texture2D texture = null;
        double minVal = 0.0;
        double maxVal = 0.0;
        float[] values = gatherer.GetClickedValues(dataType, dataMode);
        float[] currentValues = gatherer.GetCurrentValues(dataType, dataMode);
        switch (dataMode)
        {
            case DataMode.OldNorm:
                {
                    texture = reader.gradientTextures[dataType];
                    minVal = reader.dataParams[dataType].minValue;
                    maxVal = reader.dataParams[dataType].maxValue;
                }
                break;
            case DataMode.NewNorm:
                {

                    texture = reader.gradientTextures[dataType];
                    minVal = reader.dataParams[dataType].minValue;
                    maxVal = reader.dataParams[dataType].maxValue;
                }
                break;
            case DataMode.NormDifference:
                {

                    texture = reader.comparisonGradientTextures[dataType];
                    minVal = -reader.normDataRanges[dataType];
                    maxVal = reader.normDataRanges[dataType];
                }
                break;
        }
        SetLegend(texture, minVal, maxVal, values);
        SetGuessLegend(texture, currentValues);
    }

    public void PreviousType()
    {
        currentDataType--;
        if (currentDataType < 0)
        {
            currentDataType = dataTypesNames.Length - 1;
        }
        currentDataSelectionText.text = dataTypesNames[currentDataType];
        ChooseLegend(currentDataMode, currentDataType);
        reader.SetIndex(currentDataType);
    }
    public void NextType()
    {
        currentDataType = (currentDataType + 1) % dataTypesNames.Length;
        currentDataSelectionText.text = dataTypesNames[currentDataType];
        ChooseLegend(currentDataMode, currentDataType);
        reader.SetIndex(currentDataType);
    }
    public void SetDataType(int index)
    {
        currentDataType = index;
        currentDataSelectionText.text = dataTypesNames[currentDataType];
        ChooseLegend(currentDataMode, currentDataType);
        reader.SetIndex(currentDataType);
    }
    public void SetDataMode(int index)
    {
        currentDataMode = (DataMode)index;
        earthRenderer.material.SetFloat("_Choice", (float)index);
        switch(index)
        {
            case 0:
                {

                    mainUIManager.SetCurrentDataNormButton(oldClimateNormButton);
                }
                break;
            case 1:
                {

                    mainUIManager.SetCurrentDataNormButton(newClimateNormButton);
                }
                break;
            case 2:
                {

                    mainUIManager.SetCurrentDataNormButton(normDifferenceButton);
                }
                break;
        }
        ChooseLegend(currentDataMode, currentDataType);
    }

    private void OnEnable()
    {
        if (countryClicker == null)
        {
            countryClicker = FindObjectOfType<CountryClicker>(true);
            reader = FindObjectOfType<ReadMeanTemperatureFiles>(true);
            mainUIManager = FindObjectOfType<MainUIManager>(true);
            gatherer = FindObjectOfType<DataGatherer>(true);
        }
        SetDataType(currentDataType);
        countryClicker.ChooseCountry(0);
        gatherer.ResetClickedData();
        RegionSelected(-1, "");
        earthRenderer.material.SetFloat("_Choice", (float)(int)currentDataMode);
        ChooseLegend(currentDataMode, currentDataType);
        oldClimateNormButton.onClick.Invoke();
        mainUIManager.SetCurrentDataNormButton(oldClimateNormButton);
    }


}
