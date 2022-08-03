using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroRocketMotor : MonoBehaviour
{
	//FUNCTION TYPE
	public enum BoosterType
	{
		Aircraft,
		Weapon,
		//ADD MORE IF NEEDED
	}
	public BoosterType boosterType = BoosterType.Weapon;


	//---------------------SELECTIBLES
	public enum BurnType { Regressive, Neutral, Progressive }
	public BurnType burnType = BurnType.Neutral;
	public enum FuelType { Solid, Liquid }
	public FuelType fuelType = FuelType.Liquid;

	//----------------------LIQUID FUEL
	public enum LiquidFuelType
	{
		RP1, RP2, Hydrogen, MMH, UDMHComposite
	}
	public LiquidFuelType liquidfuelType = LiquidFuelType.RP1;

	//----------------------SOLID FUEL
	public enum SolidFuelType
	{
		DB, PVC_AP_AL, PBAA_AP_AL, AN_Polymer, HTPB_AP_AL, PB_AN_AP_AL, PU_AP_AI
	}
	public SolidFuelType solidFuelType = SolidFuelType.DB;

	//----------------------COMBUSTION CHAMBER
	public float chamberPressure = 1000f;
	public float combustionTemperature = 1000f;

	//THROAT
	public float throatTemperature;
	public float throatPressure;
	public float throatArea;
	public float throatDiameter;

	public float gamma = 1.23f;
	public float cp = 2520;
	float R;
	public float specificImpulse;

	//EXHAUST
	public float exhaustTemperature;
	public float exitPressure = 1000f;
	public float exhaustVelocity;
	public float pe, me, Ae = 0.1f;
	public float exhaustMach = 1f;

	//-----------------------GENERAL
	public float ambientPressure = 101.2f;
	public float areaFactor;
	public float pressureThrust, momentumThrust;
	public float meanThrust, Thrust;

	//-----------------------FACTOR
	public AnimationCurve burnCurve;
	public float thrustFactor;
	float activeTime;
	public float engineBurnTime;
	public float fireDuration = 5f;

	public float nozzleArea;

	//EXHAUST EFFECTS
	public ParticleSystem exhaustSmoke;
	ParticleSystem.EmissionModule smokeModule;
	public ParticleSystem exhaustFlame;
	ParticleSystem.EmissionModule flameModule;
	public float maximumSmokeEmissionValue = 50f;
	public float maximumFlameEmissionValue = 50f;

	public AudioClip motorSound;
	AudioSource boosterSound;
	public float maximumPitch = 1.2f;

	public Rigidbody aircraft;
	public Rigidbody weapon;
	public bool active;
	[Range(0, 100)] public float nozzleDiameterPercentage = 1f;
	public float nozzleDiameter = 1f;
	public float boosterDiameter = 1f;
	public float demoArea;
	public float overallLength = 1f;
	public float exhaustConeLength;
	public float fuelLength;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void FireRocket()
	{
		active = true;
		engineBurnTime = 0.0f;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeRocket()
	{
		// -------------------------------------- SETUP FACTORS
		nozzleDiameter = boosterDiameter * nozzleDiameterPercentage / 100f;
		demoArea = (3.142f * nozzleDiameter * nozzleDiameter) / 4f;
		nozzleArea = demoArea;
		// -------------------------------------- SETUP PARTICULES
		if (exhaustSmoke != null) { smokeModule = exhaustSmoke.emission; smokeModule.rateOverTime = 0.0f; }
		if (exhaustFlame != null) { flameModule = exhaustFlame.emission; flameModule.rateOverTime = 0.0f; }
		// -------------------------------------- SETUP SOUND
		if (motorSound) { Handler.SetupSoundSource(this.transform, motorSound, "Booster Sound", 80f, true, true, out boosterSound); }

		// -------------------------------------- SEND CURVE DATA
		PlotDataSet();


		if (boosterType == BoosterType.Aircraft && aircraft == null) { Debug.Log("Aircraft has not been assigned"); return; }
	}











	//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	void PlotDataSet()
	{
		burnCurve = new AnimationCurve();

		//PROGRESSIVE BURN
		if (burnType == BurnType.Progressive)
		{
			burnCurve.AddKey(new Keyframe(0, 0));
			burnCurve.AddKey(new Keyframe(0.041055718f, 0.102848764f));
			burnCurve.AddKey(new Keyframe(0.065982405f, 0.195789207f));
			burnCurve.AddKey(new Keyframe(0.089442815f, 0.311990335f));
			burnCurve.AddKey(new Keyframe(0.11143695f, 0.421551817f));
			burnCurve.AddKey(new Keyframe(0.126099707f, 0.524493136f));
			burnCurve.AddKey(new Keyframe(0.148093842f, 0.644021395f));
			burnCurve.AddKey(new Keyframe(0.171554252f, 0.766867041f));
			burnCurve.AddKey(new Keyframe(0.212609971f, 0.85643164f));
			burnCurve.AddKey(new Keyframe(0.269794721f, 0.916042322f));
			burnCurve.AddKey(new Keyframe(0.348973607f, 0.949001861f));
			burnCurve.AddKey(new Keyframe(0.423753666f, 0.978653754f));
			burnCurve.AddKey(new Keyframe(0.498533724f, 1.008305648f));
			burnCurve.AddKey(new Keyframe(0.57771261f, 1.044587446f));
			burnCurve.AddKey(new Keyframe(0.658357771f, 1.084186631f));
			burnCurve.AddKey(new Keyframe(0.739002933f, 1.117141298f));
			burnCurve.AddKey(new Keyframe(0.799120235f, 1.140197387f));
			burnCurve.AddKey(new Keyframe(0.857771261f, 1.15329157f));
			burnCurve.AddKey(new Keyframe(0.895894428f, 1.0568194f));
			burnCurve.AddKey(new Keyframe(0.920821114f, 0.917201703f));
			burnCurve.AddKey(new Keyframe(0.939882698f, 0.797537047f));
			burnCurve.AddKey(new Keyframe(0.957478006f, 0.64465467f));
			burnCurve.AddKey(new Keyframe(0.972140762f, 0.498426555f));
			burnCurve.AddKey(new Keyframe(0.983870968f, 0.342241405f));
			burnCurve.AddKey(new Keyframe(0.992668622f, 0.202677293f));
			burnCurve.AddKey(new Keyframe(0.998533724f, 0.086378738f));
			burnCurve.AddKey(new Keyframe(1f, 0f));
		}

		//NEUTRAL BURN
		if (burnType == BurnType.Neutral)
		{
			burnCurve.AddKey(new Keyframe(0f, 0f));
			burnCurve.AddKey(new Keyframe(0.011540828f, 0.128099174f));
			burnCurve.AddKey(new Keyframe(0.021510337f, 0.289256198f));
			burnCurve.AddKey(new Keyframe(0.036367648f, 0.47107438f));
			burnCurve.AddKey(new Keyframe(0.046363904f, 0.648760331f));
			burnCurve.AddKey(new Keyframe(0.059603092f, 0.830578512f));
			burnCurve.AddKey(new Keyframe(0.076031721f, 0.983471074f));
			burnCurve.AddKey(new Keyframe(0.142508492f, 1.066115702f));
			burnCurve.AddKey(new Keyframe(0.215324026f, 1.066115702f));
			burnCurve.AddKey(new Keyframe(0.297821552f, 1.049586777f));
			burnCurve.AddKey(new Keyframe(0.381943887f, 1.037190083f));
			burnCurve.AddKey(new Keyframe(0.454739362f, 1.024793388f));
			burnCurve.AddKey(new Keyframe(0.537250261f, 1.016528926f));
			burnCurve.AddKey(new Keyframe(0.618143037f, 1.008264463f));
			burnCurve.AddKey(new Keyframe(0.697404317f, 0.991735537f));
			burnCurve.AddKey(new Keyframe(0.771817914f, 0.979338843f));
			burnCurve.AddKey(new Keyframe(0.841377143f, 0.966942149f));
			burnCurve.AddKey(new Keyframe(0.893076841f, 0.917355372f));
			burnCurve.AddKey(new Keyframe(0.92032416f, 0.756198347f));
			burnCurve.AddKey(new Keyframe(0.936251304f, 0.599173554f));
			burnCurve.AddKey(new Keyframe(0.952198508f, 0.454545455f));
			burnCurve.AddKey(new Keyframe(0.971335152f, 0.280991736f));
			burnCurve.AddKey(new Keyframe(0.987289042f, 0.140495868f));
			burnCurve.AddKey(new Keyframe(1.001651555f, 0f));
		}


		//REGRESSIVE
		if (burnType == BurnType.Regressive)
		{
			burnCurve.AddKey(new Keyframe(0.005592615f, 0.006872852f));
			burnCurve.AddKey(new Keyframe(0.027852378f, 0.113402062f));
			burnCurve.AddKey(new Keyframe(0.045896022f, 0.23024055f));
			burnCurve.AddKey(new Keyframe(0.062505415f, 0.371134021f));
			burnCurve.AddKey(new Keyframe(0.079114807f, 0.512027491f));
			burnCurve.AddKey(new Keyframe(0.094280324f, 0.683848797f));
			burnCurve.AddKey(new Keyframe(0.106663971f, 0.841924399f));
			burnCurve.AddKey(new Keyframe(0.127489484f, 0.972508591f));
			burnCurve.AddKey(new Keyframe(0.15672317f, 1.099656357f));
			burnCurve.AddKey(new Keyframe(0.190221106f, 1.182130584f));
			burnCurve.AddKey(new Keyframe(0.249059074f, 1.171821306f));
			burnCurve.AddKey(new Keyframe(0.302323679f, 1.140893471f));
			burnCurve.AddKey(new Keyframe(0.36541627f, 1.092783505f));
			burnCurve.AddKey(new Keyframe(0.436897783f, 1.054982818f));
			burnCurve.AddKey(new Keyframe(0.505582989f, 1.013745704f));
			burnCurve.AddKey(new Keyframe(0.588302675f, 0.951890034f));
			burnCurve.AddKey(new Keyframe(0.649994706f, 0.903780069f));
			burnCurve.AddKey(new Keyframe(0.727083273f, 0.862542955f));
			burnCurve.AddKey(new Keyframe(0.805582027f, 0.81443299f));
			burnCurve.AddKey(new Keyframe(0.856088827f, 0.75257732f));
			burnCurve.AddKey(new Keyframe(0.902446889f, 0.652920962f));
			burnCurve.AddKey(new Keyframe(0.930636172f, 0.525773196f));
			burnCurve.AddKey(new Keyframe(0.943404853f, 0.408934708f));
			burnCurve.AddKey(new Keyframe(0.958979468f, 0.288659794f));
			burnCurve.AddKey(new Keyframe(0.975978708f, 0.151202749f));
			burnCurve.AddKey(new Keyframe(0.999985561f, 0f));
		}
	}





	//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

	void CollectFuelData()
	{
		//LF
		if (fuelType == FuelType.Liquid)
		{
			if (liquidfuelType == LiquidFuelType.RP1) { combustionTemperature = 3670f; }
			else if (liquidfuelType == LiquidFuelType.Hydrogen) { combustionTemperature = 2985f; }
			else if (liquidfuelType == LiquidFuelType.MMH) { combustionTemperature = 1910f; }
			else if (liquidfuelType == LiquidFuelType.RP2) { combustionTemperature = 3459f; }
		}

		//SF
		if (fuelType == FuelType.Solid)
		{
			if (solidFuelType == SolidFuelType.AN_Polymer) { combustionTemperature = 1550f; }
			if (solidFuelType == SolidFuelType.DB) { combustionTemperature = 2550f; }
			if (solidFuelType == SolidFuelType.HTPB_AP_AL) { combustionTemperature = 3538f; }
			if (solidFuelType == SolidFuelType.PBAA_AP_AL) { combustionTemperature = 3440f; }
			if (solidFuelType == SolidFuelType.PB_AN_AP_AL) { combustionTemperature = 3500f; }
			if (solidFuelType == SolidFuelType.PU_AP_AI) { combustionTemperature = 3840f; }
			if (solidFuelType == SolidFuelType.PVC_AP_AL) { combustionTemperature = 3380f; }
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		//SEND CURVE DATA
		PlotDataSet();
		//SEND FUEL DATA
		CollectFuelData();


		//DRAW MARKERS
		nozzleDiameter = boosterDiameter * nozzleDiameterPercentage / 100f;
		demoArea = (3.142f * nozzleDiameter * nozzleDiameter) / 4f;
		Handles.color = Color.red;
		Handles.DrawWireDisc(this.transform.position, this.transform.forward, (nozzleDiameter / 2f));

		Vector3 throatPoint = transform.position + transform.forward * exhaustConeLength;
		Vector3 fuelPoint = throatPoint + transform.forward * fuelLength;

		Handles.color = Color.blue;
		Handles.DrawWireDisc(fuelPoint, transform.forward, (boosterDiameter / 2f));

		Handles.color = Color.red;
		Handles.DrawLine(fuelPoint, throatPoint);
	}
#endif




	//-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	void MotorAnalysis()
	{
		// ------------------ Ambient
		float altitude = transform.position.y;
		float kiloMeter = MathBase.ConvertDistance(altitude, "FT");
		float a = 0.0000004f * kiloMeter * kiloMeter; float b = (0.0351f * kiloMeter);
		ambientPressure = (a - b + 1009.6f) / 10f;

		//BASIC
		R = ((gamma - 1) / gamma) * cp;
		activeTime = engineBurnTime / fireDuration;
		thrustFactor = burnCurve.Evaluate(activeTime);

		//========================================================CHAMBER
		float a4 = gamma / (gamma - 1);
		float b4 = 1 + ((gamma - 1) / 2) * (exhaustMach * exhaustMach);
		float c4 = Mathf.Pow((1 / b4), a4);
		// exitPressure = chamberPressure * (c4);


		//============================================================THROAT
		throatTemperature = combustionTemperature * (2 / (gamma + 1));
		float a1 = gamma / (gamma - 1);
		float b1 = (gamma + 1) / 2f;
		throatPressure = (chamberPressure * 1000) * (1 / Mathf.Pow(b1, a1));

		//============================================================NOZZLE
		float a2 = (gamma - 1) / gamma;
		float b2 = Mathf.Pow(((exitPressure * 1000) / (chamberPressure * 1000)), a2);
		float c2 = 2 * cp * combustionTemperature * (1 - b2);
		exhaustVelocity = Mathf.Sqrt(c2);
		exhaustTemperature = combustionTemperature * b2;
		pe = (exitPressure * 1000) / (R * exhaustTemperature);
		//pe = (exitPressure * 1000) / (287 * exhaustTemperature);
		float d2 = Mathf.Sqrt(gamma * R * exhaustTemperature);
		exhaustMach = exhaustVelocity / (d2);

		float a3 = (gamma + 1) / (gamma - 1);
		float b3 = 2 / (gamma + 1);
		float c3 = 1 + ((gamma - 1) / 2) * (exhaustMach * exhaustMach);
		float d3 = Mathf.Pow((b3 * c3), a3);
		areaFactor = (1 / exhaustMach) * Mathf.Sqrt(d3);

		me = pe * exhaustVelocity * Ae;
		momentumThrust = (me * exhaustVelocity); pressureThrust = (((exitPressure * 1000) - (ambientPressure * 1000)) * Ae);
		meanThrust = momentumThrust + pressureThrust;
		Thrust = meanThrust * thrustFactor;
		specificImpulse = exhaustVelocity / Physics.gravity.magnitude;

		//ANNEX
		float soundVolume = maximumPitch * thrustFactor;
		boosterSound.volume = soundVolume;
		if (exhaustFlame) { flameModule.rateOverTime = maximumFlameEmissionValue * thrustFactor; }
		if (exhaustSmoke) { smokeModule.rateOverTime = maximumSmokeEmissionValue * thrustFactor; }
		//STOP ROCKET
		if (engineBurnTime > fireDuration) { active = false; }
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (active)
		{

			engineBurnTime += Time.deltaTime;
			//SEND CALCULATIONS DATA
			MotorAnalysis();

			//APPLY THRUST
			if (Thrust > 0)
			{
				Vector3 force = this.transform.forward * Thrust;
				if (boosterType == BoosterType.Aircraft) { aircraft.AddForce(force, ForceMode.Force); }
				if (boosterType == BoosterType.Weapon) { weapon.AddForce(force, ForceMode.Force); }
			}
		}
	}
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroRocketMotor))]
[CanEditMultipleObjects]
public class SilantroRocketMotorEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroRocketMotor motor;

	//------------------------------------------------------------------------
	private SerializedProperty coneLength;
	private SerializedProperty fuelLength;

	// ------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		motor = (SilantroRocketMotor)target;

		coneLength = serializedObject.FindProperty("exhaustConeLength");
		fuelLength = serializedObject.FindProperty("fuelLength");
	}


	// ------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();


		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Motor Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("boosterType"), new GUIContent("Booster Connection"));

		if (motor.boosterType == SilantroRocketMotor.BoosterType.Aircraft)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("aircraft"), new GUIContent("Connected Aircraft"));
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Booster Dimensions", MessageType.None);
			GUI.color = backgroundColor;

			EditorGUILayout.PropertyField(serializedObject.FindProperty("boosterDiameter"), new GUIContent("Booster Diameter"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("overallLength"), new GUIContent("Booster Length"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("nozzleDiameterPercentage"), new GUIContent("Nozzle Ratio"));

			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Nozzle Diameter", motor.nozzleDiameter.ToString("0.00") + " m");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField("Nozzle Area", motor.demoArea.ToString("0.00") + " m2");
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Fuel Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			coneLength.floatValue = EditorGUILayout.Slider("Exhaust Cone Length", coneLength.floatValue, 0f, motor.overallLength / 2f);
			GUILayout.Space(2f);
			fuelLength.floatValue = EditorGUILayout.Slider("Fuel Length", fuelLength.floatValue, 0f, motor.overallLength);
		}


		if (motor.boosterType == SilantroRocketMotor.BoosterType.Weapon)
		{
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Motor Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("nozzleDiameterPercentage"), new GUIContent("Nozzle Ratio"));

			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Nozzle Diameter", motor.nozzleDiameter.ToString("0.00") + " m");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField("Nozzle Area", motor.demoArea.ToString("0.00") + " m2");
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Fuel Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			coneLength.floatValue = EditorGUILayout.Slider("Exhaust Cone Length", coneLength.floatValue, 0f, motor.overallLength / 2f);
			GUILayout.Space(2f);
			fuelLength.floatValue = EditorGUILayout.Slider("Fuel Length", fuelLength.floatValue, 0f, motor.overallLength);
		}



		GUILayout.Space(8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Point Pressures in kPa", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("chamberPressure"), new GUIContent("Chamber Pressure"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPressure"), new GUIContent("Exit Pressure"));


		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Fuel Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelType"), new GUIContent("Fuel Type"));
		GUILayout.Space(3f);

		if (motor.fuelType == SilantroRocketMotor.FuelType.Liquid)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("liquidfuelType"), new GUIContent("Liquid Fuel Type"));
		}
		if (motor.fuelType == SilantroRocketMotor.FuelType.Solid)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("solidFuelType"), new GUIContent("Solid Fuel Type"));
		}
		GUILayout.Space(1f);
		EditorGUILayout.LabelField("Combustion Temperature", motor.combustionTemperature.ToString("0") + " °K");
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fireDuration"), new GUIContent("Fire Duration"));
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("burnType"), new GUIContent("Burn Type"));
		GUILayout.Space(2f);
		EditorGUILayout.CurveField("Thrust Curve", motor.burnCurve);

		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Rocket Effects", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSmokeEmissionValue"), new GUIContent("Maximum Emission"));
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustFlame"), new GUIContent("Exhaust Flame"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumFlameEmissionValue"), new GUIContent("Maximum Emission"));


		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("motorSound"), new GUIContent("Booster Sound"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitch"), new GUIContent("Maximum Pitch"));


		GUILayout.Space(20f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Thrust Generated", motor.Thrust.ToString("0.00") + " N");


		serializedObject.ApplyModifiedProperties();
	}
}
#endif