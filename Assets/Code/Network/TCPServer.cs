using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;



public class TCPServer
{
    public volatile int messageCount;
    private TcpListener tcpListener;
    private Thread networkThread;
    private readonly string ipAddress;
    private readonly int port;
    public volatile bool listening = true;
    private List<TcpClient> connectedClients;
    private Queue<NetworkMessage> messages;
    private GameSimulation sim;
    DateTime ownStateLastUpdate = DateTime.Now;
    DateTime objectStateLastUpdate = DateTime.Now;
    private DateTime lastGameTimeUpdate = DateTime.Now;
    float timer = 0.0f;

    bool usePortInToken = true;

    public TCPServer(GameSimulation simulation)
    {

        ipAddress = ConfigValueStore.GetValue("ipaddress");
        port = Int32.Parse(ConfigValueStore.GetValue("port"));

    
        sim = simulation;
        messages = new Queue<NetworkMessage>();
        networkThread = new Thread(new ThreadStart(StartServer));
        connectedClients = new List<TcpClient>();

        networkThread.Start();

        SetupEvents();
    }


    private void StartServer()
    {
        tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        tcpListener.Start();


        while (listening)
        {
            try
            {
                var client = tcpListener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(NewClientConnection, client);
                Thread.Sleep(16);
            }
            catch (Exception ex)
            {

            }

        }
    }

    public void Close()
    {

        foreach (TcpClient c in connectedClients)
        {
            if (c.Connected)
                c.Close();

            c.Dispose();
        }

        tcpListener.Stop();
        listening = false;


    }

    private void NewClientConnection(object obj)
    {
        try
        {

            var client = (TcpClient)obj;
            Debug.Log("Client connection made: " + GetTokenFromEndpoint(client));
            lock (connectedClients)
            {
                connectedClients.Add(client);
            }


            // Get a stream object for reading 					
            using (NetworkStream stream = client.GetStream())
            {

                while (client.Connected)
                {
                    int messageType = stream.ReadByte();
                    int length = stream.ReadByte();

                    Byte[] bytes = new Byte[length];


                    // Read length bytes into byte arrary. This is our JSON package, if any
                    string jsonString = "";
                    if (length > 0)
                    {
                        stream.Read(bytes, 0, length);
                        jsonString = Encoding.ASCII.GetString(bytes);
                    }

                    lock (messages)
                    {
                        messages.Enqueue(new NetworkMessage() { type = (NetworkMessageType)messageType, data = jsonString, sender = client });
                    }
                    messageCount++;
                    Thread.Sleep(100);
                }

            }
        }
        catch (Exception ex)
        {
            //this is terrible. I give no fucks.
        }

    }

    public void Update()
    {
        if (messages.Count > 0)
        {
            var newMessage = messages.Dequeue();
            HandleMessage(newMessage);
        }


        if ((DateTime.Now - ownStateLastUpdate).TotalMilliseconds > 350)
        {
            foreach (TcpClient c in connectedClients)
            {
                UpdateClientWithOwnState(c);
            }
            ownStateLastUpdate = DateTime.Now;
        }

        if ((DateTime.Now - objectStateLastUpdate).TotalMilliseconds > 500)
        {
            foreach (TcpClient c in connectedClients)
            {
                UpdateClientWithOtherObjectState(c);
            }
            objectStateLastUpdate = DateTime.Now;
        }

        if ((DateTime.Now - lastGameTimeUpdate).TotalSeconds > 30)
        {
            UpdateGameTimeRemaining();
            lastGameTimeUpdate = DateTime.Now;
        }

        timer += Time.deltaTime;
        if(timer > 1)
        {
            Debug.Log("Messages per second: " + messageCount);
            messageCount = 0;
        }
    }

    private void SendMessage(TcpClient client, byte[] message)
    {
        if (client == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = client.GetStream();
            if (stream.CanWrite)
            {
                stream.Write(message, 0, message.Length);
            }
        }
        catch (SocketException socketException)
        {
            Console.WriteLine("Socket exception: " + socketException);
        }
    }

    private TcpClient GetClientForTank(TankController tank)
    {
        foreach (TcpClient tcpClient in connectedClients)
        {
            if (GetTokenFromEndpoint(tcpClient) == tank.Token)
                return tcpClient;
        }
        return null;
    }

    private void UpdateClientWithOwnState(TcpClient client)
    {


        try
        {

            if (client.Connected)
            {
                foreach (TankController t in sim.tankControllers)
                {
                    if (t.Token == GetTokenFromEndpoint(client))
                    {
                        var obj = GameSimulation.CreateTankState(t);
                        var json = JsonUtility.ToJson(obj);
                        SendMessage(client, MessageFactory.CreateObjectUpdateMessage(json));
                        return;
                    }
                }


            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void UpdateClientWithOtherObjectState(TcpClient client)
    {


        try
        {

            if (client.Connected)
            {
                var gameObjectsInView = sim.GetObjectsInViewOfTank(GetTokenFromEndpoint(client));

                foreach (GameObjectState s in gameObjectsInView)
                {

                    SendMessage(client, MessageFactory.CreateObjectUpdateMessage(JsonUtility.ToJson(s)));
                }


            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private string GetTokenFromEndpoint(TcpClient client)
    {
        if (usePortInToken)
            return client.Client.RemoteEndPoint.ToString();
        else
            return client.Client.RemoteEndPoint.ToString().Split(':')[0];
    }

    private void HandleMessage(NetworkMessage newMessage)
    {
        try
        {
            //Debug.Log(newMessage.type + " - " + (string)newMessage.data);

            //make the id of the tank the IP address of the client
            string clientId = GetTokenFromEndpoint(newMessage.sender);



            switch (newMessage.type)
            {

                case (NetworkMessageType.test):

                    byte[] testReturn = new byte[2];
                    testReturn[0] = 0;
                    testReturn[1] = 0;
                    newMessage.sender.Client.Send(testReturn);
                    break;


                case (NetworkMessageType.createTank):

                    CreatePlayer messageData = JsonUtility.FromJson<CreatePlayer>((string)newMessage.data);

                    sim.enqueuedCommands.Enqueue(new GameCommand()
                    {

                        Type = CommandType.PlayerCreate,
                        Payload = new PlayerCreate() { Name = messageData.Name, Token = clientId, Color = "" },
                        Token = clientId
                    });

                    break;

                case (NetworkMessageType.despawnTank):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Despawn, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.fire):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Fire, Token = clientId, Payload = null });
                    break;
                case (NetworkMessageType.stopTurret):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.StopTurret, Token = clientId, Payload = null });
                    break;
                case (NetworkMessageType.stopMove):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.StopMove, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.stopTurn):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.StopTurn, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.stopAll):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.FullStop, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.toggleTurretLeft):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.ToggleTurretLeft, Token = clientId, Payload = null });
                    break;
                case (NetworkMessageType.toggleTurretRight):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.ToggleTurretRight, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.toggleLeft):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.ToggleLeft, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.toggleRight):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.ToggleRight, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.toggleReverse):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.ToggleReverse, Token = clientId, Payload = null });
                    break;
                case (NetworkMessageType.toggleForward):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.ToggleForward, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.turnToHeading):
                    MovementParameter movement = JsonUtility.FromJson<MovementParameter>((string)newMessage.data);
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurnToHeading, Token = clientId, Payload = movement });
                    break;
                case (NetworkMessageType.turnTurretToHeading):
                    MovementParameter movement2 = JsonUtility.FromJson<MovementParameter>((string)newMessage.data);
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurnTurretToHeading, Token = clientId, Payload = movement2 });
                    break;
                case (NetworkMessageType.moveForwardDistance):
                    MovementParameter movement3 = JsonUtility.FromJson<MovementParameter>((string)newMessage.data);
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.MoveForwardDistance, Token = clientId, Payload = movement3 });
                    break;
                case (NetworkMessageType.moveBackwardsDistance):
                    MovementParameter movement4 = JsonUtility.FromJson<MovementParameter>((string)newMessage.data);
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.MoveBackDistance, Token = clientId, Payload = movement4 });
                    break;
            }
        }
        catch (Exception ex)
        {

        }


    }

    private void SetupEvents()
    {
        EventManager.healthPickupEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.healthPickup;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }
                );

        EventManager.ammoPickupEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.ammoPickup;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }
       );

        EventManager.snitchPickupEvent.AddListener(x =>
        {
            PlayerId id = new PlayerId();
            id.Id = x.GetInstanceID();
            string jsonPackage = JsonUtility.ToJson(id);


            byte[] message= Encoding.ASCII.GetBytes(jsonPackage);
            var finalMessage = MessageFactory.AddTypeAndLengthToArray(message, (byte)NetworkMessageType.snitchPickup);

            foreach(TcpClient c in connectedClients)
            {
                c.Client.Send(finalMessage);
            }

           
        });

        EventManager.destroyedEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.destroyed;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }
     );

        EventManager.hitDetectedEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.hitDetected;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }
     );
        EventManager.successfulHitEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.successfulHit;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }
   );

        EventManager.killEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.kill;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }
   );

        EventManager.goalEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.enteredGoal;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                client.Client.Send(message);
        }

       
   );

        EventManager.snitchAppearedEvent.AddListener(() =>
        {

            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.snitchAppeared;
            message[1] = 0;

            foreach (TcpClient c in connectedClients)
            {
                c.Client.Send(message);
            }
        });

       
    }

    private void UpdateGameTimeRemaining()
    {
        GameTimeUpdate time = new GameTimeUpdate();
        time.Time = (int)TrainingRoomMain.timeLeft.TotalSeconds;
        string jsonPackage = JsonUtility.ToJson(time);
        byte[] message = Encoding.ASCII.GetBytes(jsonPackage);
        var finalMessage = MessageFactory.AddTypeAndLengthToArray(message, (byte)NetworkMessageType.gameTimeUpdate);

        foreach (TcpClient c in connectedClients)
        {
            c.Client.Send(finalMessage);
        }
    }
}

public static class MessageFactory
{

    public static byte[] CreateObjectUpdateMessage(string json)
    {
        byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(json);
        return AddTypeAndLengthToArray(clientMessageAsByteArray, (byte)NetworkMessageType.objectUpdate);
    }

    public static byte[] AddTypeAndLengthToArray(byte[] bArray, byte type)
    {
        byte[] newArray = new byte[bArray.Length + 2];
        bArray.CopyTo(newArray, 2);
        newArray[0] = type;
        newArray[1] = (byte)bArray.Length;
        return newArray;
    }




}


public struct NetworkMessage
{
    public NetworkMessageType type;
    public object data;
    public TcpClient sender;
}

public struct CreatePlayer
{
    public string Name;

}

public struct GameTimeUpdate
{
    public int Time;
}

public struct PlayerId
{
    public int Id;
}




public enum NetworkMessageType
{
    test = 0,
    createTank = 1,
    despawnTank = 2,
    fire = 3,
    toggleForward = 4,
    toggleReverse = 5,
    toggleLeft = 6,
    toggleRight = 7,
    toggleTurretLeft = 8,
    toggleTurretRight = 9,
    turnTurretToHeading = 10,
    turnToHeading = 11,
    moveForwardDistance = 12,
    moveBackwardsDistance = 13,
    stopAll = 14,
    stopTurn = 15,
    stopMove = 16,
    stopTurret = 17,
    objectUpdate = 18,
    healthPickup = 19,
    ammoPickup = 20,
    snitchPickup = 21,
    destroyed = 22,
    enteredGoal = 23,
    kill = 24,
    snitchAppeared = 25,
    gameTimeUpdate = 26,
    hitDetected = 27,
    successfulHit = 28

}