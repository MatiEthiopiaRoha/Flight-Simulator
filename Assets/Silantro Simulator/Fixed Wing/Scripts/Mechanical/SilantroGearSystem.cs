using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif




/// <summary>
///
/// 
/// Use:		 Handles the wheel collider(s) operations and states e.g Rotating, Tracking with the selected wheel mesh and braking.		 
/// </summary>




public class SilantroGearSystem : MonoBehaviour
{

	public List<WheelSystem> wheelSystem = new List<WheelSystem>();
	public WheelCollider[] wheelColliders;
	public bool showWheels = true, evaluate;


	//-------------------------------------------------CONNECTIONs
	public SilantroController controller;
	public Rigidbody aircraft;

	//---------------------------------------------------GENERAL
	float aircraftSpeed;


	//---------------------------------------------------BRAKE
	public enum BrakeState { Engaged, Disengaged }
	public BrakeState brakeState = BrakeState.Engaged;
	public float brakeInput;
	public float brakeTorque = 10000f; //Nm



	//---------------------------------------------------WHEEL STEERING
	public float rudderInput, tillerInput, currentSteerAngle;
	public float maximumRudderSteer = 6f, maximumTillerSteer = 40f, maximumSteerAngle = 40f;
	public bool pedalLinkageEngaged; float rumbleLimit;



	//--------------------------------------------------STEERING AXLE CONFIG
	public Transform steeringAxle;
	public enum RotationAxis { X, Y, Z }
	public RotationAxis rotationAxis = RotationAxis.X;
	Vector3 steerAxis; Quaternion baseAxleRotation;
	public bool invertAxleRotation;



	//------------------------------------------------WHEEL RUMBLE
	public float maximumRumbleVolume = 1f, currentVolume, currentPitch;
	public AudioSource soundSource, brakeSource; public AudioClip groundRoll;
	public AudioClip brakeEngage, brakeRelease;
	bool initialized;




	//--------------------------------------------WHEEL SYSTEM
	[System.Serializable]
	public class WheelSystem
	{
		//--------------PROPERTIES
		public string Identifier; public WheelCollider collider; public Transform wheelModel;
		public enum WheelRotationAxis { X, Y, Z }
		public float wheelRPM;
		public WheelRotationAxis rotationWheelAxis = WheelRotationAxis.X; public bool steerable;
		//-------------STORAGE
		[HideInInspector] public Vector3 initialWheelPosition;[HideInInspector] public Quaternion initialWheelRotation;
		public enum WheelPosition {Forward, Left, Right, Balance }
		public WheelPosition wheelPosition = WheelPosition.Balance;
	}


	public WheelCollider leftBalanceWheel;
	public WheelCollider rightBalanceWheel;

















	/// <summary>
	/// For testing purposes only
	/// </summary>
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void Start()
	{
		if (evaluate) { InitializeStruct(); }
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeStruct()
	{
		if (aircraft != null)
		{
			foreach (WheelSystem system in wheelSystem)
			{
				if (system.wheelModel != null)
				{
					system.initialWheelPosition = system.wheelModel.transform.localPosition;
					system.initialWheelRotation = system.wheelModel.transform.localRotation;

					if (system.wheelPosition == WheelSystem.WheelPosition.Left) { leftBalanceWheel = system.collider; }
					if (system.wheelPosition == WheelSystem.WheelPosition.Right) { rightBalanceWheel = system.collider; }
				}
			}

			//----------------COLLECT AXLE DATA
			if (rotationAxis == RotationAxis.X) { steerAxis = new Vector3(1, 0, 0); }
			else if (rotationAxis == RotationAxis.Y) { steerAxis = new Vector3(0, 1, 0); }
			else if (rotationAxis == RotationAxis.Z) { steerAxis = new Vector3(0, 0, 1); }
			steerAxis.Normalize(); if (steeringAxle != null) { baseAxleRotation = steeringAxle.localRotation; }

			wheelColliders = aircraft.gameObject.GetComponentsInChildren<WheelCollider>();

			//--------------SETUP GROUND ROLL
			if (groundRoll != null) { Handler.SetupSoundSource(transform, groundRoll, "Struct Sound Point", 150f, true, true, out soundSource); }
			if (brakeRelease) { Handler.SetupSoundSource(transform, brakeEngage, "Brake Sound Point", 150f, false, false, out brakeSource); brakeSource.volume = maximumRumbleVolume; }
			initialized = true;
			pedalLinkageEngaged = true;
		}
		else { return; }
	}


	public float AntiRoll = 5000, antiRollForceRearHorizontal;
	private void FixedUpdate()
	{
		// ------------------------------------------------- Stabilize Aircraft
		if(leftBalanceWheel != null && rightBalanceWheel != null && aircraft != null)
		{
			float rearLeftGap = 1.0f, rearRightGap = 1.0f;

			bool rearLeftGrounded = leftBalanceWheel.GetGroundHit(out WheelHit RearLeftWheelHit);
			if (rearLeftGrounded) rearLeftGap = (-leftBalanceWheel.transform.InverseTransformPoint(RearLeftWheelHit.point).y - leftBalanceWheel.radius) / leftBalanceWheel.suspensionDistance;
			bool rearRightGrounded = rightBalanceWheel.GetGroundHit(out WheelHit RearRightWheelHit);
			if (rearRightGrounded) rearRightGap = (-rightBalanceWheel.transform.InverseTransformPoint(RearRightWheelHit.point).y - rightBalanceWheel.radius) / rightBalanceWheel.suspensionDistance;
			antiRollForceRearHorizontal = (rearLeftGap - rearRightGap) * AntiRoll;

			if (rearLeftGrounded) aircraft.AddForceAtPosition(leftBalanceWheel.transform.up * -antiRollForceRearHorizontal, leftBalanceWheel.transform.position);
			if (rearRightGrounded) aircraft.AddForceAtPosition(rightBalanceWheel.transform.up * antiRollForceRearHorizontal, rightBalanceWheel.transform.position);
		}
	}




	float actualSteer;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void Update()
	{
		if (initialized)
		{
			//--------------------------INPUTS
			if (controller.flightComputer.operationMode != SilantroFlightComputer.AugmentationType.Autonomous)
			{
				float pedalSteerAngle = 0; if (pedalLinkageEngaged) { pedalSteerAngle = rudderInput * maximumRudderSteer; }
				pedalSteerAngle = Mathf.Clamp(pedalSteerAngle, -maximumRudderSteer, maximumRudderSteer);
				float tillerSteerAngle = tillerInput * maximumTillerSteer; tillerSteerAngle = Mathf.Clamp(tillerSteerAngle, -maximumTillerSteer, maximumTillerSteer);
				currentSteerAngle = pedalSteerAngle + tillerSteerAngle; currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maximumSteerAngle, maximumSteerAngle);
			}
            
			
			if (invertAxleRotation) { currentSteerAngle *= -1f; }
			if (aircraft != null) { aircraftSpeed = aircraft.velocity.magnitude; }


            //--------------------- Brake Force
			if(controller.aircraft != null)
            {
				Vector3 brakeForce = -controller.aircraft.velocity * brakeInput * brakeTorque;
				controller.aircraft.AddForce(brakeForce, ForceMode.Force);
			}




			foreach (WheelSystem system in wheelSystem)
			{
				//----------------BRAKE
				BrakingSystem(system);
				//---------------SEND ROTATION DATA
				RotateWheel(system.wheelModel, system);

				if (system.collider.isGrounded)
				{
					//---------------SEND ALIGNMENT DATA
					WheelAllignment(system, system.wheelModel);
				}

				//-----------------RETURN TO BASE POINT
				if (!system.collider.isGrounded && aircraft != null && aircraft.transform.position.y > 5)
				{
					system.wheelModel.transform.localPosition = system.initialWheelPosition;
					system.wheelModel.transform.localRotation = system.initialWheelRotation;
				}


				//------------------STEERING
				if (system.collider.isGrounded) {actualSteer = currentSteerAngle; } else { actualSteer = 0f; }
				if (system.steerable && system.collider != null) { system.collider.steerAngle = actualSteer; }
				if (steeringAxle != null) { steeringAxle.localRotation = baseAxleRotation; steeringAxle.Rotate(steerAxis, actualSteer); }
			}


			//---------------------WHEEL SOUND
			if (soundSource != null) { PlayRumbleSound(); }
		}
	}











	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	// ---------------------------------------------------CONTROL FUNCTIONS--------------------------------------------------------------------------------------
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------



	//--------------------------------------------------ACTIVATE BRAKES
	public void EngageBrakes()
	{
		if (aircraft != null && initialized)
		{
			if (brakeState != BrakeState.Engaged)
			{
				brakeState = BrakeState.Engaged;
				if (brakeSource != null)
				{
					if (brakeSource.isPlaying) { brakeSource.Stop(); }
					brakeSource.PlayOneShot(brakeEngage);
				}
				else { Debug.LogError("Brake sounds for" + transform.name + " has not been assigned"); }
			}
		}
		if (aircraft == null) { Debug.LogError("Aircraft for " + transform.name + " has not been assigned"); }
	}


	//--------------------------------------------------RELEASE BRAKES
	public void ReleaseBrakes()
	{
		if (aircraft != null && initialized)
		{
			if (brakeState != BrakeState.Disengaged)
			{
				brakeState = BrakeState.Disengaged;
				if (brakeSource != null)
				{
					if (brakeSource.isPlaying) { brakeSource.Stop(); }
					brakeSource.PlayOneShot(brakeRelease);
				}
				else { Debug.LogError("Brake sounds for " + transform.name + " has not been assigned"); }
			}
		}
		if (aircraft == null) { Debug.LogError("Aircraft for " + transform.name + " has not been assigned"); }
	}



	//-------------------------------------------------- TOGGLE BRAKES
	public void ToggleBrakes()
	{
		if (aircraft != null && initialized)
		{
			if (brakeState != BrakeState.Disengaged) { ReleaseBrakes(); }
			else { EngageBrakes(); }
		}
		if (aircraft == null) { Debug.LogError("Aircraft for " + transform.name + " has not been assigned"); }
	}
















	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	// ---------------------------------------------------CALL FUNCTIONS--------------------------------------------------------------------------------------
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ROTATE WHEEL
	void RotateWheel(Transform wheel, WheelSystem system)
	{
		if (system.collider != null && system.collider.isGrounded)
		{
			float circumfrence = 2f * Mathf.PI * system.collider.radius; float speed = aircraftSpeed * 60f;
			system.wheelRPM = speed / circumfrence;
		}
		else { system.wheelRPM = 0f; }

		if (wheel != null)
		{
			if (system.rotationWheelAxis == WheelSystem.WheelRotationAxis.X) { wheel.Rotate(new Vector3(system.wheelRPM * Time.deltaTime, 0, 0)); }
			if (system.rotationWheelAxis == WheelSystem.WheelRotationAxis.Y) { wheel.Rotate(new Vector3(0, system.wheelRPM * Time.deltaTime, 0)); }
			if (system.rotationWheelAxis == WheelSystem.WheelRotationAxis.Z) { wheel.Rotate(new Vector3(0, 0, system.wheelRPM * Time.deltaTime)); }
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	////ALLIGN WHEEL TO COLLIDER
	void WheelAllignment(WheelSystem system, Transform wheel)
	{
		if (wheel != null)
		{
			RaycastHit hit; WheelHit CorrespondingGroundHit;

			//-------------------------------------------------------------------------------------
			if (system.collider != null)
			{
				Vector3 ColliderCenterPoint = system.collider.transform.TransformPoint(system.collider.center); system.collider.GetGroundHit(out CorrespondingGroundHit);

				if (Physics.Raycast(ColliderCenterPoint, -system.collider.transform.up, out hit, (system.collider.suspensionDistance + system.collider.radius) * transform.localScale.y))
				{
					wheel.position = hit.point + (system.collider.transform.up * system.collider.radius) * transform.localScale.y;
					float extension = (-system.collider.transform.InverseTransformPoint(CorrespondingGroundHit.point).y - system.collider.radius) / system.collider.suspensionDistance;
					Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point + system.collider.transform.up, extension <= 0.0 ? Color.magenta : Color.white);
					Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - system.collider.transform.forward * CorrespondingGroundHit.forwardSlip * 2f, Color.green);
					Debug.DrawLine(CorrespondingGroundHit.point, CorrespondingGroundHit.point - system.collider.transform.right * CorrespondingGroundHit.sidewaysSlip * 2f, Color.red);
				}
				else
				{
					wheel.transform.position = Vector3.Lerp(wheel.transform.position, ColliderCenterPoint - (system.collider.transform.up * system.collider.suspensionDistance) * transform.localScale.y, Time.deltaTime * 10f);
				}
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//BRAKE
	void BrakingSystem(WheelSystem wheel)
	{
		if (brakeInput < 0) { brakeInput = 0; }

		//------------------------CALCULATE BRAKE LEVER TORQUE
		float actualTorque = 0f;

		//------------------------PARKING BRAKE
		if (wheel != null && wheel.collider != null && !wheel.steerable)
		{
			if (brakeState == BrakeState.Engaged) { wheel.collider.brakeTorque = brakeTorque + actualTorque; wheel.collider.motorTorque = 0; }
			else { wheel.collider.motorTorque = 0.05f * controller.totalThrustGenerated; if (actualTorque > 10) { wheel.collider.brakeTorque = actualTorque; } else { wheel.collider.brakeTorque = 0f; } }
		}
	}






	//------------------------------------------------------------------------------------------
	public bool GroundCheck()

	{
		for (int i = 0; i < wheelColliders.Length; i++) { if (wheelColliders[i].isGrounded == true) { return true; } }
		return false;
	}




	//------------------------------------------------------------------------------------------
	void PlayRumbleSound()
	{
		if (wheelSystem[0].collider != null && wheelSystem[1].collider != null && groundRoll != null)
		{
			if (wheelSystem[0].collider.isGrounded && wheelSystem[1].collider.isGrounded)
			{

				//-----------------------SET PARAMETERS
				if (controller != null && controller.view != null)
				{
					if (controller.view.cameraState == SilantroCamera.CameraState.Exterior) { rumbleLimit = maximumRumbleVolume; }
					else if (controller.view.cameraState == SilantroCamera.CameraState.Interior) { rumbleLimit = 0.3f * maximumRumbleVolume; }
				}
				else { rumbleLimit = maximumRumbleVolume; }

				currentPitch = aircraftSpeed / 50f; currentVolume = aircraftSpeed / 20f;
				currentVolume = Mathf.Clamp(currentVolume, 0, maximumRumbleVolume); currentPitch = Mathf.Clamp(currentPitch, 0, 1f);
				soundSource.volume = Mathf.Lerp(soundSource.volume, currentVolume, 0.2f);
			}
			else { soundSource.volume = Mathf.Lerp(soundSource.volume, 0f, 0.2f); }
		}
	}



#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SilantroGearSystem))]
	public class SilantroGearEditor : Editor
	{
		Color backgroundColor;
		Color silantroColor = new Color(1, 0.4f, 0);
		SilantroGearSystem structBase;
		SerializedProperty wheelList;

		private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
		private static GUILayoutOption buttonWidth = GUILayout.Width(60f);




		//------------------------------------------------------------------------
		private SerializedProperty maximumTillerAngle;
		private SerializedProperty maximumRudderAngle;
		private SerializedProperty showWheelCommand;
		private SerializedProperty steeringAxle;
		private SerializedProperty rotationAxis;
		private SerializedProperty invertAxleRotation;
		private SerializedProperty maximumSteerAngle;

		private SerializedProperty brakeTorque;

		private SerializedProperty groundRollExterior;
		private SerializedProperty brakeEngage;
		private SerializedProperty brakeRelease;
		private SerializedProperty maximumRumbleVolume;

		private SerializedProperty aircraftBody;




		//------------------------------------------------------------------------
		void OnEnable()
		{
			structBase = (SilantroGearSystem)target;
			wheelList = serializedObject.FindProperty("wheelSystem");


			showWheelCommand = serializedObject.FindProperty("showWheels");
			maximumTillerAngle = serializedObject.FindProperty("maximumTillerSteer");
			maximumRudderAngle = serializedObject.FindProperty("maximumRudderSteer");
			steeringAxle = serializedObject.FindProperty("steeringAxle");
			rotationAxis = serializedObject.FindProperty("rotationAxis");
			maximumSteerAngle = serializedObject.FindProperty("maximumSteerAngle");
			invertAxleRotation = serializedObject.FindProperty("invertAxleRotation");

			brakeTorque = serializedObject.FindProperty("brakeTorque");

			groundRollExterior = serializedObject.FindProperty("groundRoll");
			brakeEngage = serializedObject.FindProperty("brakeEngage");
			brakeRelease = serializedObject.FindProperty("brakeRelease");
			maximumRumbleVolume = serializedObject.FindProperty("maximumRumbleVolume");
			aircraftBody = serializedObject.FindProperty("aircraft");
		}


		//-------------------------------------------------------------------------
		public override void OnInspectorGUI()
		{
			backgroundColor = GUI.backgroundColor;
			//DrawDefaultInspector();
			serializedObject.Update();


			//-------------------------------------------WHEEL BASE
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("State", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			if (structBase.evaluate) { if (GUILayout.Button("Finish Evaluation")) { structBase.evaluate = false; } silantroColor = Color.red; }
			if (!structBase.evaluate) { if (GUILayout.Button("Evaluate")) { structBase.evaluate = true; } silantroColor = new Color(1, 0.4f, 0); }
			if (structBase.evaluate)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(aircraftBody);
			}




			GUILayout.Space(10f);
			EditorGUILayout.HelpBox("Wheel Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);

			if (wheelList != null) { EditorGUILayout.LabelField("Wheel Count", wheelList.arraySize.ToString()); }
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(showWheelCommand);

			if (structBase.showWheels)
			{
				GUILayout.Space(5f);
				if (GUILayout.Button("Create Wheel")) { structBase.wheelSystem.Add(new WheelSystem()); }

				//--------------------------------------------WHEEL ELEMENTS
				if (wheelList != null)
				{
					GUILayout.Space(2f);
					//DISPLAY WHEEL ELEMENTS
					for (int i = 0; i < wheelList.arraySize; i++)
					{
						SerializedProperty reference = wheelList.GetArrayElementAtIndex(i);
						SerializedProperty Identifier = reference.FindPropertyRelative("Identifier");
						SerializedProperty position = reference.FindPropertyRelative("wheelPosition");
						SerializedProperty collider = reference.FindPropertyRelative("collider");
						SerializedProperty wheelModel = reference.FindPropertyRelative("wheelModel");
						SerializedProperty steerable = reference.FindPropertyRelative("steerable");
						SerializedProperty rotationAxis = reference.FindPropertyRelative("rotationWheelAxis");

						GUI.color = new Color(1, 0.8f, 0);
						EditorGUILayout.HelpBox("Wheel : " + (i + 1).ToString(), MessageType.None);
						GUI.color = backgroundColor;
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(Identifier);
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(position);
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(collider);
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(wheelModel);
						GUILayout.Space(3f);
						GUI.color = Color.white;
						EditorGUILayout.HelpBox("Operational Properties", MessageType.None);
						GUI.color = backgroundColor;
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(rotationAxis);
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(steerable);
						GUILayout.Space(3f);
						EditorGUILayout.LabelField("RPM", structBase.wheelSystem[i].wheelRPM.ToString("0") + " RPM");

						GUILayout.Space(3f);
						if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth))
						{
							structBase.wheelSystem.RemoveAt(i);
						}
						GUILayout.Space(5f);
					}
				}
			}



			//-------------------------------------------------------------STEERING
			GUILayout.Space(20f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Steering Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(steeringAxle);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(rotationAxis);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(invertAxleRotation);

			


			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Steering Stabilization", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("AntiRoll"), new GUIContent("Anti Roll Force"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Applied Force", structBase.antiRollForceRearHorizontal.ToString("0.0") + " N");


			//------------------------LIMITS
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Steering Limits", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(maximumTillerAngle);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(maximumRudderAngle);
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(maximumSteerAngle);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Linkage Engaged", structBase.pedalLinkageEngaged.ToString());
			if (structBase.pedalLinkageEngaged)
			{
				GUILayout.Space(3f);
				EditorGUILayout.LabelField("Steer Angle", structBase.currentSteerAngle.ToString("0.0"));
			}

			//-------------------------------------------------------------BRAKING
			GUILayout.Space(20f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Brake Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Engaged", structBase.brakeState.ToString());
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brakeTorque, new GUIContent("Park Brake Torque"));
			

			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(groundRollExterior);
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(brakeEngage);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brakeRelease);
			GUILayout.Space(3f);
			maximumRumbleVolume.floatValue = EditorGUILayout.Slider("Volume Limit", maximumRumbleVolume.floatValue, 0f, 1f);

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}