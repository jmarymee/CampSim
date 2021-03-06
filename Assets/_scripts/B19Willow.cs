﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UxUtils;

public class B19Willow : MonoBehaviour
{
    public enum B19_MaterialMode {  raw, materialed, glass, glasswalls };

    public UxSetting<bool> loadmodel = new UxSetting<bool>("B19_WillowModel", true);
    public UxSetting<bool> level01 = new UxSetting<bool>("B19_level01", true);
    public UxSetting<bool> level02 = new UxSetting<bool>("B19_level02", true);
    public UxSetting<bool> level03 = new UxSetting<bool>("B19_level03", true);
    public UxSetting<bool> hvac = new UxSetting<bool>("B19_hvac", true);
    public UxSetting<bool> floors = new UxSetting<bool>("B19_floors", true);
    public UxSetting<bool> doors = new UxSetting<bool>("B19_doors", true);


    public UxEnumSetting<B19_MaterialMode> b19_materialMode = new UxEnumSetting<B19_MaterialMode>("B19_MaterialMode", B19_MaterialMode.materialed);
    //   public UxSetting<bool> visibilityTiedToDetectability = new UxSetting<bool>("FrameVisibilityTiedToDetectability", true);
    // public B19_MaterialMode materialMode = B19_MaterialMode.materialed;

    // Start is called before the first frame update
    void Start()
    {


    }

    public void init()
    {
        b19_materialMode.GetInitial();
        loadmodel.GetInitial();
        level01.GetInitial();
        level02.GetInitial();
        level03.GetInitial();
        hvac.GetInitial();
        floors.GetInitial();
        doors.GetInitial();
    }



    bool _b19_WillowModel = false;
    bool _b19_level01 = false;
    bool _b19_level02 = false;
    bool _b19_level03 = false;
    bool _b19_hvac = false;
    bool _b19_floors = false;
    bool _b19_doors = false;
    GameObject willgo = null;



    bool ChangeHappened()
    {
        var chg = false;
        if (loadmodel.Get() != _b19_WillowModel) chg = true;
        if (level01.Get() != _b19_level01)
        {
            chg = true;
            //Debug.Log("1");
        }
        if (level02.Get() != _b19_level02)
        {
            chg = true;
            //Debug.Log("2");
        }
        if (level03.Get() != _b19_level03)
        {
            chg = true;
            //Debug.Log("3");
        }
        if (floors.Get() != _b19_floors)
        {
            chg = true;
            //Debug.Log("floors");
        }
        if (doors.Get() != _b19_doors)
        {
            chg = true;
            //Debug.Log("floors");
        }
        if (hvac.Get() != _b19_hvac)
        {
            chg = true;
            //Debug.Log("floors");
        }
        return chg;
    }

    void SetChildrenVis(string childname, bool active)
    {
        GameObject[] children = willgo.transform.GetComponentsInChildren<GameObject>();
        foreach (var goc in children)
        {
            if (goc.name == childname)
            {
                goc.SetActive(active);
            }
        }
    }

    void SetChildVis2(string childname, string grandchildname, bool active)
    {
        var legot = willgo.transform.Find(childname);
        if (legot != null)
        {
            var legot2 = legot.transform.Find(grandchildname);
            if (legot2 != null)
            {
                legot2.gameObject.SetActive(active);
            }
        }
    }

    void SetChildVis(string childname,bool active)
    {
        var legot = willgo.transform.Find(childname);
        if (legot!=null)
        {
            legot.gameObject.SetActive(active);
        }
    }
    void MakeItSo()
    {
        if (loadmodel.Get() && !_b19_WillowModel)
        {
            var obprefab = Resources.Load<GameObject>("Willow/B19-Willow");
            willgo = Instantiate<GameObject>(obprefab);
            var ftm = 0.3048f;
            willgo.transform.localScale = new Vector3(ftm,ftm,ftm);
            willgo.transform.position = new Vector3(-474.3f, 4.72f, 87.6f);
            willgo.transform.Rotate(new Vector3(-90, 26, 0));
            willgo.transform.parent = this.transform;
            _b19_WillowModel = true;
            level01.SetAndSave( true );
            _b19_level01 = true;
            level02.SetAndSave( false );
            _b19_level02 = true;
            level03.SetAndSave( false );
            _b19_level03 = true;
            floors.SetAndSave( false );
            _b19_floors = true;
            doors.SetAndSave( false );
            _b19_doors = true;
            hvac.SetAndSave( false );
            _b19_hvac = true;
        }
        else if(!loadmodel.Get() && _b19_WillowModel)
        {
            Destroy(willgo);
            loadmodel.SetAndSave( false );
            _b19_WillowModel = false;
            level01.SetAndSave( false );
            _b19_level01 = false;
            level02.SetAndSave( false );
            _b19_level02 = false;
            level03.SetAndSave( false );
            _b19_level03 = false;
            hvac.SetAndSave( false );
            _b19_hvac = false;

        }
        if (level01.Get() != _b19_level01)
        {
            //Debug.Log("Fixing 1");
            var stat = level01.Get();
            SetChildVis("L01-AR", stat);
            //SetChildVis("L01-ME", stat);
            _b19_level01 = stat;
        }
        if (level02.Get() != _b19_level02)
        {
            //Debug.Log("Fixing 2");
            var stat = level02.Get();
            SetChildVis("L02-AR", stat);
            //SetChildVis("L02-ME", stat);
            _b19_level02 = stat;
        }
        if (level03.Get() != _b19_level03)
        {
            //Debug.Log("Fixing 3");
            var stat = level03.Get();
            SetChildVis("L03-AR", stat);
            //SetChildVis("L03-ME", stat);
            _b19_level03 = stat;
        }
        if (floors.Get() != _b19_floors)
        {
            //Debug.Log("Fixing floors");
            var stat = floors.Get();
            SetChildVis2("L01-AR", "Solid", stat);
            SetChildVis2("L02-AR", "Solid", stat);
            SetChildVis2("L03-AR", "Solid", stat);
            _b19_floors = stat;
        }
        if (doors.Get() != _b19_doors)
        {
            //Debug.Log("Fixing doors");
            var stat = doors.Get();
            SetChildVis2("L01-AR", "Composite_Part", stat);
            SetChildVis2("L02-AR", "Composite_Part", stat);
            SetChildVis2("L03-AR", "Composite_Part", stat);
            _b19_doors = stat;
        }
        if (hvac.Get() != _b19_hvac)
        {
            //Debug.Log("Fixing hvac");
            var stat = hvac.Get();
            SetChildVis("L01-ME", stat);
            SetChildVis("L02-ME", stat);
            SetChildVis("L03-ME", stat);
            _b19_hvac = stat;
        }
        ActuateMaterialMode();
    }



    List<string> B19_parts = new List<string>
    {
        "B19-Willow/L03-ME/Exhaust_Air_Duct,Exhaust_Air_DuctMat",
        "B19-Willow/L03-ME/Solid,SolidMat",
        "B19-Willow/L03-ME/Supply_Air_Duct,Supply_Air_DuctMat",
        "B19-Willow/L01-AR/*Door_Frame,_Door_FrameMat",
        "B19-Willow/L01-AR/*Finish_Facade_Feature_Wall,_Finish_Facade_Feature_WallMat",
        "B19-Willow/L01-AR/*Glazing_Glass_-_Clear,_Glazing_Glass_-_ClearMat",
        "B19-Willow/L01-AR/*Metal_Aluminum,_Metal_AluminumMat",
        "B19-Willow/L01-AR/*Metal_Powdercoated_White,_Metal_Powdercoated_WhiteMat",
        "B19-Willow/L01-AR/*Metal_Stainless_Steel_-_Polished,_Metal_Stainless_Steel_-_PolishedMat",
        "B19-Willow/L01-AR/*Wall_Generic,_Wall_GenericMat",
        "B19-Willow/L01-AR/.Wall_Default,",
        "B19-Willow/L01-AR/_Aluminium,_AluminiumMat",
        "B19-Willow/L01-AR/Composite_Part,Composite_PartMat",
        "B19-Willow/L01-AR/Computer_Basic,Computer_BasicMat",
        "B19-Willow/L01-AR/Computer_Basic_2,Computer_Basic_2Mat",
        "B19-Willow/L01-AR/Computer_Basic_3,Computer_Basic_3Mat",
        "B19-Willow/L01-AR/Computer_Glass,Computer_GlassMat",
        "B19-Willow/L01-AR/Computer_Light_(ON),Computer_Light_(ON)Mat",
        "B19-Willow/L01-AR/Computer_Metal,Computer_MetalMat",
        "B19-Willow/L01-AR/Computer_Metal_2,Computer_Metal_2Mat",
        "B19-Willow/L01-AR/Generic_-_Plastic_-_Black,Generic_-_Plastic_-_BlackMat",
        "B19-Willow/L01-AR/Generic_-_Plastic_-_Grey,Generic_-_Plastic_-_GreyMat",
        "B19-Willow/L01-AR/IKE080018_2,IKE080018_2Mat",
        "B19-Willow/L01-AR/IKE080018_3,IKE080018_3Mat",
        "B19-Willow/L01-AR/IKE160124_1,IKE160124_1Mat",
        "B19-Willow/L01-AR/IKE160124_2,IKE160124_2Mat",
        "B19-Willow/L01-AR/IKE160124_3,IKE160124_3Mat",
        "B19-Willow/L01-AR/IKE160124_4,IKE160124_4Mat",
        "B19-Willow/L01-AR/IKE160130_1,IKE160130_1Mat",
        "B19-Willow/L01-AR/IKE160130_2,IKE160130_2Mat",
        "B19-Willow/L01-AR/IKE160130_3,IKE160130_3Mat",
        "B19-Willow/L01-AR/IKE160130_4,IKE160130_4Mat",
        "B19-Willow/L01-AR/Metal-Chrome-Caroma,Metal-Chrome-CaromaMat",
        "B19-Willow/L01-AR/PC_Monitor_Color,PC_Monitor_ColorMat",
        "B19-Willow/L01-AR/PC_Monitor_Glass,PC_Monitor_GlassMat",
        "B19-Willow/L01-AR/Porcelain-White-Caroma,Porcelain-White-CaromaMat",
        "B19-Willow/L01-AR/Solid,SolidMat",
        "B19-Willow/L01-ME/Copper,CopperMat",
        "B19-Willow/L01-ME/Gas_Pipe,Gas_PipeMat",
        "B19-Willow/L01-ME/Line,LineMat",
        "B19-Willow/L01-ME/Return_Air_Duct,Return_Air_DuctMat",
        "B19-Willow/L01-ME/Solid,SolidMat",
        "B19-Willow/L01-ME/Supply_Air_Duct,Supply_Air_DuctMat",
        "B19-Willow/L02-AR/*Door_Frame,_Door_FrameMat",
        "B19-Willow/L02-AR/*Fabric_Linen_Porcelain,_Fabric_Linen_PorcelainMat",
        "B19-Willow/L02-AR/*Finish_Facade_Feature_Wall,_Finish_Facade_Feature_WallMat",
        "B19-Willow/L02-AR/*Glazing_Glass_-_Clear,_Glazing_Glass_-_ClearMat",
        "B19-Willow/L02-AR/*Metal_Aluminum,_Metal_AluminumMat",
        "B19-Willow/L02-AR/*Wall_Generic,_Wall_GenericMat",
        "B19-Willow/L02-AR/.Wall_Default,",
        "B19-Willow/L02-AR/_Aluminium,_AluminiumMat",
        "B19-Willow/L02-AR/Composite_Part,Composite_PartMat",
        "B19-Willow/L02-AR/Metal-Chrome-Caroma,Metal-Chrome-CaromaMat",
        "B19-Willow/L02-AR/Porcelain-White-Caroma,Porcelain-White-CaromaMat",
        "B19-Willow/L02-AR/Solid,SolidMat",
        "B19-Willow/L02-ME/Exhaust_Air_Duct,Exhaust_Air_DuctMat",
        "B19-Willow/L02-ME/Return_Air_Duct,Return_Air_DuctMat",
        "B19-Willow/L02-ME/Solid,SolidMat",
        "B19-Willow/L02-ME/Supply_Air_Duct,Supply_Air_DuctMat",
        "B19-Willow/L03-AR/*Finish_Facade_Feature_Wall,_Finish_Facade_Feature_WallMat",
        "B19-Willow/L03-AR/*Wall_Generic,_Wall_GenericMat"
    };

    Dictionary<string, string> bldmatmap = new Dictionary<string, string>
    {
        {"SolidMat","WallWhite"},
        {"Exhaust_Air_DuctMat","Aluminium"},
        {"Supply_Air_DuctMat","Aluminium"},
        {"Return_Air_DuctMat","Aluminium"},
        {"_Door_FrameMat","Plywood"},
        {"_Finish_Facade_Feature_WallMat","FacadeWall"},
        {"_Glazing_Glass_-_ClearMat","DustyGlass"},
        {"_Metal_Powdercoated_WhiteMat","Aluminium"},
        {"_Metal_Stainless_Steel_-_PolishedMat","Steel"},
        {"_Wall_GenericMat","WallMat"},
        {"_Fabric_Linen_PorcelainMat","WallMat"},
        {"","WallMat"},
        {"_Metal_AluminumMat","Aluminium"},
        {"_AluminiumMat","Aluminium"},
        {"Composite_PartMat","WallMat"},
        {"Computer_BasicMat","PlasticHololens"},
        {"Computer_Basic_2Mat","PlasticHololens"},
        {"Computer_Basic_3Mat","PlasticHololens"},
        {"Computer_GlassMat","ComputerGlass"},
        {"Computer_Light_(ON)Mat","BlueLight"},
        {"Computer_MetalMat","Aluminium"},
        {"Computer_Metal_2Mat","Aluminium"},
        {"PC_Monitor_ColorMat","ComputerGlass"},
        {"PC_Monitor_GlassMat","ComputerGlass"},
        {"Generic_-_Plastic_-_BlackMat","PlasticHololens"},
        {"Generic_-_Plastic_-_GreyMat","PlasticHololens"},
        {"IKE080018_2Mat","Aluminium"},
        {"IKE080018_3Mat","Aluminium"},
        {"IKE160124_1Mat","Aluminium"},
        {"IKE160124_2Mat","Aluminium"},
        {"IKE160124_3Mat","Aluminium"},
        {"IKE160124_4Mat","Aluminium"},
        {"IKE160130_1Mat","Aluminium"},
        {"IKE160130_2Mat","Aluminium"},
        {"IKE160130_3Mat","Aluminium"},
        {"IKE160130_4Mat","Aluminium"},
        {"Metal-Chrome-CaromaMat","Steel"},
        {"Porcelain-White-CaromaMat","WallMat"},
        {"CopperMat","Copper"},
        {"Gas_PipeMat","Copper"},
        {"LineMat","WallMat"},
    };



    public void WriteOutPartsAndMaterials()
    {
        if (this.willgo == null)
        {
            this.willgo = GameObject.Find("B19-Willow");
        }
        if (this.willgo == null)
        {
            Debug.Log("Cound not find B19-Willow");
            return;
        }
        var lst = GraphAlgos.GraphUtil.HierarchyDescToText(this.willgo, "");
        var fname = "B19materials.txt";
        GraphAlgos.GraphUtil.writeListToFile(lst, fname);
        Debug.Log("Wrote " + lst.Count + " lines to " + fname);
    }

    public GameObject GetPart(GameObject root,string partname)
    {
        var parslh = partname.Split('/');
        var curgo = root;
        for(var i=0; i<parslh.Length; i++)
        {
            var part = parslh[i];
            if (i == 0) continue;
            var tform = curgo.transform.Find(part);
            if (tform == null)
            {
                Debug.Log("GetPart failed to find " + partname + "-  failed name part" + part);
                return null;
            }

            curgo = tform.gameObject;
        }
        return curgo;
    }
    public void AssignPartMat(GameObject rootgo,string partname,string matname)
    {
        if (matname=="")
        {
            return; // do nothing - raw mode
        }
        var pogo = GetPart(rootgo, partname);
        if (pogo!=null)
        {
            var renderer = pogo.GetComponent<Renderer>();
            if (!renderer)
            {
                Debug.Log("No renderer found for " + partname+" pogo.name:"+pogo.name);
                return;
            }
            var fullmatname = "Materials/" + matname;
            var mat = Resources.Load<Material>(fullmatname);
            if (!mat)
            {
                Debug.Log("Material "+fullmatname+" not found in Resources");
                return;
            }
            renderer.material = mat;
        }
    }

    public void ActuateMaterialMode()
    {
        if (this.willgo == null)
        {
            this.willgo = GameObject.Find("B19-Willow");
        }
        if (this.willgo == null)
        {
            Debug.Log("Cound not find B19-Willow");
            return;
        }
        foreach( var pname in B19_parts)
        {
            var parcom = pname.Split(',');
            var partname = parcom[0];
            var partmat = parcom[1];
            var defmatname = "ComputerGlass";
            var matname = defmatname;
            switch (b19_materialMode.Get())
            {
                case B19_MaterialMode.glass:
                    matname = "ComputerGlass";
                    break;
                case B19_MaterialMode.materialed:
                    matname = bldmatmap[parcom[1]];
                    break;
                case B19_MaterialMode.glasswalls:
                    var pnl = partname.ToLower();
                    if (pnl.Contains("solid") || pnl.Contains("wall") || pnl.Contains("door") || pnl.Contains("composite_part"))
                    {
                        matname = "ComputerGlass";
                    }
                    else
                    {
                        matname = bldmatmap[partmat];
                    }
                    break;
                case B19_MaterialMode.raw:
                    //matname = parcom[1];
                    matname = "";
                    break;
            }
            AssignPartMat(this.willgo,partname, matname);
        }
        //var lst = GraphAlgos.GraphUtil.HierarchyDescToText(this.willgo, "");
        //var fname = "B19materials.txt";
        //GraphAlgos.GraphUtil.writeListToFile(lst, fname);
        //Debug.Log("Wrote " + lst.Count + " lines to " + fname);
    }

    B19_MaterialMode lastMaterialMode;
    int updcount = 0;

    // Update is called once per frame
    void Update()
    {
        if (updcount==0)
        {
            lastMaterialMode = b19_materialMode.Get();
        }
        if (ChangeHappened())
        {
            //Debug.Log("ChangeHappened");
            MakeItSo();
        }
        if (lastMaterialMode!=b19_materialMode.Get())
        {
            ActuateMaterialMode();
            lastMaterialMode = b19_materialMode.Get();
        }
        updcount++;
    }
}
