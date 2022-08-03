//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroDisplay : MonoBehaviour
{
	//AIRCRAFT
	public SilantroController connectedAircraft;
	public bool displayPoints;
	//DATA
	public Text speed;
	public Text altitude;
	public Text fuel;
	public Text weight;
	public Text brake;
	public Text density;
	public Text temperature;
	public Text pressure;
	public Text engineName;
	public Text enginePower;
	public Text engineThrust;
	public Text incrementalBrake;
	public Text flapLevel;
	public Text slatLevel;
	public Text Time;
	public Text weaponCount;
	public Text currentWeapon;
	public Text ammoCount;
	public Text gLoad;

	public Text speedLabel;
	public Text altitudeLabel;
	public Text headingLabel;
	public Text climbLabel;
	public GameObject autoPilotContainer;

	public Text pitchRate;
	public Text rollRate;
	public Text yawRate;
	public Text turnRate;


	public Text commandPitch;
	public Text commandRoll;
	public Text commandYaw;

	//
	public enum UnitsSetup
	{
		Metric,
		Imperial,
		Custom
	}
	public UnitsSetup units = UnitsSetup.Metric;
	public enum SpeedUnit
	{
		MeterPerSecond,
		Knots,
		FeetPerSecond,
		MilesPerHour,
		KilometerPerHour,
		Mach
	}
	public SpeedUnit speedUnit = SpeedUnit.MeterPerSecond;
	//
	public enum AltitudeUnit
	{
		Meter,
		Feet,
		NauticalMiles,
		Kilometer
	}
	public AltitudeUnit altitudeUnit = AltitudeUnit.Meter;
	//
	public enum TemperatureUnit
	{
		Celsius,
		Fahrenheit
	}
	public TemperatureUnit temperatureUnit = TemperatureUnit.Celsius;
	//
	public enum WeightUnit
	{
		Tonne,
		Pound,
		Ounce,
		Stone,
		Kilogram
	}
	public WeightUnit weightUnit = WeightUnit.Kilogram;
	//
	public enum ForceUnit
	{
		Newton,
		KilogramForce,
		PoundForce
	}
	public ForceUnit forceUnit = ForceUnit.Newton;
	//
	public enum TorqueUnit
	{
		NewtonMeter,
		PoundForceFeet
	}
	public TorqueUnit torqueUnit = TorqueUnit.NewtonMeter;
	Text[] children;

	//AUTOPILOT VALUES
	float presetSpeed;
	float presetAltitude;
	float presetHeading;
	float presetClimb;

	bool autoPilotActive;

	public void SetAutopilot()
	{
		autoPilotActive = !autoPilotActive;
	}

	public void IncreaseSpeed() { presetSpeed += 10f; }
	public void DecreaseSpeed() { presetSpeed -= 10f; if (presetSpeed <= 0) { presetSpeed = 0f; } }

	public void IncreaseAltitude() { presetAltitude += 100f; }
	public void DecreaseAltitude() { presetAltitude -= 100f; if (presetAltitude <= 0f) { presetAltitude = 0f; } }


	public void IncreaseClimb() { presetClimb += 100f; }
	public void DecreaseClimb() { presetClimb -= 100f; if (presetClimb <= 0f) { presetClimb = 0f; } }


	public void IncreaseHeading() { presetHeading += 1f; if (presetHeading > 360f) { presetHeading = 0f; } }
	public void DecreaseHeading() { presetHeading -= 1f; if (presetHeading < 0f) { presetHeading = 360f; } }







	private void Start()
	{
		//SET BASE AUTOPILOT VALUES
		presetSpeed = 200f; presetHeading = 1f; presetAltitude = 1000f;presetClimb = 1000f;
		autoPilotActive = false;
	}



	//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (connectedAircraft != null && connectedAircraft.core != null && connectedAircraft.flightComputer != null)
		{

			//----------------------------------------------AUTOPILOT
			if (speedLabel) { speedLabel.text = presetSpeed.ToString(); }
			if (altitudeLabel) { altitudeLabel.text = presetAltitude.ToString(); }
			if (headingLabel) { headingLabel.text = presetHeading.ToString(); }
			if (climbLabel) { climbLabel.text = presetClimb.ToString(); }

			if (connectedAircraft != null)
			{

				if (connectedAircraft.flightComputer != null)
				{
					if (autoPilotActive)
					{
						if (autoPilotContainer != null && !autoPilotContainer.activeSelf) { autoPilotContainer.SetActive(true); }


						if (connectedAircraft.flightComputer.headingHold != SilantroFlightComputer.ControlState.Active)
						{
							connectedAircraft.flightComputer.headingHold = SilantroFlightComputer.ControlState.Active;
							connectedAircraft.flightComputer.altitudeHold = SilantroFlightComputer.ControlState.Active;
							connectedAircraft.flightComputer.autoThrottle = SilantroFlightComputer.ControlState.Active;
							connectedAircraft.flightComputer.machHold = SilantroFlightComputer.ControlState.Off;
						}

						connectedAircraft.flightComputer.commandHeading = presetHeading;
						connectedAircraft.flightComputer.commandAltitude = presetAltitude;
						connectedAircraft.flightComputer.commandSpeed = presetSpeed;
						connectedAircraft.flightComputer.maximumClimbRate = presetClimb;
					}
					else
					{
						if (autoPilotContainer != null && autoPilotContainer.activeSelf) { autoPilotContainer.SetActive(false); }

						if (connectedAircraft.flightComputer.headingHold == SilantroFlightComputer.ControlState.Active)
						{
							connectedAircraft.flightComputer.headingHold = SilantroFlightComputer.ControlState.Off;
							connectedAircraft.flightComputer.altitudeHold = SilantroFlightComputer.ControlState.Off;
							connectedAircraft.flightComputer.autoThrottle = SilantroFlightComputer.ControlState.Off;
							connectedAircraft.flightComputer.machHold = SilantroFlightComputer.ControlState.Off;
						}
					}
				}




				if (engineName != null)
				{
					engineName.text = connectedAircraft.aircraftName;
				}
				float gfore = connectedAircraft.core.gForce;
				if (gLoad != null)
				{
					if (gfore > 9f) { gLoad.color = Color.red; } else if (gfore < -4f) { gLoad.color = Color.yellow; } else { gLoad.color = Color.white; }
					gLoad.text = "G-Load = " + connectedAircraft.core.gForce.ToString("0.00");
				}
			}
			if (flapLevel != null) { flapLevel.text = "Flaps = " + (Mathf.Abs(connectedAircraft.flapAngle)).ToString("0.0") + " °"; }
			if (slatLevel != null) { slatLevel.text = "Slats = " + (Mathf.Abs(connectedAircraft.slatAngle)).ToString("0.0") + " °"; }
			if (connectedAircraft.gearHelper != null)
			{
				incrementalBrake.text = "Brake Lever = " + (connectedAircraft.gearHelper.brakeInput * 100f).ToString("0.0") + " %";
				//PARKING BRAKE
				brake.text = connectedAircraft.gearHelper.brakeState.ToString();
			}

			//if (connectedAircraft.core.weatherController != null)
			//{
			//	Time.text = connectedAircraft.core.weatherController.CurrentTime;
			//}


			if (pitchRate != null && pitchRate.gameObject.activeSelf)
			{
				turnRate.text = connectedAircraft.core.turnRate.ToString("0.0") + " °/s";
				if (connectedAircraft.flightComputer.operationMode != SilantroFlightComputer.AugmentationType.CommandAugmentation)
				{
					pitchRate.text = connectedAircraft.core.pitchRate.ToString("0.0") + " °/s";
					rollRate.text = connectedAircraft.core.rollRate.ToString("0.0") + " °/s";
					yawRate.text = connectedAircraft.core.yawRate.ToString("0.0") + " °/s";

					if (commandPitch != null && commandPitch.gameObject.activeSelf) { commandPitch.gameObject.SetActive(false); }
					if (commandRoll != null && commandRoll.gameObject.activeSelf) { commandRoll.gameObject.SetActive(false); }
					if (commandYaw != null && commandYaw.gameObject.activeSelf) { commandYaw.gameObject.SetActive(false); }
				}
				else
				{
					pitchRate.text = connectedAircraft.core.pitchRate.ToString("0.0") + " °/s";
					rollRate.text = connectedAircraft.core.rollRate.ToString("0.0") + " °/s";
					yawRate.text = connectedAircraft.core.yawRate.ToString("0.0") + " °/s";

					if (commandPitch != null && !commandPitch.gameObject.activeSelf) { commandPitch.gameObject.SetActive(true); }
					if (commandRoll != null && !commandRoll.gameObject.activeSelf) { commandRoll.gameObject.SetActive(true); }
					if (commandYaw != null && !commandYaw.gameObject.activeSelf) { commandYaw.gameObject.SetActive(true); }

					if (commandPitch != null)
					{
						commandPitch.text = "C: " + connectedAircraft.flightComputer.commandPitchRate.ToString("0.0") + " °/s";
						commandRoll.text = "C: " + connectedAircraft.flightComputer.commandRollRate.ToString("0.0") + " °/s";
						commandYaw.text = "C: " + connectedAircraft.flightComputer.commandYawRate.ToString("0.0") + " °/s";
					}
				}
			}

			//WEIGHT SETTINGS
			float Weight = connectedAircraft.core.totalWeight;
			if (weightUnit == WeightUnit.Kilogram)
			{
				weight.text = "Weight = " + Weight.ToString("0.0") + " kg";
			}
			if (weightUnit == WeightUnit.Tonne)
			{
				float tonneWeight = Weight * 0.001f;
				weight.text = "Weight = " + tonneWeight.ToString("0.00") + " T";
			}
			if (weightUnit == WeightUnit.Pound)
			{
				float poundWeight = Weight * 2.20462f;
				weight.text = "Weight = " + poundWeight.ToString("0.0") + " lb";
			}
			if (weightUnit == WeightUnit.Ounce)
			{
				float ounceWeight = Weight * 35.274f;
				weight.text = "Weight = " + ounceWeight.ToString("0.0") + " Oz";
			}
			if (weightUnit == WeightUnit.Stone)
			{
				float stonneWeight = Weight * 0.15747f;
				weight.text = "Weight = " + stonneWeight.ToString("0.0") + " St";
			}
			//FUEL
			float Fuel = connectedAircraft.fuelLevel;
			if (weightUnit == WeightUnit.Kilogram)
			{
				fuel.text = "Fuel = " + Fuel.ToString("0.0") + " kg";
			}
			if (weightUnit == WeightUnit.Tonne)
			{
				float tonneWeight = Fuel * 0.001f;
				fuel.text = "Fuel = " + tonneWeight.ToString("0.00") + " T";
			}
			if (weightUnit == WeightUnit.Pound)
			{
				float poundWeight = Fuel * 2.20462f;
				fuel.text = "Fuel = " + poundWeight.ToString("0.0") + " lb";
			}
			if (weightUnit == WeightUnit.Ounce)
			{
				float ounceWeight = Fuel * 35.274f;
				fuel.text = "Fuel = " + ounceWeight.ToString("0.0") + " Oz";
			}
			if (weightUnit == WeightUnit.Stone)
			{
				float stonneWeight = Fuel * 0.15747f;
				fuel.text = "Fuel = " + stonneWeight.ToString("0.0") + " St";
			}
			//SPEED
			float Speed = connectedAircraft.core.currentSpeed;
			float mach = connectedAircraft.core.machSpeed;

			if (mach < 0.5f)
			{
				if (speedUnit == SpeedUnit.Knots)
				{
					float speedly = Speed * 1.944f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " knots";
				}
				if (speedUnit == SpeedUnit.MeterPerSecond)
				{
					float speedly = Speed;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " m/s";
				}
				if (speedUnit == SpeedUnit.FeetPerSecond)
				{
					float speedly = Speed * 3.2808f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " ft/s";
				}
				if (speedUnit == SpeedUnit.MilesPerHour)
				{
					float speedly = Speed * 2.237f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " mph";
				}
				if (speedUnit == SpeedUnit.KilometerPerHour)
				{
					float speedly = Speed * 3.6f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " kmh";
				}
			}
			else
			{
				float speedly = connectedAircraft.core.machSpeed;
				speed.text = "Airspeed = " + speedly.ToString("0.00") + " Mach";
			}
			//THRUST
			if (forceUnit == ForceUnit.Newton)
			{
				engineThrust.text = "Total Thrust = " + connectedAircraft.totalThrustGenerated.ToString("0.0") + " N";
			}
			if (forceUnit == ForceUnit.KilogramForce)
			{
				engineThrust.text = "Total Thrust = " + (connectedAircraft.totalThrustGenerated * 0.101972f).ToString("0.0") + " kgF";
			}
			if (forceUnit == ForceUnit.PoundForce)
			{
				engineThrust.text = "Total Thrust = " + (connectedAircraft.totalThrustGenerated * 0.224809f).ToString("0.0") + " lbF";
			}
			//ENGINE POWER
			enginePower.text = "Engine Throttle = " + (connectedAircraft.flightComputer.processedThrottle * 100f).ToString("0.0") + " %";
			//ALTITUDE
			float Altitude = connectedAircraft.core.currentAltitude * Oyedoyin.MathBase.toFt;
			if (altitudeUnit == AltitudeUnit.Feet)
			{
				float distance = Altitude;
				altitude.text = "Altitude = " + distance.ToString("0.0") + " ft";
			}
			if (altitudeUnit == AltitudeUnit.NauticalMiles)
			{
				float distance = Altitude * 0.00054f;
				altitude.text = "Altitude = " + distance.ToString("0.0") + " NM";
			}
			if (altitudeUnit == AltitudeUnit.Kilometer)
			{
				float distance = Altitude / 3280.8f;
				altitude.text = "Altitude = " + distance.ToString("0.0") + " km";
			}
			if (altitudeUnit == AltitudeUnit.Meter)
			{
				float distance = Altitude / 3.2808f;
				altitude.text = "Altitude = " + distance.ToString("0.0") + " m";
			}
			//AMBIENT
			pressure.text = "Pressure = " + connectedAircraft.core.ambientPressure.ToString("0.0") + " kpa";
			density.text = "Air Density = " + connectedAircraft.core.airDensity.ToString("0.000") + " kg/m3";
			//
			float Temperature = connectedAircraft.core.ambientTemperature;
			if (temperatureUnit == TemperatureUnit.Celsius)
			{
				temperature.text = "Temperature = " + Temperature.ToString("0.0") + " °C";
			}
			if (temperatureUnit == TemperatureUnit.Fahrenheit)
			{
				float temp = (Temperature * (9 / 5)) + 32f;
				temperature.text = "Temperature = " + temp.ToString("0.0") + " °F";
			}



			//WEAPON
			if (connectedAircraft.Armaments != null)
			{
				//ACTIVATE
				if (!weaponCount.gameObject.activeSelf)
				{
					weaponCount.gameObject.SetActive(false);
					currentWeapon.gameObject.SetActive(false);
					ammoCount.gameObject.SetActive(false);
				}
				//SET VALUES
				weaponCount.text = "Weapon Count: " + connectedAircraft.Armaments.availableWeapons.Count.ToString();
				currentWeapon.text = "Current Weapon: " + connectedAircraft.Armaments.currentWeapon;
				if (connectedAircraft.Armaments.currentWeapon == "Gun")
				{
					int ammoTotal = 0;
					foreach (SilantroGun gun in connectedAircraft.Armaments.attachedGuns)
					{
						ammoTotal += gun.currentAmmo;
					}
					ammoCount.text = "Ammo Count: " + ammoTotal.ToString();
				}
				//
				if (connectedAircraft.Armaments.currentWeapon == "Missile")
				{
					ammoCount.text = "Ammo Count: " + connectedAircraft.Armaments.missiles.Count.ToString();
				}
				//
				if (connectedAircraft.Armaments.currentWeapon == "Bomb")
				{
					ammoCount.text = "Ammo Count: " + connectedAircraft.Armaments.bombs.Count.ToString();
				}
				//
				if (connectedAircraft.Armaments.currentWeapon == "Rocket")
				{
					ammoCount.text = "Ammo Count: " + connectedAircraft.Armaments.rockets.Count.ToString();
				}

			}
			else
			{
				if (weaponCount != null && weaponCount.gameObject.activeSelf)
				{
					weaponCount.gameObject.SetActive(false);
					currentWeapon.gameObject.SetActive(false);
					ammoCount.gameObject.SetActive(false);
				}
			}
		}
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroDisplay))]
public class SilantroDisplayEditor : Editor
{
	Color backgroundColor;
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		SilantroDisplay control = (SilantroDisplay)target;
		serializedObject.Update();

		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox("Connected Aircraft", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("connectedAircraft"), new GUIContent(" "));

		GUILayout.Space(15f);
		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox("Unit Display Setup", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.units = (SilantroDisplay.UnitsSetup)EditorGUILayout.EnumPopup("Unit System", control.units);
		GUILayout.Space(3f);
		if (control.units == SilantroDisplay.UnitsSetup.Custom)
		{
			//
			EditorGUI.indentLevel++;
			GUILayout.Space(3f);
			control.speedUnit = (SilantroDisplay.SpeedUnit)EditorGUILayout.EnumPopup("Speed Unit", control.speedUnit);
			GUILayout.Space(3f);
			control.altitudeUnit = (SilantroDisplay.AltitudeUnit)EditorGUILayout.EnumPopup("Altitude Unit", control.altitudeUnit);
			GUILayout.Space(3f);
			control.temperatureUnit = (SilantroDisplay.TemperatureUnit)EditorGUILayout.EnumPopup("Temperature Unit", control.temperatureUnit);
			GUILayout.Space(3f);
			control.forceUnit = (SilantroDisplay.ForceUnit)EditorGUILayout.EnumPopup("Force Unit", control.forceUnit);
			GUILayout.Space(3f);
			control.weightUnit = (SilantroDisplay.WeightUnit)EditorGUILayout.EnumPopup("Weight Unit", control.weightUnit);
			GUILayout.Space(3f);
			control.torqueUnit = (SilantroDisplay.TorqueUnit)EditorGUILayout.EnumPopup("Torque Unit", control.torqueUnit);
			EditorGUI.indentLevel--;
		}
		else if (control.units == SilantroDisplay.UnitsSetup.Metric)
		{
			//
			control.speedUnit = SilantroDisplay.SpeedUnit.MeterPerSecond;
			control.altitudeUnit = SilantroDisplay.AltitudeUnit.Meter;
			control.temperatureUnit = SilantroDisplay.TemperatureUnit.Celsius;
			control.forceUnit = SilantroDisplay.ForceUnit.Newton;
			control.weightUnit = SilantroDisplay.WeightUnit.Kilogram;
			control.torqueUnit = SilantroDisplay.TorqueUnit.NewtonMeter;
			//
		}
		else if (control.units == SilantroDisplay.UnitsSetup.Imperial)
		{
			//
			//
			control.speedUnit = SilantroDisplay.SpeedUnit.Knots;
			control.altitudeUnit = SilantroDisplay.AltitudeUnit.Feet;
			control.temperatureUnit = SilantroDisplay.TemperatureUnit.Fahrenheit;
			control.forceUnit = SilantroDisplay.ForceUnit.PoundForce;
			control.weightUnit = SilantroDisplay.WeightUnit.Pound;
			control.torqueUnit = SilantroDisplay.TorqueUnit.PoundForceFeet;
			//
		}
		//
		GUILayout.Space(5f);
		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox("Output Ports", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.displayPoints = EditorGUILayout.Toggle("Show", control.displayPoints);
		if (control.displayPoints)
		{
			GUILayout.Space(5f);
			control.speed = EditorGUILayout.ObjectField("Speed Text", control.speed, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.altitude = EditorGUILayout.ObjectField("Altitude Text", control.altitude, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.fuel = EditorGUILayout.ObjectField("Fuel Text", control.fuel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.weight = EditorGUILayout.ObjectField("Weight Text", control.weight, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.gLoad = EditorGUILayout.ObjectField("G-Load", control.gLoad, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.engineName = EditorGUILayout.ObjectField("Engine Name Text", control.engineName, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.enginePower = EditorGUILayout.ObjectField("Engine Power Text", control.enginePower, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.engineThrust = EditorGUILayout.ObjectField("Engine Thrust Text", control.engineThrust, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.density = EditorGUILayout.ObjectField("Density Text", control.density, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.pressure = EditorGUILayout.ObjectField("Pressure Text", control.pressure, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.temperature = EditorGUILayout.ObjectField("Temperature Text", control.temperature, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.Time = EditorGUILayout.ObjectField("Time Text", control.Time, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.brake = EditorGUILayout.ObjectField("Parking Brake Text", control.brake, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.flapLevel = EditorGUILayout.ObjectField("Flap Text", control.flapLevel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.slatLevel = EditorGUILayout.ObjectField("Slat Text", control.slatLevel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.incrementalBrake = EditorGUILayout.ObjectField("Incremental Brake Text", control.incrementalBrake, typeof(Text), true) as Text;


			GUILayout.Space(5f);
			control.weaponCount = EditorGUILayout.ObjectField("Weapon Count", control.weaponCount, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.currentWeapon = EditorGUILayout.ObjectField("Current Weapon", control.currentWeapon, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.ammoCount = EditorGUILayout.ObjectField("Ammo Count", control.ammoCount, typeof(Text), true) as Text;

			GUILayout.Space(5f);
			control.speedLabel = EditorGUILayout.ObjectField("Speed Label", control.speedLabel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.altitudeLabel = EditorGUILayout.ObjectField("Altitude Label", control.altitudeLabel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.headingLabel = EditorGUILayout.ObjectField("Heading Label", control.headingLabel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.climbLabel = EditorGUILayout.ObjectField("Climb Label", control.climbLabel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.autoPilotContainer = EditorGUILayout.ObjectField("Container", control.autoPilotContainer, typeof(GameObject), true) as GameObject;



			GUILayout.Space(5f);
			control.pitchRate = EditorGUILayout.ObjectField("Pitch Rate Label", control.pitchRate, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.rollRate = EditorGUILayout.ObjectField("Roll Rate Label", control.rollRate, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.yawRate = EditorGUILayout.ObjectField("Yaw Rate Label", control.yawRate, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.turnRate = EditorGUILayout.ObjectField("Turn Rate Label", control.turnRate, typeof(Text), true) as Text;

			GUILayout.Space(5f);
			control.commandPitch = EditorGUILayout.ObjectField("Command Pitch Label", control.commandPitch, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.commandRoll = EditorGUILayout.ObjectField("Command Roll Label", control.commandRoll, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.commandYaw = EditorGUILayout.ObjectField("Command Yaw Label", control.commandYaw, typeof(Text), true) as Text;
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif