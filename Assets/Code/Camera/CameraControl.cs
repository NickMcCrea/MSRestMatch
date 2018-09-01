using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private float m_DampTime = 0.2f;

    public List<Transform> m_Targets;


    private Camera m_Camera;

    private Vector3 m_MoveVelocity;
    private Vector3 m_DesiredPosition;
    private Vector3 offset;
    private float minZoom = 60f;
    private float maxZoom = 10f;
    private float zoomLimiter = 60f; //bigger this is, the more we'll end up zoomed out
    private float offsetDistance = 30f;
    private float rotationSpeed = 10f;
    private float currentGreatestDistance;
    private float minDistance = 30f;

    private void Awake()
    {
        m_Camera = GetComponentInChildren<Camera>();
        offset = new Vector3(-offsetDistance, 0, -offsetDistance);
        Move();
        Zoom();
    }


    private void FixedUpdate()
    {
        Move();
        Zoom();
    }

    private void Zoom()
    {

        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        m_Camera.fieldOfView = newZoom;
    }

    private float GetGreatestDistance()
    {
        float maxDist = 0;
        foreach (Transform t in m_Targets)
        {
            foreach (Transform u in m_Targets)
            {
                if (t == u)
                    continue;

                if ((t.position - u.position).magnitude > maxDist)
                    maxDist = (t.position - u.position).magnitude;
            }
        }
        currentGreatestDistance = maxDist;

        if (currentGreatestDistance < minDistance)
            currentGreatestDistance = minDistance;

        return currentGreatestDistance;
    }

    private void Move()
    {
      
        FindAveragePosition();

        if (m_Targets.Count == 0)
            return;

        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);


    }

    private void FindAveragePosition()
    {
      

        Vector3 averagePos = new Vector3();


        m_Targets.Clear();

        var tanks = GameObject.FindObjectsOfType<TankController>();

        if (tanks.Length == 0)
            return;

        foreach (TankController t in tanks)
            m_Targets.Add(t.transform);

        var bounds = new Bounds(m_Targets[0].position, Vector3.zero);

        foreach (Transform t in m_Targets)
        {
            bounds.Encapsulate(t.position);
        }

        var targetRotation = Quaternion.LookRotation(bounds.center - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        averagePos = bounds.center;
        averagePos.y = transform.position.y;
        m_DesiredPosition = averagePos + new Vector3(currentGreatestDistance / 1.5f, 0, currentGreatestDistance/1.5f);
        m_DesiredPosition.y = currentGreatestDistance;
    }


}