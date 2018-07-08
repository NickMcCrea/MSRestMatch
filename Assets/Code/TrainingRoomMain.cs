using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using UnityEngine;

public class TrainingRoomMain : MonoBehaviour
{


    WebServiceHost host;
    GameSimulation simulation;
    // Use this for initialization
    void Start()
    {
        var ruleSet = new GameSimRules();
        ruleSet.TrainingMode = true;
        simulation = new GameSimulation(ruleSet);

        var service = new Service(simulation);
        host = new WebServiceHost(service, new Uri("http://localhost:8000/"));
        var behaviour = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
        behaviour.InstanceContextMode = InstanceContextMode.Single;
        var ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
        host.Open();

    }

    // Update is called once per frame
    void Update()
    {


        simulation.Update();

    }

    
}
