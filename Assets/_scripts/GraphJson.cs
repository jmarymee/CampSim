﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace GraphAlgos
{
    [Serializable]
    public class JsonLcNode
    {
        public string name;
        public Vector3 pt;
        public string comment;
        public int regionIdx;
    }

    [Serializable]
    public class JsonLcLink
    {
        public string name;
        public string n1;
        public string n2;
        public string usetype;
    }

    [Serializable]
    public class JsonLcNodeList
    {
        public List<JsonLcNode> list;

        public JsonLcNodeList()
        {
            this.list = new List<JsonLcNode>();
        }

        public void Add(JsonLcNode p)
        {
            list.Add(p);
        }

    }

    [Serializable]
    public class JsonLcLinkList
    {
        public List<JsonLcLink> list;

        public JsonLcLinkList()
        {
            this.list = new List<JsonLcLink>();
        }

        public void Add(JsonLcLink lnk)
        {
            list.Add(lnk);
        }

    }

    [Serializable]
    public class JsonLinkCloud
    {
        // the following members will be convered to json 
        // but only if they are serilizable!!!
        public NodeRegion region;
        public graphtex floorplan;
        public JsonLcNodeList nodes;
        public JsonLcLinkList links;

        public JsonLinkCloud(NodeRegion region,graphtex gt)
        {
            this.region = region;
            floorplan = new graphtex(gt);
            nodes = new JsonLcNodeList();
            links = new JsonLcLinkList();
        }

        public void AddNode(string name, Vector3 pt)
        {
            var node = new JsonLcNode();
            node.name = name;
            node.pt = pt;
            nodes.Add(node);
        }
        public void AddLink(string name, string nodename1, string nodename2,string usetype)
        {
            var link = new JsonLcLink();
            link.name = name;
            link.n1 = nodename1;
            link.n2 = nodename2;
            link.usetype = usetype;
            links.Add(link);
        }

        public static void WriteToFile(JsonLinkCloud jlc, string fname)
        {
            string jstring = JsonUtility.ToJson(jlc);
            using (FileStream fs = new FileStream(fname, FileMode.Create))
            {
                var sw = new StreamWriter(fs);
                sw.Write(jstring);
                sw.Flush();
                fs.Close();
            }
        }

        public void WriteToConsole()
        {
            Console.WriteLine(JsonUtility.ToJson(this));
        }
        public static JsonLinkCloud ReadFromFile(string fname)
        {
            var jstring = "";
            using (FileStream fs = new FileStream(fname, FileMode.Open))
            {
                var sr = new StreamReader(fs);
                jstring = sr.ReadToEnd();
                fs.Close();
            }
            var jlc = JsonUtility.FromJson<JsonLinkCloud>(jstring);
            return jlc;
        }
    }
}