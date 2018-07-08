using System;
using System.Collections.Generic;
using UnityEngine;

internal class GameSimulation
{
    GameSimRules rules;
    List<Player> activePlayers;

    public GameSimulation(GameSimRules ruleset)
    {
        activePlayers = new List<Player>();
        rules = ruleset;
    }

    internal Player CreatePlayer(PlayerCreate create)
    {

        return new Player();
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