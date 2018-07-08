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

    [OperationContract, WebInvoke(UriTemplate = "/player/create/", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    PlayerJson CreatePlayer(PlayerCreate create);

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

    public PlayerJson CreatePlayer(PlayerCreate create)
    {
        Debug.Log("Player create request: " + create.Name);
        Player p = simulation.CreatePlayer(create);
        return new PlayerJson() { Id = p.ID, Name = p.Name };
    }

}


public class PlayerCreate
{
    public string Name { get; set; }
}

public class PlayerJson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Heading { get; set; }
    public int Health { get; set; }
}
