using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using UnityEngine;


[ServiceContract]
public interface IService
{

    [OperationContract, WebGet(UriTemplate = "/test/", ResponseFormat = WebMessageFormat.Json)]
    string TestRestService();

    [OperationContract, WebInvoke(UriTemplate = "/tank/create/", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    void CreatePlayer(PlayerCreate create);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/forward/", ResponseFormat = WebMessageFormat.Json)]
    void Forward(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/reverse/", ResponseFormat = WebMessageFormat.Json)]
    void Reverse(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/left/", ResponseFormat = WebMessageFormat.Json)]
    void Left(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/right/", ResponseFormat = WebMessageFormat.Json)]
    void Right(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/turretleft/", ResponseFormat = WebMessageFormat.Json)]
    void TurretLeft(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/turretright/", ResponseFormat = WebMessageFormat.Json)]
    void TurretRight(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/stop/", ResponseFormat = WebMessageFormat.Json)]
    void Stop(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/stopturret/", ResponseFormat = WebMessageFormat.Json)]
    void StopTurret(string token);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{token}/fire/", ResponseFormat = WebMessageFormat.Json)]
    void Fire(string token);


    [OperationContract, WebGet(UriTemplate = "/tank/{token}/state/", ResponseFormat = WebMessageFormat.Json)]
    TankJson GetTankState(string token);



}
class Service : IService
{

    GameSimulation simulation;

    public Service(GameSimulation simulation)
    {
        this.simulation = simulation;
    }

    public string TestRestService()
    {
        return "Test Successful";
    }

    public void CreatePlayer(PlayerCreate create)
    {
        Debug.Log("Player create request: " + create.Name);

        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.PlayerCreate, Payload = create });
        }

    }

    public void Forward(string token)
    {
        Debug.Log("Player forward request: " + token);

        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Forward, Token = token, Payload = null });
        }
    }

    public void Reverse(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Reverse, Token = token, Payload = null });
        }
    }

    public void Right(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Right, Token = token, Payload = null });
        }
    }

    public void Left(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Left, Token = token, Payload = null });
        }
    }

    public void TurretRight(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurretRight, Token = token, Payload = null });
        }
    }
    public void TurretLeft(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.TurretLeft, Token = token, Payload = null });
        }
    }
    public void Stop(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Stop, Token = token, Payload = null });
        }
    }
    public void StopTurret(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.StopTurret, Token = token, Payload = null });
        }
    }
    public void Fire(string token)
    {
        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Fire, Token = token, Payload = null });
        }
    }

    public TankJson GetTankState(string token)
    {
        lock (simulation.tankControllers)
        {
            foreach (TankController t in simulation.tankControllers)
            {
                if (t.Token == token)
                    return new TankJson() { Name = t.Name, Health = t.Health, Ammo = t.Ammo, X = t.X, Y = t.Y, Heading = t.Heading };
            }
            
        }
        return null;
    }


}


public class PlayerCreate
{
    public string Name { get; set; }
    public string Token { get; set; }
    public string Color { get; set; }
}

public class TankJson
{
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Heading { get; set; }
    public int Health { get; set; }
    public int Ammo { get; set; }
}
