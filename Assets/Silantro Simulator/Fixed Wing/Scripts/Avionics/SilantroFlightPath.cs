using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroFlightPath : MonoBehaviour {
	
	[HideInInspector]public List<Vector3> logPoints = new List<Vector3> ();
	[HideInInspector]public TextAsset asset;

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (logPoints.Count > 3) {
			for (int i = 0; i < logPoints.Count-1; i++) {
				Vector3 point1 = logPoints [i];
				Vector3 point2 = logPoints [i + 1];
				Handles.DrawLine (point1, point2);
				//
				Gizmos.color = Color.red;
				Gizmos.DrawSphere (point1, 5f);
			}
		}
	}
	#endif

	public void LoadPath()
	{
		logPoints = new List<Vector3> ();
		string[] dataPlots = asset.text.Split ('\n');
		for (int i = 1; (i < dataPlots.Length - 1); i++) {
			string[] staticfields = dataPlots [i].Split (',');
			float vx = (float.Parse (staticfields [0]));
			float vy = (float.Parse (staticfields [1]));
			float vz  =(float.Parse (staticfields [2]));
			//
			Vector3 logPosition = new Vector3(vx,vy,vz);
			logPoints.Add (logPosition);
		}
	}

	public void ClearPath()
	{
		logPoints = new List<Vector3> ();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SilantroFlightPath))]
public class SilantroPathEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.75f,0.016f,1f);
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();
		SilantroFlightPath box = (SilantroFlightPath)target;
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Data Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		box.asset = EditorGUILayout.ObjectField ("Flight Path", box.asset, typeof(TextAsset), true) as TextAsset;
		GUILayout.Space (5f);
		if (GUILayout.Button ("Load")) {
			box.LoadPath ();
		}

		if (box.logPoints != null && box.logPoints.Count > 3) {
			GUILayout.Space (10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Clear Plotted Path", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			if (GUILayout.Button ("Clear")) {
				box.ClearPath ();
			}
		}
	}
}
#endif