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
    private Queue<NetworkMessage> messages;
    private GameSimulation sim;

    public TCPServer(GameSimulation simulation)
    {
        sim = simulation;
        messages = new Queue<NetworkMessage>();
        connectedClients = new List<TcpClient>();
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
                DecodeMessage((NetworkMessageType)bytes[0], bytes, length);
            }
        }

    }

    private void DecodeMessage(NetworkMessageType messageType, byte[] bytes, int length)
    {

        var incomingData = new byte[length];
        Array.Copy(bytes, 1, incomingData, 0, length);
        string clientMessage = Encoding.ASCII.GetString(incomingData);

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
        Debug.Log(newMessage.type + " - " + (string)newMessage.data);

        switch (newMessage.type)
        {

            case (NetworkMessageType.test):
              
                break;


            case (NetworkMessageType.createTank):

                var arguments = ((string)newMessage.data).Split(':');

                sim.enqueuedCommands.Enqueue(new GameCommand()
                {
                    Type = CommandType.PlayerCreate,
                    Payload = new PlayerCreate() { Name = arguments[0], Token = arguments[1], Color = arguments[2] },
                    Token = arguments[1]
                });

                break;


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
    fire,
    forward,
    reverse,
    left,
    right,
    stop,
    turretLeft,
    turretRight,
    stopTurret,

}