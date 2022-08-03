using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SilantroPID
{
	public float Kp = 1;
	public float Ki = 0;
	public float Kd = 0.1f;

	[Header("Computation Values")]
	public float proportional;
	public float derivative;
	public float integral;

	[Header("Limits")]
	public float minimum = -1;
	public float maximum = 1;

	public float output;
	float prevError;

	void Start()
	{
		//RESET VALUES
		proportional = 0f; integral = 0f; derivative = 0f;
	}

	public float CalculateOutput(float error, float dt)
	{
		//1. PROPORTIONAL
		proportional = error * Kp;

		//2. INTEGRAL
		integral += error * dt * Ki;
		if (integral > maximum) { integral = maximum; }
		if (integral < minimum) { integral = minimum; }


		//3. DERIVATIVE
		derivative = Kd * ((error - prevError) / dt);
		prevError = error;

		//OUTPUT
		output = proportional + integral + derivative;
		if (output > maximum) { output = maximum; }
		if (output < minimum) { output = minimum; }

		return output;
	}

	public void Reset()
	{
		proportional = 0f;
		integral = 0f;
		derivative = 0f;
	}
}
