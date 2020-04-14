using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCollision
{
    Vehicle v1;
    Vehicle v2;
    Vector3 pos;
    float t;
    float distance;
    public Vector3 GetPosition()
    {
        return pos;
    }
    public void InactivateVehicles()
    {
        v1.avaGo.SetActive(false);
        v2.avaGo.SetActive(false);
    }

    public void RestartVehicles()
    {
        v1.ResetStartTime();
        v2.ResetStartTime();
    }

    public VehicleCollision(Vehicle v1, Vehicle v2, Vector3 pos)
    {
        this.v1 = v1;
        this.v2 = v2;
        t = Time.time;
        distance = Vehicle.Dist(v1, v2);
        this.pos = pos;
    }
    public float Distance()
    {
        return distance;
    }
}

public class Vehicle : MonoBehaviour
{
    VehicleTrackMan vtm;
    VehicleTrack vehicleTrack;
    static int totcnt = 0;
    public float starttime;
    public float avaSecpersec;
    public float curTrackTime;
    //bool usesphere = false;
    public GameObject avaGo;
    SimpleDf sdf;

    public void Init(VehicleTrack vehicleTrack, SimpleDf sdf,GameObject vehicleParent, string avatartype, float startime, float avaSecpersecInput)
    {
        this.vehicleTrack = vehicleTrack;
        this.vtm = vehicleTrack.vtm;
        this.sdf = sdf;
        this.avaSecpersec = avaSecpersecInput;
        var cnt = Count(avatartype);
        totcnt++;

        avaGo = new GameObject(avatartype + "_" + cnt);
        avaGo.transform.parent = vehicleParent.transform;
        GameObject prefabgo = null;
        var shift = Vector3.zero;
        var angle = 0;
        var scale = 1;
        switch (avatartype)
        {
            case "DumpTruck":
                scale = 1;
                angle = 180;
                prefabgo = Resources.Load<GameObject>("obj/DumpTruck_TS1");
                break;
            case "Dozer1":
                scale = 1;
                angle = 0;
                shift = new Vector3(-28.80f, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Dozer1");
                break;
            case "Dozer2":
                scale = 1;
                angle = 0;
                shift = new Vector3(-20, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Dozer2");
                break;
            case "Minehaul1":
                scale = 1;
                angle = 0;
                shift = new Vector3(0, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Minehaul1");
                break;
            case "Shovel1":
                scale = 1;
                angle = 0;
                shift = new Vector3(-10, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Shovel1");
                break;
            case "Rover":
                scale = 1;
                angle = 0;
                shift = new Vector3(0, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Rover2");
                break;
            case "Sailboat1":
                scale = 1;
                angle = 90;
                shift = new Vector3(0, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Sailboat1");
                break;
            case "Sailboat100":
                scale = 100;
                angle = 90;
                this.avaSecpersec = avaSecpersec * 100;
                shift = new Vector3(0, 0, 0);
                prefabgo = Resources.Load<GameObject>("obj/Sailboat1");
                break;
        }
        switch (vtm.vehicleInitialPlacement)
        {
            case VehicleInitialPlacement.zero:
                this.starttime = avaSecpersec * vehicleTrack.vtm.GetSimulationTime();
                break;
            case VehicleInitialPlacement.random:
                this.starttime = vehicleTrack.GetRandomTrackTime();
                break;
        }
        if (prefabgo == null)
        {
            Debug.LogWarning($"Prefab not loaded for {avatartype}");
            return;
        }
        var skav3 = prefabgo.transform.localScale * scale;
        Transform t = UnityEngine.Object.Instantiate<Transform>(prefabgo.transform);
        t.localScale = skav3;
        t.localRotation = Quaternion.Euler(0, angle, 0);
        t.position = shift;
        t.SetParent(avaGo.transform, worldPositionStays: false);
        MoveToTracktime();
    }


    static Dictionary<string, int> count = new Dictionary<string, int>();
    static int Count(string avatartype)
    {
        if (!count.ContainsKey(avatartype))
        {
            count[avatartype] = 0;
        }
        count[avatartype]++;
        return count[avatartype];
    }

    public void ResetStartTime()
    {
        //var maxTimeGap = Time.time - this.starttime;
        //this.starttime = this.starttime + qut.GetRanFloat(maxTimeGap);
        this.starttime = vehicleTrack.GetRandomTrackTime();

        MoveToTracktime();
    }

    public Vector3 GetPosOnTrack(float t)
    {
        curTrackTime = avaSecpersec*t - starttime;
        while (curTrackTime < 0)
        {
            curTrackTime += vehicleTrack.maxTrackTime;
        }
        while (curTrackTime > vehicleTrack.maxTrackTime)
        {
            curTrackTime -= vehicleTrack.maxTrackTime;
        }

        var (imin, imax, lamb) = sdf.InterpolateFloat("Elaptime", curTrackTime);
        //Debug.Log($"curTrackTime:{curTrackTime} imin:{imin} imax:{imax} lamb:{lamb}");
        var lat = sdf.DeInterploateDouble("y", imin, imax, lamb);
        var lng = sdf.DeInterploateDouble("x", imin, imax, lamb);
        var ll = new LatLng(lat, lng);
        (var pos, _, _) =  VehicleTrack.qmesh.GetWcMeshPosFromLatLng(ll);
        //Debug.Log("MoveAvaGo at time " + curtime + " ll:" + ll.ToString() + "  pos:" + pos);
        return pos;
    }
    public static float Dist(Vehicle v1, Vehicle v2)
    {
        var dst = Vector3.Distance(v1.avaGo.transform.position, v2.avaGo.transform.position);
        //Debug.Log("Dist v1-v2:" + dst);
        return dst;
    }
    public static VehicleCollision CheckCollision(Vehicle v1, Vehicle v2)
    {
        VehicleCollision rv = null;
        if (Dist(v1, v2) < 10.0)
        {
            var pos = (v1.avaGo.transform.position + v2.avaGo.transform.position) / 2;
            rv = new VehicleCollision(v1, v2, pos);
        }
        return rv;
    }
    public void MoveToTracktime()
    {
        var t = vehicleTrack.vtm.GetSimulationTime();
        var pos1 = GetPosOnTrack(t);
        var pos2 = GetPosOnTrack(t + 0.1f);
        avaGo.transform.position = pos1;
        avaGo.transform.LookAt(pos2);
        this.transform.position = pos1;
        this.transform.LookAt(pos2);
        //Debug.Log("MoveAvaGo at time " + t + "  pos:" + pos1 + "  lookat:" + pos2);
    }
    public void DestroyGos()
    {
        Destroy(avaGo);
        avaGo = null;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
            MoveToTracktime();
    }
}
