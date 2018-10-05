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
                    Debug.Log(pair[0] + ":" + pair[1]);
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

            Debug.Log("LOADING DEFAULT CONFIG");

        }


    }

    private static void LoadDefaults()
    {
        defaultKvps.Add("ipaddress", "0.0.0.0");
        defaultKvps.Add("port", "8052");
        defaultKvps.Add("game_time", "300");
        defaultKvps.Add("kill_capture_mode", "true");
        defaultKvps.Add("snitch_enabled", "true");
        defaultKvps.Add("snitch_spawn_threshold", "90");
        defaultKvps.Add("snitch_goal_points", "20");
        defaultKvps.Add("snitch_kill_points", "5");
        defaultKvps.Add("kill_points", "1");
        defaultKvps.Add("health_packs_active", "2");
        defaultKvps.Add("ammo_packs_active", "2");
        defaultKvps.Add("turret_rotation_speed", "2.5");
        defaultKvps.Add("movement_speed", "10");
        defaultKvps.Add("turn_speed", "100");
        defaultKvps.Add("fire_interval", "2");
        defaultKvps.Add("projectile_force", "2000");
        defaultKvps.Add("starting_health", "10");
        defaultKvps.Add("starting_ammo", "10");


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

    public static float GetFloatValue(string property)
    {
        if (keyValuePairs.ContainsKey(property))
            return float.Parse(keyValuePairs[property]);
        if (defaultKvps.ContainsKey(property))
            return float.Parse(defaultKvps[property]);

        return 0;
    }
}
