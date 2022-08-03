using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
///
/// 
/// Use:		based on the Unity Standard Assets Waypoint system
/// </summary>

public class SilantroWaypoint : MonoBehaviour
{
    public List<Transform> waypoints;
    public Transform currentWaypoint;
    public Transform nextWaypoint;
    public Transform previousPoint;

    public float totalDistance;
    public int pointState = 0;



    private void Start()
    {
        SetWaypoint(0);
    }



    private void OnDrawGizmos()
    {
        totalDistance = 0f;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 newPoint = waypoints[i].position;
            Vector3 lastPoint = waypoints[i + 1].position;
            float currentDistance = Vector3.Distance(newPoint, lastPoint);
            totalDistance += currentDistance;

            Debug.DrawLine(newPoint, lastPoint,Color.yellow);
            Gizmos.color = Color.red; 
            Gizmos.DrawSphere(newPoint, 2f);
        }
        Debug.DrawLine(waypoints[0].position, waypoints[waypoints.Count - 1].position, Color.yellow);
    }




    public void SetWaypoint(int state)
    {
        if(waypoints.Count < 2) { return; }
        else
        {
            currentWaypoint = waypoints[ValidatePoint(state)];
            int prevState = state - 1;
            int nextState = state + 1;
            previousPoint = waypoints[ValidatePoint(prevState)];
            nextWaypoint = waypoints[ValidatePoint(nextState)];
        }
    }



    public int ValidatePoint(int state)
    {
        int returnState = state;
        if (returnState < 0) { returnState = waypoints.Count - 1; }
        if (returnState > waypoints.Count - 1) { returnState = 0; }
        return returnState;
    }
}
 