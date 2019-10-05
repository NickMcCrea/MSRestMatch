using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using UnityEngine;
using UnityEngine.UI;
using TMPro.Examples;
using System.Text;


public class TrainingRoomMain : MonoBehaviour
{

    public enum GameState
    {
        notStarted,
        playing,
        gameOver
    }

    public static GameState currentGameState = GameState.notStarted;
    public static GameSimulation simulation;
    TCPServer server;
    StadiumCam cam;
    int aiTankCount = 0;
    int dummyTankCount = 0;
    public static TimeSpan timeLeft;

    Text scoreBoard;
    Text timer;
    DateTime scoreRefreshTime;

    DateTime gameStart;
    TimeSpan gameDuration;


    // Use this for initialization
    void Start()
    {
       
        simulation = new GameSimulation();


        server = new TCPServer(simulation);

        cam = GameObject.Find("CameraRig").GetComponent<StadiumCam>();

        scoreBoard = GameObject.Find("Scoreboard").GetComponent<Text>();
        timer = GameObject.Find("Timer").GetComponent<Text>();

        gameDuration = new TimeSpan(0, 0, Int32.Parse(ConfigValueStore.GetValue("game_time")));

        timer.text = string.Format("{0:hh\\:mm\\:ss}", gameDuration);

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



        if (currentGameState == GameState.notStarted)
        {

            GameObject.Find("MainLogo").GetComponent<Fade>().FadeIn(0.8f);
            cam.Left(0.1f);

            if (cam.transform.position.magnitude < 250)
                cam.ZoomOut();

        }


        if (currentGameState == GameState.notStarted)
        {
            if (Input.GetKeyUp(KeyCode.Space))
                GameStart();
        }

        if (currentGameState == GameState.gameOver)
        {
            if (Input.GetKeyUp(KeyCode.Space))
                GamePrep();
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
            if (currentGameState == GameState.notStarted)
                GameStart();

            dummyTankCount++;
            simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank" + dummyTankCount, simulation.RandomArenaPosition(), true, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (currentGameState == GameState.notStarted)
                GameStart();
            dummyTankCount++;
            simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank" + dummyTankCount, simulation.RandomArenaPosition(), false, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            if (currentGameState == GameState.notStarted)
                GameStart();

            aiTankCount++;
            simulation.CreateAITank(GenerateRandomHexColorString(), "AITank" + aiTankCount, simulation.RandomArenaPosition(), false, true);
        }

        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            if (currentGameState == GameState.notStarted)
                GameStart();

            simulation.CreateManualTank();
        }

        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            simulation.ClearAllNonPlayerTanks();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            cam.ResetCamera();
        }



        if (Input.GetKeyUp(KeyCode.Delete))
        {
            simulation.ClearAllTanks();
            GamePrep();
        }

        if ((DateTime.Now - scoreRefreshTime).TotalSeconds > 5 && currentGameState == GameState.playing)
        {
            RefreshScores();
        }


        if (TrainingRoomMain.currentGameState == GameState.playing)
        {
            TimeSpan timeSinceStart = DateTime.Now - gameStart;
            timeLeft = gameDuration - timeSinceStart;

            timer.text = string.Format("{0:hh\\:mm\\:ss}", timeLeft);

            if (timeLeft.TotalSeconds < 30)
                timer.color = Color.red;
            else
                timer.color = Color.white;

            if (timeSinceStart > gameDuration)
            {
                GameOver();
            }


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

    private void GameOver()
    {
        currentGameState = GameState.gameOver;
        cam.SetLeaderBoardMode();
        Debug.Log("GAME OVER");

    }

    private void GamePrep()
    {
        simulation.ClearAllTanks();
        currentGameState = GameState.notStarted;
        cam.SetCenterCircleMode();
        GameObject.Find("MainLogo").GetComponent<Fade>().FadeIn(0.8f);
        Debug.Log("GAME PREP");
    }

    private void GameStart()
    {
        currentGameState = GameState.playing;
        gameStart = DateTime.Now;
        GameObject.Find("MainLogo").GetComponent<Fade>().FadeOut(2f);
        Debug.Log("GAME START");
    }
}
