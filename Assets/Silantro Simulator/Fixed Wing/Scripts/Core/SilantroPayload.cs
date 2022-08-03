using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class SilantroPayload : MonoBehaviour
{

	//-----GENERAL CLASS
	public enum PayloadType
	{
		Crew, Cargo, Equipment
		//ADD MORE
	}
	[HideInInspector] public PayloadType payloadType = PayloadType.Crew;

	//-----CREW CLASS
	public enum CrewType
	{
		Pilot, CoPilot, Passenger
	}
	[HideInInspector] public CrewType crewType = CrewType.Pilot;

	//----EQUIPMENT CLASS
	public enum EquipmentType
	{
		Tyre
		//ADD MORE
	}
	[HideInInspector] public EquipmentType equipmentType = EquipmentType.Tyre;

	//-----PROPERTIES
	[HideInInspector] public float weight;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnDrawGizmos()
	{
		//DRAW IDENTIFIER
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position, 0.1f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(this.transform.position, (this.transform.up * 2f + this.transform.position));
	}
}



#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroPayload))]
public class SilantroPayloadEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f, 0.40f, 0f);
	SilantroPayload payload;

	//------------------------------------------------------------------------
	private SerializedProperty payloadType;
	private SerializedProperty crewType;
	private SerializedProperty weight;
	private SerializedProperty equipmentType;


	private void OnEnable()
	{
		payload = (SilantroPayload)target;

		payloadType = serializedObject.FindProperty("payloadType");
		crewType = serializedObject.FindProperty("crewType");
		equipmentType = serializedObject.FindProperty("equipmentType");
		weight = serializedObject.FindProperty("weight");
	}

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Payload Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(payloadType);
		GUILayout.Space(3f);
		if (payload.payloadType == SilantroPayload.PayloadType.Crew)
		{
			EditorGUILayout.PropertyField(crewType);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(weight);
		}
		//2. EQUIPMENTS
		if (payload.payloadType == SilantroPayload.PayloadType.Equipment)
		{
			EditorGUILayout.PropertyField(equipmentType);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(weight);
		}
		//3. CAROG
		if (payload.payloadType == SilantroPayload.PayloadType.Cargo)
		{
			EditorGUILayout.PropertyField(weight);
		}


		serializedObject.ApplyModifiedProperties();
	}
}
#endif