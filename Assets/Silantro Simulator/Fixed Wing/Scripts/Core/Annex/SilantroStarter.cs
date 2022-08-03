using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroStarter : MonoBehaviour
{
	[Header("Connected Aircraft")]
	public SilantroController aircraft;

	void Start()
	{
		if(aircraft.startMode == SilantroController.StartMode.Hot) { StartCoroutine(StartUpAircraft()); }
	}


	IEnumerator StartUpAircraft()
	{

		yield return new WaitForSeconds(0.002f);//JUST LAG A BIT BEHIND CONTROLLER SCRIPT
												//STARTUP AIRCRAFT	
		aircraft.StartAircraft();

		//RAISE GEAR
		if (aircraft.flightComputer.gearSolver != null && aircraft.flightComputer.gearSolver.actuatorState == SilantroActuator.ActuatorState.Engaged) { aircraft.flightComputer.gearSolver.DisengageActuator(); }
	}
}
