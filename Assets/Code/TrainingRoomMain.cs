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
        {
            cam.Left(0.1f);
            if (cam.transform.position.magnitude < 250)
                cam.ZoomOut();
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            cam.SetTargetFollowMode(simulation.GetNextTank());
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            cam.SetTargetFollowMode(simulation.GetPreviousTank());
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            cam.SetCenterCircleMode();
        }

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
