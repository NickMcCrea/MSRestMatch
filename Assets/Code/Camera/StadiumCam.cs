using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StadiumCam : MonoBehaviour {

    public CameraMode currentCameraMode;
    private GameObject currentTarget;
    private Vector3 targetPoint;
    private float rotationSpeed = 0.1f;
    private float zoomDistance = 0.1f;
    private float zoomChange;
    private Vector3 desiredPosition;

    public enum CameraMode
    {
        targetTrack,
        centerCircle
    }

	// Use this for initialization
	void Start () {

        currentCameraMode = CameraMode.centerCircle;
        targetPoint = Vector3.zero;

        zoomChange = 0;
        desiredPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        if (currentCameraMode == CameraMode.centerCircle)
            targetPoint = Vector3.zero;
        else
            targetPoint = currentTarget.transform.position;


        if (Input.GetKey(KeyCode.LeftArrow))
            Left();

        if (Input.GetKey(KeyCode.RightArrow))
            Right();


        if (Input.GetKey(KeyCode.UpArrow))
            ZoomIn();

        if (Input.GetKey(KeyCode.DownArrow))
            ZoomOut();


        Vector3 toTarget = targetPoint - transform.position;
        toTarget.Normalize();
        desiredPosition = transform.position + toTarget * zoomChange;
        Vector3 newPos = Vector3.Lerp(transform.position, desiredPosition, 0.9f);
        transform.position = newPos;
        zoomChange *= 0.9f;
        

    }

    public void SetTargetFollowMode(GameObject newTarget)
    {
        currentCameraMode = CameraMode.targetTrack;
        currentTarget = newTarget;
    }

    public void SetCenterCircleMode()
    {
        currentCameraMode = CameraMode.centerCircle;
    }


    public void Right()
    {
        transform.RotateAround(targetPoint, Vector3.up, -rotationSpeed);
    }

    public void Left()
    {
        transform.RotateAround(targetPoint, Vector3.up, rotationSpeed);
    }
    public void Right(float speed)
    {
        transform.RotateAround(targetPoint, Vector3.up, -speed);
    }

    public void Left(float speed)
    {
        transform.RotateAround(targetPoint, Vector3.up, speed);
    }
    public void Up()
    {
        
    }

    public void Down()
    {
        
    }

    public void ZoomIn()
    {
        zoomChange += zoomDistance;

    }

    public void ZoomOut()
    {
        zoomChange -= zoomDistance;
    }
}
