using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroHanger : MonoBehaviour
{
	[HideInInspector] public bool contained;
	[HideInInspector] GameObject aircraftBody;
	[HideInInspector] public SilantroController connectedAircraft;
	public KeyCode activateSystem = KeyCode.U;
	public float minimumSpeed = 2f;
	public SilantroHydraulicSystem doorActuator;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		doorActuator.InitializeActuator();
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnTriggerStay(Collider aircraft)
	{
		if (aircraft.gameObject.tag == "Player" && !contained)
		{
			if (aircraftBody == null)
			{
				aircraftBody = aircraft.gameObject;
				connectedAircraft = aircraftBody.GetComponent<SilantroController>();
			}
			contained = true;
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnTriggerExit(Collider aircraft)
	{
		if (aircraft.gameObject.tag == "Player" && contained)
		{
			if (aircraftBody != null)
			{
				aircraftBody = null; connectedAircraft = null;
			}
			contained = false;
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DISPLAY ENTERY INFORMATION
	void OnGUI()
	{
		if (connectedAircraft && contained)
		{
			if (connectedAircraft.Armaments != null)
			{
				GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 300, 100), "Press " + activateSystem.ToString() + " to Resupply Aircraft");
			}
			else
			{
				GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 300, 100), "Press " + activateSystem.ToString() + " to Refuel Aircraft");
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (Input.GetKeyDown(activateSystem) && connectedAircraft != null)
		{
			if (connectedAircraft.core.currentSpeed < minimumSpeed)
			{

				if (!connectedAircraft.silantroActive)
				{
					if (connectedAircraft.fuelLevel < connectedAircraft.TotalFuelCapacity)
					{
						connectedAircraft.fuelSystem.refuelRate = 200f;
						connectedAircraft.fuelSystem.ActivateTankRefill();
						Debug.Log("Please wait for aircraft refuel completion");
					}

					if (connectedAircraft.Armaments != null)
					{
						//2.........................REARM AIRCRAFT
						connectedAircraft.RefreshWeapons();
					}
				}
				else
				{
					Debug.Log("Shutdown engines to resupply aircraft");
				}

			}
			else
			{
				Debug.Log("Reduce aircraft speed to resupply aircraft");
			}
		}
	}
}
