using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;





public static class ConfigValueStore
{

    private static Dictionary<string, string> keyValuePairs;
    private static Dictionary<string, string> defaultKvps;

    static ConfigValueStore()
    {
        keyValuePairs = new Dictionary<string, string>();
        defaultKvps = new Dictionary<string, string>();


        if (!Application.isEditor && File.Exists("tanks.ini"))
        {
            try
            {
                var lines = File.ReadAllLines("tanks.ini");

                foreach (string line in lines)
                {
                    var pair = line.Split(':');
                    keyValuePairs.Add(pair[0], pair[1]);
                }
            }
            catch(Exception ex)
            {
                LoadDefaults();
            }
        }
        else
        {
            //some defaults
            LoadDefaults();

        }


    }

    private static void LoadDefaults()
    {
        defaultKvps.Add("ipaddress", "127.0.0.1");
        defaultKvps.Add("port", "8052");
        defaultKvps.Add("use_port_in_token", "true");
        defaultKvps.Add("game_time", "300");
        defaultKvps.Add("kill_capture_mode", "true");
        defaultKvps.Add("snitch_enabled", "true");
        defaultKvps.Add("snitch_spawn_threshold", "90");
    }

    public static string GetValue(string property)
    {
        if (keyValuePairs.ContainsKey(property))
            return keyValuePairs[property];
        if (defaultKvps.ContainsKey(property))
            return defaultKvps[property];

        return "";
    }

    public static bool GetBoolValue(string property)
    {
        if (keyValuePairs.ContainsKey(property))
            return bool.Parse(keyValuePairs[property]);
        if (defaultKvps.ContainsKey(property))
            return bool.Parse(defaultKvps[property]);

        return false;

    }

    public static int GetIntValue(string property)
    {
        if (keyValuePairs.ContainsKey(property))
            return int.Parse(keyValuePairs[property]);
        if (defaultKvps.ContainsKey(property))
            return int.Parse(defaultKvps[property]);

        return 0;
    }
}
