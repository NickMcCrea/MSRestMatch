using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankFactory : MonoBehaviour
{
    int spawnPoint = 1;
  
    // Use this for initialization
    public TankFactory()
    {

        //tank1 = CreateTank<ManualTankController>("#EA9414");
        //SetTankEmissive(tank1, Color.red, 10);
        //PlaceTankAtSpawnPoint(tank1, "SpawnPoint 1");

        //tank2 = CreateTank<DummyTank>("#14EA94");
        //PlaceTankAtSpawnPoint(tank2, "SpawnPoint 2");
     


    }

    public GameObject CreateTank(string mainColor, string name, string token)
    {
        var tank = CreateTank<TankController>(mainColor);
        PlaceTankAtSpawnPoint(tank, "SpawnPoint " + spawnPoint.ToString());
        tank.GetComponent<TankController>().Name = name;
        tank.GetComponent<TankController>().Token = token;

        spawnPoint++;
        if (spawnPoint > 2)
            spawnPoint = 1;


        return tank;
    }

    private void PlaceTankAtSpawnPoint(GameObject tank, string spawnPointName)
    {
        var spawnPoint = GameObject.Find(spawnPointName);
        tank.transform.position = SetTankOnFloor(spawnPoint.transform.position);
    }

    private Vector3 SetTankOnFloor(Vector3 position)
    {
        var newPos = position;
        newPos.y = 0.75f;
        return newPos;
    }

    private GameObject CreateTank<T>(string tankColor)
    {
        var tank = Instantiate(Resources.Load("Prefabs/ToyTank")) as GameObject;
        tank.AddComponent(typeof(T));
        SetTankColor(tank, tankColor);
        return tank;
    }

    private void SetTankColor(GameObject tankRootObject, string hexColorString)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hexColorString, out color);
        SetTankColor(tankRootObject, color);
    }

    private void SetTankColor(GameObject tankRootObject, Color color)
    {
        var renderers = tankRootObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in renderers)
        {
            foreach (Material m in mr.materials)
                m.color = color;
        }
    }


    private void SetTankEmissive(GameObject tankRootObject, Color color, float level)
    {
        var renderers = tankRootObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in renderers)
        {
            foreach (Material m in mr.materials)
                m.SetColor("_EmissionColor", color);
        }
    }
}
