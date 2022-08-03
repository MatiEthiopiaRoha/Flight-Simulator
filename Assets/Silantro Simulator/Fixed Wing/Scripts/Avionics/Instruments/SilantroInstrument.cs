using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



public class SilantroInstrument : MonoBehaviour
{


	// --------------------------------------------------------- Selectibles
	public enum DisplayType { Speedometer, Mach, Altimeter, Variometer, Compass, Horizon, FuelQuantity, EnginePower, Temperature, Tachometer }
	public DisplayType displayType = DisplayType.Speedometer;
	public enum SpeedType { Knots, MPH, KPH }
	public SpeedType speedType = SpeedType.Knots;
	public enum FuelType { kiloGrams, Pounds, Gallons }
	public FuelType fuelType = FuelType.kiloGrams;
	public enum EngineType { TurboFan, TurboJet, TurboProp, ElectricMotor, PistonEngine }
	public EngineType engineType = EngineType.PistonEngine;



	// --------------------------------------------------------- Connections
	public SilantroController connectedAircraft;
	public SilantroTurboFan turbofan;
	public SilantroTurboJet turbojet;
	public SilantroTurboProp turboprop;
	public SilantroPistonEngine piston;
	public RectTransform needle;
	public RectTransform pitchTape;
	public RectTransform rollAnchor;
	public RectTransform TemperatureNeedle;
	public Text valueOutput;


	// --------------------------------------------------------- Variables
	public float currentValue;
	public float maximumValue;
	public float maximumRotation = 360f;
	public float minimumRotation = 0f;
	float needleRotation;
	float smoothRotation;

	// --------------------------------------------------------- Variables
	public float inputFactor = 1f;
	public float rotationFactor = 1f;
	public float rollValue;
	public float pitchValue;
	public float minimumPosition;
	public float movementFactor;
	public float maximumPitch;
	public float minimumPitch;

	public float minimumRoll;
	public float maximumRoll;
	public float minimumValue;
	private float TemperatureNeedlePosition = 0.0f;
	private float smoothedTemperatureNeedlePosition = 0.0f;
	public float minimumTemperaturePosition = 20.0f;
	public float maximumTemperaturePosition = 160.0f;





	void Update()
	{

		if (connectedAircraft != null && connectedAircraft.gameObject.activeSelf)
		{
			// --------------------------------- Speed
			if (displayType == DisplayType.Speedometer)
			{
				float baseSpeed = connectedAircraft.core.currentSpeed;

				if (speedType == SpeedType.Knots) { currentValue = MathBase.ConvertSpeed(baseSpeed, "KTS"); }
				if (speedType == SpeedType.MPH) { currentValue = MathBase.ConvertSpeed(baseSpeed, "MPH"); }
				if (speedType == SpeedType.KPH) { currentValue = MathBase.ConvertSpeed(baseSpeed, "KPH"); }
			}


			// -------------------------------- Altitude
			if (displayType == DisplayType.Altimeter)
			{
				float baseAltitude = MathBase.ConvertDistance(connectedAircraft.core.currentAltitude, "FT");
				currentValue = baseAltitude / 1000f;
			}

			// -------------------------------- Compass
			if (displayType == DisplayType.Compass) { currentValue = connectedAircraft.core.headingDirection; }


			// -------------------------------- RPM
			if (displayType == DisplayType.Tachometer)
			{
				if (engineType == EngineType.TurboJet && turbojet != null) { currentValue = turbojet.core.coreFactor * 100; }
				if (engineType == EngineType.TurboFan && turbofan != null) { currentValue = turbofan.core.coreFactor * 100; }
				if (engineType == EngineType.TurboProp && turboprop != null) { currentValue = turboprop.core.coreFactor * 100; }
				if (engineType == EngineType.PistonEngine && piston != null) { currentValue = piston.core.coreFactor * 100; }
			}



			// -------------------------------- Mach
			if (displayType == DisplayType.Mach)
			{
				currentValue = connectedAircraft.core.machSpeed;
			}


			// -------------------------------- Variometer
			if (displayType == DisplayType.Variometer)
			{
				currentValue = connectedAircraft.core.verticalSpeed * MathBase.toFtMin;

				//if(currentValue > maximumValue) { currentValue = maximumValue; }
				//if(currentValue < minimumValue) { currentValue = minimumValue; }
				float valueDelta = (currentValue - minimumValue) / (maximumValue - minimumValue);
				float angleDeltaDegrees = minimumRotation + ((maximumRotation - minimumRotation) * valueDelta);

				// ------------------------ Set
				needle.transform.eulerAngles = new Vector3(needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -angleDeltaDegrees);
			}


			// -------------------------------- Fuel
			if (displayType == DisplayType.FuelQuantity)
			{
				currentValue = connectedAircraft.fuelLevel;
			}


			// -------------------------------- Power
			if (displayType == DisplayType.EnginePower)
			{
				currentValue = connectedAircraft.silantroEnginePower * 100f;
			}


			// -------------------------------- Temperature
			if (displayType == DisplayType.Temperature)
			{
				currentValue = connectedAircraft.silantroEnginePower * maximumValue;
				TemperatureNeedlePosition = Mathf.Lerp(minimumTemperaturePosition, maximumTemperaturePosition, currentValue / maximumValue);
				smoothedTemperatureNeedlePosition = Mathf.Lerp(smoothedTemperatureNeedlePosition, TemperatureNeedlePosition, Time.deltaTime * 5);
				TemperatureNeedle.transform.localPosition = new Vector3(TemperatureNeedle.transform.eulerAngles.x, smoothedTemperatureNeedlePosition, TemperatureNeedle.transform.eulerAngles.z);
				valueOutput.text = currentValue.ToString("0.0") + " °C";
			}


			// -------------------------------- Horizon
			if (displayType == DisplayType.Horizon && connectedAircraft.aircraft != null)
			{
				if (pitchTape != null)
				{
					pitchValue = Mathf.DeltaAngle(0, -connectedAircraft.aircraft.transform.rotation.eulerAngles.x);
					float extension = minimumPosition + movementFactor * Mathf.Clamp(pitchValue, minimumPitch, maximumPitch) / 10f;
					pitchTape.anchoredPosition3D = new Vector3(pitchTape.anchoredPosition3D.x, extension, pitchTape.anchoredPosition3D.z);
				}

				if (rollAnchor != null)
				{
					rollValue = Mathf.DeltaAngle(0, -connectedAircraft.aircraft.transform.eulerAngles.z);
					float rotation = Mathf.Clamp(rollValue, minimumRoll, maximumRoll);
					rollAnchor.localEulerAngles = new Vector3(rollAnchor.localEulerAngles.x, rollAnchor.localEulerAngles.y, rotation);
				}
			}


			// -------------------------------- Rotate Needle
			if (displayType != DisplayType.Variometer && displayType != DisplayType.Temperature)
			{
				//CONVERT
				float dataValue = currentValue * inputFactor;
				//ROTATE
				if (needle != null)
				{
					needleRotation = Mathf.Lerp(minimumRotation, (maximumRotation * rotationFactor), dataValue / (maximumValue * rotationFactor));
					smoothRotation = Mathf.Lerp(smoothRotation, needleRotation, Time.deltaTime * 5);
					needle.transform.eulerAngles = new Vector3(needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -smoothRotation);
				}
			}


			// -------------------------------- Display Text
			if (valueOutput != null)
			{
				float dataValue = currentValue * inputFactor;
				valueOutput.text = dataValue.ToString("0.0");
			}
		}
	}
}


#if UNITY_EDITOR

// ---------------------------------------------------- Dial
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroInstrument))]
public class SilantroInstrumentEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroInstrument dial;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable() { dial = (SilantroInstrument)target; }


	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Connection", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("connectedAircraft"), new GUIContent(" "));

		GUILayout.Space(5f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Display Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("displayType"), new GUIContent(" "));
		GUILayout.Space(3f);

		// -------------------------------- Altimeterr
		if (dial.displayType == SilantroInstrument.DisplayType.Speedometer)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Speedometer Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("speedType"), new GUIContent(" "));
		}

		// -------------------------------- Fuel
		if (dial.displayType == SilantroInstrument.DisplayType.FuelQuantity)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Fuel Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelType"), new GUIContent(" "));
		}


		// ------------------------------- RPM
		if (dial.displayType == SilantroInstrument.DisplayType.Tachometer)
		{
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Engine Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent(" "));
			GUILayout.Space(5f);

			if (dial.engineType == SilantroInstrument.EngineType.ElectricMotor)
			{
				//EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroInstrument.EngineType.TurboFan)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turbofan"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroInstrument.EngineType.TurboJet)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turbojet"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroInstrument.EngineType.TurboProp)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turboprop"), new GUIContent(" "));
			}
			if (dial.engineType == SilantroInstrument.EngineType.PistonEngine)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("piston"), new GUIContent(" "));
			}
		}



		if (dial.displayType == SilantroInstrument.DisplayType.Horizon)
		{
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Roll Rotation Amounts", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumRoll"), new GUIContent("Minimum Roll"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRoll"), new GUIContent("Maximum Roll"));


			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Pitch Deflection Amounts", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumPitch"), new GUIContent("Minimum Pitch"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitch"), new GUIContent("Maximum Pitch"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("movementFactor"), new GUIContent("Movement Factor"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchTape"), new GUIContent("Pitch Tape"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAnchor"), new GUIContent("Roll Anchor"));
		}
		else if (dial.displayType == SilantroInstrument.DisplayType.Temperature)
		{
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Movement Amounts", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumTemperaturePosition"), new GUIContent("Minimum Position"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumTemperaturePosition"), new GUIContent("Maximum Position"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Data Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("inputFactor"), new GUIContent("Multiplier"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumValue"), new GUIContent("Minimum Value"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumValue"), new GUIContent("Maximum Value"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Amount", dial.currentValue.ToString("0.00"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("TemperatureNeedle"), new GUIContent("Temperature Tape"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("valueOutput"), new GUIContent("Text Display"));
		}
		else
		{

			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Rotation Amounts", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumRotation"), new GUIContent("Minimum Angle"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRotation"), new GUIContent("Maximum Angle"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Data Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("inputFactor"), new GUIContent("Multiplier"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumValue"), new GUIContent("Minimum Value"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumValue"), new GUIContent("Maximum Value"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Amount", dial.currentValue.ToString("0.00"));


			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("needle"), new GUIContent("Needle"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("valueOutput"), new GUIContent("Text Display"));
		}


		serializedObject.ApplyModifiedProperties();
	}
}
#endif