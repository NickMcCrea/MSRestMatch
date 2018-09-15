using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;



public class TCPServer
{

    private TcpListener tcpListener;
    private Thread networkThread;
    private readonly string ipAddress = "127.0.0.1";
    private readonly int port = 8052;
    public volatile bool listening = true;
    private List<TcpClient> connectedClients;
    private Dictionary<string, TcpClient> tokenToClientMap;
    private Queue<NetworkMessage> messages;
    private GameSimulation sim;

    public TCPServer(GameSimulation simulation)
    {
        sim = simulation;
        messages = new Queue<NetworkMessage>();
        connectedClients = new List<TcpClient>();
        tokenToClientMap = new Dictionary<string, TcpClient>();
        networkThread = new Thread(new ThreadStart(StartServer));

        networkThread.Start();
    }

    private void StartServer()
    {
        tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        tcpListener.Start();


        while (listening)
        {
            var client = tcpListener.AcceptTcpClient();
            ThreadPool.QueueUserWorkItem(NewClientConnection, client);
            Thread.Sleep(16);
        }
    }

    public void Close()
    {

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

        //map a client to a token. Allows us to identify who to send updates to.
        if (messageType == NetworkMessageType.createTank)
            tokenToClientMap.Add(token, client);

        //try mapping the client to a token, to allow us to link tanks to TCP connections
        //if (tokenToClientMap.ContainsKey(token))
        //    tokenToClientMap[token] = client;

        lock (messages)
        {
            messages.Enqueue(new NetworkMessage() { type = messageType, data = clientMessage });
        }

    }


    public void Update()
    {
        if (messages.Count > 0)
        {
            var newMessage = messages.Dequeue();
            HandleMessage(newMessage);
        }
    }

    private void HandleMessage(NetworkMessage newMessage)
    {
        try
        {
            Debug.Log(newMessage.type + " - " + (string)newMessage.data);

            var arguments = ((string)newMessage.data).Split(':');

            switch (newMessage.type)
            {

                case (NetworkMessageType.test):

                    break;


                case (NetworkMessageType.createTank):

                    sim.enqueuedCommands.Enqueue(new GameCommand()
                    {
                        Type = CommandType.PlayerCreate,
                        Payload = new PlayerCreate() { Name = arguments[1], Token = arguments[0], Color = arguments[2] },
                        Token = arguments[1]
                    });

                    break;

                case (NetworkMessageType.despawnTank):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Despawn, Token = arguments[0], Payload = null });
                    break;

                case (NetworkMessageType.fire):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Fire, Token = arguments[0], Payload = null });
                    break;
                case (NetworkMessageType.stopTurret):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.StopTurret, Token = arguments[0], Payload = null });
                    break;

                case (NetworkMessageType.stop):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Stop, Token = arguments[0], Payload = null });
                    break;

                case (NetworkMessageType.turretLeft):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurretLeft, Token = arguments[0], Payload = null });
                    break;
                case (NetworkMessageType.turretRight):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurretRight, Token = arguments[0], Payload = null });
                    break;

                case (NetworkMessageType.left):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Left, Token = arguments[0], Payload = null });
                    break;

                case (NetworkMessageType.right):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Right, Token = arguments[0], Payload = null });
                    break;

                case (NetworkMessageType.reverse):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Reverse, Token = arguments[0], Payload = null });
                    break;
                case (NetworkMessageType.forward):
                    sim.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Forward, Token = arguments[0], Payload = null });
                    break;


            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }


    }


}


public struct NetworkMessage
{
    public NetworkMessageType type;
    public object data;
}


public enum NetworkMessageType
{
    test = 0,
    createTank = 1,
    despawnTank = 2,
    fire=3,
    forward=4,
    reverse=5,
    left=6,
    right=7,
    stop=8,
    turretLeft=9,
    turretRight=10,
    stopTurret=11,

}