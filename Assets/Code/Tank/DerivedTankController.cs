using UnityEngine;
using System.Collections;

public class DerivedTankController : TankController
{


    public override void Update()
    {
        Forward();
        

        base.Update();
    }

    

}
