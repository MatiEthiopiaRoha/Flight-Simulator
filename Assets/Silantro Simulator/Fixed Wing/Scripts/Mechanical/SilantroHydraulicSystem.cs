//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroHydraulicSystem : MonoBehaviour
{

	//------------------------SELECTIBLES
	public enum CurrentState { Open, Closed }
	[HideInInspector] public CurrentState currentState = CurrentState.Closed;

	public enum StartState { Open, Closed }
	[HideInInspector] public StartState startState = StartState.Open;
	[HideInInspector] public List<Door> components = new List<Door>();
	[System.Serializable]
	public class Door
	{
		public enum MovementType
		{
			Extension,
			Rotation
		}
		[HideInInspector] public MovementType movementType = MovementType.Rotation;

		[SerializeField] public string Identifier;
		public Transform doorElement;
		public float xAxis;
		public float yAxis;
		public float zAxis;
		[HideInInspector] public Quaternion initialPosition;
		[HideInInspector] public Quaternion finalPosition;

		[HideInInspector] public Vector3 initialPoint;
		[HideInInspector] public Vector3 finalPoint;

		public float DragCoefficient;
		public float speedFactor = 1f;
	}

	[HideInInspector] public float currentRotation;
	[HideInInspector] public bool open;
	bool activated;
	[HideInInspector] public bool started;
	[HideInInspector] public bool close;
	[HideInInspector] public float currentDragPercentage = 0;
	[HideInInspector] public bool generatesDragWhenOpened = false;
	[HideInInspector] public float dragAmount;
	[HideInInspector] public float currentDrag;
	[HideInInspector] public float openTime = 5f;
	[HideInInspector] public float closeTime = 5f;
	[HideInInspector] public float rotateSpeed = 30f;
	//
	AudioSource doorSound;
	[HideInInspector] public AudioClip openSound;
	[HideInInspector] public AudioClip closeSound;
	//
	[HideInInspector] public bool playSound = true;
	[HideInInspector] public int toolbarTab;
	[HideInInspector] public string currentTab;
	[HideInInspector] public bool isControllable = true;


	// ---------------------------------------------------CONTROLS-------------------------------------------------------------------------------------------------------
	//OPEN
	public void EngageActuator()
	{ open = true; }
	//CLOSE
	public void DisengageActuator()
	{ close = true; }

	private void Start()
	{
		InitializeActuator();
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//INITIALIZE
	public void InitializeActuator()
	{
		//SETUP SOUND
		if (openSound != null && closeSound != null)
		{
			GameObject soundPoint = new GameObject();
			soundPoint.transform.parent = this.transform;
			soundPoint.transform.localPosition = new Vector3(0, 0, 0);
			soundPoint.name = this.name + " Sound Point";
			doorSound = soundPoint.AddComponent<AudioSource>();
		}
		//SETUP DOOR COMPONENTS
		if (startState == StartState.Closed)
		{
			currentDragPercentage = 0f;
			currentState = CurrentState.Closed;
			//SET DOOR ROTATION PROPERTIES
			foreach (Door door in components)
			{
				if (door.movementType == Door.MovementType.Rotation)
				{
					door.finalPosition = door.doorElement.localRotation;
					door.initialPosition = Quaternion.Euler(door.xAxis, door.yAxis, door.zAxis);
				}
				else
				{
					door.finalPoint = door.doorElement.localPosition;
					door.initialPoint = new Vector3(door.xAxis, door.yAxis, door.zAxis);
				}
			}
		}
		else if (startState == StartState.Open)
		{
			currentDragPercentage = 100f;
			currentState = CurrentState.Open;
			//SET DOOR ROTATION PROPERTIES
			foreach (Door door in components)
			{
				if (door.movementType == Door.MovementType.Rotation)
				{
					door.initialPosition = door.doorElement.localRotation;
					door.finalPosition = Quaternion.Euler(door.xAxis, door.yAxis, door.zAxis);
				}
				else
				{
					door.initialPoint = door.doorElement.localPosition;
					door.finalPoint = new Vector3(door.xAxis, door.yAxis, door.zAxis);
				}
			}
		}
		started = true;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAG CALCULATION
	void CalculateDrag()
	{
		dragAmount = 0;
		foreach (Door door in components)
		{
			float sampleDrag = door.DragCoefficient * currentDragPercentage / 100f;
			dragAmount += sampleDrag;
			if (dragAmount < 0.0)
			{
				dragAmount = 0;
			}
		}
	}











	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//COMPONENT ADDITION
	public void AddActuator()
	{
		components.Add(new Door());
	}
	//RETURN FUNCTIONS
	IEnumerator Open()
	{                                                               //PUT YOUR ANIMATION STUFF ON THIS LINE
		yield return new WaitForSeconds(openTime);
		CloseSwitches();
		currentState = CurrentState.Open; activated = false;
	}
	IEnumerator Close()
	{                                                               //PUT YOUR ANIMATION STUFF ON THIS LINE
		yield return new WaitForSeconds(closeTime);
		currentState = CurrentState.Closed;
		CloseSwitches(); activated = false;
	}
	void CloseSwitches()
	{
		open = false; close = false;
	}










	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//LERP DRAG SYSTEM
	IEnumerator Decrease()
	{
		float time = (openTime + closeTime) / 2f;
		float timeSlice = 100f / time;
		while (currentDragPercentage >= 0)
		{
			currentDragPercentage -= timeSlice;
			yield return new WaitForSeconds(1);
			if (currentDragPercentage <= 0)
				break;
		}
		if (currentDragPercentage < 0)
		{
			currentDragPercentage = 0;
		}
		yield return null;
	}
	//
	IEnumerator Increase()
	{
		float time = (openTime + closeTime) / 2f;
		float timeSlice = 100f / time;
		while (currentDragPercentage >= 0)
		{
			currentDragPercentage += timeSlice;
			yield return new WaitForSeconds(1);
			if (currentDragPercentage >= 100)
				break;
		}
		if (currentDragPercentage > 100)
		{
			currentDragPercentage = 100;
		}
		yield return null;
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL MOVEMENT
	void Update()
	{
		if (isControllable)
		{
			//CALCULATE DRAG
			if (generatesDragWhenOpened)
			{
				CalculateDrag();
			}
			//CLOSING MOVEMENT
			if (open && currentState == CurrentState.Closed)
			{
				foreach (Door door in components)
				{
					if (door.movementType == Door.MovementType.Rotation)
					{
						door.doorElement.localRotation = Quaternion.RotateTowards(door.doorElement.localRotation, door.initialPosition, Time.deltaTime * rotateSpeed * door.speedFactor);
					}
					else
					{
						door.doorElement.localPosition = Vector3.Lerp(door.doorElement.localPosition, door.initialPoint, Time.deltaTime * rotateSpeed / 50);
					}
				}
				if (!activated)
				{
					StartCoroutine(Open());
					StartCoroutine(Increase());
					activated = true;
					if (openSound != null && playSound)
					{
						doorSound.PlayOneShot(openSound);
					}
				}
			}
			//OPENING MOVEMENT
			if (close && currentState == CurrentState.Open)
			{
				foreach (Door door in components)
				{
					if (door.movementType == Door.MovementType.Rotation)
					{
						door.doorElement.localRotation = Quaternion.RotateTowards(door.doorElement.localRotation, door.finalPosition, Time.deltaTime * rotateSpeed * door.speedFactor);
					}
					else
					{
						door.doorElement.localPosition = Vector3.Lerp(door.doorElement.localPosition, door.finalPoint, Time.deltaTime * rotateSpeed / 50);
					}
				}
				if (!activated)
				{
					StartCoroutine(Close());
					StartCoroutine(Decrease());
					activated = true;
					if (closeSound != null && playSound)
					{
						doorSound.PlayOneShot(closeSound);
					}
				}
			}
		}
	}
}







#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroHydraulicSystem))]
public class SilantroHydraulicSystemEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f, 0.4f, 0f);

	SilantroHydraulicSystem hydraulic;
	SerializedProperty doorList;

	private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
	private static GUILayoutOption buttonWidth = GUILayout.Width(60f);

	void OnEnable()
	{
		hydraulic = (SilantroHydraulicSystem)target;
		doorList = serializedObject.FindProperty("components");
	}


	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector();  
		serializedObject.Update();

		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox("Note: This component will be deprecated in the next update, please consider using the Actuator component", MessageType.Warning);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		EditorGUILayout.LabelField("Actuator Count", doorList.arraySize.ToString());
		GUILayout.Space(3f);
		if (GUILayout.Button("Create Actuator"))
		{
			hydraulic.components.Add(new SilantroHydraulicSystem.Door());
		}
		GUILayout.Space(2f);

		//DISPLAY WHEEL ELEMENTS
		for (int i = 0; i < doorList.arraySize; i++)
		{
			SerializedProperty reference = doorList.GetArrayElementAtIndex(i);
			SerializedProperty Identifier = reference.FindPropertyRelative("Identifier");
			SerializedProperty doorElement = reference.FindPropertyRelative("doorElement");
			SerializedProperty xValue = reference.FindPropertyRelative("xAxis");
			SerializedProperty yValue = reference.FindPropertyRelative("yAxis");
			SerializedProperty zValue = reference.FindPropertyRelative("zAxis");
			SerializedProperty mode = reference.FindPropertyRelative("movementType");
			SerializedProperty DragCoefficient = reference.FindPropertyRelative("DragCoefficient");
			SerializedProperty speedFactor = reference.FindPropertyRelative("speedFactor");


			GUI.color = new Color(1, 0.76f, 0.545f);
			EditorGUILayout.HelpBox("Actuator : " + (i + 1).ToString(), MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(Identifier);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(mode);
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(doorElement);

			if (hydraulic.components[i].movementType == SilantroHydraulicSystem.Door.MovementType.Rotation)
			{
				GUILayout.Space(3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Rotation Angles", MessageType.None);
				GUI.color = backgroundColor;
			}
			else
			{
				GUILayout.Space(3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Extension Amounts", MessageType.None);
				GUI.color = backgroundColor;
			}
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(xValue);
			GUILayout.Space(1f);
			EditorGUILayout.PropertyField(yValue);
			GUILayout.Space(1f);
			EditorGUILayout.PropertyField(zValue);
			//
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Drag Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(DragCoefficient);
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Speed Setting", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(speedFactor);
			GUILayout.Space(3f);
			if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth))
			{
				hydraulic.components.RemoveAt(i);
			}
			GUILayout.Space(5f);

		}

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Controls", MessageType.None);
		GUI.color = backgroundColor;
		if (!hydraulic.started)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("startState"), new GUIContent("Start State"));
		}
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Current State", hydraulic.currentState.ToString());
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("open"), new GUIContent("Engage"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("close"), new GUIContent("Disengage"));
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Actuator Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Actuation Level", hydraulic.currentDragPercentage.ToString("0") + " %");
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("openTime"), new GUIContent("Open Time"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("closeTime"), new GUIContent("Close Time"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateSpeed"), new GUIContent("Movement Speed"));



		GUILayout.Space(15f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("generatesDragWhenOpened"), new GUIContent("Generates Drag"));

		if (hydraulic.generatesDragWhenOpened)
		{
			GUILayout.Space(3f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Drag Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Total Drag Coefficient", (hydraulic.dragAmount).ToString("0.000"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Current Drag Force", hydraulic.currentDrag.ToString("0.00") + " N");
		}


		GUILayout.Space(15f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("playSound"), new GUIContent("Play Sounds"));

		if (hydraulic.playSound)
		{
			GUILayout.Space(2f);
			EditorGUILayout.HelpBox("Sound Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("openSound"), new GUIContent("Open Sound"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("closeSound"), new GUIContent("Close Sound"));
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(hydraulic);
			EditorSceneManager.MarkSceneDirty(hydraulic.gameObject.scene);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif