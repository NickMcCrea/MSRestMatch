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


    }

    public GameObject CreateTank(string mainColor, string name, string token, Vector3 startingPosition)
    {



        var tank = CreateTank<TankController>(mainColor);

        // PlaceTankAtSpawnPoint(tank, "SpawnPoint " + spawnPoint.ToString());

        PlaceTank(tank, startingPosition);

        tank.GetComponent<TankController>().Name = name;
        tank.GetComponent<TankController>().Token = token;

        spawnPoint++;
        if (spawnPoint > 2)
            spawnPoint = 1;


        return tank;
    }

    public GameObject CreateDummyTank(string mainColor, string name, Vector3 startingPosition)
    {
        var tank = CreateTank<DummyTank>(mainColor);


        PlaceTank(tank, startingPosition);

        tank.GetComponent<DummyTank>().Name = name;
        tank.GetComponent<DummyTank>().Token = new Guid().ToString();


        return tank;
    }

    public GameObject CreateAITank(string mainColor, string name, Vector3 startingPosition)
    {
        var tank = CreateTank<AITankController>(mainColor);

        PlaceTank(tank, startingPosition);
        tank.GetComponent<AITankController>().Name = name;
        tank.GetComponent<AITankController>().Token = new Guid().ToString();


        return tank;
    }


    private void PlaceTankAtSpawnPoint(UnityEngine.GameObject tank, string spawnPointName)
    {
        var spawnPoint = UnityEngine.GameObject.Find(spawnPointName);
        tank.transform.position = SetTankOnFloor(spawnPoint.transform.position);
    }

    private void PlaceTank(UnityEngine.GameObject tank, Vector3 pos)
    {
        tank.transform.position = SetTankOnFloor(pos);
    }

    private Vector3 SetTankOnFloor(Vector3 position)
    {
        var newPos = position;
        newPos.y = 0.75f;
        return newPos;
    }

    private GameObject CreateTank<T>(string tankColor)
    {

        GameObject tank;
        int randomTank = new System.Random().Next(1, 11);
        tank = Instantiate(Resources.Load("Prefabs/Tanks/Tank" + randomTank)) as UnityEngine.GameObject;
        tank.AddComponent(typeof(T));

        return tank;
    }

    private void SetTankColor(UnityEngine.GameObject tankRootObject, string hexColorString)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hexColorString, out color);
        SetTankColor(tankRootObject, color);
    }

    private void SetTankColor(UnityEngine.GameObject tankRootObject, Color color)
    {
        tankRootObject.GetComponent<TankController>().mainTankColor = color;

        var renderers = tankRootObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in renderers)
        {
            foreach (Material m in mr.materials)
                m.color = color;
        }
    }


    private void SetTankEmissive(UnityEngine.GameObject tankRootObject, Color color, float level)
    {
        var renderers = tankRootObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in renderers)
        {
            foreach (Material m in mr.materials)
                m.SetColor("_EmissionColor", color);
        }
    }
}
