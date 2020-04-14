using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{

    public GameObject visualPanelGo;
    public VisualsPanel visualsPanel;
    public GameObject mapFitPanelGo;
    public MapFitPanel mapFitPanel;
    public GameObject framePanelGo;
    public FramePanel framePanel;
    public GameObject b19PanelGo;
    public B19Panel b19Panel;
    public GameObject generalPanelGo;
    public GeneralPanel generalPanel;
    public GameObject helpPanelGo;
    public HelpPanel helpPanel;
    public GameObject aboutPanelGo;
    public AboutPanel aboutPanel;
    public Toggle visualToggle;
    public Toggle mapfitToggle;
    public Toggle frameToggle;
    public Toggle b19Toggle;
    public Toggle generalToggle;
    public Toggle helpToggle;
    public Toggle aboutToggle;
    public ToggleGroup togGroup;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Options Panel Start:"+name);
        aboutPanelGo = transform.Find("AboutPanel").gameObject;
        aboutPanel = aboutPanelGo.GetComponent<AboutPanel>();
        visualPanelGo = transform.Find("VisualsPanel").gameObject;
        visualsPanel = visualPanelGo.GetComponent<VisualsPanel>();
        mapFitPanelGo = transform.Find("MapFitPanel").gameObject;
        mapFitPanel = mapFitPanelGo.GetComponent<MapFitPanel>();
        framePanelGo = transform.Find("FramePanel").gameObject;
        framePanel = framePanelGo.GetComponent<FramePanel>();
        b19PanelGo = transform.Find("B19Panel").gameObject;
        b19Panel = b19PanelGo.GetComponent<B19Panel>();
        generalPanelGo = transform.Find("GeneralPanel").gameObject;
        generalPanel = generalPanelGo.GetComponent<GeneralPanel>();
        helpPanelGo = transform.Find("HelpPanel").gameObject;
        helpPanel = helpPanelGo.GetComponent<HelpPanel>();
        var vgo = transform.Find("VisualsToggle").gameObject;
        visualToggle = vgo.GetComponent<Toggle>();
        var mgo = transform.Find("MapFitToggle").gameObject;
        mapfitToggle = mgo.GetComponent<Toggle>();
        var fgo = transform.Find("FrameToggle").gameObject;
        frameToggle = fgo.GetComponent<Toggle>();
        var bgo = transform.Find("B19Toggle").gameObject;
        b19Toggle = bgo.GetComponent<Toggle>();
        var ggo = transform.Find("GeneralToggle").gameObject;
        generalToggle = ggo.GetComponent<Toggle>();
        var hgo = transform.Find("HelpToggle").gameObject;
        helpToggle = hgo.GetComponent<Toggle>();
        var ago = transform.Find("AboutToggle").gameObject;
        aboutToggle = ago.GetComponent<Toggle>();
        togGroup = GetComponent<ToggleGroup>();
        togGroup.allowSwitchOff = true; // otherwise it does not save state correctly when we turn off the panel
        SyncState();
    }
    public void SyncState()
    {
        if (!visualPanelGo) return; // not initialized yet
        visualPanelGo.SetActive(visualToggle.isOn);
        mapFitPanelGo.SetActive(mapfitToggle.isOn);
        framePanelGo.SetActive(frameToggle.isOn);
        b19PanelGo.SetActive(b19Toggle.isOn);
        generalPanelGo.SetActive(generalToggle.isOn);
        helpPanelGo.SetActive(helpToggle.isOn);
        aboutPanelGo.SetActive(aboutToggle.isOn);
        if (aboutToggle.isOn)
        {
            aboutPanel.FillAboutPanel();
        }
        if (visualToggle.isOn)
        {
            visualsPanel.InitVals();
        }
        if (frameToggle.isOn)
        {
            framePanel.InitVals();
        }
        if (b19Toggle.isOn)
        {
            b19Panel.InitVals();
        }
        if (helpToggle.isOn)
        {
            helpPanel.FillHelpPanel();
        }
        if (generalToggle.isOn)
        {
            generalPanel.InitVals();
        }
    }
    public void ChangingOptionsDialog(bool isOpening)
    {
        if (isOpening)
        {
            SyncState();
        }
        else
        {
            // must be closeing then
            if (visualToggle.isOn)
            {
                visualsPanel.SetVals();
            }
            if (frameToggle.isOn)
            {
                framePanel.SetVals();
            }
            if (b19Toggle.isOn)
            {
                b19Panel.SetVals();
            }
            if (generalToggle.isOn)
            {
                generalPanel.SetVals();
            }
        }
    }
    public void OptionsTabToggle()
    {
        SyncState();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
