//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



public class SilantroPilot : MonoBehaviour
{

	// ------------------------------------------------------------- Variables
	public float maxRayDistance = 2f;
	public Transform head;

	// ------------------------------------------------------------- Selections
	public enum ControlType { ThirdPerson, FirstPerson }
	public ControlType controlType = ControlType.ThirdPerson;
	public bool isClose = false;//Is the Player Close to an aircraft
	public bool canEnter = false;
	SilantroController controller;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENTER 
	public void SendEntryData()
	{
		if (isClose && canEnter)
		{
			//PLAYER INFO
			if (controller != null)
			{
				controller.player = this.gameObject;
				if (controlType == ControlType.FirstPerson)
				{
					controller.playerType = SilantroController.PlayerType.FirstPerson;
				}
				if (controlType == ControlType.ThirdPerson)
				{
					controller.playerType = SilantroController.PlayerType.ThirdPerson;
				}
				//SEND ACCEPT
				controller.EnterAircraft();
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		if (controlType == ControlType.FirstPerson)
		{
			GameObject mainCameraObject = Camera.main.gameObject;
			if (mainCameraObject != null)
			{
				mainCameraObject.SetActive(true);
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DISPLAY ENTERY INFORMATION
	void OnGUI()
	{
		if (isClose && canEnter)
		{
			GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 100), "Press F to Enter");
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAW EYE SIGHT
	void OnDrawGizmos()
	{
		if (head != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(head.position, transform.forward * maxRayDistance);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CHECK AIRCRAFT DISTANCE
	void Update()
	{
		//SEND CHECK DATA
		CheckAircraftState();
		//ENTER
		if (Input.GetKeyDown (KeyCode.F)) {SendEntryData ();}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CheckAircraftState()
	{
		Vector3 direction = transform.TransformDirection(Vector3.forward);
		RaycastHit aircraft;

		if (Physics.Raycast(head.position, direction, out aircraft, maxRayDistance))
		{
			//COLLECT AIRCRAFT CONTROLLER
			controller = aircraft.transform.gameObject.GetComponent<SilantroController>();

			//PROCESS IF CONTROLLER IS AVAILABLE
			if (controller != null) { if (!controller.pilotOnboard) { isClose = true; } canEnter = true; }
			else { isClose = false; canEnter = false; }
		}

		else { isClose = false; canEnter = false; }
	}
}











#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPilot))]
public class SilantroPilotEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		SilantroPilot pilot = (SilantroPilot)target;
		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Player Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("controlType"), new GUIContent(" "));
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("head"), new GUIContent("Head"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRayDistance"), new GUIContent("Sight Distance"));


		serializedObject.ApplyModifiedProperties();
	}
}
#endif