using UnityEngine;

public class AITankController : TankController
{


    private float firingThreshold = 40f;
    private float distanceThreshold = 30f;
 

    // Use this for initialization
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {

        //Should do the following:
        //1. Identify the closest target.
        //2. Swivel the barrel to keep aim at it.
        //3. Move within range.
        //4. Fire
        if (Sim != null && currentState != TankState.destroyed)
        {
            TankController closest = null;
            float distance = float.MaxValue;
            FindClosest(ref closest, ref distance);

            if (closest != null)
            {
                if (closest.currentState != TankState.destroyed)
                {
                    KeepTurretAimedAtTarget(closest);
                    GetInRange(closest);
                    
                }
            }

        }



        base.Update();
    }

    private void FireIfInRange(TankController closest)
    {
        if ((closest.transform.position - transform.position).magnitude < firingThreshold)
            Fire();
    }

    private void GetInRange(TankController closest)
    {
        Vector3 vecToTarget = closest.transform.position - transform.position;
        float dotBetweenForwardAndTarget = Vector3.Dot(vecToTarget, transform.right);

        if (dotBetweenForwardAndTarget > 0.1f)
            TurnRight();
        else if (dotBetweenForwardAndTarget < -0.1f)
            TurnLeft();
        else
        {
            if (vecToTarget.magnitude > distanceThreshold)
                Forward();
        }

      

    }

    private void KeepTurretAimedAtTarget(TankController closest)
    {

        Vector3 vecToTarget = closest.transform.position - transform.position;
        float dotBetweenForwardAndTarget = Vector3.Dot(vecToTarget, -turret.transform.right);

        if (dotBetweenForwardAndTarget > 0.1f)
            TurretRight();
        else if (dotBetweenForwardAndTarget < -0.1f)
            TurretLeft();

        if (dotBetweenForwardAndTarget < 0.1 && dotBetweenForwardAndTarget > -0.1f)
            FireIfInRange(closest);

    }

    private void FindClosest(ref TankController closest, ref float distance)
    {
        foreach (TankController t in Sim.tankControllers)
        {
            if (t == this)
                continue;

            if (t.currentState == TankState.destroyed)
                continue;

            float distToTarget = (t.transform.position - transform.position).magnitude;
            if (distToTarget < distance)
            {
                distance = distToTarget;
                closest = t;
            }

        }
    }
}
