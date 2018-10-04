using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StadiumCam : MonoBehaviour
{

    private CameraMode currentCameraMode = CameraMode.centerCircle;
    private GameObject currentTarget;
    private Vector3 targetPoint;

    private float rotationSpeed = 0.6f;
    private float zoomDistance = 0.1f;
    private float zoomChange;
    private Vector3 desiredPosition;

    public enum CameraMode
    {
        targetTrack,
        centerCircle,
        leaderboard
    }

    // Use this for initialization
    void Start()
    {

      
        targetPoint = Vector3.zero;

        zoomChange = 0;
        desiredPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (currentCameraMode == CameraMode.centerCircle)
            targetPoint = Vector3.zero;

        if (currentCameraMode == CameraMode.targetTrack)
        {
            if (currentTarget != null)
                targetPoint = currentTarget.transform.position;
        }
        if (currentCameraMode == CameraMode.leaderboard)
        {
            if (currentTarget != null)
                targetPoint = currentTarget.transform.position;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            LookAtLeaderBoard();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
            Left();

        if (Input.GetKey(KeyCode.RightArrow))
            Right();


        if (Input.GetKey(KeyCode.UpArrow))
            ZoomIn();

        if (Input.GetKey(KeyCode.DownArrow))
            ZoomOut();


        if (Input.GetKey(KeyCode.RightShift))
            Up();

        if (Input.GetKey(KeyCode.RightControl))
            Down();

        Vector3 toTarget = targetPoint - transform.position;
        toTarget.Normalize();
        desiredPosition = transform.position + toTarget * zoomChange;

        Vector3 newPos = Vector3.Lerp(transform.position, desiredPosition, 0.9f);
        transform.position = newPos;
        zoomChange *= 0.9f;

        Vector3 lTargetDir = targetPoint - transform.position;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lTargetDir), Time.time * 0.1f);

    }

    public void SetTargetFollowMode(GameObject newTarget)
    {
        currentCameraMode = CameraMode.targetTrack;
        currentTarget = newTarget;
        Debug.Log("Cam: Follow mode");

    }

    public void SetCenterCircleMode()
    {
        currentCameraMode = CameraMode.centerCircle;
        Debug.Log("Cam: Circle mode");

    }

    public void LookAtLeaderBoard()
    {
        targetPoint = GameObject.Find("ScoreboardCanvas").transform.position;
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
        transform.RotateAround(targetPoint, transform.right, rotationSpeed);
    }

    public void Down()
    {
        transform.RotateAround(targetPoint, transform.right, -rotationSpeed);
    }

    public void ZoomIn()
    {
        zoomChange += zoomDistance;

    }

    public void ZoomOut()
    {
        zoomChange -= zoomDistance;
    }

    public void ZoomIn(float amount)
    {
        zoomChange += amount;
    }

    public void ZoomOut(float amount)
    {
        zoomChange -= amount;
    }

    internal void SetLeaderBoardMode()
    {
        currentTarget = GameObject.Find("ScoreboardCanvas");
        currentCameraMode = CameraMode.leaderboard;
        Debug.Log("Cam: Leaderboard mode");
    }
}
