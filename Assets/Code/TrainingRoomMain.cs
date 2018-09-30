using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using UnityEngine;
using UnityEngine.UI;
using TMPro.Examples;
using System.Text;

public static class GameFlags
{

    public static bool BasicTank = false;
}

public class TrainingRoomMain : MonoBehaviour
{

    GameSimulation simulation;
    TCPServer server;
    StadiumCam cam;
    int aiTankCount = 0;
    int dummyTankCount = 0;
    private bool playMode = false;
    Text scoreBoard;
    DateTime scoreRefreshTime;

    // Use this for initialization
    void Start()
    {
        var ruleSet = new GameSimRules();
        ruleSet.TrainingMode = true;
        simulation = new GameSimulation(ruleSet);


        server = new TCPServer(simulation);

        cam = GameObject.Find("CameraRig").GetComponent<StadiumCam>();

        scoreBoard = GameObject.Find("Scoreboard").GetComponent<Text>();


        scoreRefreshTime = DateTime.Now;

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
            GameObject.Find("MainLogo").GetComponent<Fade>().FadeIn(0.8f);

            cam.Left(0.1f);
            if (cam.transform.position.magnitude < 250)
                cam.ZoomOut();

            if (playMode)
            {
                GameStop();
            }
        }
        else
        {
            if (!playMode)
            {
                GameStart();
            }
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            cam.ZoomOut(1f);

        }
        if (Input.mouseScrollDelta.y > 0)
            cam.ZoomIn(1f);

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
            dummyTankCount++;
            simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank" + dummyTankCount, simulation.RandomArenaPosition(), true, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            dummyTankCount++;
            simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank" + dummyTankCount, simulation.RandomArenaPosition(), false, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            aiTankCount++;
            simulation.CreateAITank(GenerateRandomHexColorString(), "AITank" + aiTankCount, simulation.RandomArenaPosition(), false, false);
        }

        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            simulation.ClearAllNonPlayerTanks();
        }


        if (Input.GetKeyUp(KeyCode.Delete))
        {
            simulation.ClearAllTanks();
        }



        if((DateTime.Now - scoreRefreshTime).TotalSeconds > 5)
        {
            RefreshScores();
        }

        simulation.Update();
        server.Update();
    }

    private void RefreshScores()
    {
        var scores = simulation.GetScores();


        StringBuilder sb = new StringBuilder();
        sb.Append("LEADERBOARD");

        sb.AppendLine();
        sb.AppendLine();
        foreach (TankController t in scores)
        {
            sb.Append(t.Name + " - " + t.Points);
            sb.AppendLine();
        }

        scoreBoard.text = sb.ToString();

        scoreRefreshTime = DateTime.Now;
    }

    private void GameStop()
    {
        simulation.GameInProgress = false;
        cam.SetTargetFollowMode(simulation.GetNextTank());
        playMode = false;
        GameObject.Find("MainLogo").GetComponent<Fade>().FadeIn(0.8f);
    }

    private void GameStart()
    {
        simulation.GameInProgress = true;
        cam.SetTargetFollowMode(simulation.GetNextTank());
        playMode = true;
        GameObject.Find("MainLogo").GetComponent<Fade>().FadeOut(2f);
    }
}
