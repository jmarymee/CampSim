using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  // just for mathf and vector3?

namespace GraphAlgos
{
    public class CodeFileSaver
    {
        GraphCtrl grc;

        int nwarn = 0;
        int nrev = 0;
        int naddnode = 0;
        int nlinkto = 0;
        int nlinkbyname = 0;
        int lineidx;
        string indent;
        string apd;
        List<string> lines;
        public CodeFileSaver(GraphCtrl grc)
        {
            this.grc = grc;

        }

        void init()
        {
            nwarn = 0;
            nrev = 0;
            naddnode = 0;
            nlinkto = 0;
            nlinkbyname = 0;
            lines = new List<string>();
            lineidx = 1;
            indent = "    ";
            apd = "";
        }

        public string fpt(Vector3 pt)
        {
            var fmt = "f3";
            var rv = "," + pt.x.ToString(fmt) + "," + pt.y.ToString(fmt) + "," + pt.z.ToString(fmt);
            return rv;
        }
        private string q(string s)
        {
            var rv = '"' + s + '"';
            return rv;
        }
        private string nodashes(string s)
        {

            var rv = s.Replace("-", "_");
            return rv;
        }
        void ApdPrefix(NodeRegion region)
        {
            lines.Add("public void createPointsFor_" + nodashes(region.name) + "()  // machine generated - do not edit");
            lines.Add("{");
            var s = indent + "grc.regman.NewNodeRegion("+q(region.name)+","+q(region.color)+", saveToFile:true);";
            lines.Add(s);
        }
        void ApdPostFix()
        {
            var defname = "default";
            var s = indent + "grc.regman.SetRegion("+q(defname)+");";
            lines.Add(s);
            lines.Add("}");
        }
        void ApdNewNode(string name, Vector3 pt, string comment)
        {
            var s = indent + "grc.AddNodePtxyz(  " + q(name) + fpt(pt) + ", comment:" + q(comment) + apd;
            lines.Add(s);
            naddnode++;
        }
        void ApdNewLinkTo(string frname, string tuname, Vector3 pt, LinkUse usetype, string comment)
        {
            var s = indent + "grc.LinkToPtxyz( " + q(frname) + "," + q(tuname) + fpt(pt) + ", LinkUse." + usetype.ToString() + ", comment:" + q(comment) + apd;
            lines.Add(s);
            nlinkto++;
        }
        void ApdNewLinkByName( LcLink link, LinkUse usetype)
        {
            var frname = link.node1.name;
            if (link.node1spec!="")
            {
                frname = link.node1spec;
            }
            var tuname = link.node2.name;
            if (link.node2spec != "")
            {
                tuname = link.node2spec;
            }
            var s = indent + "grc.AddLinkByNodeName( " + q(frname) + "," + q(tuname) + ", LinkUse." + usetype.ToString() + apd;
            lines.Add(s);
            nlinkbyname++;
        }
        void ApdNewWarning(string wrn)
        {
            var s = indent + "// " + wrn + apd;
            lines.Add(s);
            Debug.LogWarning(wrn);
            nwarn++;
        }

        public void SaveToFile(string fname, NodeRegion region)
        {
            init();
            var regnodes = grc.GetNodesInRegion(region.regid);
            var reglinks = grc.GetLinksInRegion(region.regid);

            ApdPrefix(region);

            while (lineidx < region.maxDefStepIdx)
            {
                var inodes = regnodes.FindAll(n => n.regionStepIdx == lineidx);
                var ilinks = reglinks.FindAll(l => l.regionStepIdx == lineidx);
                var nn = inodes.Count;
                var nl = ilinks.Count;
                apd = "); //  " + lineidx + " nn:" + nn + " nl:" + nl;
                if (nn > 1)
                {
                    var wrn = "// more than one node for step index nn:" + nn;
                    ApdNewWarning(wrn);
                }
                else if (ilinks.Count > 1)
                {
                    var wrn = "more than one link for step index" + nl;
                    ApdNewWarning(wrn);
                }
                else if (nn == 0 && nl == 0)
                {
                    var s = indent + "// ( empty " + apd;
                    lines.Add(s);
                }
                else if (nn == 1 && nl == 0)
                {
                    var n = inodes[0];
                    ApdNewNode(n.name, n.pt, n.comment);
                }
                else if (nn == 1 && nl == 1)
                {
                    var n = inodes[0];
                    var l = ilinks[0];
                    var frname = l.node1.name;
                    var tuname = l.node2.name;
                    var tupt = l.node2.pt;
                    var tucmt = l.node2.comment;
                    var nodesok = false;
                    if (tuname != n.name)
                    {
                        // maybe inverted?
                        frname = l.node2.name;
                        tuname = l.node1.name;
                        tupt = l.node1.pt;
                        tucmt = l.node1.comment;
                        if (tuname != n.name)
                        {
                            ApdNewNode(n.name, n.pt, n.comment);
                            ApdNewLinkByName(l, l.usetype);
                            //var wrn = "Unexpected node name - both l.node1:" + l.node1.name + "  l.node2:" + l.node2.name + " are not equal to nname:" + n.name;
                            //ApdNewWarning(wrn);
                        }
                        else
                        {
                            nrev++;
                            nodesok = true;
                        }
                    }
                    else
                    {
                        nodesok = true;
                    }
                    if (nodesok)
                    {
                        ApdNewLinkTo( frname, tuname, tupt, l.usetype, tucmt);
                    }
                }
                else if (nn == 0 && nl == 1)
                {
                    var l = ilinks[0];
                    ApdNewLinkByName(l, l.usetype);
                }
                else
                {
                    var wrn = " Wierd nn and nl counts nn:" + nn + " nl:" + nl;
                    ApdNewWarning(wrn);
                }
                lineidx++;
            }
            ApdPostFix();
            string[] lar = lines.ToArray<string>();
            System.IO.File.WriteAllLines(fname, lar);
            Debug.Log("Wrote ** " + lines.Count + " lines to " + fname + "  nwrn:" + nwarn + " nrev:" + nrev + " nadn:" + naddnode + " nlto:" + nlinkto + " nlbn:" + nlinkbyname);
        }
    }

}