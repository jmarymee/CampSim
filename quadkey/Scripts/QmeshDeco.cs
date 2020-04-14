using System.Collections;
using UnityEngine;
public abstract class QmeshDeco : MonoBehaviour
{
    public enum DecoType { Template, TriPoints,TriLines, MeshNodes,CoordDefNodes,ExtentDefNodes,NativeNodes,FrameQuadKeys, SprinklePrefabs,SprinkleTrucks,SprinkleTowers,SubMeshShowNodes }
    public DecoType decotype;
    public string deconame = "deco";

    public bool showDeco;

    public GameObject decoroot = null;

    protected QmapMesh qmm;
    protected MfWrap mfw;
    protected GameObject preferedParent;
    protected Layer layer=null;
    protected string initstring;
    // Start is called before the first frame update

    public void Init(QmapMesh qmm,GameObject preferedParent,MfWrap mfw=null,string initstring="")
    {
        this.qmm = qmm;
        this.mfw = mfw;
        this.preferedParent = preferedParent;
        this.initstring = initstring;
        InitDecoType();
    }

    public abstract void InitDecoType();


    public virtual void Destruct()
    {
        if (decoroot != null)
        {
            Destroy(decoroot);
            decoroot = null;
        }
    }

    public void ConstructBase()
    {

        decoroot = new GameObject("objects");
        if (preferedParent != null)
        {
            decoroot.transform.parent = preferedParent.transform;
        }
    }

    public virtual void Construct()
    {
        ConstructBase();
    }

    public virtual void EnsureExistence()
    {
        if (showDeco)
        {
            if (layer==null)
            {
                layer = qmm.layerman.AddLayer(deconame);
                preferedParent = layer.gameObject;
            }
            if (decoroot == null)
            {
                Construct();
            }
        }
        else
        {
            Destruct();
        }
    }

    public virtual void UpdateState()
    {
    }
    public virtual void Update()
    {
        EnsureExistence();
        UpdateState();
    }
}
public class TemplateDeco : QmeshDeco
{
    public GameObject sphere;
    public override void InitDecoType()
    {
        decotype = DecoType.Template;
        deconame = "template";
    }

    public override void Construct()
    {
        ConstructBase();

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = decoroot.transform;
        qut.SetColorOfGo(sphere, Color.yellow);
    }
}
public class TriPointDeco : QmeshDeco
{

    public GameObject tpnode1;
    public GameObject tpnode2;
    public GameObject tpnode3;
    public GameObject tpnodem;

    public override void InitDecoType()
    {
        decotype = DecoType.TriPoints;
        deconame = "tripoints";
    }

    public override void Construct()
    {
        ConstructBase();
        var ska = qmm.triDiag * 0.05f;
        var skav = new Vector3(ska, ska, ska);
        var skam = new Vector3(1,1,1);
        tpnode1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tpnode1.name = "tpnode1";
        tpnode1.transform.localScale = skav;
        tpnode1.transform.parent = decoroot.transform;

        tpnode2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tpnode2.name = "tpnode2";
        tpnode2.transform.localScale = skav;
        tpnode2.transform.parent = decoroot.transform;

        tpnode3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tpnode3.name = "tpnode3";
        tpnode3.transform.localScale = skav;
        tpnode3.transform.parent = decoroot.transform;

        tpnodem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tpnodem.name = "tpnodem";
        tpnodem.transform.localScale = skam;
        tpnodem.transform.parent = decoroot.transform;

        qut.SetColorOfGo(tpnode1, Color.yellow);
        qut.SetColorOfGo(tpnode2, Color.cyan);
        qut.SetColorOfGo(tpnode3, Color.magenta);
        qut.SetColorOfGo(tpnodem, Color.blue);
        Debug.Log("tpnodes constructed");
    }

    public override void UpdateState()
    {
        if (decoroot != null)
        {
            tpnode1.transform.position = qmm.bpt1;
            tpnode2.transform.position = qmm.bpt2;
            tpnode3.transform.position = qmm.bpt3;
            tpnodem.transform.position = qmm.bptm;
        }
    }

}

public class SprinklePrefabs : QmeshDeco
{
    public bool sprinkle;
    public string prefabName = "prefabs";
    public string [] sel = { "Dozer1" };

    public override void InitDecoType()
    {
        decotype = DecoType.SprinklePrefabs;
        deconame = "sprinkles";
    }

    string GetRandomPrefab()
    {
        int i = qut.GetRanInt(0, sel.Length, prefabName);
        var rv = "obj/" + sel[i];
        return rv;
    }

    public override void Construct()
    {
        ConstructBase();
    }

    int nobjs = 0;
    public void Sprinkle(int ndodo=2)
    {
        var ska = 1.0f;
        var lambmin = 0.1f;
        var lambmax = 0.9f;
        for (int i = 0; i < ndodo; i++)
        {
            var prefabname = GetRandomPrefab();
            var lambx = qut.GetRanFloat(lambmin, lambmax,prefabname);
            var lambz = qut.GetRanFloat(lambmin, lambmax,prefabname);
            (var vv, _, var istat) = qmm.GetWcMeshPosFromLambda(lambz, lambx);
            var vvs = vv.ToString("f1");
            Debug.Log($"Prefab {i} ({prefabname}) at p:{vvs}");
            var pos = vv;
            var rot = qut.GetRanFloat(0, 360,prefabname);
            var prefab = Resources.Load<GameObject>(prefabname);
            if (prefab==null)
            {
                Debug.Log($"Sprinkle could not load prefab {prefabname} i:{i}");
                continue;
            }
            Transform t = UnityEngine.Object.Instantiate<Transform>(prefab.transform);
            t.transform.parent = preferedParent.transform;
            ska = 0.016f;
            if (prefabname.Contains("TS!"))
            {
                ska = 1;
            }
            t.transform.localScale = new Vector3(ska, ska, ska);
            t.transform.localRotation = Quaternion.Euler(0, rot, 0);
            t.transform.position = pos;
            nobjs++;
        }
    }

    public override void UpdateState()
    {
        if (showDeco && sprinkle)
        {
            Sprinkle();
        }
        sprinkle = false;
    }
}

public class SprinkleTrucksDeco : SprinklePrefabs
{
    public override void InitDecoType()
    {
        decotype = DecoType.SprinkleTrucks;
        deconame = "trucks";
        prefabName = "trucks";
        sel = new string [] { "Dozer1", "Dozer2", "Dozer3", "Minehaul1", "DumpTruck_TS1" };
    }
}

public class SprinkleTowersDeco : SprinklePrefabs
{
    public override void InitDecoType()
    {
        decotype = DecoType.SprinkleTowers;
        deconame = "towers";
        prefabName = "towers";
        sel = new string[] { "Tower00", "Tower01", "Tower02", "Tower03", "Tower04" };
    }
}

public class TrilinesDeco : QmeshDeco
{
    public int nHorzLines;
    public int nVertLines;
    public int nDiagLines;
    public int nTotalTriLines;
    public int nTriangles;
    public int nTotalElements;
    public float generationTime;
    public bool addRanPoints;
    public bool addRanLines;
    public bool earlyTerminate;

    public override void InitDecoType()
    {
        decotype = DecoType.TriLines;
        deconame = "trilines";
    }
    GameObject ranpoints = null;
    public int nranpts = 0;
    public void AddRandomPoints()
    {
        string[] clrs = { "black", "lilac", "yellow", "orange", "red" };
        if (ranpoints == null)
        {
            ranpoints = new GameObject("ranpoints");
            ranpoints.transform.position = Vector3.zero;
            ranpoints.transform.parent = preferedParent.transform;
        }
        var ska = qmm.triDiag * 0.025f;
        //var lmn = 0.1f;
        //var lmx = 0.9f;
        var lmn = -0.1f;
        var lmx = 1.1f;
        for (int i = 0; i < 10000; i++)
        {
            var lambLat = qut.GetRanFloat(lmn, lmx, "ranpoints");
            var lambLng = qut.GetRanFloat(lmn, lmx, "ranpoints");
            (var vv, var nrm, var istat) = qmm.GetWcMeshPosFromLambda(lambLng, lambLat);
            var pos = vv;
            var name = "ranpt:" + nranpts;
            //SphInfo.DoInfoSphere(ranpoints, name, pos, ska, clrs[istat], addSphInfo: false);
            var sphgo = GpuInst.CreateSphereGpu(name, pos, ska, clrs[istat]);
            sphgo.transform.parent = ranpoints.transform;
            nranpts++;
        }
    }

    GameObject ranlines = null;
    public int nranplines = 0;
    public void AddRandomLines(int nlines = 1)
    {
        string[] clrs = { "black", "lilac", "yellow", "orange", "red" };
        if (ranlines == null)
        {
            ranlines = new GameObject("ranlines");
            ranlines.transform.position = Vector3.zero;
            ranlines.transform.parent = preferedParent.transform;
        }
        var ska = qmm.triDiag * 0.05f;
        //var lmn = 0.1f;
        //var lmx = 0.9f;
        var lmn = -0.1f;
        var lmx = 1.1f;
        for (int i = 0; i < nlines; i++)
        {
            var lambLat1 = qut.GetRanFloat(lmn, lmx, "ranlines");
            var lambLng1 = qut.GetRanFloat(lmn, lmx, "ranlines");
            (var vv1, _, _) = qmm.GetWcMeshPosFromLambda(lambLng1, lambLat1);
            var lambLat2 = qut.GetRanFloat(lmn, lmx, "ranlines");
            var lambLng2 = qut.GetRanFloat(lmn, lmx, "ranlines");
            (var vv2, _, _) = qmm.GetWcMeshPosFromLambda(lambLng2, lambLat2);
            var lname = "ranline:" + nranplines;
            qmm.qtt.AddFragLine(lname, vv1, vv2, ska, nclr:"black");
            nranplines++;
        }
    }

    public IEnumerator YieldingConstruct()
    {
        var sw = new StopWatch();
        ConstructBase();
        var qkm = qmm.qkm;
        var qtt = qmm.qtt;
        //var ska = qmm.nodefak * qkm.llbox.diagonalInMeters / 450;

        //var pp1 = qmm.GetMeshNodePos(0, 0);
        //var pp2 = qmm.GetMeshNodePos(1, 1);
        //var triDiag = Vector3.Distance(pp1, pp2);
        var ska = qmm.triDiag * 0.05f;
        Debug.Log($"Construct Trilinesdeco - ska:{ska}  triDiag:{qmm.triDiag}");


        var nHorzSecs = qmm.nHorzSecs;
        var nVertSecs = qmm.nVertSecs;
        var nDiagSecs = nHorzSecs + nVertSecs - 1;
        Vector3 p1, p2;
        string pname;
        int nlines = 0;
        int nhlines = 0;
        int nvlines = 0;
        int ndlines = 0;
        int nsegs = 0;
        for (int i = 0; i <= nHorzSecs; i++)
        {
            nlines++;
            p1 = qmm.GetMeshNodePos(i, 0);
            for (int j = 1; j <= nVertSecs; j++)
            {
                p2 = qmm.GetMeshNodePos(i, j);
                pname = $"horzseg_{i}_{j}";
                var cgo = GpuInst.CreateCylinderGpu(pname, p1, p2, ska, "cyan");
                cgo.transform.parent = decoroot.transform;
                p1 = p2;
                nsegs++;
            }
            nhlines++;
            if (sw.ElapfOverYieldTime())
            {
                Debug.Log($"Yielding at horzlines step:{i}");
                yield return null;
                if (earlyTerminate) break;
            }
        }
        if (!earlyTerminate)
        {
            for (int i = 0; i <= nVertSecs; i++)
            {
                nlines++;
                p1 = qmm.GetMeshNodePos(0, i);
                for (int j = 1; j <= nHorzSecs; j++)
                {
                    p2 = qmm.GetMeshNodePos(j, i);
                    pname = $"vertseg_{j}_{i}";
                    var cgo = GpuInst.CreateCylinderGpu(pname, p1, p2, ska, "yellow");
                    cgo.transform.parent = decoroot.transform;
                    p1 = p2;
                    nsegs++;
                }
                nvlines++;
                if (sw.ElapfOverYieldTime())
                {
                    Debug.Log($"Yielding at vertlines step:{i}");
                    yield return null;
                    if (earlyTerminate) break;
                }
            }
        }

        if (!earlyTerminate)
        {
            for (int i = 0; i < nHorzSecs; i++)
            {
                nlines++;
                for (int j = 0; j < nVertSecs; j++)
                {
                    p1 = qmm.GetMeshNodePos(i, j);
                    p2 = qmm.GetMeshNodePos(i + 1, j + 1);
                    pname = $"crosseg_{i}_{j}";
                    var cgo = GpuInst.CreateCylinderGpu(pname, p1, p2, ska, "purple");
                    cgo.transform.parent = decoroot.transform;
                    nsegs++;
                }
                ndlines++;
                if (sw.ElapfOverYieldTime())
                {
                    Debug.Log($"Yielding at diaglines step:{i}");
                    yield return null;
                    if (earlyTerminate) break;
                }
            }

        }

        //for (int i = 1; i <= nDiagSecs; i++)
        //{
        //    var clr = "purple";
        //    if (i <= nHorzSecs)
        //    {
        //        p1 = qmm.GetMeshNodePos(nHorzSecs - i, 0);
        //    }
        //    else
        //    {
        //        clr = "orange";
        //        p1 = qmm.GetMeshNodePos(0, i - nHorzSecs);
        //    }
        //    if (i <= nVertSecs)
        //    {
        //        p2 = qmm.GetMeshNodePos(nHorzSecs, i);
        //    }
        //    else
        //    {
        //        p2 = qmm.GetMeshNodePos(nHorzSecs - i + nVertSecs, nVertSecs);
        //    }
        //    pname = "crosseg_" + i;
        //    qtt.AddFragLine(pname, p1, p2, ska, lclr:clr);
        //}

        this.nDiagLines = ndlines;
        this.nHorzLines = nhlines;
        this.nVertLines = nvlines;
        this.nTotalTriLines = nHorzLines + nVertLines + nDiagLines;
        this.nTriangles = (qtt.nHorzLines - 1) * (qtt.nVertLines - 1) * 2;
        this.nTotalElements = (nDiagLines - 1) * nVertLines + (nVertLines - 1) * nHorzLines + nTriangles;
        sw.Stop();
        Debug.Log($"Generating {nTotalTriLines} trilines with {nTotalElements} elements took {sw.ElapSecs(3)} secs");
        earlyTerminate = false;
        //yield return null;
    }

    public override void Construct()
    {
        StartCoroutine(YieldingConstruct());
    }

    //public override void Construct()
    //{
    //    var sw = new StopWatch();
    //    ConstructBase();
    //    var qkm = qmm.qkm;
    //    var qtt = qmm.qtt;
    //    var ska = qmm.nodefak * qkm.llbox.diagonalInMeters / 450;
    //    for (int i = 0; i < qtt.trilinesegs.Count; i++)
    //    {
    //        var (t, pname, clr) = qtt.trilinesegs[i];
    //        var p1 = t.From;
    //        var p2 = t.Toto;
    //        var lgo = qtt.AddFragLine(pname, p2, p1, ska, lclr: clr, omit: i);
    //        lgo.transform.parent = decoroot.transform;
    //    }
    //    this.nDiagLines = qtt.nDiagLines;
    //    this.nHorzLines = qtt.nHorzLines;
    //    this.nVertLines = qtt.nVertLines;
    //    this.nTotalTriLines = nHorzLines + nVertLines + nDiagLines;
    //    this.nTriangles = (nDiagLines - 1) * (nHorzLines - 1) * 2;
    //    this.nTotalElements = (nDiagLines - 1) * nVertLines + (nVertLines - 1) * nHorzLines + nTriangles;
    //    sw.Stop();
    //    this.generationTime = (float)sw.Elap().TotalSeconds;
    //    Debug.Log($"Generating {nTotalTriLines} trilines with {nTotalElements} elements took {sw.ElapSecs(3)} secs");
    //}

    public override void UpdateState()
    {
        if (addRanPoints)
        {
            AddRandomPoints();
            addRanPoints = false;
        }
        if (addRanLines)
        {
            AddRandomLines();
            addRanLines = false;
        }
    }
}

public class MeshNodesDeco : QmeshDeco
{
    public bool earlyTerminate;
    public override void InitDecoType()
    {
        decotype = DecoType.MeshNodes;
        deconame = "meshnodes";
    }

    IEnumerator YieldingConstruct()
    {
        var sw = new StopWatch();
        sw.Start();
        ConstructBase();
        var nHorzSecs = qmm.nHorzSecs;
        var nVertSecs = qmm.nVertSecs;
        var ska = qmm.triDiag * 0.1f;


        int imn = 0;
        for (int i = 0; i <= nHorzSecs; i++)
        {
            for (int j = 0; j <= nVertSecs; j++)
            {
                var pos = qmm.GetMeshNodePos(i, j);
                var sname = "sph:" + i + "-" + j;
                //Debug.Log("making " + sname);
                var ll = qmm.GetMeshNodeLatLng(i, j);
                SphInfo.DoInfoCubeSlim(decoroot, sname, pos, ska, "green", ll);
                imn++;
            }
            if (sw.ElapfOverYieldTime())
            {
                Debug.Log($"Yielding on node {imn}");
                yield return null;
                if (earlyTerminate) break;
            }
        }
        sw.Stop();
        Debug.Log($"Generated {imn} meshnodes in {sw.ElapSecs(3)} secs");
        earlyTerminate = false;
        yield return null;
    }

    public override void Construct()
    {
        StartCoroutine(YieldingConstruct());
    }
}

public class CoordDefiningNodesDeco : QmeshDeco
{

    public override void InitDecoType()
    {
        decotype = DecoType.CoordDefNodes;
        deconame = "coordefnodes";
    }

    public override void Construct()
    {
        ConstructBase();

        var ska = qmm.triDiag * 0.5f;
        int i = 0;
        foreach (var md in qmm.llmap.mapcoord.mapdata)
        {
            var sname = "mc:" + i;
            //var v = llmap.mapcoord.glbmap(md.x, 0, md.z);
            var ll = new LatLng(md.lat, md.lng);
            var posll = qmm.GetPosFromLatLng(ll);
            var pos = new Vector3((float)md.x, posll.y, (float)md.z);
            var spi = SphInfo.DoInfoSphere(decoroot, sname, pos, ska, "orange", ll);
            spi.mapPoint = md;
            i++;
        }
    }
}

public class ExtendDefiningNodesDeco : QmeshDeco
{

    public override void InitDecoType()
    {
        decotype = DecoType.ExtentDefNodes;
        deconame = "extentdefnodes";
    }

    public override void Construct()
    {
        ConstructBase();

        var ska = qmm.triDiag * 0.5f;
        var qkm = qmm.qkm;

        //var pos1 = GetPosFromLatLng(qkm.ll1, yval0);
        var pos1 = qmm.GetPosFromLatLng(qkm.ll1);
        var name1 = "ll1";
        SphInfo.DoInfoSphere(decoroot, name1, pos1, ska, "red", qkm.ll1);

        //var pos2 = GetPosFromLatLng(qkm.ll2, yval1);
        var pos2 = qmm.GetPosFromLatLng(qkm.ll2);
        var name2 = "ll2";
        SphInfo.DoInfoSphere(decoroot, name2, pos2, ska, "red", qkm.ll2);

        //var pos3 = GetPosFromLatLng(qkm.tileul.llul, yval0);
        var pos3 = qmm.GetPosFromLatLng(qkm.tileul.llul);
        var name3 = "qkt_ll_ul";
        SphInfo.DoInfoSphere(decoroot, name3, pos3, ska, "blue", qkm.tileul.llul);
        //var pos4 = GetPosFromLatLng(qkm.tilebr.llbr, yval1);
        var pos4 = qmm.GetPosFromLatLng(qkm.tilebr.llbr);
        var name4 = "qkt_ll_br";
        SphInfo.DoInfoSphere(decoroot, name4, pos4, ska, "blue", qkm.tilebr.llbr);
    }
}

public class NativeMapNodesDeco : QmeshDeco
{

    public override void InitDecoType()
    {
        decotype = DecoType.NativeNodes;
        deconame = "nativemapnodes";
    }

    public override void Destruct()
    {
        qmm.llmap.mapcoord.DestroyNativeCoordMarkers();
    }

    public override void Construct()
    {
        ConstructBase();
        var ska = qmm.qkm.llbox.diagonalInMeters / 250;
        qmm.llmap.mapcoord.MakeNativeCoordMarkers(ska: ska, clr: "purple");
    }
}

public class FrameQuadkeysDeco : QmeshDeco
{
    public bool earlyTerminate = false;
    public bool oldway = false;
    public bool showQuadkeyLabels = false;
    public override void InitDecoType()
    {
        decotype = DecoType.FrameQuadKeys;
        deconame = "framequadkeys";
    }

   IEnumerator YieldingConstruct()
    {
        var sw = new StopWatch();
        sw.Start();
        ConstructBase();

        var qkm = qmm.qkm;

//        var ska = qmm.nodefak * qkm.llbox.diagonalInMeters / 450;
        var ska = qmm.triDiag * 0.075f;

        var qtt = qmm.qtt;
        int i = 0;
        GameObject pipeu,piper,pipeb,pipel;
        foreach (var qktile in qkm.qktiles)
        {
            var llul = qktile.llul;
            var llbr = qktile.llbr;
            var llur = new LatLng(llul.lat, llbr.lng);
            var llbl = new LatLng(llbr.lat, llul.lng);
            var pul = qmm.GetPosFromLatLng(llul);
            var pur = qmm.GetPosFromLatLng(llur);
            var pbl = qmm.GetPosFromLatLng(llbl);
            var pbr = qmm.GetPosFromLatLng(llbr);
            var qkname = qktile.name;
            if (oldway)
            {
                SphInfo.DoInfoSphere(decoroot, "pur" + i, pur, ska, "navyblue", llur);
                pipeu = GpuInst.CreateCylinderGpu(qkname + "-u", pul, pur, ska, "cyan");
                pipeu.transform.parent = decoroot.transform;
                piper = GpuInst.CreateCylinderGpu(qkname + "-r", pur, pbr, ska, "cyan");
                piper.transform.parent = decoroot.transform;
                pipeb = GpuInst.CreateCylinderGpu(qkname + "-b", pbr, pbl, ska, "cyan");
                pipeb.transform.parent = decoroot.transform;
                pipel = GpuInst.CreateCylinderGpu(qkname + "-l", pbl, pul, ska, "cyan");
                pipel.transform.parent = decoroot.transform;
            }
            else
            {
                SphInfo.DoInfoSphereSlim(decoroot, "pur" + i, pur, ska, "navyblue", llur);
                pipeu = qtt.AddFragLine(qkname + "-u", pul, pur, ska, lclr:"blue");
                pipeu.transform.parent = decoroot.transform;
                piper = qtt.AddFragLine(qkname + "-r", pur, pbr, ska, lclr: "blue");
                piper.transform.parent = decoroot.transform;
                pipeb = qtt.AddFragLine(qkname + "-b", pbr, pbl, ska, lclr: "blue");
                pipeb.transform.parent = decoroot.transform;
                pipel = qtt.AddFragLine(qkname + "-l", pbl, pul, ska, lclr: "blue");
                pipel.transform.parent = decoroot.transform;
            }
            if (showQuadkeyLabels)
            {
                qut.MakeTextGo(pipeu, qkname, yoff: 100, backoff: 0.01f, sfak: 20);
            }
            if (sw.ElapfOverYieldTime())
            {
                Debug.Log($"Yielding on qktile frame {i} of {qkm.qktiles.Count}");
                yield return null;
                if (earlyTerminate) break;
            }
            i++;
        }
        sw.Stop();
        Debug.Log($"Framed {i} tiles in {sw.ElapSecs(3)} secs");
        earlyTerminate = false;
        yield return null;
    }

    public override void Construct()
    {
        StartCoroutine(YieldingConstruct());
    }
}

public class ShowSubMeshNodes : QmeshDeco
{
    public bool showJustEdges = false;

    public override void InitDecoType()
    {
        decotype = DecoType.SubMeshShowNodes;
        deconame = "submeshnodes";
    }
    public string GetColor(int ix,int iz)
    {
        var clr = "black";
        if (ix == 0) clr = "blue";
        if (ix == mfw.nHorzSecs) clr = "cyan";
        if (iz == 0) clr = "red";
        if (iz == mfw.nVertSecs) clr = "yellow";
        return clr;
    }
    public override void Construct()
    {
        ConstructBase();

        var qkm = qmm.qkm;

        var ska = 4*mfw.skamult * qmm.qkm.llbox.diagonalInMeters / 650;
        var cylsphthickratio = 10f;
        var normlength = 2 * ska;
        int imn = 0;
        bool drawnorm = true;
        var nVertSecs = mfw.nVertSecs;
        var nHorzSecs = mfw.nHorzSecs;
        var irowstart = mfw.irowstart;
        for (int iz = 0; iz <= nVertSecs; iz++)
        {
            var isOnEdgeZmn = (iz == 0);
            var isOnEdgeZmx = (iz == nVertSecs);
            var iz1 = irowstart + iz;
            for (int ix = 0; ix <= nHorzSecs; ix++)
            {
                var isOnEdgeXmn = (ix == 0);
                var isOnEdgeXmx = (ix == nHorzSecs);
                var isOnEdge = isOnEdgeZmn || isOnEdgeZmx || isOnEdgeXmn || isOnEdgeXmx;
                if (showJustEdges && !isOnEdge)
                {
                    continue;
                }
                var pos = qmm.GetMeshNodePos(ix, iz1);
                var sname = "node:" + ix + "-" + iz1 + "/" + iz;
                //Debug.Log("making " + sname);
                var ll = qmm.GetMeshNodeLatLng(ix, iz1);

                var clr = GetColor(ix, iz);
                var spi = SphInfo.DoInfoSphereSlim(decoroot, sname, pos, ska, clr, ll);

                var spni = new SphNodeInfo();
                //meshnodespis[ix, iz] = spni;
                spni.globalMeshCoord = new MeshCoord(ix, iz1);
                spni.localMeshCoord = new MeshCoord(ix, iz);
                var spos = mfw.GetPosLocal(ix, iz);
                var snrm = mfw.GetNormalLocal(ix, iz);
                spni.vertCoord = spos;
                spni.normal1 = snrm;
                if (drawnorm)
                {
                    var nname = "vorm:" + ix + "-" + iz1 + "/" + iz;
                    var epos = spos + snrm * normlength;
                    //var nrmgo = qut.CreatePipe(nname, spos, epos, ska / cylsphthickratio, "yellow");
                    var nrmgo = GpuInst.CreateCylinderGpu(nname, spos, epos,  size: ska / cylsphthickratio, "yellow");
                    nrmgo.transform.parent = decoroot.transform;
                }

                var uv = qmm.GetUv(ix, iz1);
                spni.textureCoord = new TextureCoord(uv.x, uv.y);
                spi.nodeInfo = spni;
                imn++;
            }
        }
        Debug.Log("Generated " + imn + " meshwrapnodes");
    }
}