using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

public static class XMLController
{
    private struct XML
    {
        public string path;
        public XDocument document;

        public XML(string p, XDocument d)
        {
            path = p;
            document = d;
        }
    }
    
    private static Dictionary<string, XML> Documents = new Dictionary<string, XML>();

    /// <summary>
    /// Constructs the xml path from the directory and the file name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns>File path</returns>
    private static string ConstructPath(string name, string dir)
    {
        return dir + name + ".xml";
    }

    /// <summary>
    /// Loads in the desired xml file.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dir"></param>
    /// <returns>Loaded XDocument</returns>
    public static XDocument LoadXML(string name, string dir)
    {
        if(Documents.ContainsKey(name))
        {
            Debug.Log("XML: " + name + " was already loaded.");
            return Documents[name].document;
        }

        try
        {
            string path = ConstructPath(name, dir);
            XDocument doc = XDocument.Load(path);
            Documents.Add(name, new XML(path, doc));

            Debug.Log("XML: " + name + " has been loaded.");
            return doc;
        }
        catch
        {
            Debug.Log("XML: " + name + " failed to load.");
            return null;
        }
    }

    /// <summary>
    /// Saves the desired xml file.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Saved XDocument</returns>
    public static XDocument SaveXML(string name)
    {
        if (!Documents.ContainsKey(name))
        {
            Debug.Log("XML: " + name + " does not exist.");
            return null;
        }

        try
        { 
            XML xml = Documents[name];
            xml.document.Save(xml.path);

            Debug.Log("XML: " + name + " has been saved.");
            return xml.document;
        }
        catch
        {
            Debug.Log("XML: " + name + " failed to save.");
            return null;
        }
    }
}
