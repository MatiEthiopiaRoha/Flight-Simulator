using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//

public class SilantroSapphire : MonoBehaviour
{

	public Light sun;
	public float fullDaySeconds = 100f;
	public float startHour = 9;//Start at 9am
	public float currentTime = 0;
	public float multiplier = 1f;
	public int hour, minute;
	public string CurrentTime;
	public float sunMaximumIntensity = 1.3f;
	public float localTemperature;
	public AnimationCurve temperatureCurve;
	public float maximumSolarIntensity = 1000f;
	public float currentSolarIntensity;
	public float sunAngle;
	[Range(0, 359f)] public float sunClimateAngle = 107;
	float superlation;

	//WIND SETTINGS
	public float magnitude, headingDirection;
	public Vector3 airVelocity;
	public AnimationCurve windCurve;
	public float currentSeconds;

	public enum BeaufortScale
	{
		Calm,//0.0485
		LightAir,//0.097
		LightBreeze,//0.267
		GentleBreeze,//0.485
		ModerateBreeze,//0.75
		FreshBreeze,//1.04
		StrongBreeze,//1.36
		ModerateGale,//1.699
		FreshGale,//2.06
		StrongGale,//2.451
		Storm,//2.86
		ViolentStorm,//3.3
		Hurricane//3.54
	}
	public BeaufortScale windScale;
	public float speedFactor, knotWind, changeTime, directionChangeSpeed = 5; float step, timer;
	public Transform directionPointer;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		float actualStartHour = startHour / 24f;
		currentTime = actualStartHour;

		//CREATE POINTER
		if (directionPointer == null)
		{
			GameObject pointer = new GameObject("Direction Pointer");
			directionPointer = pointer.transform; directionPointer.parent = this.transform;
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	IEnumerator ChangeDirectionRoutine(Vector3 newDirection)
	{
		Quaternion initRotation = directionPointer.rotation;
		Quaternion goalRotation = Quaternion.LookRotation(newDirection);
		while (initRotation != goalRotation)
		{
			directionPointer.rotation = Quaternion.RotateTowards(directionPointer.rotation, goalRotation, directionChangeSpeed * Time.deltaTime);
			_direction = directionPointer.forward;
			yield return null;
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public Vector3 direction { get { return _direction; } }
	public Vector3 _direction;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PlotWindSpeeds()
	{
		windCurve = new AnimationCurve(); windCurve.AddKey(new Keyframe(0, 13.8f));
		windCurve.AddKey(new Keyframe(0.5833f, 13.8f)); windCurve.AddKey(new Keyframe(1, 19.5f));
		windCurve.AddKey(new Keyframe(1.25f, 16.1f)); windCurve.AddKey(new Keyframe(1.5833f, 20.7f));
		windCurve.AddKey(new Keyframe(2, 16f)); windCurve.AddKey(new Keyframe(2.3333f, 13.8f));
		windCurve.AddKey(new Keyframe(2.90f, 11.5f)); windCurve.AddKey(new Keyframe(3.333f, 12.5f));
		windCurve.AddKey(new Keyframe(3.90f, 10.4f)); windCurve.AddKey(new Keyframe(4, 12.7f));
		windCurve.AddKey(new Keyframe(4.333f, 11.5f)); windCurve.AddKey(new Keyframe(4.5833f, 9.2f));
		windCurve.AddKey(new Keyframe(5.0833f, 11.5f)); windCurve.AddKey(new Keyframe(5.333f, 9.2f));
		windCurve.AddKey(new Keyframe(5.5833f, 8.1f)); windCurve.AddKey(new Keyframe(6.333f, 9.2f));
		windCurve.AddKey(new Keyframe(6.5833f, 8.1f)); windCurve.AddKey(new Keyframe(7.0833f, 5.7f));
		windCurve.AddKey(new Keyframe(7.5833f, 6.9f)); windCurve.AddKey(new Keyframe(7.90f, 5.7f));
		windCurve.AddKey(new Keyframe(8.333f, 8.1f)); windCurve.AddKey(new Keyframe(8.583f, 8.1f));
		windCurve.AddKey(new Keyframe(8.90f, 11.5f)); windCurve.AddKey(new Keyframe(9.0833f, 8.1f));
		windCurve.AddKey(new Keyframe(9.33f, 10.4f)); windCurve.AddKey(new Keyframe(9.90f, 9.2f));
		windCurve.AddKey(new Keyframe(10.0833f, 9.2f)); windCurve.AddKey(new Keyframe(10.5833f, 11.5f));
		windCurve.AddKey(new Keyframe(11.5833f, 9.2f)); windCurve.AddKey(new Keyframe(11.90f, 10.4f));
		windCurve.AddKey(new Keyframe(12.25f, 8.1f)); windCurve.AddKey(new Keyframe(12.583f, 11.2f));
		windCurve.AddKey(new Keyframe(12.9167f, 10.4f)); windCurve.AddKey(new Keyframe(13.333f, 8.1f));
		windCurve.AddKey(new Keyframe(13.9167f, 11.5f)); windCurve.AddKey(new Keyframe(14.333f, 10.4f));
		windCurve.AddKey(new Keyframe(14.583f, 15f)); windCurve.AddKey(new Keyframe(14.90f, 10.4f));
		windCurve.AddKey(new Keyframe(15.583f, 13.8f)); windCurve.AddKey(new Keyframe(15.90f, 13.8f));
		windCurve.AddKey(new Keyframe(16.333f, 11.5f)); windCurve.AddKey(new Keyframe(16.583f, 13.8f));
		windCurve.AddKey(new Keyframe(16.90f, 13.8f)); windCurve.AddKey(new Keyframe(18f, 10.4f));
		windCurve.AddKey(new Keyframe(19f, 10.3f)); windCurve.AddKey(new Keyframe(20f, 9.1f));
		windCurve.AddKey(new Keyframe(21f, 8.1f)); windCurve.AddKey(new Keyframe(22f, 8.5f));
		windCurve.AddKey(new Keyframe(23f, 9.1f)); windCurve.AddKey(new Keyframe(24f, 10.5f));
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ChangeDirection(Vector3 newDirection = default(Vector3))
	{
		timer = 0f;
		if (newDirection == default(Vector3))
		{
			newDirection = Random.insideUnitSphere; newDirection.y = 0; newDirection = newDirection.normalized;
		}
		StopAllCoroutines(); StartCoroutine(ChangeDirectionRoutine(newDirection));
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		UpdateSun();

		//SOLVE TIME
		currentTime += (Time.deltaTime / fullDaySeconds) * multiplier;
		if (currentTime >= 1) { currentTime = 0; }
		headingDirection = directionPointer.transform.eulerAngles.y;

		//CALCULATE LOCAL TEMPERATURE
		float localTime = 24f * currentTime;
		if (temperatureCurve != null) { localTemperature = temperatureCurve.Evaluate(localTime); }

		//WIND
		if (windScale == BeaufortScale.Calm) { speedFactor = 0.0485f; step = 1; }
		if (windScale == BeaufortScale.LightAir) { speedFactor = 0.097f; step = 2; }
		if (windScale == BeaufortScale.LightBreeze) { speedFactor = 0.267f; step = 3; }
		if (windScale == BeaufortScale.GentleBreeze) { speedFactor = 0.485f; step = 4; }
		if (windScale == BeaufortScale.ModerateBreeze) { speedFactor = 0.75f; step = 5; }
		if (windScale == BeaufortScale.FreshBreeze) { speedFactor = 1.04f; step = 6; }
		if (windScale == BeaufortScale.StrongBreeze) { speedFactor = 1.36f; step = 7; }
		if (windScale == BeaufortScale.ModerateGale) { speedFactor = 1.699f; step = 8; }
		if (windScale == BeaufortScale.FreshGale) { speedFactor = 2.06f; step = 9; }
		if (windScale == BeaufortScale.StrongGale) { speedFactor = 2.451f; step = 10; }
		if (windScale == BeaufortScale.Storm) { speedFactor = 2.86f; step = 11; }
		if (windScale == BeaufortScale.ViolentStorm) { speedFactor = 3.30f; step = 12; }
		if (windScale == BeaufortScale.Hurricane) { speedFactor = 3.54f; step = 13; }
		directionChangeSpeed = ((95 * (step - 1)) + 60f) / 12f;
		changeTime = ((1.9f * (1 - step)) + 24f) / 12f;
		magnitude = windCurve.Evaluate(currentTime * 24f) * 0.4472f * speedFactor;
		knotWind = magnitude * 1.946f;
		airVelocity = directionPointer.forward * magnitude;


		timer += Time.deltaTime;
		if (timer > changeTime) { ChangeDirection(); }
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		PlotTemperature();
		if (windCurve == null) { PlotWindSpeeds(); }
	}
#endif

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PlotTemperature()
	{
		temperatureCurve = new AnimationCurve();
		temperatureCurve.AddKey(new Keyframe(0, 23f)); temperatureCurve.AddKey(new Keyframe(1, 23f));
		temperatureCurve.AddKey(new Keyframe(2, 22f)); temperatureCurve.AddKey(new Keyframe(3, 22f));
		temperatureCurve.AddKey(new Keyframe(4, 22f)); temperatureCurve.AddKey(new Keyframe(5, 22f));
		temperatureCurve.AddKey(new Keyframe(6, 21f)); temperatureCurve.AddKey(new Keyframe(7, 22f));
		temperatureCurve.AddKey(new Keyframe(8, 23f)); temperatureCurve.AddKey(new Keyframe(9, 24f));
		temperatureCurve.AddKey(new Keyframe(10, 26f)); temperatureCurve.AddKey(new Keyframe(11, 27f));
		temperatureCurve.AddKey(new Keyframe(12, 28f)); temperatureCurve.AddKey(new Keyframe(13, 29f));
		temperatureCurve.AddKey(new Keyframe(14, 28f)); temperatureCurve.AddKey(new Keyframe(15, 27f));
		temperatureCurve.AddKey(new Keyframe(16, 27f)); temperatureCurve.AddKey(new Keyframe(17, 27f));
		temperatureCurve.AddKey(new Keyframe(18, 26.1f)); temperatureCurve.AddKey(new Keyframe(19, 23.99f));
		temperatureCurve.AddKey(new Keyframe(20, 23.95f)); temperatureCurve.AddKey(new Keyframe(21, 22.95f));
		temperatureCurve.AddKey(new Keyframe(22, 21.95f)); temperatureCurve.AddKey(new Keyframe(23, 21.85f));
	}
	//
	public float currentHour; public float currentMinute;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void UpdateSun()
	{
		string definition;
		currentHour = (24f * currentTime);
		currentMinute = 60 * (currentHour - Mathf.Floor(currentHour));
		if (currentHour >= 13) { currentHour = currentHour - 12; definition = " PM"; } else { definition = " AM"; }
		currentSeconds = 60 * (currentMinute - Mathf.Floor(currentMinute));

		hour = (int)currentHour;
		minute = (int)currentMinute;
		CurrentTime = hour.ToString() + ":" + minute.ToString("00") + definition;
		sun.transform.eulerAngles = new Vector3(currentTime * 360 - 90, sunClimateAngle, 180);
		//sun.transform.localRotation = Quaternion.Euler ((currentTime * 360f) - 90, 107, 0);
		sunAngle = sun.transform.localRotation.eulerAngles.x;
		if (sunAngle > 0 && sunAngle < 180)
		{
			float sunRadians = sunAngle * Mathf.Deg2Rad;
			superlation = 1 - Mathf.Cos(sunRadians);
		}
		else
		{
			superlation = 0;
		}


		float intensityMultiplier = 1;
		if (currentTime <= 0.23f || currentTime >= 0.75f) { intensityMultiplier = 0; }
		else if (currentTime <= 0.25f) { intensityMultiplier = Mathf.Clamp01((currentTime - 0.23f) * (1 / 0.02f)); }
		else if (currentTime >= 0.73f) { intensityMultiplier = Mathf.Clamp01(1 - ((currentTime - 0.73f) * (1 / 0.02f))); }

		sun.intensity = sunMaximumIntensity * intensityMultiplier;
		currentSolarIntensity = maximumSolarIntensity * intensityMultiplier * superlation;
	}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroSapphire))]
public class WeatherEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);


	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor; 
		//DrawDefaultInspector();
		SilantroSapphire weather = (SilantroSapphire)target;
		serializedObject.Update();


		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Weather Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Time Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("startHour"), new GUIContent("Start Hour"));

		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Game time in Real-World minutes", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fullDaySeconds"), new GUIContent(" "));
		GUILayout.Space(6f);
		EditorGUILayout.LabelField("Current Time", weather.CurrentTime);

		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Solar Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("sun"), new GUIContent("Sun"));
		GUILayout.Space(8f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("sunMaximumIntensity"), new GUIContent("Maximum Intensity"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("currentSolarIntensity"), new GUIContent("Radiation Intensity"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("sunClimateAngle"), new GUIContent("Solar Angle"));


		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Temperature Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.CurveField("Temperature Curve", weather.temperatureCurve);
		GUILayout.Space(6f);
		EditorGUILayout.LabelField("Local Temperature", weather.localTemperature.ToString("0.0") + " °C");

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Wind Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("windScale"), new GUIContent("Beaufort Scale"));
		GUILayout.Space(3f);
		EditorGUILayout.CurveField("Wind Curve", weather.windCurve);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Wind Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Speed", weather.knotWind.ToString("0.00") + " knots");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Direction", weather.headingDirection.ToString("0.0") + " °");

		serializedObject.ApplyModifiedProperties();
	}

}
#endif