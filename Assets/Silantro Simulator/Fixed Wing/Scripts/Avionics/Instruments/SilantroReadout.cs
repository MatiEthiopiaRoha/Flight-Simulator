using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroReadout : MonoBehaviour
{

	//DATA SOURCE
	[HideInInspector] public SilantroController connectedAircraft;


	//TYPE
	public enum DialType { Airspeed, Altitude }
	[HideInInspector] public DialType dialType = DialType.Airspeed;

	public enum AltimeterType { FourDigit, ThreeDigit }
	[HideInInspector] public AltimeterType altimeterType = AltimeterType.ThreeDigit;

	//DIGITS
	[HideInInspector] public float digitOne;
	[HideInInspector] public float digitTwo;
	[HideInInspector] public float digitThree;
	[HideInInspector] public float digitFour;
	[HideInInspector] public float digitFive;

	//DATA
	float dialValue;

	//POSITIONS
	[HideInInspector] public float digitOneTranslation;
	[HideInInspector] public float digitTwoTranslation;
	[HideInInspector] public float digitThreeTranslation;
	[HideInInspector] public float digitFourTranslation;
	//public float digitFiveTranslation;


	//DIAL ROLLS
	[HideInInspector] public RectTransform digitOneContainer;
	[HideInInspector] public RectTransform digitTwoContainer;
	[HideInInspector] public RectTransform digitThreeContainer;
	[HideInInspector] public RectTransform digitFourContainer;
	//public RectTransform digitFiveContainer;

	[HideInInspector] public RectTransform needle;
	float needleRotation;
	float smoothRotation;
	[HideInInspector] public float maximumValue;

	void Update()
	{
		if (connectedAircraft != null && connectedAircraft.gameObject.activeSelf)
		{
			///--------------------------------------------------------AIRSPEED
			if (dialType == DialType.Airspeed)
			{
				//COLLECT VALUE
				dialValue = connectedAircraft.core.currentSpeed * Oyedoyin.MathBase.toKnots;

				//EXTRACT DIGITS
				digitOne = (dialValue % 10f);
				digitTwo = Mathf.Floor((dialValue % 100.0f) / 10.0f);
				digitThree = Mathf.Floor((dialValue % 1000.0f) / 100.0f);

				//CALCULATE DIAL POSITIONS
				float digitOnePosition = digitOne * -digitOneTranslation;
				float digitTwoPosition = digitTwo * -digitTwoTranslation; if (digitOne > 9.0f) { digitTwoPosition += (digitOne - 9.0f) * -digitTwoTranslation; }
				float digitThreePosition = digitThree * -digitThreeTranslation; if ((digitTwo * 10) > 99.0f) { digitThreePosition += ((digitTwo * 10f) - 99.0f) * -digitThreeTranslation; }

				//SET POSITIONS
				if (digitOneContainer != null) { digitOneContainer.localPosition = new Vector3(0, digitOnePosition, 0); }
				if (digitTwoContainer != null) { digitTwoContainer.localPosition = new Vector3(0, digitTwoPosition, 0); }
				if (digitThreeContainer != null) { digitThreeContainer.localPosition = new Vector3(0, digitThreePosition, 0); }
			}


			///---------------------------------------------------------ALTIMETER
			if (dialType == DialType.Altitude)
			{
				//COLLECT VALUE
				dialValue = connectedAircraft.core.currentAltitude * Oyedoyin.MathBase.toFt;
				maximumValue = 10000f;

				//EXTRACT DIGITS
				digitOne = ((dialValue % 100.0f) / 20.0f);//20FT Spacing
				digitTwo = Mathf.Floor((dialValue % 1000.0f) / 100.0f);
				digitThree = Mathf.Floor((dialValue % 10000.0f) / 1000.0f);
				digitFour = Mathf.Floor((dialValue % 100000.0f) / 10000.0f);

				//CALCULATE DIAL POSITIONS
				float digitOnePosition = digitOne * -digitOneTranslation;
				float digitTwoPosition = digitTwo * -digitTwoTranslation;
				if ((digitOne * 20) > 90.0f) { digitTwoPosition += ((digitOne * 20f) - 90.0f) / 10.0f * -digitTwoTranslation; }
				float digitThreePosition = digitThree * -digitThreeTranslation;
				if ((digitTwo * 100) > 990.0f) { digitThreePosition += ((digitTwo * 100) - 990.0f) / 10.0f * -digitThreeTranslation; }
				float digitFourPosition = 0f;
				if (altimeterType == AltimeterType.FourDigit) { digitFourPosition = digitFour * -digitFourTranslation; if ((digitThree * 1000) > 9990.0f) { digitFourPosition += ((digitThree * 1000) - 9990.0f) / 10f * -digitFourTranslation; } }


				//SET POSITIONS
				if (digitOneContainer != null) { digitOneContainer.localPosition = new Vector3(0, digitOnePosition, 0); }
				if (digitTwoContainer != null) { digitTwoContainer.localPosition = new Vector3(0, digitTwoPosition, 0); }
				if (digitThreeContainer != null) { digitThreeContainer.localPosition = new Vector3(0, digitThreePosition, 0); }
				if (altimeterType == AltimeterType.FourDigit) { if (digitFourContainer != null) { digitFourContainer.localPosition = new Vector3(0, digitFourPosition, 0); } }

			}


			//-----------------------------------------------------------------NEEDLE
			if (needle != null)
			{
				needleRotation = Mathf.Lerp(0, 360, dialValue / maximumValue);
				smoothRotation = Mathf.Lerp(smoothRotation, needleRotation, Time.deltaTime * 5);
				needle.transform.eulerAngles = new Vector3(needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -smoothRotation);
			}
		}
	}
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroReadout))]
public class SilantroReadoutEditor : Editor
{
	Color backgroundColor; Color silantroColor = new Color(1, 0.4f, 0);
	SilantroReadout dial;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		dial = (SilantroReadout)target;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector();
		serializedObject.Update();

		GUI.color = silantroColor; EditorGUILayout.HelpBox("Connected Aircraft", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("connectedAircraft"), new GUIContent(" "));
		GUILayout.Space(3f); GUI.color = silantroColor; EditorGUILayout.HelpBox("Dial Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("dialType"), new GUIContent(" "));
		GUILayout.Space(5f);
		if (dial.dialType == SilantroReadout.DialType.Altitude)
		{
			GUILayout.Space(3f); GUI.color = Color.white;
			EditorGUILayout.HelpBox("Altimeter Type", MessageType.None);
			GUI.color = backgroundColor; GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("altimeterType"), new GUIContent(" "));
		}

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Dial 'Per Digit' Translation", MessageType.None); GUI.color = backgroundColor;
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("digitOneTranslation"), new GUIContent("Digit One"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("digitTwoTranslation"), new GUIContent("Digit Two"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("digitThreeTranslation"), new GUIContent("Digit Three"));

		if (dial.dialType == SilantroReadout.DialType.Altitude && dial.altimeterType == SilantroReadout.AltimeterType.FourDigit)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("digitFourTranslation"), new GUIContent("Digit Four"));
		}


		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Digit Containers", MessageType.None); GUI.color = backgroundColor;
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("digitOneContainer"), new GUIContent("Digit One Container"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("digitTwoContainer"), new GUIContent("Digit Two Container"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("digitThreeContainer"), new GUIContent("Digit Three Container"));

		if (dial.dialType == SilantroReadout.DialType.Altitude && dial.altimeterType == SilantroReadout.AltimeterType.FourDigit)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("digitFourContainer"), new GUIContent("Digit Four Container"));
		}

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Digit Display", MessageType.None); GUI.color = backgroundColor;
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.LabelField("Current Value", dial.digitFour.ToString("0") + "| " + dial.digitThree.ToString("0") + "| " + dial.digitTwo.ToString("0") + "| " + dial.digitOne.ToString("0"));


		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Dial Face Settings", MessageType.None); GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("needle"), new GUIContent("Needle"));

		if (dial.dialType == SilantroReadout.DialType.Airspeed)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumValue"), new GUIContent("Maximum Speed"));
		}




		serializedObject.ApplyModifiedProperties();
	}
}
#endif