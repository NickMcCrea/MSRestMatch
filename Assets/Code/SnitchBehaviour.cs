using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnitchBehaviour : MonoBehaviour
{
    private float speed = 10f;
    private float amplitude = 0.03f;
    private float frequency = 2f;
    public TankController collector;
    private float wanderCircleDistance = 20f;
    private float wanderCircleRadius = 3f;



    private enum SnitchMode
    {
        wander,
        collected
    }

    private SnitchMode currentMode = SnitchMode.wander;
    private Vector3 tempPos = new Vector3();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        // Float up/down with a Sin()
        tempPos = transform.position;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.position = tempPos;

        if (currentMode == SnitchMode.wander)
        {
            Wander();

        }
        if (currentMode == SnitchMode.collected)
        {
            if (collector != null && collector.currentState != TankController.TankState.destroyed)
            {
                transform.position = collector.transform.position + new Vector3(0, 5, 0);
               
            }
            else
            {
                currentMode = SnitchMode.wander;
                collector = null;
            }
        }


    }

    private void Wander()
    {
        Vector3 wanderCircleCenter = transform.position + transform.forward * wanderCircleDistance;
        var unitCirclePoint = Random.insideUnitCircle;
        Vector3 targetPoint = wanderCircleCenter + new Vector3(unitCirclePoint.x * wanderCircleRadius, 0, unitCirclePoint.y * wanderCircleRadius);


        Vector3 targetForward = targetPoint - transform.position;

        //bias the wandering towards the center circle
        Vector3 centerBias = -transform.position * 0.01f;
        targetForward += centerBias;



        //avoid any tanks
        var tanks = TrainingRoomMain.simulation.tankControllers;
        foreach (TankController t in tanks)
        {
            Vector3 awayFromTank = transform.position - t.transform.position;
            if (awayFromTank.magnitude < 10f)
            {
                awayFromTank = awayFromTank * (0.1f);
                targetForward += awayFromTank;
            }
        }



        //rotate us to our desired heading.
        targetForward.Normalize();
        targetForward.y = 0;
        transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<TankController>() != null)
        {
            currentMode = SnitchMode.collected;
            collector = other.gameObject.GetComponent<TankController>();
            EventManager.snitchPickupEvent.Invoke(collector);

        }


    }

}
