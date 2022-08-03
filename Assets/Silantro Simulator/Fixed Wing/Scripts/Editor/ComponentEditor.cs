using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif




#if UNITY_EDITOR

// ---------------------------------------------------- Dial
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroDial))]
public class SilantroDialEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroDial dial;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable() { dial = (SilantroDial)target; }


	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();



		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Dial Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("dialType"), new GUIContent(" "));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationMode"), new GUIContent("Rotation Mode"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("needleType"), new GUIContent("Needle Type"));


		// -------------------------------- Altimeterr
		if (dial.dialType == SilantroDial.DialType.Speed)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Speedometer Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("speedType"), new GUIContent(" "));
		}

		// -------------------------------- Fuel
		if (dial.dialType == SilantroDial.DialType.Fuel)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Fuel Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelType"), new GUIContent(" "));
		}


		// ------------------------------- RPM
		if (dial.dialType == SilantroDial.DialType.RPM)
		{
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Engine Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent(" "));
			GUILayout.Space(5f);

			if (dial.engineType == SilantroDial.EngineType.ElectricMotor)
			{
				//EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroDial.EngineType.TurboFan)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turbofan"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroDial.EngineType.TurboJet)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turbojet"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroDial.EngineType.TurboProp)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turboprop"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroDial.EngineType.PistonEngine)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("piston"), new GUIContent(" "));
			}
		}



		// --------------------------------------------------------------------------------------------------------------------
		if (dial.dialType != SilantroDial.DialType.ArtificialHorizon && dial.dialType != SilantroDial.DialType.Clock)
		{

			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Rotation Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"), new GUIContent("Rotation Direction"));

			if (dial.needleType == SilantroDial.NeedleType.Dual)
			{
				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Support Needle", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("supportRotationAxis"), new GUIContent("Rotation Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("supportDirection"), new GUIContent("Rotation Direction"));
			}


			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Rotation Amounts", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("MinimumAngleDegrees"), new GUIContent("Minimum Angle"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumAngleDegrees"), new GUIContent("Maximum Angle"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Data Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("MinimumValue"), new GUIContent("Minimum Value"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumValue"), new GUIContent("Maximum Value"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Amount", dial.currentValue.ToString("0.00"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);


			if (dial.needleType == SilantroDial.NeedleType.Dual)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("Needle"), new GUIContent("Main Needle"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("supportNeedle"), new GUIContent("Support Needle"));
			}
			else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("Needle"), new GUIContent("Needle"));
			}
		}




		else if (dial.dialType == SilantroDial.DialType.ArtificialHorizon)
		{
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchRotationAxis"), new GUIContent("Pitch Rotation Axis"));
			GUILayout.Space(2f);
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchDirection"), new GUIContent("Pitch Rotation Direction"));

			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("yawRotationAxis"), new GUIContent("Yaw Rotation Axis"));
			GUILayout.Space(2f);
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("yawDirection"), new GUIContent("Yaw Rotation Direction"));

			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rollRotationAxis"), new GUIContent("Roll Rotation Axis"));
			GUILayout.Space(2f);
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rollDirection"), new GUIContent("Roll Rotation Direction"));

			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			dial.Needle = EditorGUILayout.ObjectField("Ball Indicator", dial.Needle, typeof(Transform), true) as Transform;
		}

		serializedObject.ApplyModifiedProperties();
	}
}


// ---------------------------------------------------- Body
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroBody))]
public class SilantroBodyEditor : Editor
{

	private static readonly GUIContent deleteButton = new GUIContent("Remove", "Delete");
	private static readonly GUILayoutOption buttonWidth = GUILayout.Width(60f);

	Color backgroundColor; Color silantroColor = new Color(1, 0.4f, 0);
	SilantroBody body;
	SerializedProperty sectionList;


	SerializedProperty diameter, resolution, finish;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		body = (SilantroBody)target;
		sectionList = serializedObject.FindProperty("sectionPoints");

		diameter = serializedObject.FindProperty("maximumDiameter");
		resolution = serializedObject.FindProperty("resolution");
		finish = serializedObject.FindProperty("surfaceFinish");
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		GUILayout.Space(1f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Section Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(diameter);
		GUILayout.Space(5f);
		resolution.intValue = EditorGUILayout.IntSlider("Resolution", resolution.intValue, 5, 50);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Sections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		if (sectionList != null)
		{
			EditorGUILayout.LabelField("Section Count", sectionList.arraySize.ToString());
		}

		GUILayout.Space(5f);
		if (GUILayout.Button("Create Section")) { body.AddElement(); }

		if (sectionList != null)
		{
			GUILayout.Space(2f);
			for (int i = 0; i < sectionList.arraySize; i++)
			{
				SerializedProperty reference = sectionList.GetArrayElementAtIndex(i);
				SerializedProperty widthPercentage = reference.FindPropertyRelative("sectionDiameterPercentage");
				SerializedProperty heightPercentage = reference.FindPropertyRelative("sectionHeightPercentage");
				SerializedProperty sectionHeight = reference.FindPropertyRelative("height");
				SerializedProperty sectionWidth = reference.FindPropertyRelative("width");

				GUI.color = new Color(1, 0.8f, 0);
				EditorGUILayout.HelpBox("Section : " + (i + 1).ToString(), MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				widthPercentage.floatValue = EditorGUILayout.Slider("Width Percentage", widthPercentage.floatValue, 1f, 100f);
				GUILayout.Space(3f);
				heightPercentage.floatValue = EditorGUILayout.Slider("Height Percentage", heightPercentage.floatValue, 1f, 100f);
				GUILayout.Space(3f);
				EditorGUILayout.LabelField("Section Width", sectionWidth.floatValue.ToString("0.00") + " m");
				GUILayout.Space(1f);
				EditorGUILayout.LabelField("Section Height", sectionHeight.floatValue.ToString("0.00") + " m");

				GUILayout.Space(3f);
				if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth))
				{
					Transform trf = body.sectionPoints[i].sectionTransform;
					body.sectionPoints.RemoveAt(i);
					if (trf != null)
					{
						DestroyImmediate(trf.gameObject);
					}
				}
				GUILayout.Space(5f);
			}
		}

		GUILayout.Space(8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField("Total Length", body.aircraftLength.ToString("0.00") + " m");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Equivalent Diameter", body.maximumSectionDiameter.ToString("0.00") + " m");


		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Flow Consideration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(finish);
		GUILayout.Space(5f);
		EditorGUILayout.LabelField("Wetted Area", body.totalArea.ToString("0.000") + " m2");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Sector Area", body.maximumCrossArea.ToString("0.000") + " m2");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Fineness Ratio", body.finenessRatio.ToString("0.00"));

		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Output Data", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField("Friction Coefficient", body.skinDragCoefficient.ToString("0.00000"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Drag", body.totalDrag.ToString("0.0") + " N");


		serializedObject.ApplyModifiedProperties();
	}

	//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	public override bool RequiresConstantRepaint()
	{
		return true;
	}
}


// ---------------------------------------------------- Lever
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroLever))]
public class SilantroLeverEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroLever lever;


	//------------------------------------------------------------------------
	private SerializedProperty leverType;
	private SerializedProperty stickType;
	private SerializedProperty leverObj;
	private SerializedProperty yokeObj;

	private SerializedProperty pitchRotationAxis;
	private SerializedProperty MaximumPitchDeflection;
	private SerializedProperty pitchAxis;

	private SerializedProperty rollRotationAxis;
	private SerializedProperty MaximumRollDeflection;
	private SerializedProperty rollAxis;



	private void OnEnable()
	{
		lever = (SilantroLever)target;

		leverType = serializedObject.FindProperty("leverType");
		stickType = serializedObject.FindProperty("stickType");
		leverObj = serializedObject.FindProperty("lever");
		yokeObj = serializedObject.FindProperty("yoke");

		pitchRotationAxis = serializedObject.FindProperty("pitchRotationAxis");
		MaximumPitchDeflection = serializedObject.FindProperty("MaximumPitchDeflection");
		pitchAxis = serializedObject.FindProperty("pitchDirection");

		rollRotationAxis = serializedObject.FindProperty("rollRotationAxis");
		MaximumRollDeflection = serializedObject.FindProperty("MaximumRollDeflection");
		rollAxis = serializedObject.FindProperty("rollDirection");
	}


	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Lever Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(leverType, new GUIContent(" "));


		// -------------------------------------------------------- Control Stick
		if (lever.leverType == SilantroLever.LeverType.Stick)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(stickType, new GUIContent("Control Type"));
			GUILayout.Space(5f);
			if (lever.stickType == SilantroLever.StickType.Joystick)
			{
				GUILayout.Space(5f);
				EditorGUILayout.PropertyField(leverObj, new GUIContent("Stick"));
			}
			else
			{
				GUILayout.Space(5f);
				EditorGUILayout.PropertyField(leverObj, new GUIContent("Yoke Stick"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(yokeObj, new GUIContent("Yoke Wheel"));
			}

			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(pitchRotationAxis, new GUIContent("Pitch Rotation Axis"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(MaximumPitchDeflection, new GUIContent("Max Pitch Deflection"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(pitchAxis, new GUIContent("Pitch Direction"));

			GUILayout.Space(8f);
			EditorGUILayout.PropertyField(rollRotationAxis, new GUIContent("Roll Rotation Axis"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(MaximumRollDeflection, new GUIContent("Max Roll Deflection"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(rollAxis, new GUIContent("Roll Direction"));
		}



		// -------------------------------------------------------- Throttle
		if (lever.leverType == SilantroLever.LeverType.Throttle)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(leverObj, new GUIContent("Throttle Lever"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleMode"), new GUIContent("Mode"));

			if (lever.throttleMode == SilantroLever.ThrottleMode.Deflection)
			{
				GUILayout.Space(10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"), new GUIContent("Rotation Direction"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Maximum Deflection"));
			}

			if (lever.throttleMode == SilantroLever.ThrottleMode.Sliding)
			{
				GUILayout.Space(10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Sliding Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Sliding Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Maximum Slide Distance"));
			}
		}



		// -------------------------------------------------------- Flaps
		if (lever.leverType == SilantroLever.LeverType.Flaps)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(leverObj, new GUIContent("Flap Lever"));
			GUILayout.Space(2f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"), new GUIContent("Rotation Direction"));
		}


		// -------------------------------------------------------- Rudder Pedals
		if (lever.leverType == SilantroLever.LeverType.Pedal)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pedalType"), new GUIContent("Pedal Type"));

			if (lever.pedalType == SilantroLever.PedalType.Hinged)
			{
				GUILayout.Space(10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Right Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rightPedal"), new GUIContent("Right Pedal"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rightRotationAxis"), new GUIContent("Rotation Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rightDirection"), new GUIContent("Rotation Deflection"));


				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Left Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("leftPedal"), new GUIContent("Left Pedal"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("leftRotationAxis"), new GUIContent("Rotation Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("leftDirection"), new GUIContent("Rotation Deflection"));

				GUILayout.Space(10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("pedalMode"), new GUIContent("Clamped Together"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Maximum Deflection"));
			}

			if (lever.pedalType == SilantroLever.PedalType.Sliding)
			{
				GUILayout.Space(10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Right Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rightPedal"), new GUIContent("Right Pedal"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rightRotationAxis"), new GUIContent("Sliding Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rightDirection"), new GUIContent("Sliding Deflection"));


				GUILayout.Space(5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Left Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("leftPedal"), new GUIContent("Left Pedal"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("leftRotationAxis"), new GUIContent("Sliding Axis"));
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("leftDirection"), new GUIContent("Sliding Deflection"));


				GUILayout.Space(10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Sliding Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSlidingDistance"), new GUIContent("Sliding Distance (cm)"));
			}
		}



		if (lever.leverType == SilantroLever.LeverType.GearIndicator)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(leverObj, new GUIContent("Lever Handle"));
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;

			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"), new GUIContent("Rotation Direction"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Maximum Deflection"));


			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Bulb Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bulbMaterial"), new GUIContent("Bulb Material"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumBulbEmission"), new GUIContent("Maximum Emission"));
		}
		serializedObject.ApplyModifiedProperties();
	}
}

#endif