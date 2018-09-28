using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;



public class TCPServer
{
    private TimeSpan updateMessageInterval = new TimeSpan(0, 0, 0, 0, 300);
    private TcpListener tcpListener;
    private Thread networkThread;
    private readonly string ipAddress = "127.0.0.1";
    private readonly int port = 8052;
    public volatile bool listening = true;
    private List<TcpClient> connectedClients;
    private Queue<NetworkMessage> messages;
    private GameSimulation sim;
    DateTime ownStateLastUpdate = DateTime.Now;
    DateTime objectStateLastUpdate = DateTime.Now;
    

    public TCPServer(GameSimulation simulation)
    {
        sim = simulation;
        messages = new Queue<NetworkMessage>();
        networkThread = new Thread(new ThreadStart(StartServer));
        connectedClients = new List<TcpClient>();
      
        networkThread.Start();
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
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    public void Close()
    {
        
        foreach(TcpClient c in connectedClients)
        {
            if (c.Connected)
                c.Close();

            c.Dispose();
        }

        tcpListener.Stop();
        
    }

    private void NewClientConnection(object obj)
    {
        try
        {
            var client = (TcpClient)obj;

            lock (connectedClients)
            {
                connectedClients.Add(client);
            }

            Byte[] bytes = new Byte[1024];
            // Get a stream object for reading 					
            using (NetworkStream stream = client.GetStream())
            {

                int length;
                // Read incomming stream into byte arrary. 						
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    DecodeMessage(client, (NetworkMessageType)bytes[0], bytes, length);
                }
            }
        }
        catch (Exception ex)
        {
            //this is terrible. I give no fucks.
        }

    }

    private void DecodeMessage(TcpClient client, NetworkMessageType messageType, byte[] bytes, int length)
    {

        var incomingData = new byte[length];
        Array.Copy(bytes, 1, incomingData, 0, length);
        string clientMessage = Encoding.ASCII.GetString(incomingData);


        var strings = clientMessage.Split(':');
        var token = strings[0];


        lock (messages)
        {
            messages.Enqueue(new NetworkMessage() { type = messageType, data = clientMessage, sender = client});
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

    private void UpdateClientWithOwnState(TcpClient client)
    {
        

        try
        {

            if (client.Connected)
            {
                foreach (TankController t in sim.tankControllers)
                {
                    if (t.Token == client.Client.RemoteEndPoint.ToString())
                    {
                        var obj = new GameObjectState() { Name = t.Name, Type = "Tank", Health = t.Health, Ammo = t.Ammo, X = t.X, Y = t.Y, Heading = t.Heading, TurretHeading = t.TurretHeading };
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
                var gameObjectsInView = sim.GetObjectsInViewOfTank(client.Client.RemoteEndPoint.ToString());

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

    private void HandleMessage(NetworkMessage newMessage)
    {
        try
        {
            Debug.Log(newMessage.type + " - " + (string)newMessage.data);

            string clientId = newMessage.sender.Client.RemoteEndPoint.ToString();

           

            switch (newMessage.type)
            {

                case (NetworkMessageType.test):

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

                case (NetworkMessageType.stop):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Stop, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.turretLeft):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurretLeft, Token = clientId, Payload = null });
                    break;
                case (NetworkMessageType.turretRight):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurretRight, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.left):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Left, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.right):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Right, Token = clientId, Payload = null });
                    break;

                case (NetworkMessageType.reverse):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Reverse, Token = clientId, Payload = null });
                    break;
                case (NetworkMessageType.forward):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Forward, Token = clientId, Payload = null });
                    break;


            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }


}

public static class MessageFactory
{

    public static byte[] CreateObjectUpdateMessage(string json)
    {

        byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(json);
        return AddByteStartOfToArray(clientMessageAsByteArray, 12);

    }

    public static byte[] AddByteStartOfToArray(byte[] bArray, byte newByte)
    {
        byte[] newArray = new byte[bArray.Length + 1];
        bArray.CopyTo(newArray, 1);
        newArray[0] = newByte;
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


public enum NetworkMessageType
{
    test = 0,
    createTank = 1,
    despawnTank = 2,
    fire = 3,
    forward = 4,
    reverse = 5,
    left = 6,
    right = 7,
    stop = 8,
    turretLeft = 9,
    turretRight = 10,
    stopTurret = 11,
    objectUpdate=12,

}