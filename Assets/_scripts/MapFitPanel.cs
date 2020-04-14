using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CampusSimulator;

public class MapFitPanel : MonoBehaviour
{

    Slider mapHeight;
    float oldMapHeight;
    Text mapHeightText;

    Slider mapRotate;
    float oldMapRotate;
    Text mapRotateText;

    Slider mapXval;
    float oldMapXval;
    Text mapXvalText;

    Slider mapZval;
    float oldMapZval;
    Text mapZvalText;

    SceneMan sman;
    LinkCloudMan lman;
    MapMan mman;

    bool panelActive = false;

    void Start()
    {
        panelActive = false;
        LinkObjectsAndComponents();
    }

    public void LinkObjectsAndComponents()
    {
        sman = FindObjectOfType<SceneMan>();
        if (sman == null)
        {
            throw new UnityException("MapFit panel could not find RegionMan");
        }
        lman = FindObjectOfType<LinkCloudMan>();
        mman = FindObjectOfType<MapMan>();

        if (lman == null)
        {
            Debug.Log("lman null");
        }
        {
            var go = transform.Find("MapHeightSlider").gameObject;
            mapHeight = go.GetComponent<Slider>();
            var go1 = go.transform.Find("MapHeightText").gameObject;
            mapHeightText = go1.GetComponent<Text>();
        }
        {
            var go = transform.Find("MapRotateSlider").gameObject;
            mapRotate = go.GetComponent<Slider>();
            var go1 = go.transform.Find("MapRotateText").gameObject;
            mapRotateText = go1.GetComponent<Text>();
        }
        {
            var go = transform.Find("MapXvalSlider").gameObject;
            mapXval = go.GetComponent<Slider>();
            var go1 = go.transform.Find("MapXvalText").gameObject;
            mapXvalText = go1.GetComponent<Text>();
        }
        {
            var go = transform.Find("MapZvalSlider").gameObject;
            mapZval = go.GetComponent<Slider>();
            var go1 = go.transform.Find("MapZvalText").gameObject;
            mapZvalText = go1.GetComponent<Text>();
        }

        panelActive = true;
    }
    public void InitVals()
    {
        Debug.Log("InitVals called");

        mapHeight.minValue = -2;
        mapHeight.maxValue = 10;
        oldMapHeight = sman.GetCurrentPosition().y;
        mapHeight.value = oldMapHeight;

        mapRotate.minValue =  -180;
        mapRotate.maxValue =   180;
        oldMapRotate = sman.rgoRotate;
        mapRotate.value = oldMapRotate;

        mapXval.minValue = -50;
        mapXval.maxValue = 50;
        oldMapXval = sman.GetCurrentPosition().x;
        mapXval.value = oldMapXval;

        mapZval.minValue = -50;
        mapZval.maxValue = 50;
        oldMapZval = sman.GetCurrentPosition().z;
        mapZval.value = oldMapZval;
        panelActive = true;
    }

    int nSetTextValuesCalled = 0;
    private void SetTextValues()
    {
        nSetTextValuesCalled += 1;
        mapHeightText.text = "Height " + mapHeight.value.ToString("F1");
        mapRotateText.text = "Rotate " + mapRotate.value.ToString("F1");
        mapXvalText.text = "X val " + mapXval.value.ToString("F1");
        mapZvalText.text = "Z val " + mapZval.value.ToString("F1");
    }
    private void UpdateMapHeight()
    {
        var callsettextvalues = false;
        if (oldMapHeight != mapHeight.value)
        {
            Debug.Log("Map height changed to " + mapHeight.value);
            var diffv = new Vector3(0, mapHeight.value - oldMapHeight, 0);
            sman.CorrectPositionDiff(diffv);
            oldMapHeight = mapHeight.value;
            callsettextvalues = true;
        }
        if (oldMapRotate != mapRotate.value)
        {
            Debug.Log("Map rotation changed to " + mapRotate.value);
            var diff = mapRotate.value - oldMapRotate;
            sman.CorrectAngle(diff);
            oldMapRotate = mapRotate.value;
            callsettextvalues = true;
        }
        if (oldMapXval != mapXval.value)
        {
            Debug.Log("Map Xval changed to " + mapXval.value);
            var diffv = new Vector3(mapXval.value - oldMapXval, 0, 0);
            sman.CorrectPositionDiff(diffv);
            oldMapXval = mapXval.value;
            callsettextvalues = true;
        }
        if (oldMapZval != mapZval.value)
        {
            Debug.Log("Map Zval changed to " + mapZval.value);
            var diffv = new Vector3(0,0,mapZval.value - oldMapZval);
            sman.CorrectPositionDiff(diffv);
            oldMapZval = mapZval.value;
            callsettextvalues = true;
        }
        if (callsettextvalues)
        {
            SetTextValues();
            sman.RequestRefresh("MapFitPanel-updateMapHeight");
            SetTextValues();
        }
    }


    public void SetVals()
    {
        Debug.Log("SetVals called");
        UpdateMapHeight();
        panelActive = false;
        sman.RequestRefresh("MapFitPanel-SetVals");
    }

    // Update is called once per frame
    void Update()
    {
        if (panelActive)
        {
            if (nSetTextValuesCalled==0)
            {
                SetTextValues();
            }
            UpdateMapHeight();
        }
    }
}
