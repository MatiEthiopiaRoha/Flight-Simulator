using Oyedoyin;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SilantroWaypointCircuit : MonoBehaviour
{
    // ------------------------------------Selectibles
    [Serializable]
    public class WaypointList
    {
        public SilantroWaypointCircuit circuit;
        public List<Transform> items = new List<Transform>(0);
    }
    public struct RoutePoint
    {
        public Vector3 position; public Vector3 direction;
        public RoutePoint(Vector3 position, Vector3 direction)
        { this.position = position; this.direction = direction; }
    }
    public List<Transform> Waypoints { get { return waypointList.items; } }
    public enum WaypointType { Circuit, SinglePath }
    public WaypointType waypointType = WaypointType.SinglePath;


    // ------------------------------------Variables
    public WaypointList waypointList = new WaypointList();
    public Vector3[] points;
    public float[] distances;
    public int pointCount;
    private int segments;
    public List<Vector3> pathPoints;
    public List<Vector3> basePoints;
    public float Length { get; private set; }
    [Range(5, 200)] public int resolution = 50;
    public bool smoothRoute = true;
    public float totalDistance;
    GameObject point;

    // ------------------------------------Cache Points
    private int point1;
    private int point2;
    private int point3;
    private int point4;


    private float i;
    private Vector3 P1;
    private Vector3 P2;
    private Vector3 P3;
    private Vector3 P4;






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SetChildWaypoints()
    {
        waypointList.items = new List<Transform>();
        Transform[] childObjects = GetComponentsInChildren<Transform>();

        foreach (Transform child in childObjects)
        {
            if (child != transform && child != null) { Waypoints.Add(child); }
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void RenameWaypoints()
    {
        int n = 0;
        foreach (Transform child in Waypoints)
        {
            if (child != transform && child != null) { child.name = "Waypoint " + (n++).ToString("000"); }
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void AddWaypoint()
    {
        if (waypointList.items != null) { point = new GameObject("Waypoint " + (waypointList.items.Count).ToString("000")); }
        point.transform.parent = this.transform;
        if (waypointList.items != null && waypointList.items.Count > 1)
        {
            Vector3 predisessorPosition = waypointList.items[waypointList.items.Count - 1].localPosition;
            point.transform.localPosition = predisessorPosition + waypointList.items[waypointList.items.Count - 1].forward * 100f;
        }
        waypointList.items.Add(point.transform);
    }






































    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (Waypoints.Count > 1) { CachePositionsAndDistances(); }
        pointCount = Waypoints.Count;
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnDrawGizmos() { DrawCircuit(false); }
    private void OnDrawGizmosSelected() { DrawCircuit(true); }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public RoutePoint GetRoutePoint(float dist)
    {
        Vector3 p1 = GetRoutePosition(dist);
        Vector3 p2 = GetRoutePosition(dist + 0.1f);
        Vector3 delta = p2 - p1;
        return new RoutePoint(p1, delta.normalized);
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void CachePositionsAndDistances()
    {

        if (waypointType == WaypointType.Circuit)
        {
            points = new Vector3[Waypoints.Count + 1];
            distances = new float[Waypoints.Count + 1];

            float accumulateDistance = 0;
            for (int i = 0; i < points.Length; ++i)
            {
                var t1 = Waypoints[(i) % Waypoints.Count]; var t2 = Waypoints[(i + 1) % Waypoints.Count];
                if (t1 != null && t2 != null)
                {
                    Vector3 p1 = t1.position; Vector3 p2 = t2.position;
                    points[i] = Waypoints[i % Waypoints.Count].position;
                    distances[i] = accumulateDistance;
                    accumulateDistance += (p1 - p2).magnitude;
                }
            }
        }


        if (waypointType == WaypointType.SinglePath)
        {
            points = new Vector3[pathPoints.Count + 1];
            distances = new float[pathPoints.Count + 1];

            float accumulateDistance = 0;
            for (int i = 0; i < points.Length; ++i)
            {
                var t1 = pathPoints[(i) % pathPoints.Count]; var t2 = pathPoints[(i + 1) % pathPoints.Count];
                if (t1 != null && t2 != null)
                {
                    Vector3 p1 = t1; Vector3 p2 = t2;
                    points[i] = pathPoints[i % pathPoints.Count];
                    distances[i] = accumulateDistance;
                    accumulateDistance += (p1 - p2).magnitude;
                }
            }
        }

        basePoints = new List<Vector3>();
        foreach(Transform point in waypointList.items)
        {
            basePoints.Add(point.position);
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public Vector3 GetRoutePosition(float dist)
    {
        int point = 0;
        if (Length == 0) { Length = distances[distances.Length - 1]; }

        dist = Mathf.Repeat(dist, Length);
        while (distances[point] < dist) { ++point; }

        point2 = ((point - 1) + pointCount) % pointCount; point3 = point;
        i = Mathf.InverseLerp(distances[point2], distances[point3], dist);

        if (smoothRoute)
        {
            if (waypointType == WaypointType.Circuit)
            {
                point1 = ((point - 2) + pointCount) % pointCount;
                point4 = (point + 1) % pointCount;
                point3 %= pointCount;

                P1 = points[point1];
                P2 = points[point2];
                P3 = points[point3];
                P4 = points[point4];

                return MathBase.CatmullRom(P1, P2, P3, P4, i);
            }
            else
            {
                point2 = ((point - 1) + pointCount) % pointCount;
                point3 = point;
                return Vector3.Lerp(points[point2], points[point3], i);
            }
        }
        else
        {
            point2 = ((point - 1) + pointCount) % pointCount;
            point3 = point;
            return Vector3.Lerp(points[point2], points[point3], i);
        }
    }




    


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void CreateCurve(List<Vector3> controlPoints)
    {
        segments = controlPoints.Count / 3;
        pathPoints = new List<Vector3>();

        for (int s = 0; s < controlPoints.Count - 3; s += 3)
        {
            Vector3 p0 = controlPoints[s];
            Vector3 p1 = controlPoints[s + 1];
            Vector3 p2 = controlPoints[s + 2];
            Vector3 p3 = controlPoints[s + 3];

            if (s == 0)
            {
                pathPoints.Add(MathBase.BezierPathCalculation(p0, p1, p2, p3, 0.0f));
            }

            for (int p = 0; p < (resolution / segments); p++)
            {
                float t = (1.0f / (resolution / segments)) * p;
                Vector3 point = new Vector3();
                point = MathBase.BezierPathCalculation(p0, p1, p2, p3, t);
                pathPoints.Add(point);
            }
        }
    }







    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void DrawCircuit(bool selected)
    {
#if UNITY_EDITOR
        // ----------------------------------------------- Clamp
        if (resolution < 10) { resolution = 10; }
        waypointList.circuit = this;
        CachePositionsAndDistances();
        Length = distances[distances.Length - 1];
        Gizmos.color = Color.red;

        Handles.color = Color.green; Handles.ArrowHandleCap(0, waypointList.items[0].position, transform.rotation * Quaternion.LookRotation(Vector3.up), 10f, EventType.Repaint);
        Vector3 endPoint = Vector3.zero;

        // ----------------------------------------------- Draw Path
        if (waypointType == WaypointType.SinglePath)
        {
            totalDistance = 0f;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                if (pathPoints[i] != null)
                {
                    Vector3 newPoint = pathPoints[i];
                    Vector3 lastPoint = pathPoints[i + 1];
                    float currentDistance = Vector3.Distance(newPoint, lastPoint);
                    totalDistance += currentDistance;
                }
            }


            if (smoothRoute)
            {
                CreateCurve(basePoints);
                for (int i = 0; i < pathPoints.Count - 1; i++)
                {
                    if (pathPoints[i] != null)
                    {
                        Vector3 newPoint = pathPoints[i];
                        Vector3 lastPoint = pathPoints[i + 1];
                        Gizmos.DrawLine(newPoint, lastPoint);
                    }
                }
                endPoint = pathPoints[pathPoints.Count - 1];
            }
            else
            {
                for (int i = 0; i < waypointList.items.Count - 1; i++)
                {
                    if (waypointList.items[i] != null)
                    {
                        Vector3 newPoint = waypointList.items[i].position;
                        Vector3 lastPoint = waypointList.items[i + 1].position;
                        Gizmos.DrawLine(newPoint, lastPoint);
                    }
                }

                endPoint = waypointList.items[waypointList.items.Count - 1].position;
            }
        }



        Handles.color = Color.red; Handles.ArrowHandleCap(0, endPoint, transform.rotation * Quaternion.LookRotation(Vector3.up), 15f, EventType.Repaint);


        // ----------------------------------------------- Draw Circuit
        if (waypointType == WaypointType.Circuit)
        {
            if (Waypoints.Count > 1)
            {
                pointCount = Waypoints.Count;
                totalDistance = 0f;

                for (int i = 0; i < Waypoints.Count - 1; i++)
                {
                    if (Waypoints[i] != null)
                    {
                        Vector3 newPoint = Waypoints[i].position;
                        Vector3 lastPoint = Waypoints[i + 1].position;
                        float currentDistance = Vector3.Distance(newPoint, lastPoint);
                        totalDistance += currentDistance;
                    }
                }

                Gizmos.color = selected ? Color.yellow : new Color(1, 1, 0, 0.5f);
                Vector3 prev = Waypoints[0].position;
                if (smoothRoute)
                {
                    for (float dist = 0; dist < Length; dist += Length / resolution)
                    {
                        Vector3 next = GetRoutePosition(dist + 1);
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                    Gizmos.DrawLine(prev, Waypoints[0].position);
                }
                else
                {
                    for (int n = 0; n < Waypoints.Count; ++n)
                    {
                        Vector3 next = Waypoints[(n + 1) % Waypoints.Count].position;
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }
            }
        }
#endif
    }
}




#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroWaypointCircuit))]
public class SilantroWaypointCircuitEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);
    SilantroWaypointCircuit circuit;
    SerializedProperty waypointList;

  
    //------------------------------------------------------------------------
    void OnEnable()
    {
        circuit = (SilantroWaypointCircuit)target;
        waypointList = serializedObject.FindProperty("waypointList").FindPropertyRelative("items");
    }

    
    
    //-------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector();
        serializedObject.Update();

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Waypoint Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("waypointType"), new GUIContent("Type"));
        GUILayout.Space(10f);

        for (int i = 0; i < waypointList.arraySize; i++)
        {
            SerializedProperty reference = waypointList.GetArrayElementAtIndex(i);

            Transform point = circuit.waypointList.items[i];
            if(point == null) { circuit.waypointList.items.Remove(point); }

            GUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(reference, new GUIContent(""));
            if (GUILayout.Button("Delete")) { 
                //waypointList.DeleteArrayElementAtIndex(i);
                if (point != null)
                {
                    circuit.waypointList.items.RemoveAt(i);
                    DestroyImmediate(point.gameObject);
                }
            }
            if (GUILayout.Button("^")) { if (i < waypointList.arraySize - 1) { waypointList.MoveArrayElement(i, i - 1); } }
            if (GUILayout.Button("v")) { if (i > 0) { waypointList.MoveArrayElement(i, i + 1); } }
            EditorGUILayout.EndHorizontal();
        }

        GUI.color = Color.yellow;
        GUILayout.Space(3f);
        if (GUILayout.Button("Create Waypoint"))
        {
            if(circuit.waypointType == SilantroWaypointCircuit.WaypointType.SinglePath) { for (int n = 0; n < 3; ++n) { circuit.AddWaypoint(); } }
            if(circuit.waypointType == SilantroWaypointCircuit.WaypointType.Circuit) { circuit.AddWaypoint(); }
        }
        GUI.color = backgroundColor;



        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Display", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothRoute"), new GUIContent("Smooth Circuit"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resolution"), new GUIContent("Resolution"));
        


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(20f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Commands", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        if (GUILayout.Button("Assign all child objects")) { circuit.SetChildWaypoints(); }

        GUILayout.Space(5f);
        if (GUILayout.Button("Rename & Arrange Waypoints")) { circuit.RenameWaypoints(); }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(20f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Circuit Data", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        if (circuit.totalDistance < 1000)
        {
            EditorGUILayout.LabelField("Length", circuit.totalDistance.ToString("0.00") + " m");
        }
        else
        {
            float lenght = circuit.totalDistance / 1000f;
            EditorGUILayout.LabelField("Length", lenght.ToString("0.00") + " km");
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
