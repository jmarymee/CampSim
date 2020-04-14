using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour {

    public static List<GameObject> pipelist = null;
    public Slider slider;
    public GameObject CreatePipe(Vector3 frpt,Vector3 topt)
    {
        var dst = Vector3.Distance(frpt, topt);
        var dst2 = dst / 2;
        var dlt = topt - frpt;

        var pcyl = new GameObject("pcyl");
        pcyl.transform.localPosition = frpt;


        var fsph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fsph.transform.parent = pcyl.transform;
        fsph.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        fsph.transform.localPosition = new Vector3(0, 0, 0);
        // fsph.transform.position = frpt;
        fsph.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);

        var tsph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tsph.transform.parent = pcyl.transform;
        tsph.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        tsph.transform.localPosition = new Vector3(dst,0,0);
        tsph.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);

        var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        cyl.transform.parent = pcyl.transform;
        cyl.transform.localScale = new Vector3(0.01f, dst2, 0.01f);
        cyl.transform.localPosition = new Vector3(dst2, 0, 0);
        cyl.transform.Rotate(0, 0, -90);

        var angy = -180 * Mathf.Atan2(dlt.z, dlt.x) / Mathf.PI;
        var angz = 180 * Mathf.Atan2(dlt.y, dlt.x) / Mathf.PI;
        pcyl.transform.Rotate(0, angy, angz);



        // var q = Quaternion.LookRotation(dlt, Vector3.up);
        //      cyl.transform.rotation = q; 
        return (cyl);
    }
    public void CreatePipe()
    {
        if (pipelist == null) {
            var pt1 = new Vector3(1, 0, 0);
            var pt2 = new Vector3(5, 2, 1);
            var pt3 = new Vector3(10, 0, 0);
            pipelist = new List<GameObject>();
            pipelist.Add(CreatePipe(pt1, pt2));
            pipelist.Add(CreatePipe(pt2, pt3));
        }
    }
    public void DeletePipe()
    {
        if (pipelist != null)
        {
            foreach (var p in pipelist)
            {
                Destroy(p);
            }
            pipelist = null;
        }
    }
    public void DoSomething()
    {
        Debug.Log(slider.value.ToString());
    }
}
