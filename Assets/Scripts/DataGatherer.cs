using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.Burst.CompilerServices;
using UnityEngine.UIElements;
using Unity.Mathematics;





#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DataGatherer))]
public class DataGathererEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DataGatherer myTarget = (DataGatherer)target;
        if (GUILayout.Button("Gather Data"))
        {
            myTarget.GatherData();
            EditorUtility.SetDirty(myTarget);
        }
    }
}
#endif


public class DataGatherer : MonoBehaviour
{
    [SerializeField]
    private Texture2D countryTexture;
    [SerializeField]
    private Texture2DArray[] oldNormArrays;
    [SerializeField]
    private Texture2DArray[] newNormArrays;
    [SerializeField]
    private Texture2DArray[] comparisonArrays;
    [SerializeField]
    private Country[] countries;


    public Transform targetMarker;

    public float[][] currentOldValues; 
    public float[][] currentNewValues; 
    public float[][] currentComparisonValues;

    public float[][] clickedOldValues;
    public float[][] clickedNewValues;
    public float[][] clickedComparisonValues;

    private Color[] texturePixels;
    private int width;
    private int height;
    private int valueWidth;
    private int valueHeight;
    private float countryToValueWidth;
    private float countryToValueHeight;

    private int currentValueX;
    private int currentValueY;
    private int clickedValueX;
    private int clickedValueY;
    private DataUIManager dataUIManager;
    private CameraController cameraController;
    private int currentGuessCountry;

    [SerializeField]
    public List<int>[] countryTextureValueIndices;
    [SerializeField]
    private Color[][][] oldNormTextures;
    [SerializeField]
    private Color[][][] newNormTextures;
    [SerializeField]
    private Color[][][] comparisonTextures;
    [SerializeField]
    private float[] countryXs;
    [SerializeField]
    private float[] countryYs;

    private void Start()
    {
        width = countryTexture.width;
        height = countryTexture.height;
        valueWidth = oldNormArrays[0].width;
        countryToValueWidth = (float)oldNormArrays[0].width / (float)width;
        countryToValueHeight = (float)oldNormArrays[0].height / (float)height;
        dataUIManager = FindObjectOfType<DataUIManager>(true);
        cameraController = FindObjectOfType<CameraController>(true);
        GatherData();
    }

    public float GetCurrentClickedDistance()
    {
        float diffX = currentValueX - clickedValueX * countryToValueWidth;
        float diffY = currentValueY - clickedValueY * countryToValueWidth;
        return Mathf.Sqrt(diffX * diffX + diffY * diffY);
    }

    public float GetCurrentClickedScore()
    {
        float diffX = currentValueX - clickedValueX * countryToValueWidth;
        float diffY = currentValueY - clickedValueY * countryToValueWidth;
        float distance = Mathf.Sqrt(diffX * diffX + diffY * diffY);

        float count = countryTextureValueIndices[currentGuessCountry].Count;
        count = Mathf.Sqrt((float)count);
        float low = 0.1f * count;
        float high = 0.5f * count;
        float t = Mathf.Clamp01((distance - low) / (high - low));
        return (1.0f - t)*100.0f;
    }

    public float[] GetClickedValues(int dataType, DataUIManager.DataMode dataMode)
    {
        if(clickedOldValues == null)
        {
            return null;
        }
        switch(dataMode)
        {
            case DataUIManager.DataMode.OldNorm:
                {
                    return clickedOldValues[dataType];
                }
                break;
            case DataUIManager.DataMode.NewNorm:
                {
                    return clickedNewValues[dataType];
                }
                break;
            case DataUIManager.DataMode.NormDifference:
                {
                    return clickedComparisonValues[dataType];
                }
                break;
        }
        return null;
    }
    public float[] GetCurrentValues(int dataType, DataUIManager.DataMode dataMode)
    {
        if (currentOldValues == null)
        {
            return null;
        }
        switch (dataMode)
        {
            case DataUIManager.DataMode.OldNorm:
                {
                    return currentOldValues[dataType];
                }
                break;
            case DataUIManager.DataMode.NewNorm:
                {
                    return currentNewValues[dataType];
                }
                break;
            case DataUIManager.DataMode.NormDifference:
                {
                    return currentComparisonValues[dataType];
                }
                break;
        }
        return null;
    }
    public void ChoosePointInCountry()
    {
        int countryChoice = UnityEngine.Random.Range(0, 5*countries.Length);
        int countryIndex = 0;
        while(countryChoice > 0)
        {
            if (countryTextureValueIndices[countryIndex].Count > 50)
            {
                --countryChoice;
                if(countryChoice == 0)
                {
                    break;
                }
            }
            countryIndex = (countryIndex + 1) % countries.Length;
        }
        int pointIndex = UnityEngine.Random.Range(0, countryTextureValueIndices[countryIndex].Count);
        int countryValueIndex = countryTextureValueIndices[countryIndex][pointIndex];
        int countryX = countryValueIndex % valueWidth;
        int countryY = countryValueIndex / valueWidth;
        FillDataAtPoint(countryX, countryY);
        if(dataUIManager == null)
        {
            dataUIManager = FindObjectOfType<DataUIManager>(true);
        }
        dataUIManager.GuessRegionSelected(countryIndex, countries[countryIndex].name);

        float countryFX = ((float)countryX) / (float)(valueWidth-1);
        float countryFY = ((float)countryY) / (float)(valueHeight-1);

        float fy = (float)countryFY;


        float polarDiv = Mathf.PI;
        float azimuthDiv = (2.0f * Mathf.PI);

        float currPolar = -(1.0f - fy) * polarDiv;

        float currAzimuth = (countryFX + 0.5f) * azimuthDiv;

        Vector3 position = 0.5f * new Vector3(math.sin(currPolar) * math.cos(currAzimuth), math.cos(currPolar), math.sin(currPolar) * math.sin(currAzimuth));

        targetMarker.position = position;
        targetMarker.localRotation = Quaternion.FromToRotation(Vector3.up, position.normalized);

        PointCameraAtCountry(countryIndex);

        currentGuessCountry = countryIndex;
    }

    public void PointCameraAtCountry(int countryIndex)
    {
        if (countryIndex > -1)
        {
            cameraController.SetTargetAt(countryXs[countryIndex], countryYs[countryIndex]);
        }

    }

    public void FillDataAtPoint(int countryValueX, int countryValueY)
    {
        currentValueX = (int)countryValueX;
        currentValueY = (int)countryValueY;
        int valueIndex = (int)countryValueY * valueWidth + (int)countryValueX;

        if (currentOldValues == null)
        {
            currentOldValues = new float[11][];
            currentNewValues = new float[11][];
            currentComparisonValues = new float[11][];
        }

        for(int i = 0; i < 11; ++i)
        {
            if (currentOldValues[i] == null)
            {
                currentOldValues[i] = new float[12];
                currentNewValues[i] = new float[12];
                currentComparisonValues[i] = new float[12];
            }

            for (int j = 0; j < 12; ++j)
            {
                currentOldValues[i][j] = oldNormTextures[i][j][valueIndex].r;
                currentNewValues[i][j] = newNormTextures[i][j][valueIndex].r;
                currentComparisonValues[i][j] = comparisonTextures[i][j][valueIndex].r;
            }
        }
    }

    public void ResetClickedData()
    {
        clickedOldValues = null;
        clickedNewValues = null;
        clickedComparisonValues = null;
    }

    public void FillDataAtClickedPoint(int countryTextureX, int countryTextureY)
    {
        float valueX = (float)countryTextureX * countryToValueWidth;
        float valueY = (float)countryTextureY * countryToValueHeight;
        clickedValueX = (int)countryTextureX;
        clickedValueY = (int)countryTextureY;
        int valueIndex = (int)valueY*valueWidth + (int)valueX;

        if (clickedOldValues == null)
        {
            clickedOldValues = new float[11][];
            clickedNewValues = new float[11][];
            clickedComparisonValues = new float[11][];
        }
        for (int i = 0; i < 11; ++i)
        {
            if (clickedOldValues[i] == null)
            {
                clickedOldValues[i] = new float[12];
                clickedNewValues[i] = new float[12];
                clickedComparisonValues[i] = new float[12];
            }
            for (int j = 0; j < 12; ++j)
            {
                clickedOldValues[i][j] = oldNormTextures[i][j][valueIndex].r;
                clickedNewValues[i][j] = newNormTextures[i][j][valueIndex].r;
                clickedComparisonValues[i][j] = comparisonTextures[i][j][valueIndex].r;
            }
        }
    }

    public void GatherData()
    {

        texturePixels = countryTexture.GetPixels();
        countryTextureValueIndices = new List<int>[countries.Length];
        for(int i = 0; i < countries.Length; ++i)
        {
            countryTextureValueIndices[i] = new List<int>();
        }
        width = countryTexture.width;
        height = countryTexture.height;
        countryXs = new float[countries.Length];
        countryYs = new float[countries.Length];
        float[] countryXValues = new float[countries.Length];
        float[] countryYValues = new float[countries.Length];
        float[] countryValuesCounts = new float[countries.Length];
        oldNormTextures = new Color[11][][];
        newNormTextures = new Color[11][][];
        comparisonTextures = new Color[11][][];
        for (int i = 0; i < 11; ++i)
        {
            oldNormTextures[i] = new Color[12][];
            newNormTextures[i] = new Color[12][];
            comparisonTextures[i] = new Color[12][];
            for (int j = 0; j < 12; ++j)
            {
                oldNormTextures[i][j] = oldNormArrays[i].GetPixels(j);
                newNormTextures[i][j] = newNormArrays[i].GetPixels(j);
                comparisonTextures[i][j] = comparisonArrays[i].GetPixels(j);
            }
        }
        valueWidth = oldNormArrays[0].width;
        valueHeight = oldNormArrays[0].height;
        countryToValueWidth = (float)oldNormArrays[0].width/(float)width;
        countryToValueHeight = (float)oldNormArrays[0].height/(float)height;
        List<int>[] ownedByCountries = new List<int>[valueWidth*valueHeight];
        for(int i = 0; i < valueWidth*valueHeight; ++i)
        {
            ownedByCountries[i] = new List<int>();
        }
        float yValue = 0.0f;
        int index = 0;
        for(int y = 0; y < height; ++y)
        {
            float xValue = 0.0f;
            for (int x = 0; x < width; ++x)
            {
                float redValue = texturePixels[index].r;
                if (redValue > 0.0f)
                {
                    float val = 255.0f * redValue - 1.0f;

                    int countryIndex = (int)val;

                    countryXValues[countryIndex] += (float)x / (float)width;
                    countryYValues[countryIndex] += (float)y / (float)height;
                    countryValuesCounts[countryIndex]++;

                    int valueIndex = (int)yValue * valueWidth + (int)xValue;

                    List<int> currentOwned = ownedByCountries[valueIndex];
                    if (currentOwned.Contains(countryIndex) == false)
                    {
                        List<int> currentList = countryTextureValueIndices[countryIndex];
                        currentList.Add(valueIndex);
                        currentOwned.Add(countryIndex);
                    }

                }
                xValue += countryToValueWidth;
                ++index;
            }
            yValue += countryToValueHeight;
        }

        for(int i = 0; i < countries.Length; ++i)
        {
            countryXs[i] = countryXValues[i] / (float)countryValuesCounts[i];
            countryYs[i] = countryYValues[i] / (float)countryValuesCounts[i];
        }
    }
}
