using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroBattery : MonoBehaviour
{

	// --------------------------------------------- Selectibles
	public enum BatteryType
	{
		LeadAcid,
		NickelCadmium,
		NickelMetalHydride
		//ADD MORE
	}
	public BatteryType batteryType = BatteryType.LeadAcid;
	public enum State
	{
		Charging,
		Discharging
	}
	public State state = State.Discharging;


	// --------------------------------------------- Variables
	public int cellCount = 5;
	public float nominalCellVoltage;
	public float dischargeCellVoltage;
	public float currentCellVolage;
	public float capacity = 10;

	public float chargeEfficiency;
	public float dischargeEfficiency;
	public float actualVoltage;

	public float standbyDischargeCurrent;
	public float outputVoltage;
	public float outputCurrent;
	public float chargingCurrent;
	float chargingCapacity;

	public bool BatteryLow;
	public bool BatteryFull;
	public float availablePower;
	public float currentCapacity;
	public float timeRemaining;
	float dischargeTime;
	float chargingTime = 1;
	public float batteryLevel;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void Start()
	{
		currentCapacity = capacity;
		chargingCapacity = capacity + (capacity * 0.4f);

		actualVoltage = nominalCellVoltage * cellCount;
		currentCellVolage = (dischargeCellVoltage + ((nominalCellVoltage - dischargeCellVoltage) * batteryLevel / 100f));
		outputVoltage = (currentCellVolage * cellCount) * (dischargeEfficiency / 100f);
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void StartCharging()
	{
		dischargeTime = 0.0f;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		batteryLevel = (currentCapacity / capacity) * 100f;

		if (state == State.Discharging)
		{
			BatteryFull = false;
			if (outputCurrent > 0)
			{
				timeRemaining = (currentCapacity) / outputCurrent;//time remaining in seconds
				currentCapacity -= (outputCurrent * (dischargeTime / (60 * 60)));
			}
			else { currentCapacity -= (standbyDischargeCurrent * (dischargeTime / (60 * 60))); }

			if (batteryLevel < 20) { BatteryLow = true; }
		}
		if (state == State.Charging)
		{
			BatteryLow = false;
			float currentCharge = (currentCapacity + (currentCapacity * 0.4f));
			timeRemaining = (chargingCapacity - currentCharge) / chargingCurrent;
			currentCapacity += (chargingCurrent * (chargingTime / (60 * 60)));

			if (batteryLevel > 100.2f)
			{
				state = State.Discharging;
				BatteryFull = true;
			}
		}

		if (currentCapacity < 0) { currentCapacity = 0; }
		currentCellVolage = (dischargeCellVoltage + ((nominalCellVoltage - dischargeCellVoltage) * batteryLevel / 100f));
		outputVoltage = (currentCellVolage * cellCount) * (dischargeEfficiency / 100f);
		availablePower = capacity * outputVoltage;
	}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroBattery))]
[CanEditMultipleObjects]
public class SilantroBatteryEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroBattery battery;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable() { battery = (SilantroBattery)target; }



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();


		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Specifications", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("capacity"), new GUIContent("Capacity"));

		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Internals", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("batteryType"), new GUIContent("Battery Type"));

		//ASSIGNMENTS
		if (battery.batteryType == SilantroBattery.BatteryType.LeadAcid)
		{
			battery.nominalCellVoltage = 2.1f;
			battery.dischargeCellVoltage = 1.8f;
			battery.dischargeEfficiency = 90f;
			battery.chargeEfficiency = 50f;
			battery.standbyDischargeCurrent = 0.153f;
		}
		if (battery.batteryType == SilantroBattery.BatteryType.NickelCadmium)
		{
			battery.nominalCellVoltage = 1.2f;
			battery.dischargeCellVoltage = 0.91f;
			battery.dischargeEfficiency = 90f;
			battery.chargeEfficiency = 70f;
			battery.standbyDischargeCurrent = 0.1258f;
		}
		if (battery.batteryType == SilantroBattery.BatteryType.NickelMetalHydride)
		{
			battery.nominalCellVoltage = 1.2f;
			battery.dischargeCellVoltage = 1.01139f;
			battery.dischargeEfficiency = 92f;
			battery.chargeEfficiency = 66f;
			battery.standbyDischargeCurrent = 0.139f;
		}

		GUILayout.Space(5f);
		battery.cellCount = EditorGUILayout.IntField("Cell Count", battery.cellCount);
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Nominal Cell Voltage", battery.nominalCellVoltage.ToString() + " Volts");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Current Cell Voltage", battery.currentCellVolage.ToString("0.0") + " Volts");

		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Performance", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Rated Voltage", battery.actualVoltage.ToString("0.0") + " Volts");

		GUILayout.Space(5f);
		EditorGUILayout.LabelField("Output Voltage", battery.outputVoltage.ToString("0.0") + " Volts");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Output Current", battery.outputCurrent.ToString("0.0") + " Amps");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Current Power", battery.currentCapacity.ToString("0.0") + " Ah");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Battery Level", battery.batteryLevel.ToString("0.00") + " %");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Effective Power", (battery.availablePower / 1000).ToString("0.0") + " kWh");

		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Current State", battery.state.ToString());
		//
		if (battery.BatteryLow)
		{
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Battery Low, Please connect a suitable charger", MessageType.Warning);
		}
		if (battery.BatteryFull)
		{
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Battery Full, Please disconnect charger", MessageType.Warning);
		}
		//
		if (battery.state == SilantroBattery.State.Charging)
		{
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Charging Current", battery.chargingCurrent.ToString("0.0") + " Amps");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Charging Time", battery.timeRemaining.ToString("0.0") + " Hours");
		}
		else if (battery.state == SilantroBattery.State.Discharging)
		{
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Time Remaining", battery.timeRemaining.ToString("0.0") + " Hours");
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
