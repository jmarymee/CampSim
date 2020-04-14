using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TextureCoord
{
    public float u;
    public float v;
    public TextureCoord(float u,float v)
    {
        this.u = u;
        this.v = v;
    }
}

[System.Serializable]
public struct MeshCoord
{
    public int i;
    public int j;
    public MeshCoord(int i, int j)
    {
        this.i = i;
        this.j = j;
    }
}

[System.Serializable]
public class SphNodeInfo
{
    public MeshCoord globalMeshCoord;
    public MeshCoord localMeshCoord;
    public Vector3 vertCoord;
    public TextureCoord textureCoord;
    public Vector3 normal1;
    public Vector3 normal2;
}

public class SphInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public LatLng latLng = null;
    public SphNodeInfo nodeInfo = null;
    public MapCoordPoint mapPoint = null;

    static GameObject sphgo = null;

    public static void DoInfoSphereSlimOld(GameObject parent, string sname, Vector3 pos, float ska, string color, LatLng ll = null)
    {
        if (sphgo == null)
        {
            sphgo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //qut.SetColorOfGo(sphgo, color);
        }
        var spht = Instantiate(sphgo.transform);
        spht.parent = parent.transform;
        spht.position = pos;
        spht.localScale = new Vector3(ska, ska, ska);
    }
    public static SphInfo DoInfoSphereSlim(GameObject parent, string sname, Vector3 pos, float ska, string color, LatLng ll = null, bool addSphInfo = true)
    {
        var sphtran = GpuInst.CreateSphereGpu(sname, pos, ska, color);
        sphtran.transform.parent = parent.transform;
        SphInfo spi = null;
        if (addSphInfo)
        {
            spi = sphtran.gameObject.AddComponent<SphInfo>();
            spi.latLng = ll;
            spi.nodeInfo = null;
        }
        return spi;
    }
    public static SphInfo DoInfoCubeSlim(GameObject parent, string sname, Vector3 pos, float ska, string color, LatLng ll = null, bool addSphInfo = true)
    {
        var sphtran = GpuInst.CreateCubeGpu(sname, pos, ska, color);
        sphtran.transform.parent = parent.transform;
        SphInfo spi = null;
        if (addSphInfo)
        {
            spi = sphtran.gameObject.AddComponent<SphInfo>();
            spi.latLng = ll;
            spi.nodeInfo = null;
        }
        return spi;
    }
    public static SphInfo DoInfoSphere(GameObject parent, string sname, Vector3 pos, float ska, string color, LatLng ll = null,bool addSphInfo=true)
    {
        var sphgo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //var sph = Instantiate(sphgo);
        sphgo.name = sname;
        sphgo.transform.parent = parent.transform;
        sphgo.transform.position = pos;
        sphgo.transform.localScale = new Vector3(ska, ska, ska);
        qut.SetColorOfGo(sphgo, color);
        SphInfo spi = null;
        if (addSphInfo)
        {
            spi = sphgo.AddComponent<SphInfo>();
            spi.latLng = ll;
            spi.nodeInfo = null;
        }
        return spi;
    }

    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
