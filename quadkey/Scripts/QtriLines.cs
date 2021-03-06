﻿using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Qtrilines : MonoBehaviour
{
    QmapMesh qmm;

    public int nHorzLines;
    public int nVertLines;
    public int nDiagLines;

    public void Init(QmapMesh qmm)
    {
        this.qmm = qmm;       
    }

    public List<(LineSegment2xz, string, string)> trilinesegs = null;

    public (int, int) AddPoint(string lname, List<(Vector3, float)> ptlist, Vector3 pt, float dist)
    {
        var ipos = 0;
        var eps = 1e-6;
        bool minmax = true;
        int icomps = 0;
        var imin = 0;
        var imax = ptlist.Count - 1;
        if (ptlist.Count > 0)
        {
            var pt1 = ptlist[imin];
            if (dist < pt1.Item2)
            {
                //Debug.Log($"Inserting at begining pt1.Item2:{pt1.Item2}");
                ipos = 0;
            }
            else if (ptlist.Count == 1)
            {
                ipos = 1;
            }
            else
            {
                if (minmax)
                {
                    if (Math.Abs(pt1.Item2 - dist) < eps)
                    {
                        ipos = -1;
                    }
                    var pt2 = ptlist[imax];
                    if (Math.Abs(pt2.Item2 - dist) < eps)
                    {
                        ipos = -1;
                    }
                    if (0 <= ipos)
                    {
                        var niter = 0;
                        while (imin < imax)
                        {
                            //Debug.Log($"{niter} imax:{imax} dmin:{ptlist[imin].Item2}   imin:{imin} dmax:{ptlist[imax].Item2}");
                            var inew = (imin + imax) / 2;
                            var pnx = ptlist[inew];
                            if (Math.Abs(pnx.Item2 - dist) < eps)
                            {
                                ipos = -1;
                                break;
                            }
                            if (dist < pnx.Item2)
                            {
                                imax = inew;
                            }
                            else if (pnx.Item2 < dist)
                            {
                                imin = inew;
                            }
                            if ((imax - imin) <= 1)
                            {
                                ipos = imin + 1;
                                break;
                            }
                            icomps++;
                            niter++;
                            if (niter > ptlist.Count + 1)
                            {
                                break; // emergency out if we iterate too much
                            }
                        }
                    }
                }
                else
                {
                    for (int i = imin; i < imax; i++)
                    {
                        //Debug.Log($"Iterating i:{i}");
                        if (Math.Abs(pt1.Item2 - dist) < eps)
                        {
                            ipos = -1;
                            break;
                        }
                        var pt2 = ptlist[i + 1];
                        if (Math.Abs(pt2.Item2 - dist) < eps)
                        {
                            ipos = -1;
                            break;
                        }
                        icomps++;
                        if (pt1.Item2 < dist && dist < pt2.Item2)
                        {
                            ipos = i + 1;
                            break;
                        }
                        pt1 = pt2;
                    }
                }
            }
        }
        //Debug.Log($"Adding to {lname} at {ipos} cnt:{ptlist.Count} point {pt.ToString("f2")} t:{dist}");
        if (ipos >= 0)
        {
            ptlist.Insert(ipos, (pt, dist));
        }
        return (ptlist.Count, icomps);
    }

    public List<(Vector3, float)> GetIsectList(string lname, Vector3 pt1, Vector3 pt2, int omit = -1)
    {
        var rv = new List<(Vector3, float)>();
        var (_, icomp1) = AddPoint(lname, rv, pt1, 0);
        var (_, icomp2) = AddPoint(lname, rv, pt2, 1);
        nTotComps += icomp1 + icomp2;
        //rv.Add((pt2, Vector3.Distance(pt1,pt2)));
        var ls1 = new LineSegment2xz(pt1, pt2);
        for (int i = 0; i < trilinesegs.Count; i++)
        {
            var (ls2, pname, clr) = trilinesegs[i];
            if (i != omit)
            {
                var isect = ls1.TryIntersect(ls2, out var pti, out var t, out var u);
                if (isect)
                {
                    var (ptii, _, _) = qmm.GetWcMeshPosProjectedAlongY(pti);
                    var (_, icomp) = AddPoint(lname, rv, ptii, t);
                    nTotComps += icomp;
                }
            }
        }
        return rv;
    }
    public int ntotIsects = 0;
    public int nTotComps = 0;
    public int nFragLines = 0;
    public GameObject AddFragLine(string lname, Vector3 pt1, Vector3 pt2, float lska = 1.0f, float nska = 4.0f, string lclr = "red", string nclr = "", int omit = -1,float widratio=1)
    {
        var ptlist = GetIsectList(lname, pt1, pt2, omit: omit);
        ntotIsects += ptlist.Count;
        nFragLines++;
        //Debug.Log($"AddFragLine found {ptlist.Count} intersections");

        var lgo = new GameObject(lname);
        string sname;
        GameObject sphgo;

        for (int i = 0; i < ptlist.Count - 1; i++)
        {
            var (pit1, d1) = ptlist[i];
            var (pit2, d2) = ptlist[i + 1];
            var llname = $"{lname}_{i}";
            if (nclr!="" && i == 0)
            {
                sname = $"{llname}_pt1_{d1.ToString("f4")}";
                sphgo = qut.CreateMarkerSphere(sname, pit1, lska * nska, nclr);
                sphgo.transform.parent = lgo.transform;
            }
            var ptform = GpuInst.CreateCylinderGpu(llname, pit1, pit2, lska, lclr,widratio:widratio );
            ptform.transform.parent = lgo.transform;
            if (nclr != "")
            {
                sname = $"{llname}_pt2_{d2.ToString("f4")}";
                var sphgotform = GpuInst.CreateSphereGpu(sname, pit2, lska * nska, nclr);
                sphgotform.transform.parent = lgo.transform;
            }
        }
        return lgo;
    }

    enum CrissCrossE { cris, cros }
    public void GenerateTriLines()
    {
        var loc_trilinesegs = new List<(LineSegment2xz, string, string)>();
        Vector3 p1, p2;
        string pname;
        int nHorzSecs = qmm.nHorzSecs;
        int nVertSecs = qmm.nVertSecs;
        nHorzLines = nVertSecs + 1;
        nVertLines = nHorzSecs + 1;
        nDiagLines = nHorzSecs + nVertSecs - 1;
        for (int i = 0; i <= nHorzSecs; i++)
        {
            p1 = qmm.GetMeshNodePos(i, 0);
            p2 = qmm.GetMeshNodePos(i, nVertSecs);
            pname = "horzseg_" + i;
            loc_trilinesegs.Add((new LineSegment2xz(p1, p2), pname, "cyan"));
        }
        for (int j = 0; j <= nVertSecs; j++)
        {
            p1 = qmm.GetMeshNodePos(0, j);
            p2 = qmm.GetMeshNodePos(nHorzSecs, j);
            pname = "vertseg_" + j;
            loc_trilinesegs.Add((new LineSegment2xz(p1, p2), pname, "yellow"));
        }
        int nDiagSecs = nHorzSecs + nVertSecs - 1;

        var cc = CrissCrossE.cros;

        switch (cc)
        {
            case CrissCrossE.cris:
                {
                    for (int i = 1; i <= nDiagSecs; i++)
                    {
                        var clr = "purple";
                        if (i <= nHorzSecs)
                        {
                            p1 = qmm.GetMeshNodePos(i, 0);
                        }
                        else
                        {
                            clr = "orange";
                            p1 = qmm.GetMeshNodePos(nHorzSecs, i - nHorzSecs);
                        }
                        if (i <= nVertSecs)
                        {
                            p2 = qmm.GetMeshNodePos(0, i);
                        }
                        else
                        {
                            p2 = qmm.GetMeshNodePos(i - nVertSecs, nVertSecs);
                        }
                        pname = "crisseg_" + i;
                        loc_trilinesegs.Add((new LineSegment2xz(p1, p2), pname, clr));
                    }
                    break;
                }
            case CrissCrossE.cros:
                {
                    for (int i = 1; i <= nDiagSecs; i++)
                    {
                        var clr = "purple";
                        if (i <= nHorzSecs)
                        {
                            p1 = qmm.GetMeshNodePos(nHorzSecs - i, 0);
                        }
                        else
                        {
                            clr = "orange";
                            p1 = qmm.GetMeshNodePos(0, i - nHorzSecs);
                        }
                        if (i <= nVertSecs)
                        {
                            p2 = qmm.GetMeshNodePos(nHorzSecs, i);
                        }
                        else
                        {
                            p2 = qmm.GetMeshNodePos(nHorzSecs - i + nVertSecs, nVertSecs);
                        }
                        pname = "crosseg_" + i;
                        loc_trilinesegs.Add((new LineSegment2xz(p1, p2), pname, clr));
                    }
                    break;
                }
        }
        trilinesegs = loc_trilinesegs;
    }

    public async void AsyncGenerateTrilines()
    {
        Debug.Log("Constructing TriLines");
        ntotIsects = 0;
        nFragLines = 0;
        System.Action genTriLines = () => GenerateTriLines();
        await Task.Run(genTriLines);
        Debug.Log($"Finsished Constructing TriLines total nFragLines:{nFragLines} iSects:{ntotIsects}");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
