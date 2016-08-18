using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Runtime.Serialization;

public static class IOHandler {

    static public void SerializeCreature(Creature cr)
    {
        using (XmlWriter XWstream = XmlWriter.Create("Creature.dcs", new XmlWriterSettings() { Indent = true }))
        {
            Type[] extraTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                 from assemblyType in domainAssembly.GetTypes()
                                 where typeof(Node).IsAssignableFrom(assemblyType)
                                 select assemblyType).ToArray();
            DataContractSerializer dcs = new DataContractSerializer(typeof(Creature), extraTypes, 1000, false, true, null);
            dcs.WriteObject(XWstream, cr);
            Debug.Log("New XML File Written");
        }
    }

    static public Creature DeserialiseCreature()
    {
        using (XmlReader XRstream = XmlReader.Create("Creature.dcs", new XmlReaderSettings() { }))
        {
            Type[] extraTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                 from assemblyType in domainAssembly.GetTypes()
                                 where typeof(Node).IsAssignableFrom(assemblyType)
                                 select assemblyType).ToArray();
            DataContractSerializer dcs = new DataContractSerializer(typeof(Creature), extraTypes);
            Creature rt = (Creature)dcs.ReadObject(XRstream);
            Debug.Log("Deserialised!");
            return rt;
        }
    }

    static public void ListSaving(string url, List<object> list)
    {
        using (StreamWriter sw = new StreamWriter(url,true))
        {
            foreach (object obj in list)
            {
                sw.WriteLine(obj.ToString());
            }
        }
    }

    static public List<string> ListLoading(string url)
    {
        using (StreamReader sr = new StreamReader(url))
        {
            List<string> ret = new List<string>();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                ret.Add(line);
            }
            return ret;
        }
    }

    static public void DebugWriteToFile(string line)
    {
        StreamWriter writer = new StreamWriter("debugLog.csv",true);
        writer.WriteLine(line);
        writer.Close();
    }
}
