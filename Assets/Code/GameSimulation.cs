using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectState
{
    public int Id;
    public string Name;
    public string Type;
    public float X;
    public float Y;
    public float Heading;
    public float TurretHeading;
    public int Health;
    public int Ammo;
}

[System.Serializable]
public class TankEvent<TankController> : UnityEvent<TankController>
{

}

public static class EventManager
{
    public static TankEvent<TankController> healthPickupEvent;
    public static TankEvent<TankController> ammoPickupEvent;
    public static TankEvent<TankController> snitchPickupEvent;

    public static TankEvent<TankController> destroyedEvent;
    public static TankEvent<TankController> killEvent;
    public static TankEvent<TankController> goalEvent;

    static EventManager()
    {

    }

    public static void Initialise()
    {
        healthPickupEvent = new TankEvent<TankController>();
        ammoPickupEvent = new TankEvent<TankController>();
        snitchPickupEvent = new TankEvent<TankController>();


        destroyedEvent = new TankEvent<TankController>();
        killEvent = new TankEvent<TankController>();
        goalEvent = new TankEvent<TankController>();

    }
}

public class GameSimulation
{
    GameSimRules rules;
    List<Player> activePlayers;
    TankFactory tankFactory;
    public Queue<GameCommand> enqueuedCommands;
    public List<TankController> tankControllers;
    private List<GameObjectState> allObjects;
    private Dictionary<string, List<GameObjectState>> objectsInFieldOfView;
    private List<TankController> tanksToBeRemoved;
    private int currentTank = 0;
    private bool snitchSpawned = false;
    private List<GameObject> healthPickups;
    private List<GameObject> ammoPickups;
    private List<GameObject> healthPickupsToRemove;
    private List<GameObject> ammoPickupsToRemove;

    public GameObject snitch;

    public float fov = 50;
    public float maxdistance = 100;
    private float arenaSize = 80f;


    //some events



    public GameSimulation(GameSimRules ruleset)
    {
        activePlayers = new List<Player>();
        rules = ruleset;
        tankFactory = new TankFactory();
        enqueuedCommands = new Queue<GameCommand>();
        tankControllers = new List<TankController>();
        allObjects = new List<GameObjectState>();
        objectsInFieldOfView = new Dictionary<string, List<GameObjectState>>();
        tanksToBeRemoved = new List<TankController>();

        healthPickups = new List<GameObject>();
        ammoPickups = new List<GameObject>();
        healthPickupsToRemove = new List<GameObject>();
        ammoPickupsToRemove = new List<GameObject>();


        //event setup
        EventManager.Initialise();

    }

    internal GameObject CreatePlayer(PlayerCreate create)
    {
        //get a random point in the arena
        Vector3 potentialStartPoint = RandomArenaPosition();


        //TODO - check starting point for obstacles. Don't start too close to other tanks

        //already exists. Ignore.
        if (FindTankObject(create.Token) != null)
        {
            return null;
        }


        var t = tankFactory.CreateTank(create.Color, create.Name, create.Token, potentialStartPoint);
        //randomly rotate the tank
        t.GetComponent<TankController>().transform.Rotate(Vector3.up, UnityEngine.Random.Range(0, 360));
        t.GetComponent<TankController>().Ruleset = rules;
        t.GetComponent<TankController>().Sim = this;
        tankControllers.Add(t.GetComponent<TankController>());
        return t;
    }

    internal GameObject GetNextTank()
    {
        GameObject toReturn = null;
        if (tankControllers.Count == 0)
        {
            currentTank = 0;
            return null;
        }

        if (tankControllers.Count > currentTank)
            toReturn = tankControllers[currentTank].gameObject;

        currentTank++;
        if (currentTank >= tankControllers.Count)
            currentTank = 0;


        return toReturn;
    }

    internal GameObject CreateManualTank()
    {
        var t = tankFactory.CreateManualTank("ManualTank", RandomArenaPosition());

        t.GetComponent<ManualTankController>().Ruleset = rules;
        t.GetComponent<ManualTankController>().Sim = this;
        t.GetComponent<ManualTankController>().infiniteAmmo = false;
        t.GetComponent<ManualTankController>().infiniteHealth = false;
        tankControllers.Add(t.GetComponent<ManualTankController>());
        return t;
    }

    internal GameObject CreatePlayerTest(PlayerCreateTest create)
    {

        var t = tankFactory.CreateTank(create.Color, create.Name, create.Token, new Vector3(float.Parse(create.X), 5, float.Parse(create.Y)));

        t.GetComponent<TankController>().transform.Rotate(Vector3.up, float.Parse(create.Angle));

        t.GetComponent<TankController>().Ruleset = rules;
        t.GetComponent<TankController>().Sim = this;
        tankControllers.Add(t.GetComponent<TankController>());
        return t;
    }

    internal GameObject GetPreviousTank()
    {
        GameObject toReturn = null;
        if (tankControllers.Count == 0)
        {
            currentTank = 0;
            return null;

        }

        if (tankControllers.Count > currentTank)
            toReturn = tankControllers[currentTank].gameObject;

        currentTank--;
        if (currentTank < 0)
            currentTank = tankControllers.Count - 1;


        return toReturn;

    }

    internal void ClearAllNonPlayerTanks()
    {
        foreach (TankController t in tankControllers)
        {
            if (t is DummyTank || t is AITankController)
                tanksToBeRemoved.Add(t);
        }
    }

    internal GameObject CreateAITank(string color, string name, Vector3 startingPos, bool infiniteHealth, bool infiniteAmmo)
    {
        var t = tankFactory.CreateAITank(color, name, startingPos);

        t.GetComponent<AITankController>().Ruleset = rules;
        t.GetComponent<AITankController>().Sim = this;
        t.GetComponent<AITankController>().infiniteAmmo = infiniteAmmo;
        t.GetComponent<AITankController>().infiniteHealth = infiniteHealth;
        tankControllers.Add(t.GetComponent<AITankController>());
        return t;
    }

    internal void ClearAllTanks()
    {
        foreach (TankController t in tankControllers)
        {
            tanksToBeRemoved.Add(t);
        }
        GameObject.Destroy(snitch);
        snitch = null;
    }

    internal GameObject CreateDummyTank(string color, string name, Vector3 startingPos, bool infiniteHealth, bool infiniteAmmo)
    {
        var t = tankFactory.CreateDummyTank(color, name, startingPos);

        t.GetComponent<DummyTank>().Ruleset = rules;
        t.GetComponent<DummyTank>().Sim = this;
        t.GetComponent<DummyTank>().infiniteAmmo = infiniteAmmo;
        t.GetComponent<DummyTank>().infiniteHealth = infiniteHealth;
        tankControllers.Add(t.GetComponent<DummyTank>());
        return t;
    }

    internal Vector3 RandomArenaPosition()
    {
        var randomCirclePoint = UnityEngine.Random.insideUnitCircle;

        //random start point
        Vector3 potentialStartPoint = new Vector3(randomCirclePoint.x, 0, randomCirclePoint.y);
        potentialStartPoint *= UnityEngine.Random.Range(0, arenaSize - 10);
        return potentialStartPoint;
    }

    public void Update()
    {
        allObjects.Clear();


        HandlePickupLogic();

        foreach (TankController t in tanksToBeRemoved)
            RemoveTank(t);
        tanksToBeRemoved.Clear();

        if (enqueuedCommands.Count > 0)
        {
            GameCommand command = enqueuedCommands.Dequeue();
            HandleCommand(command);
        }

        lock (allObjects)
        {
            UpdateTankState();
        }

        lock (objectsInFieldOfView)
        {
            var tanks = UnityEngine.GameObject.FindObjectsOfType<TankController>();
            foreach (TankController t in tanks)
            {

                UpdateTankViewObjects(t);

            }

        }

        int pickupCount = 2;
        if (TrainingRoomMain.currentGameState == TrainingRoomMain.GameState.playing)
        {

            if (healthPickups.Count < pickupCount)
            {
                SpawnHealthPickup();
            }
            if (ammoPickups.Count < pickupCount)
            {
                SpawnAmmoPickup();
            }

            if (TrainingRoomMain.timeLeft.TotalSeconds < ConfigValueStore.GetIntValue("snitch_spawn_threshold"))
            {
                if (!snitchSpawned && ConfigValueStore.GetBoolValue("snitch_enabled"))
                {
                    SpawnSnitch();
                }
            }
        }


    }

    private void SpawnSnitch()
    {
        snitch = GameObject.Instantiate(Resources.Load("Prefabs/Snitch")) as UnityEngine.GameObject;
        snitch.transform.position = RandomArenaPosition();
        snitchSpawned = true;
    }

    private void HandlePickupLogic()
    {

        float pickupDistance = 5f;
        foreach (GameObject pickup in healthPickups)
        {
            foreach (TankController t in tankControllers)
            {
                float distanceToPickup = (t.transform.position - pickup.transform.position).magnitude;
                if (distanceToPickup < pickupDistance)
                {
                    t.ReplenishHealth();
                    healthPickupsToRemove.Add(pickup);
                    GameObject.Destroy(pickup);
                    Debug.Log(t.Name + " picks up health");

                    EventManager.healthPickupEvent.Invoke(t);



                }
            }
        }
        foreach (GameObject pickup in ammoPickups)
        {
            foreach (TankController t in tankControllers)
            {
                float distanceToPickup = (t.transform.position - pickup.transform.position).magnitude;
                if (distanceToPickup < pickupDistance)
                {
                    t.ReplenishAmmo();
                    ammoPickupsToRemove.Add(pickup);
                    GameObject.Destroy(pickup);
                    Debug.Log(t.Name + " picks up ammo");

                    EventManager.ammoPickupEvent.Invoke(t);
                }
            }
        }



        foreach (GameObject pickup in healthPickupsToRemove)
            healthPickups.Remove(pickup);

        healthPickupsToRemove.Clear();

        foreach (GameObject pickup in ammoPickupsToRemove)
            ammoPickups.Remove(pickup);

        ammoPickupsToRemove.Clear();
    }

    private void SpawnAmmoPickup()
    {
        var pickup = GameObject.Instantiate(Resources.Load("Prefabs/AmmoPickup")) as UnityEngine.GameObject;
        pickup.transform.position = RandomArenaPosition();
        ammoPickups.Add(pickup);
    }

    private void SpawnHealthPickup()
    {
        var pickup = GameObject.Instantiate(Resources.Load("Prefabs/HealthPickup")) as UnityEngine.GameObject;

        pickup.transform.position = RandomArenaPosition();
        healthPickups.Add(pickup);
    }

    internal void RespawnTank(TankController tankController)
    {
        //get a random point in the arena
        Vector3 potentialStartPoint = RandomArenaPosition();
        tankController.transform.position = potentialStartPoint;

        tankController.transform.Rotate(Vector3.up, UnityEngine.Random.Range(0, 360));
        tankController.ReActivate();
    }

    private void UpdateTankViewObjects(TankController t)
    {
        var objectsToAdd = new List<GameObjectState>();
        var tanks = UnityEngine.GameObject.FindObjectsOfType<TankController>();
        foreach (TankController t2 in tanks)
        {

            //this is us, don't bother returning.
            if (t == t2)
                continue;

            if (t.turret == null)
                return;


            Transform checkAgainst = t2.transform;

            if (CheckIfInFoV(t, checkAgainst))
            {
                var obState = CreateTankState(t2);
                objectsToAdd.Add(obState);
            }

        }

        foreach (GameObject healthPickup in healthPickups)
        {
            if (CheckIfInFoV(t, healthPickup.transform))
            {
                GameObjectState s = new GameObjectState();
                s.Id = healthPickup.GetInstanceID();
                s.Type = "HealthPickup";
                s.X = healthPickup.transform.position.x;
                s.Y = healthPickup.transform.position.z;
                objectsToAdd.Add(s);
            }

        }

        foreach (GameObject ammoPickup in ammoPickups)
        {
            if (CheckIfInFoV(t, ammoPickup.transform))
            {
                GameObjectState s = new GameObjectState();
                s.Id = ammoPickup.GetInstanceID();
                s.Type = "AmmoPickup";
                s.X = ammoPickup.transform.position.x;
                s.Y = ammoPickup.transform.position.z;
                objectsToAdd.Add(s);
            }

        }

        if (snitch != null)
        {

            if (CheckIfInFoV(t, snitch.transform))
            {
                GameObjectState s = new GameObjectState();
                s.Id = snitch.GetInstanceID();
                s.Type = "Snitch";
                s.X = snitch.transform.position.x;
                s.Y = snitch.transform.position.z;
                objectsToAdd.Add(s);
            }
        }

        objectsInFieldOfView[t.Token] = objectsToAdd;
    }

    private bool CheckIfInFoV(TankController t, Transform checkAgainst)
    {
        if (t.turret == null)
            return false;

        float distanceBetweenTanks = (t.transform.position - checkAgainst.position).magnitude;
        Vector3 toTank = checkAgainst.position - t.transform.position;
        toTank.Normalize();

        //turret is the wrong way round, so need to use up.
        float angle = Vector3.Angle(-t.turret.transform.up, toTank);
        float dot = Vector3.Dot(-t.turret.transform.up, toTank);
        bool isInFov = false;
        if (distanceBetweenTanks < maxdistance && (Mathf.Abs(angle) < fov / 2))
        {
            if (dot > 0 && dot < 1)
            {

                isInFov = true;

            }

        }

        return isInFov;
    }

    private void UpdateTankState()
    {
        var tanks = UnityEngine.GameObject.FindObjectsOfType<TankController>();
        foreach (TankController t in tanks)
        {
            GameObjectState s = CreateTankState(t);
            allObjects.Add(s);


        }
    }

    internal void RecordFrag(TankController victim, TankController killer)
    {

        EventManager.killEvent.Invoke(killer);
        EventManager.destroyedEvent.Invoke(victim);

        Debug.Log(victim.Name + " killed by " + killer.Name);
        victim.Deaths++;

        if (snitch != null)
        {
            if (snitch.GetComponent<SnitchBehaviour>().collector == victim)
            {
                killer.RewardSnitchPoints();
            }
        }

        killer.AddKillPoints();



    }

    public static GameObjectState CreateTankState(TankController t)
    {
        GameObjectState s = new GameObjectState();
        s.Id = t.gameObject.GetInstanceID();
        s.Ammo = t.Ammo;
        s.Heading = t.Heading;
        s.Health = t.Health;
        s.Name = t.Name;
        s.Type = "Tank";
        s.X = t.X;
        s.Y = t.Y;
        s.TurretHeading = t.TurretHeading;
        return s;
    }

    private void HandleCommand(GameCommand command)
    {
        TankController t = FindTankObject(command.Token);

        if (TrainingRoomMain.currentGameState == TrainingRoomMain.GameState.gameOver)
            return;

        if (TrainingRoomMain.currentGameState == TrainingRoomMain.GameState.notStarted)
            if (command.Type != CommandType.PlayerCreate)
                return;

        MovementParameter p = null;

        switch (command.Type)
        {
            case (CommandType.PlayerCreate):

                PlayerCreate create = command.Payload as PlayerCreate;
                var tank = CreatePlayer(create);
                break;

            case (CommandType.Despawn):

                if (t != null)
                    RemoveTank(t);

                break;

            case (CommandType.PlayerCreateTest):

                PlayerCreateTest createtest = command.Payload as PlayerCreateTest;
                var tanktest = CreatePlayerTest(createtest);
                break;

            case (CommandType.TurnToHeading):
                p = command.Payload as MovementParameter;
                if (t != null)
                    t.TurnToHeading(p.Amount);
                break;

            case (CommandType.TurnTurretToHeading):
                p = command.Payload as MovementParameter;
                if (t != null)
                    t.TurnTurretToHeading(p.Amount);
                break;


            case (CommandType.MoveForwardDistance):
                p = command.Payload as MovementParameter;
                if (t != null)
                    t.MoveDistance(p.Amount);
                break;


            case (CommandType.MoveBackDistance):
                p = command.Payload as MovementParameter;
                if (t != null)
                    t.MoveDistance(-p.Amount);
                break;



            case (CommandType.ToggleForward):
                if (t != null)
                    t.ToggleForward();
                break;
            case (CommandType.ToggleReverse):
                if (t != null)
                    t.ToggleReverse();
                break;
            case (CommandType.ToggleRight):
                if (t != null)
                    t.ToggleRight();
                break;
            case (CommandType.ToggleLeft):
                if (t != null)
                    t.ToggleLeft();
                break;
            case (CommandType.ToggleTurretLeft):
                if (t != null)
                    t.ToggleTurretLeft();
                break;
            case (CommandType.ToggleTurretRight):
                if (t != null)
                    t.ToggleTurretRight();
                break;
            case (CommandType.FullStop):
                if (t != null)
                    t.FullStop();
                break;
            case (CommandType.StopTurret):
                if (t != null)
                    t.StopTurret();
                break;
            case (CommandType.StopMove):
                if (t != null)
                    t.StopMove();
                break;
            case (CommandType.StopTurn):
                if (t != null)
                    t.StopTurn();
                break;

            case (CommandType.Fire):
                if (t != null)
                    t.Fire();
                break;


        }
    }

    private void RemoveTank(TankController t)
    {
        tankControllers.Remove(t);
        objectsInFieldOfView.Remove(t.Token);
        GameObject.Destroy(t.gameObject);
    }

    public TankController FindTankObject(string token)
    {
        var tanks = UnityEngine.GameObject.FindObjectsOfType<TankController>();
        foreach (TankController t in tanks)
        {
            if (t.Token == token)
                return t;
        }
        return null;
    }

    internal List<GameObjectState> GetObjectsInViewOfTank(string token)
    {
        lock (objectsInFieldOfView)
        {
            if (objectsInFieldOfView.Count > 0)
                if (objectsInFieldOfView.ContainsKey(token))
                    return objectsInFieldOfView[token];

        }
        return new List<GameObjectState>();
    }

    internal List<TankController> GetScores()
    {
        return tankControllers.OrderByDescending(x => x.Points).ToList();
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