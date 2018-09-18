using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using UnityEngine;

public class TrainingRoomMain : MonoBehaviour
{


    //WebServiceHost host;
    GameSimulation simulation;
    TCPServer server;
    // Use this for initialization
    void Start()
    {
        var ruleSet = new GameSimRules();
        ruleSet.TrainingMode = true;
        simulation = new GameSimulation(ruleSet);

        //var service = new Service(simulation);
        //host = new WebServiceHost(service, new Uri("http://localhost:8000/"));
        //var behaviour = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
        //behaviour.InstanceContextMode = InstanceContextMode.Single;
        //var ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
        //host.Open();


        simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank1", new Vector3(0, 0, 30), true, true);
        simulation.CreateDummyTank(GenerateRandomHexColorString(), "DummyTank2", new Vector3(0, 0, -30), true, true);

        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank1", new Vector3(0, 5, 0), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank2", new Vector3(30, 5, -30), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank3", new Vector3(30, 5, 0), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank4", new Vector3(0, 5, -30), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank5", new Vector3(30, 5, 30), false, true);
        //simulation.CreateAITank(GenerateRandomHexColorString(), "AITank6", new Vector3(-30, 5, -30), false, true);

        server = new TCPServer(simulation);
    }

    private void OnApplicationQuit()
    {
        server.Close();
       
    }

    private static string GenerateRandomHexColorString()
    {
        return "#" + ColorUtility.ToHtmlStringRGB(UnityEngine.Random.ColorHSV());
    }

    // Update is called once per frame
    void Update()
    {


        simulation.Update();
        server.Update();
    }


}
