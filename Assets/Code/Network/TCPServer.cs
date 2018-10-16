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
    public volatile int messageOutCount;

    private TcpListener tcpListener;
    private Thread networkThread;
    private readonly string ipAddress;
    private readonly int port;
    public volatile bool listening = true;
    private List<TcpClient> connectedClients;
    private List<TcpClient> justAddedClients = new List<TcpClient>();
    private List<TcpClient> justRemovedClients = new List<TcpClient>();

    private Dictionary<TcpClient, string> clientToTokenMap = new Dictionary<TcpClient, string>();
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
                Thread.Sleep(32);
            }
            catch (Exception ex)
            {

            }

        }
    }

    public void Close()
    {

        tcpListener.Stop();
        listening = false;


    }

    private void NewClientConnection(object obj)
    {
        try
        {

            var client = (TcpClient)obj;
            lock (justAddedClients)
            {
                justAddedClients.Add(client);

            }
            lock (clientToTokenMap)
            {
                clientToTokenMap.Add(client, GetTokenFromEndpoint(client));
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
                    Thread.Sleep(16);
                }

            }
        }
        catch (Exception ex)
        {
            //this is terrible. I give no fucks.
            Debug.Log("Exception on listening thread...disconnecting client");
            RemoveClient((TcpClient)obj);
        }

    }

    public void Update()
    {
        lock (messages)
        {
            while (messages.Count > 0)
            {
                var newMessage = messages.Dequeue();
                HandleMessage(newMessage);
            }
        }


        if (justAddedClients.Count > 0)
        {
            lock (justAddedClients)
            {
                foreach (TcpClient tcpClient in justAddedClients)
                {
                    connectedClients.Add(tcpClient);
                }
                justAddedClients.Clear();
            }
        }

        if (justRemovedClients.Count > 0)
        {
            lock (justRemovedClients)
            {
                foreach (TcpClient tcpClient in justRemovedClients)
                {
                    connectedClients.Remove(tcpClient);
                }
                justRemovedClients.Clear();
            }
        }

        foreach(TankController t in sim.tankControllers)
        {
            TcpClient client = GetClientForTank(t);
            if ((DateTime.Now - t.lastOwnUpdateTime).TotalMilliseconds > 350)
            {
                t.lastOwnUpdateTime = DateTime.Now;
                UpdateClientWithOwnState(client);
                continue;
            }

            if ((DateTime.Now - t.lastOtherUpdateTime).TotalMilliseconds > 500)
            {
                t.lastOtherUpdateTime = DateTime.Now;
                UpdateClientWithOtherObjectState(client);
                continue;
            }
        }

        //if ((DateTime.Now - ownStateLastUpdate).TotalMilliseconds > 350)
        //{
        //    foreach (TcpClient c in connectedClients)
        //    {
        //        UpdateClientWithOwnState(c);
        //    }
        //    ownStateLastUpdate = DateTime.Now;
        //}

        //if ((DateTime.Now - objectStateLastUpdate).TotalMilliseconds > 500)
        //{
        //    foreach (TcpClient c in connectedClients)
        //    {
        //        UpdateClientWithOtherObjectState(c);
        //    }
        //    objectStateLastUpdate = DateTime.Now;
        //}

        if ((DateTime.Now - lastGameTimeUpdate).TotalSeconds > 30)
        {
            UpdateGameTimeRemaining();
            lastGameTimeUpdate = DateTime.Now;
        }

        timer += Time.deltaTime;
        if (timer > 1)
        {
          //  Debug.Log("Message in count per second: " + messageCount);
            //Debug.Log("Message out count per second: " + messageOutCount);
            messageCount = 0;
            messageOutCount = 0;
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
                stream.Flush();
                messageOutCount++;
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception. Closing client " + socketException.Message.ToString());
            RemoveClient(client);
        }
        catch (ObjectDisposedException disposedException)
        {
            Debug.Log("Client is disposed. Closing client " + disposedException.Message.ToString());
            RemoveClient(client);
        }
        catch (InvalidOperationException invalidException)
        {
            Debug.Log("Invaid operation. Closing client " + invalidException.Message.ToString());
            RemoveClient(client);
        }
    }

    private void RemoveClient(TcpClient client)
    {
        lock (justRemovedClients)
        {
            justRemovedClients.Add(client);
        }

        TokenCarrier t = new TokenCarrier();
        lock (clientToTokenMap)
        {
            t.Token = clientToTokenMap[client];
        }
        EventManager.clientDisconnect.Invoke(t);
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

    public static string GetTokenFromEndpoint(TcpClient client)
    {
        return client.Client.RemoteEndPoint.ToString();
    }

    private void HandleMessage(NetworkMessage newMessage)
    {
        try
        {

            //make the id of the tank the IP address of the client
            string clientId = GetTokenFromEndpoint(newMessage.sender);



            switch (newMessage.type)
            {

                case (NetworkMessageType.test):

                    byte[] testReturn = new byte[2];
                    testReturn[0] = 0;
                    testReturn[1] = 0;

                    SendMessage(newMessage.sender, testReturn);

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
                SendMessage(client, message);
        }
                );

        EventManager.ammoPickupEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.ammoPickup;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                SendMessage(client, message);
        }
       );

        EventManager.snitchPickupEvent.AddListener(x =>
        {
            PlayerId id = new PlayerId();
            id.Id = x.gameObject.GetInstanceID();
            string jsonPackage = JsonUtility.ToJson(id);


            byte[] message = Encoding.ASCII.GetBytes(jsonPackage);
            var finalMessage = MessageFactory.AddTypeAndLengthToArray(message, (byte)NetworkMessageType.snitchPickup);

            foreach (TcpClient c in connectedClients)
            {
                SendMessage(c, finalMessage);
            }


        });

        EventManager.destroyedEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.destroyed;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                SendMessage(client, message);
        }
     );

        EventManager.hitDetectedEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.hitDetected;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                SendMessage(client, message);
        }
     );
        EventManager.successfulHitEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.successfulHit;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                SendMessage(client, message);
        }
   );

        EventManager.killEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.kill;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                SendMessage(client, message);
        }
   );

        EventManager.goalEvent.AddListener(x =>
        {
            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.enteredGoal;
            message[1] = 0;
            var client = GetClientForTank(x);

            if (client != null)
                SendMessage(client, message);
        }





   );





        EventManager.snitchAppearedEvent.AddListener(() =>
        {

            byte[] message = new byte[2];
            message[0] = (byte)NetworkMessageType.snitchAppeared;
            message[1] = 0;

            foreach (TcpClient c in connectedClients)
            {
                SendMessage(c, message);
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

            SendMessage(c, finalMessage);
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