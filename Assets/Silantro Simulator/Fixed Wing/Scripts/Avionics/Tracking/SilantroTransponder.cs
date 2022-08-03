using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroTransponder : MonoBehaviour
{
	// --------------------------------------------PROPERTIES--------------------------------------------------------------------------------------------------------------
	public enum SilantroTag
	{
		Aircraft, Truck, Airport, Missile, Undefined, SAMBattery, Tank//Add more if you wish
	}
	[HideInInspector] public SilantroTag silantroTag = SilantroTag.Undefined;
	[HideInInspector] public Texture2D silantroTexture;
	[HideInInspector] public string AssignedID = "Default";[HideInInspector] public string TrackingID = "Default";
	[HideInInspector] public bool isTracked;[HideInInspector] public bool isLockedOn;
	[HideInInspector] public float radarSignature = 1f;
}










#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroTransponder))]
public class SilantroTransponderEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroTransponder mark;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable() { mark = (SilantroTransponder)target; }



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector();
		serializedObject.Update();

		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Definition", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("silantroTag"), new GUIContent(" "));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("radarSignature"), new GUIContent("RCS"));
		GUILayout.Space(3f);
		serializedObject.FindProperty("silantroTexture").objectReferenceValue = EditorGUILayout.ObjectField("Display Icon", serializedObject.FindProperty("silantroTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;


		GUILayout.Space(5f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Sensor Data", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Is Tracked", mark.isTracked.ToString());
		if (mark.isTracked)
		{
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Target ID", mark.AssignedID.ToString());
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Tracker ID", mark.TrackingID.ToString());
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Is Locked", mark.isLockedOn.ToString());
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif