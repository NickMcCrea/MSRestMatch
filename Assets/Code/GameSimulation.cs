using System;
using System.Collections.Generic;
using UnityEngine;

internal class GameSimulation
{
    GameSimRules rules;
    List<Player> activePlayers;
    TankFactory tankFactory;
    public Queue<GameCommand> enqueuedCommands;
    public List<TankController> tankControllers;

    public GameSimulation(GameSimRules ruleset)
    {
        activePlayers = new List<Player>();
        rules = ruleset;
        tankFactory = new TankFactory();
        enqueuedCommands = new Queue<GameCommand>();
        tankControllers = new List<TankController>();
    }

    internal GameObject CreatePlayer(PlayerCreate create)
    {
        var t = tankFactory.CreateTank(create.Color, create.Name, create.Token);
        tankControllers.Add(t.GetComponent<TankController>());
        return t;
    }

    public void Update()
    {
        if (enqueuedCommands.Count > 0)
        {
            GameCommand command = enqueuedCommands.Dequeue();
            HandleCommand(command);
        }
    }

    private void HandleCommand(GameCommand command)
    {
        TankController t = FindTankObject(command.Token);

        switch (command.Type)
        {
            case (CommandType.PlayerCreate):

                PlayerCreate create = command.Payload as PlayerCreate;
                GameObject tank = CreatePlayer(create);
                break;

            case (CommandType.Forward):
                if (t != null)
                    t.ToggleForward();
                break;
            case (CommandType.Reverse):
                if (t != null)
                    t.ToggleReverse();
                break;
            case (CommandType.Right):
                if (t != null)
                    t.ToggleRight();
                break;
            case (CommandType.Left):
                if (t != null)
                    t.ToggleLeft();
                break;
            case (CommandType.TurretLeft):
                if (t != null)
                    t.ToggleTurretLeft();
                break;
            case (CommandType.TurretRight):
                if (t != null)
                    t.ToggleTurretRight();
                break;
            case (CommandType.Stop):
                if (t != null)
                    t.Stop();
                break;
            case (CommandType.StopTurret):
                if (t != null)
                    t.StopTurret();
                break;
            case (CommandType.Fire):
                if (t != null)
                    t.Fire();
                break;
        }
    }

    public TankController FindTankObject(string token)
    {
        var tanks = GameObject.FindObjectsOfType<TankController>();
        foreach (TankController t in tanks)
        {
            if (t.Token == token)
                return t;
        }
        return null;
    }
}

public class GameSimRules
{
    public int FragWinLimit { get; set; }
    public int RespawnTime { get; set; }
    public int GameTimeLimit { get; set; }
    public bool TrainingMode { get; set; }

    public GameSimRules()
    {
        FragWinLimit = 10;
        RespawnTime = 5;
        GameTimeLimit = 300;
        TrainingMode = false;
    }
}