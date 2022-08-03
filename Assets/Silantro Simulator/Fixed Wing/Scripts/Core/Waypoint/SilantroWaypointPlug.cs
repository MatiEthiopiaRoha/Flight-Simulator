using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SilantroWaypointPlug
{
    // ------------------------------------Connection
    public SilantroWaypointCircuit track;
    public Transform target;
    public SilantroController aircraft;
    public SilantroWaypointCircuit.RoutePoint progressPoint { get; private set; }


    // ------------------------------------Variables
    public float turnOffset = 50f;
    public float turnFactor = 0.1f;
    public float speedOffset = 20f;
    public float speedFactor = 0.5f;
    public float pointOffset = 15f;

    private float progressDistance;
    public int currentPoint;
    public float currentSpeed;
    private Vector3 lastPosition;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializePlug()
    {
        target = new GameObject(aircraft.name + " Waypoint Target").transform;
        progressDistance = 0;
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void DrawTrackingBeacon()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(aircraft.transform.position, target.position);
            //Gizmos.DrawWireSphere(track.GetRoutePosition(progressDistance), 1);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(target.position, target.position + target.forward);
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateTrack()
    {
        // ----------------------------------------------------------------------- Point to Point Tracking
        if (track.waypointType == SilantroWaypointCircuit.WaypointType.SinglePath)
        {
            Vector3 targetDelta = target.position - aircraft.transform.position;
            if (targetDelta.magnitude < pointOffset) { currentPoint = (currentPoint + 1) % track.pathPoints.Count; }


            target.position = track.pathPoints[currentPoint];
            progressPoint = track.GetRoutePoint(progressDistance);
            Vector3 progressDelta = progressPoint.position - aircraft.transform.position;
            if (Vector3.Dot(progressDelta, progressPoint.direction) < 0) { progressDistance += progressDelta.magnitude; }
            lastPosition = aircraft.transform.position;
        }

        // ----------------------------------------------------------------------- Route Tracking
        if (track.waypointType == SilantroWaypointCircuit.WaypointType.Circuit)
        {
            if (Time.deltaTime > 0) { currentSpeed = Mathf.Lerp(currentSpeed, (lastPosition - aircraft.transform.position).magnitude / Time.deltaTime, Time.deltaTime); }
            target.position = track.GetRoutePoint(progressDistance + turnOffset + turnFactor * currentSpeed).position;
            target.rotation = Quaternion.LookRotation(track.GetRoutePoint(progressDistance + speedOffset + speedFactor * currentSpeed).direction);


            progressPoint = track.GetRoutePoint(progressDistance);
            Vector3 progressDelta = progressPoint.position - aircraft.transform.position;
            if (Vector3.Dot(progressDelta, progressPoint.direction) < 0) { progressDistance += progressDelta.magnitude * 0.5f; }
            lastPosition = aircraft.transform.position;
        }
    }
}
