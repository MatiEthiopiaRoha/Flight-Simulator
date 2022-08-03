using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroHangerSensor : MonoBehaviour {
	public SilantroHanger connection;
	public bool doorOpen;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnTriggerExit(Collider aircraft)
	{
		if(aircraft.gameObject.tag == "Player" && doorOpen)
		{
			doorOpen = false;
			connection.doorActuator.DisengageActuator ();
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnTriggerEnter(Collider aircraft)
	{
		if(aircraft.gameObject.tag == "Player" && !doorOpen)
		{
			doorOpen = true;
			connection.doorActuator.EngageActuator ();
		}
	}
}
