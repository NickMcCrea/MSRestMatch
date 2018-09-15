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
    private Queue<string> messages;

    public TCPServer()
    {
        messages = new Queue<string>();
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
                var incomingData = new byte[length];
                Array.Copy(bytes, 0, incomingData, 0, length);
                // Convert byte array to string message. 							
                string clientMessage = Encoding.ASCII.GetString(incomingData);          
                lock (messages)
                {
                    messages.Enqueue(clientMessage);
                }
            }
        }

    }

    public void Update()
    {

        if (messages.Count > 0)
            Debug.Log(messages.Dequeue());


    }


}