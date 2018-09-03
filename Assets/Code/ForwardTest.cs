using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        Debug.Log(name +  " FORWARD: " + transform.forward + " RIGHT: " +transform.right + " UP: " + transform.up);
        transform.RotateAround(Vector3.up, 0.01f);
 

	}
}
