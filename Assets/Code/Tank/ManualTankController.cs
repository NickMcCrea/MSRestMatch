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
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Forward();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Reverse();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            TurnRight();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            TurnLeft();
        }
        if (Input.GetKey(KeyCode.Delete))
        {
            TurretLeft();
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            TurretRight();
        }
        if (Input.GetKey(KeyCode.Space))
        {
            Fire();
        }
      
    }
}
