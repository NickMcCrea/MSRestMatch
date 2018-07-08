using System;
using System.Collections.Generic;
using UnityEngine;

internal class GameSimulation
{
    GameSimRules rules;
    List<Player> activePlayers;
    TankFactory tankFactory;
    public Queue<GameCommand> enqueuedCommands;

    public GameSimulation(GameSimRules ruleset)
    {
        activePlayers = new List<Player>();
        rules = ruleset;
        tankFactory = new TankFactory();
        enqueuedCommands = new Queue<GameCommand>();
    }

    internal GameObject CreatePlayer(PlayerCreate create)
    {
        return tankFactory.CreateTank(create.Color, create.Name);
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
        switch (command.Type)
        {
            case (CommandType.PlayerCreate):

                PlayerCreate create = command.Payload as PlayerCreate;
                GameObject tank = CreatePlayer(create);
                break;

            case (CommandType.Forward):

                TankController t = FindTankObject(command.Name);
                if (t != null)
                    t.ToggleForward();
                break;
        }
    }

    public TankController FindTankObject(string name)
    {
        var tanks = GameObject.FindObjectsOfType<TankController>();
        foreach (TankController t in tanks)
        {
            if (t.Name == name)
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