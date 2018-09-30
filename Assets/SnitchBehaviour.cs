using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnitchBehaviour : MonoBehaviour
{
    private float speed = 10f;
    private float degreesPerSecond = 35.0f;
    private float amplitude = 0.2f;
    private float frequency = 2f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    // Use this for initialization
    void Start () {
        posOffset = transform.position;
    }
	
	// Update is called once per frame
	void Update () {

        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;


        

    }
}
