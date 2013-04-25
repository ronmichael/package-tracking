using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Xml;


public static class Extensions
{
    public static string GetValue(this XmlDocument doc, string nodeName)
    {
        XmlNode n = doc.SelectSingleNode(nodeName);
        if (n == null)
            return "";
        else
            return n.InnerText;
    }
    public static string GetValue(this XmlNode doc, string nodeName)
    {
        XmlNode n = doc.SelectSingleNode(nodeName);
        if (n == null)
            return "";
        else
            return n.InnerText;
    }




}
