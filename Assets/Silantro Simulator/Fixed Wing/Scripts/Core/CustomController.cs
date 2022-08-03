using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FixedWing
{
	public class CustomController : MonoBehaviour
	{

		//-------------------------------------------------------------------------
		//------------------------------CUSTOM AIRCRAFT CONTROLLER---------------
		//-------------------------------------------------------------------------
		//Use this script to extend or connect to the main aircraft controller to send or recieve commands and variables
		//-------------------------------------------------------------------------
		public SilantroController connectedAircraft;

		//SAMPLE INPUT VARIABLE
		[Range(-1, 1)] public float sampleRollInput;
		[Range(-1, 1)] public float samplePitchInput;
		[Range(-1, 1)] public float sampleYawInput;
		[Range(0, 1)] public float sampleThrottleInput;


		//---------------------------------//SWITCH CONTROLLER TYPE TO CUSTOM----------------------------------------
		void Awake()
		{
			if (connectedAircraft != null)
			{
				connectedAircraft.inputType = SilantroController.InputType.Custom;
			}
		}

		//---------------------------------SEND VARIABLES----------------------------------------
		void Update()
		{
			//SEND SAMPLE VARIABLE TO AIRCRAFT
			if (connectedAircraft != null)
			{
				connectedAircraft.input.rawRollInput = sampleRollInput;
				connectedAircraft.input.rawPitchInput = samplePitchInput;
				connectedAircraft.input.rawYawInput = sampleYawInput;
				connectedAircraft.input.rawThrottleInput = sampleThrottleInput;
			}
		}

		//-------------------------------CALL SAMPLE COMMAND (START ENGINES)------------------------------------------
		public void SampleCommandA()
		{
			if (connectedAircraft != null)
			{
				connectedAircraft.TurnOnEngines();
			}
		}

		//----------------------------------CALL SAMPLE COMMAND (SWITCH LIGHTS)---------------------------------------
		public void SampleCommandB()
		{
			if (connectedAircraft != null)
			{
				if (connectedAircraft.input != null)
				{
					connectedAircraft.input.ToggleLightState();
				}
			}
		}
	}
}
