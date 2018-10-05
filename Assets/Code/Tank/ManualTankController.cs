using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTankController : TankController {

	// Use this for initialization
	void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {

        base.Update();
        if (Input.GetKey(KeyCode.W))
        {
            Forward();
        }
        if (Input.GetKey(KeyCode.S))
        {
            Reverse();
        }
        if (Input.GetKey(KeyCode.D))
        {
            TurnRight();
        }
        if (Input.GetKey(KeyCode.A))
        {
            TurnLeft();
        }
        if (Input.GetKey(KeyCode.Q))
        {
            TurretLeft();
        }
        if (Input.GetKey(KeyCode.E))
        {
            TurretRight();
        }
        if (Input.GetKey(KeyCode.F))
        {
            Fire();
        }
      
    }
}
