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

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/forward/", ResponseFormat = WebMessageFormat.Json)]
    void Forward(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/reverse/", ResponseFormat = WebMessageFormat.Json)]
    void Reverse(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/left/", ResponseFormat = WebMessageFormat.Json)]
    void Left(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/right/", ResponseFormat = WebMessageFormat.Json)]
    void Right(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/turretleft/", ResponseFormat = WebMessageFormat.Json)]
    void TurretLeft(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/turretright/", ResponseFormat = WebMessageFormat.Json)]
    void TurretRight(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/stop/", ResponseFormat = WebMessageFormat.Json)]
    void Stop(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/stopturret/", ResponseFormat = WebMessageFormat.Json)]
    void StopTurret(string name);

    [OperationContract, WebInvoke(UriTemplate = "/tank/{name}/fire/", ResponseFormat = WebMessageFormat.Json)]
    void Fire(string name);


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

    public void Forward(string name)
    {
        Debug.Log("Player forward request: " + name);

        lock (simulation.enqueuedCommands)
        {
            simulation.enqueuedCommands.Enqueue(new GameCommand() { Type = CommandType.Forward, Name =  name, Payload = null });
        }
    }
    public void Reverse(string name)
    {

    }

    public void Right(string name)
    {

    }

    public void Left(string name)
    {

    }

    public void TurretRight(string name)
    {

    }
    public void TurretLeft(string name)
    {

    }
    public void Stop(string name)
    {

    }
    public void StopTurret(string name)
    {

    }
    public void Fire(string name)
    {

    }


}


public class PlayerCreate
{
    public string Name { get; set; }
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
