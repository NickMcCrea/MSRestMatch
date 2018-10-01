using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ConfigReader
{

    private static Dictionary<string, string> keyValuePairs;

    static ConfigReader()
    {
        keyValuePairs = new Dictionary<string, string>();

        if (!Application.isEditor && File.Exists("tanks.ini"))
        {

            var lines = File.ReadAllLines("tanks.ini");

            foreach (string line in lines)
            {
                var pair = line.Split(':');
                keyValuePairs.Add(pair[0], pair[1]);
            }
        }
        else
        {
            //some defaults
            keyValuePairs.Add("ipaddress", "127.0.0.1");
            keyValuePairs.Add("port", "8052");
        }


    }


    public static string GetValue(string property)
    {
        if (keyValuePairs.ContainsKey(property))
            return keyValuePairs[property];
        return "";
    }
}
