using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using UnityEngine;

public static class GameFlags
{

    public static bool BasicTank = false;
}

public class TrainingRoomMain : MonoBehaviour
{
    
    GameSimulation simulation;
    TCPServer server;
    StadiumCam cam;

    // Use this for initialization
    void Start()
    {
        var ruleSet = new GameSimRules();
        ruleSet.TrainingMode = true;
        simulation = new GameSimulation(ruleSet);



        //simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank1", new Vector3(0, 0, 30), true, true);
        //simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank2", new Vector3(0, 0, -30), true, true);

        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank1", new Vector3(0, 5, 0), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank2", new Vector3(30, 5, -30), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank3", new Vector3(30, 5, 0), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank4", new Vector3(0, 5, -30), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank5", new Vector3(30, 5, 30), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank6", new Vector3(-30, 5, -30), false, true);

        server = new TCPServer(simulation);

        cam = GameObject.Find("CameraRig").GetComponent<StadiumCam>();
    }

    private void OnApplicationQuit()
    {
        server.Close();
       
    }

    private static string GenerateRandomHexColorString()
    {
        return "#" + ColorUtility.ToHtmlStringRGB(UnityEngine.Random.ColorHSV());
    }

    // Update is called once per frame
    void Update()
    {


        if (simulation.tankControllers.Count == 0)
            cam.Left(0.1f);

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank", simulation.RandomArenaPosition(), true, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank", simulation.RandomArenaPosition(), false, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            simulation.CreateAITank(GenerateRandomHexColorString(), "AITank", simulation.RandomArenaPosition(), false, true);
        }

        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            simulation.ClearAllNonPlayerTanks();
        }


        if (Input.GetKeyUp(KeyCode.Delete))
        {
            simulation.ClearAllTanks();
        }




        simulation.Update();
        server.Update();
    }


}
