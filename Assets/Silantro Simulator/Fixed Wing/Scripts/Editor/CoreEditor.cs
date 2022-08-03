using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroCore))]
public class SilantroCoreEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroCore core;
    


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { core = (SilantroCore)target; }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.Update();

        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("COG Configuration", MessageType.None);
        GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("functionality"), new GUIContent("Functionality"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("tensorMode"), new GUIContent("Tensor Mode"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("speedType"), new GUIContent("Mode"));
	


		if (core.functionality == SilantroCore.SystemType.Advanced)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("emptyCenterOfMass"), new GUIContent("Empty COG"));
			float deviation = 0f;
			if (core.currentCOM != null) { deviation = Vector3.Distance(core.emptyCenterOfMass.position, core.currentCOM.position); }
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Deviation", deviation.ToString("0.00") + " m");
		}

		if (core.tensorMode == SilantroCore.TensorMode.Manual)
		{
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Intertia Tensor Data", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			GUI.color = Color.yellow;
			EditorGUILayout.HelpBox("Making changes to the tensor will have significant effects on the rigidbody performance", MessageType.Warning);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("xInertia"), new GUIContent("Pitch Inertia"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("zInertia"), new GUIContent("Roll Inertia"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("yInertia"), new GUIContent("Yaw Inertia"));


			if (!Application.isPlaying)
			{
				GUILayout.Space(6f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Assign aircraft rigidbody to view default Unity values, they will be replaced with the values above once the application starts", MessageType.Info);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("sampleAircraft"), new GUIContent(" "));


				float sampleX = 0f, sampleY = 0f, sampleZ = 0f;
				if (core.sampleAircraft != null)
				{
					sampleX = core.sampleAircraft.inertiaTensor.x;
					sampleY = core.sampleAircraft.inertiaTensor.y;
					sampleZ = core.sampleAircraft.inertiaTensor.z;
					GUILayout.Space(5f);
					EditorGUILayout.LabelField("Pitch Inertia", sampleX.ToString("0.0") + " kg/m2");
					GUILayout.Space(3f);
					EditorGUILayout.LabelField("Roll Inertia", sampleZ.ToString("0.0") + " kg/m2");
					GUILayout.Space(3f);
					EditorGUILayout.LabelField("Yaw Inertia", sampleY.ToString("0.0") + " kg/m2");
				}
			}
		}

		if (core.speedType == SilantroCore.SpeedType.Supersonic)
		{
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Effect Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sonicBoom"), new GUIContent("Sonic Boom"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sonicCone"), new GUIContent("Sonic Cone"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("sonicEmission"), new GUIContent("Maximum Emission"));
		}

		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Pressure Breathing", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("breathing"), new GUIContent("State"));

		if (core.breathing == SilantroCore.PressureBreathing.Active)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("breathingLoopSound"), new GUIContent("Breathing Loop"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("breathingEndSound"), new GUIContent("Breathing End"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("soundVolume"), new GUIContent("Sound Volume"));
		}

		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Performance Data", MessageType.None);
		GUI.color = backgroundColor; 
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Basic", MessageType.None);
		GUI.color = backgroundColor; 
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Indicated Airspeed", (core.currentSpeed * Oyedoyin.MathBase.toKnots).ToString("0.0") + " knots");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("True Airspeed", (core.trueSpeed * Oyedoyin.MathBase.toKnots).ToString("0.0") + " knots");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Acceleration", (core.acceleration * Oyedoyin.MathBase.toKnots).ToString("0.0") + " knots/s");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Mach", core.machSpeed.ToString("0.00"));

		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Altitude", (core.currentAltitude * Oyedoyin.MathBase.toFt).ToString("0.0") + " feet");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Vertical Speed", (core.verticalSpeed * Oyedoyin.MathBase.toFtMin).ToString("0.0") + " ft/min");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Heading Direction", core.headingDirection.ToString("0.0") + " °");
		
		
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Advanced", MessageType.None);
		GUI.color = backgroundColor; 
		GUILayout.Space(5f);
		EditorGUILayout.LabelField("G-Load", core.gForce.ToString("0.0"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Pitch Rate", core.pitchRate.ToString("0.0") + " °/s");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Roll Rate", core.rollRate.ToString("0.0") + " °/s");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Yaw Rate", core.yawRate.ToString("0.0") + " °/s");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Turn Rate", core.turnRate.ToString("0.0") + " °/s");


		GUILayout.Space(3f);
		EditorGUILayout.LabelField("α", core.alpha.ToString("0.0") + " °");


		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Low Pass Filter", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothGSpeed"), new GUIContent("G Smooth Speed"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothRateSpeed"), new GUIContent("Rate Smooth Speed"));

		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Ambient", MessageType.None);
		GUI.color = backgroundColor; GUILayout.Space(2f);
		GUILayout.Space(5f);
		EditorGUILayout.LabelField("Air Density", core.airDensity.ToString("0.000") + " kg/m3");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Temperature", core.ambientTemperature.ToString("0.0") + " °C");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Pressure", core.ambientPressure.ToString("0.0") + " kPa"); 

		serializedObject.ApplyModifiedProperties();
	}
}
#endif





#if UNITY_EDITOR
[CustomEditor(typeof(SilantroController))]
public class SilantroControllerEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f, 0.40f, 0f);
	SilantroController controller;
	SerializedProperty hover;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable() { controller = (SilantroController)target; hover = serializedObject.FindProperty("hoverSystem"); }


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Aircraft Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		controller.aircraftName = controller.transform.gameObject.name;
		EditorGUILayout.LabelField("Label", controller.aircraftName);
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("aircraftType"), new GUIContent("Aircraft Type"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent("Engine Type"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("engineCount"), new GUIContent("Engine Count"));

		if (controller.engineCount != SilantroController.EngineCount.Single)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineStartMode"), new GUIContent("Start Sequence"));
		}



		if (controller.aircraftType == SilantroController.AircraftType.VTOL)
		{
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("VTOL Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("configuration"), new GUIContent("Configuration"));
			
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Balance Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("BrakingTorque"), new GUIContent("Braking Torque"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("RollCompensationTorque"), new GUIContent("Longitudinal Balance Torque"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("PitchCompensationTorque"), new GUIContent("Lateral Balance Torque"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Mode", controller.hoverSystem.mode.ToString());
			
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Hover Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			SerializedProperty layerMask = hover.FindPropertyRelative("surfaceMask");
			EditorGUILayout.PropertyField(layerMask);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("hoverDamper"), new GUIContent("Hover Damper"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("hoverAngleDrift"), new GUIContent("Hover Angle Drift"));
			GUILayout.Space(4f);
			EditorGUILayout.PropertyField(hover.FindPropertyRelative("maximumTorque"), new GUIContent("Control Torque"));
		}


		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Control Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		controller.controlType = (SilantroController.ControlType)EditorGUILayout.EnumPopup("Control", controller.controlType);
		if (controller.controlType == SilantroController.ControlType.Internal)
		{
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Cockpit Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Pilot Onboard", controller.pilotOnboard.ToString());
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasDisplay"), new GUIContent("Display Canvas"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("interiorPilot"), new GUIContent("Interior Pilot"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("getOutPosition"), new GUIContent("Exit Point"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox(" ", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
		}


		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("inputType"), new GUIContent("Input"));

		if (controller.inputType == SilantroController.InputType.VR)
		{
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("VR Levers", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("controlStick"), new GUIContent("Control Stick"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleLever"), new GUIContent("Throttle Lever"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox(" ", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
		}


		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("startMode"), new GUIContent("Start Mode"));

		if (controller.startMode == SilantroController.StartMode.Hot)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Instantenous Start (Speed = m/s, Altitude = m)", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("startSpeed"), new GUIContent("Start Speed"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("startAltitude"), new GUIContent("Start Altitude"));
		}

		
		GUILayout.Space(15f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Weight Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("emptyWeight"), new GUIContent("Empty Weight"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumWeight"), new GUIContent("Maximum Weight"));
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Current Weight", controller.currentWeight.ToString("0.0") + " kg");

		if (controller.engineType != SilantroController.EngineType.Electric && controller.engineType != SilantroController.EngineType.Unpowered)
		{

			if (controller.engineType == SilantroController.EngineType.Piston)
			{
				if (controller.gasType == SilantroController.GasType.AVGas100) { controller.combustionEnergy = 42.8f; }
				if (controller.gasType == SilantroController.GasType.AVGas100LL) { controller.combustionEnergy = 43.5f; }
				if (controller.gasType == SilantroController.GasType.AVGas82UL) { controller.combustionEnergy = 43.6f; }
			}
			else
			{
				if (controller.jetType == SilantroController.JetType.JetB) { controller.combustionEnergy = 42.8f; }
				if (controller.jetType == SilantroController.JetType.JetA1) { controller.combustionEnergy = 43.5f; }
				if (controller.jetType == SilantroController.JetType.JP6) { controller.combustionEnergy = 43.02f; }
				if (controller.jetType == SilantroController.JetType.JP8) { controller.combustionEnergy = 43.28f; }
			}
			controller.combustionEnergy *= 1000f;


			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Fuel Configuration", MessageType.None);
			GUI.color = backgroundColor;
			if (controller.engineType == SilantroController.EngineType.Piston)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("gasType"), new GUIContent("Fuel Type"));
			}
			else
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("jetType"), new GUIContent("Fuel Type"));

			}
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Q ", controller.combustionEnergy.ToString("0.0") + " KJ");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Capacity", controller.TotalFuelCapacity.ToString("0.0") + " kg");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Fuel Level", controller.fuelLevel.ToString("0.0") + " kg");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Flow Rate", controller.totalConsumptionRate.ToString("0.00") + " kg/s");
		}



		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Aircraft Data Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		if (controller.wingType == SilantroController.WingType.Biplane)
		{
			EditorGUILayout.LabelField("Upper Wing Span", controller.upperWingSpan.ToString("0.00") + " m");
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Upper Wing Span", controller.lowerWingSpan.ToString("0.00") + " m");
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Wing Gap", controller.wingGap.ToString("0.00") + " m");
		}
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Engine Count", controller.AvailableEngines.ToString());
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Total Thrust", controller.totalThrustGenerated.ToString("0.0") + " N");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Wing Loading", controller.wingLoading.ToString("0.0") + " kg/m2");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Thrust/Weight Ratio", controller.thrustToWeightRatio.ToString("0.000"));

		serializedObject.ApplyModifiedProperties();
	}
}
#endif




#if UNITY_EDITOR
[CustomEditor(typeof(SilantroFlightComputer))]
public class SilantroFlightComputerEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f, 0.40f, 0f);
	SilantroFlightComputer computer;
	SerializedProperty brain;
	SerializedProperty tracker;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		computer = (SilantroFlightComputer)target;
		brain = serializedObject.FindProperty("brain");
		tracker = brain.FindPropertyRelative("tracker");
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector();
		serializedObject.Update();
		computer.PlotInputCurve();

		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Control Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("operationMode"), new GUIContent("Mode"));
		GUILayout.Space(5f);
		
		if (computer.operationMode == SilantroFlightComputer.AugmentationType.StabilityAugmentation)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Stability Augmentation", MessageType.None);
			GUI.color = backgroundColor; 
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("controlType"), new GUIContent("Control Type"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("augmentationMode"), new GUIContent("Input Mode"));
		}
		else if(computer.operationMode == SilantroFlightComputer.AugmentationType.Autonomous)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Autonomous Control", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Flight State", computer.brain.flightState.ToString());
			GUILayout.Space(3f);
			if (computer.brain.flightState == SilantroBrain.FlightState.Taxi)
			{
				EditorGUILayout.LabelField("Taxi State", computer.brain.taxiState.ToString());
			}
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("General Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("checkListTime"), new GUIContent("Checklist Time"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("transitionTime"), new GUIContent("State Transition Time"));
			//GUILayout.Space(3f);
			//EditorGUILayout.PropertyField(brain.FindPropertyRelative("evaluateTime"), new GUIContent("State Evaluate Time"));
		

			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Taxi Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("maximumTaxiSpeed"), new GUIContent("Maximum Taxi Speed"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("recommendedTaxiSpeed"), new GUIContent("Base Taxi Speed"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("maximumTurnSpeed"), new GUIContent("Maximum Turn Speed"));


			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Track Control", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(tracker.FindPropertyRelative("track"), new GUIContent("Taxi Track"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(tracker.FindPropertyRelative("turnOffset"), new GUIContent("Turn Point Offset"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(tracker.FindPropertyRelative("pointOffset"), new GUIContent("Point Threshold"));


			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Steer Control", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("maximumSteeringAngle"), new GUIContent("Maximum Steer Angle"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("minimumSteeringAngle"), new GUIContent("Minimum Steer Angle"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("steerSensitivity"), new GUIContent("Steer Sensitivity"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("brakeSensitivity"), new GUIContent("Brake Sensitivity"));



			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Takeoff Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("takeoffSpeed"), new GUIContent("Takeoff Speed (kts)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("climboutPitchAngle"), new GUIContent("Climbout Angle (°)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("climboutPitchRate"), new GUIContent("Climbout Rate (°/s)"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Takeoff Heading", computer.brain.takeoffHeading.ToString("0.0") + " °");
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumGearSpeed"), new GUIContent("Retract Gear Speed"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Cruise Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("cruisePitchRate"), new GUIContent("Cruise Pitch Rate (°/s)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceRollRate"), new GUIContent("Balance Roll Rate (°/s)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollRate"), new GUIContent("Maximum Roll Rate (°/s)"));


			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("cruiseSpeed"), new GUIContent("Cruise Speed (kts)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("cruiseHeading"), new GUIContent("Cruise Heading (°)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("cruiseClimbRate"), new GUIContent("Cruise Climb (ft/min)"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(brain.FindPropertyRelative("cruiseAltitude"), new GUIContent("Cruise Altitude (ft)"));
		}

		else if(computer.operationMode == SilantroFlightComputer.AugmentationType.CommandAugmentation)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Command Augmentation", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("commandPreset"), new GUIContent("Command Preset"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("commandMode"), new GUIContent("Switch Mode"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Gain State", computer.state.ToString());
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumControlSpeed"), new GUIContent("Activation Speed"));

			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Roll Performance (Rates °/s)", MessageType.None);
			GUI.color = backgroundColor;
			if (computer.commandPreset == SilantroFlightComputer.CommandPreset.Fighter)
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("climboutRollRate"), new GUIContent("Climbout Limit"));
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cruiseRollRate"), new GUIContent("Cruise Limit"));
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollRate"), new GUIContent("Maximum Rate"));
			}
			else
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollRate"), new GUIContent("Maximum Rate"));
			}


			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Pitch Performance (Rates °/s)", MessageType.None);
			GUI.color = backgroundColor;
			

			if (computer.commandPreset == SilantroFlightComputer.CommandPreset.Fighter)
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("longitudinalCommand"), new GUIContent("Command Type"));
				if (computer.longitudinalCommand == SilantroFlightComputer.CommandInput.RateOnly)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("climboutPitchRate"), new GUIContent("Climbout Limit"));
					GUILayout.Space(2f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("cruisePitchRate"), new GUIContent("Cruise Limit"));
					GUILayout.Space(2f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchRate"), new GUIContent("Maximum Rate"));
				}
				else
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("climboutG"), new GUIContent("Climbout G Limit"));
					GUILayout.Space(2f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("cruiseG"), new GUIContent("Cruise G Limit"));
				}
			}
			else
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("climboutPitchRate"), new GUIContent("Climbout Limit"));
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cruisePitchRate"), new GUIContent("Cruise Limit"));
			}
		}






		if (computer.operationMode == SilantroFlightComputer.AugmentationType.StabilityAugmentation)
		{
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Wing Leveler Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceRollRate"), new GUIContent("Level Roll Rate"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceRollAuthority"), new GUIContent("Balance Authority"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("commandRollAuthority"), new GUIContent("Command Authority"));

			if (computer.controlType != SilantroFlightComputer.ControlType.SingleAxis)
			{
				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Nose Leveler Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("balancePitchRate"), new GUIContent("Level Pitch Rate"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("balancePitchAuthority"), new GUIContent("Balance Authority"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("commandPitchAuthority"), new GUIContent("Command Authority"));
			}

			if(computer.controlType == SilantroFlightComputer.ControlType.ThreeAxis)
			{
				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Yaw Damper Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceYawRate"), new GUIContent("Damp Yaw Rate"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceYawAuthority"), new GUIContent("Balance Authority"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("commandYawAuthority"), new GUIContent("Command Authority"));
			}
		}

		if (computer.operationMode != SilantroFlightComputer.AugmentationType.Autonomous)
		{
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Input Tuning", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Pitch", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchDeadZone"), new GUIContent("Dead Zone"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchScale"), new GUIContent("Curvature"));
			GUILayout.Space(3f);
			EditorGUILayout.CurveField("Curve", computer.pitchInputCurve);

			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Roll", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rollDeadZone"), new GUIContent("Dead Zone"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rollScale"), new GUIContent("Curvature"));
			GUILayout.Space(3f);
			EditorGUILayout.CurveField("Curve", computer.rollInputCurve);


			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Yaw", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("yawDeadZone"), new GUIContent("Dead Zone"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("yawScale"), new GUIContent("Curvature"));
			GUILayout.Space(3f);
			EditorGUILayout.CurveField("Curve", computer.yawInputCurve);


			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Input Cycle Frequency", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("controlCylceRate"), new GUIContent(" "));






			GUILayout.Space(20f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Autopilot Functions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Speed Hold", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("autoThrottle"), new GUIContent("State"));
			if (computer.autoThrottle == SilantroFlightComputer.ControlState.Active)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("machHold"), new GUIContent("Mach Hold"));
				if (computer.machHold == SilantroFlightComputer.ControlState.Off)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("commandSpeed"), new GUIContent("Command Speed"));
				}
				else
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("commandMach"), new GUIContent("Command Mach"));
				}
			}

			if (computer.operationMode != SilantroFlightComputer.AugmentationType.Manual)
			{
				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Altitude Hold", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("altitudeHold"), new GUIContent("State"));
				if (computer.altitudeHold == SilantroFlightComputer.ControlState.Active)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("commandAltitude"), new GUIContent("Command Altitude"));
					GUILayout.Space(5f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumClimbRate"), new GUIContent("Maximum Climb Rate"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchRate"), new GUIContent("Maximum Pitch Rate"));
				}

				if (computer.altitudeHold != SilantroFlightComputer.ControlState.Active)
				{
					GUILayout.Space(5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox("Pitch Hold", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchHold"), new GUIContent("State"));
					if (computer.pitchHold == SilantroFlightComputer.ControlState.Active)
					{
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("commandPitchAngle"), new GUIContent("Command Pitch Angle"));
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("balancePitchRate"), new GUIContent("Pitch Rate Limit"));
						GUILayout.Space(3f);
						EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchBreakPoint"), new GUIContent("Break Point"));
					}
				}


				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Bank Hold", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("bankHold"), new GUIContent("State"));
				if (computer.bankHold == SilantroFlightComputer.ControlState.Active)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("commandBankAngle"), new GUIContent("Command Bank"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceRollRate"), new GUIContent("Roll Rate Limit"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("rollBreakPoint"), new GUIContent("Break Point"));
				}


				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Heading Hold", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("headingHold"), new GUIContent("State"));
				if (computer.headingHold == SilantroFlightComputer.ControlState.Active)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("commandHeading"), new GUIContent("Command Heading"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTurnBank"), new GUIContent("Maximum Bank"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTurnRate"), new GUIContent("Turn Rate Limit"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("balanceRollRate"), new GUIContent("Roll Rate Limit"));
					GUILayout.Space(10f);
				}
			}
		}





		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Limit Functions", MessageType.None);
		GUI.color = backgroundColor;

		if (computer.operationMode != SilantroFlightComputer.AugmentationType.Manual)
		{
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Bank Limiter", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bankLimiter"), new GUIContent("State"));
			if (computer.bankLimiter == SilantroFlightComputer.ControlState.Active)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumBankAngle"), new GUIContent("Maximum Bank"));
				GUILayout.Space(5f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rollBreakPoint"), new GUIContent("Roll Break Point"));
				if (computer.operationMode == SilantroFlightComputer.AugmentationType.CommandAugmentation && computer.commandPreset == SilantroFlightComputer.CommandPreset.Airliner)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("absoluteBankAngle"), new GUIContent("Absolute Bank"));
				}
				GUILayout.Space(10f);
			}

			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Pitch Limiter", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchLimiter"), new GUIContent("State"));
			if (computer.pitchLimiter == SilantroFlightComputer.ControlState.Active)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchAngle"), new GUIContent("Maximum Pitch"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumPitchAngle"), new GUIContent("Minimum Pitch"));
				GUILayout.Space(5f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchBreakPoint"), new GUIContent("Pitch Break Point"));
				GUILayout.Space(10f);
			}

			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Load Factor(G) Limiter", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("gLimiter"), new GUIContent("State"));
			if (computer.gLimiter == SilantroFlightComputer.ControlState.Active)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumLoadFactor"), new GUIContent("Maximum +G"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumLoadFactor"), new GUIContent("Maximum -G"));
				
				
				GUILayout.Space(5f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("gWarner"), new GUIContent("G Warner"));

				if (computer.gWarner == SilantroFlightComputer.ControlState.Active)
				{
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gClip"), new GUIContent("Warning Tone"));
					GUILayout.Space(3f);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("gAlarmVolume"), new GUIContent("Warner Volume"));
					GUILayout.Space(3f);
					EditorGUILayout.LabelField("G Threshold", computer.gThreshold.ToString() + " G");
				}
				GUILayout.Space(10f);
			}
		}


		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Stall Warner", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("stallWarner"), new GUIContent("State"));
		if (computer.stallWarner == SilantroFlightComputer.ControlState.Active)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("stallClip"), new GUIContent("Warning Tone"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("alarmVolume"), new GUIContent("Warner Volume"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("alphaThreshold"), new GUIContent("Stall Threshold"));

			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Base Stall α", computer.baseStallAngle.ToString("0.00"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Alpha Floor", computer.alphaFloor.ToString("0.00"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Alpha Prot", computer.alphaProt.ToString("0.00"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Curren α Max", computer.maximumWingAlpha.ToString("0.00"));
			GUILayout.Space(10f);
		}


		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Speed Protector", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("speedProtector"), new GUIContent("State"));
		if (computer.speedProtector == SilantroFlightComputer.ControlState.Active)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("protectorMode"), new GUIContent("Mode"));
			GUILayout.Space(3f);
			if(computer.protectorMode == SilantroFlightComputer.ControlMode.Automatic)
			{
				EditorGUILayout.LabelField("Stall Speed", (computer.stallSpeed*Oyedoyin.MathBase.toKnots).ToString("0.0") + " knots");
			}
			else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumSpeed"), new GUIContent("Minimum Speed"));
			}
			GUILayout.Space(10f);
		}

		if (computer.operationMode != SilantroFlightComputer.AugmentationType.Autonomous)
		{
			GUILayout.Space(20f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Structural Functions", MessageType.None);
			GUI.color = backgroundColor;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("flapControl"), new GUIContent("Flap Control"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("gearControl"), new GUIContent("Gear Control"));
			if (computer.gearControl == SilantroFlightComputer.ControlMode.Automatic)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumGearSpeed"), new GUIContent("Maximum Gear Speed"));
				GUILayout.Space(3f);
			}

			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("autoSlat"), new GUIContent("Auto Slat"));
			if (computer.autoSlat == SilantroFlightComputer.ControlState.Active)
			{
				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Slat Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("takeOffSlat"), new GUIContent("Takeoff Slat"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("performanceSlat"), new GUIContent("Performance Slat"));
				GUILayout.Space(5f);
			}

			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("airbrakeControl"), new GUIContent("Airbrake Control"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("airbrakeType"), new GUIContent("Airbrake Type"));

			if (computer.airbrakeType != SilantroFlightComputer.AirbrakeType.ActuatorOnly)
			{
				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Control Surface Airbrake", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("aileronSet"), new GUIContent("Aileron Deflection"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("ruddervatorSet"), new GUIContent("Ruddervator Deflection"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("flapSet"), new GUIContent("Flap Deflection"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("actuationSpeed"), new GUIContent("Actuation Speed"));
			}
		}


		GUILayout.Space(20f);
		GUI.color = silantroColor;
		if (GUILayout.Button("Configure Gains"))
		{
			Selection.activeGameObject = computer.gainBoard.gameObject;
		}
		GUI.color = backgroundColor;


		serializedObject.ApplyModifiedProperties();
	}
}
#endif



