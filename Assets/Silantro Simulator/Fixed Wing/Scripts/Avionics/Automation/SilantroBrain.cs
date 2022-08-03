using Oyedoyin;
using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 
/// Use:		 Handles the AI control section of the flight computer
/// </summary>




[Serializable]
public class SilantroBrain
{
    // ----------------------------------------- Selectibles
    public enum FlightState { Grounded, Taxi, Takeoff, Cruise, Loiter, Decent, Landing }
    public FlightState flightState = FlightState.Grounded;

    public enum TaxiState { Stationary, Moving, Holding}
    public TaxiState taxiState = TaxiState.Stationary;


    // ----------------------------------------- Connections
    public SilantroWaypointPlug tracker;
    public SilantroController controller;
    public AnimationCurve steerCurve;
    public SilantroFlightComputer computer;



    // ----------------------------------------- Variables
    public float currentSpeed;
    public float takeoffSpeed = 100f;
    public float climboutPitchAngle = 8f;
    public float checkListTime = 2f;
    public float transitionTime = 5f;
    private float evaluateTime = 12f;
    public float takeoffHeading;
    public float inputCheckFactor;
    private float currentTestTime;



    // -------------------------------- Control Markers
    public bool flightInitiated;
    public bool checkedSurfaces;
    public bool groundChecklistComplete;
    public bool isTaxing;
    public bool checkingSurfaces;
    public bool clearedForTakeoff;
   

    // -------------------------------- Taxi
    public float maximumTaxiSpeed = 10f;
    public float recommendedTaxiSpeed = 8f;
    public float maximumTurnSpeed = 5f;
    public float maximumSteeringAngle = 30f;
    public float minimumSteeringAngle = 15f;
    public float steeringAngle;
    public float targetTaxiSpeed;

    [Range(0, 1)] public float steerSensitivity = 0.05f;
    [Range(0, 2)] public float brakeSensitivity = 0.85f;
    public float brakeInput;


    // -------------------------------- Cruise
    public float cruiseAltitude = 1000f;
    public float cruiseSpeed = 300f;
    public float cruiseHeading = 0f;
    public float cruiseClimbRate = 1500f;




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeBrain()
    {
        // -------------------------
        if(tracker != null) { tracker.aircraft = controller; tracker.InitializePlug(); }

        // ------------------------ Plot
        PlotSteerCurve();
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateBrain()
    {
        //---------------------------------------------OPERATION MODE
        switch (flightState)
        {
            case FlightState.Grounded: GroundMode(); break;
            case FlightState.Taxi: TaxiMode(); break;
            case FlightState.Takeoff: TakeoffMode(); break;
            case FlightState.Cruise: CruiseMode(); break;
        }


        // ---------------------------------------------- COLLECT DATA
        currentSpeed = controller.core.currentSpeed;

        // ---------------------------------------------- SURFACE CHECK
        if (checkingSurfaces) { CheckControlSurfaces(inputCheckFactor); }
    }









    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void PlotSteerCurve()
    {
        // ------------------------- Plot Steer Curve
        steerCurve = new AnimationCurve();

        steerCurve.AddKey(new Keyframe(0.0f * maximumSteeringAngle, maximumTaxiSpeed));
        steerCurve.AddKey(new Keyframe(0.5f * maximumSteeringAngle, recommendedTaxiSpeed));
        steerCurve.AddKey(new Keyframe(0.8f * maximumSteeringAngle, maximumTurnSpeed));

#if UNITY_EDITOR
        for (int i = 0; i < steerCurve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(steerCurve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(steerCurve, i, AnimationUtility.TangentMode.Auto);
        }
#endif
    }




    // -------------------------------------------GROUND MODE----------------------------------------------------------------------------------------------------------
    void GroundMode()
    {
        //------------------------- Start Flight Process
        if(flightInitiated && !controller.engineRunning) { controller.TurnOnEngines(); flightInitiated = false; }
        if(flightInitiated && controller.engineRunning) { flightInitiated = false; }


        if (!controller.engineRunning)
        {
            // Check States
            if (controller.lightState == SilantroController.LightState.On) { controller.input.TurnOffLights(); }
            if (controller.gearHelper && controller.gearHelper.brakeState == SilantroGearSystem.BrakeState.Disengaged) { controller.gearHelper.EngageBrakes(); }
        }
        else
        {
            // -------------------------------------------------------------------------------------- Check List
            if (!groundChecklistComplete)
            {
                //Debug.Log("Engine Start Complete, commencing ground checklist");
                computer.StartCoroutine(GroundCheckList());
            }
        }
    }

    bool flapSet;
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator GroundCheckList()
    {
        // --------------------------- Lights
        controller.input.TurnOnLights();

        // --------------------------- Actuators
        if (controller.canopyActuator && controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.canopyActuator.DisengageActuator(); }
        if (controller.speedBrakeActuator && controller.speedBrakeActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.speedBrakeActuator.DisengageActuator(); }
        if (controller.wingActuator && controller.wingActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.wingActuator.DisengageActuator(); }

        // --------------------------- Flaps
        yield return new WaitForSeconds(checkListTime);
        foreach (SilantroAerofoil foil in controller.wings)
        {
            if (foil.flapSetting != 1 && !flapSet && foil.flapAngleSetting == SilantroAerofoil.FlapAngleSetting.ThreeStep) { foil.SetFlaps(1, 1); }
            if (foil.flapSetting != 2 && !flapSet && foil.flapAngleSetting == SilantroAerofoil.FlapAngleSetting.FiveStep) { foil.SetFlaps(2, 1); }
        }
        flapSet = true;

        // --------------------------- Slats
        yield return new WaitForSeconds(checkListTime);
        foreach (SilantroAerofoil foil in controller.flightComputer.wingFoils)
        {
            if (foil.slatState == SilantroAerofoil.ControlState.Active) { foil.baseSlat = Mathf.MoveTowards(foil.baseSlat, controller.flightComputer.takeOffSlat, foil.slatActuationSpeed * Time.fixedDeltaTime); }
        }


        // --------------------------- Control Surfaces
        yield return new WaitForSeconds(checkListTime);
        if (!checkingSurfaces && currentTestTime < 1f) { currentTestTime = evaluateTime; checkingSurfaces = true; }
        if (!checkedSurfaces)
        {
            float startRange = -1.0f; float endRange = 1.0f; float cycleRange = (endRange - startRange) / 2f;
            float offset = cycleRange + startRange;
            inputCheckFactor = offset + Mathf.Sin(Time.time * 5f) * cycleRange;
        }

        yield return new WaitForSeconds(evaluateTime);
        checkedSurfaces = true;checkingSurfaces = false;
        computer.processedPitch = 0f;
        computer.processedRoll = 0f;
        computer.processedYaw = 0f;
        computer.processedStabilizerTrim = 0f;

        groundChecklistComplete = true;

        // ---------------------------- Transition
        yield return new WaitForSeconds(transitionTime);
        flightState = FlightState.Taxi;
        if (controller.gearHelper != null) { controller.gearHelper.ReleaseBrakes(); }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void CheckControlSurfaces(float controlInput)
    {
        if (checkingSurfaces)
        {
            currentTestTime -= Time.deltaTime;
            if(currentTestTime < 0) { currentTestTime = 0f; }

            //--------------------- Pitch
            if(currentTestTime < evaluateTime && currentTestTime > (0.75f*evaluateTime))
            {
                computer.processedPitch = controlInput;
                computer.processedRoll = 0f;
                computer.processedYaw = 0f;
                computer.processedStabilizerTrim = 0f;
            }
            //--------------------- Roll
            if (currentTestTime < (0.75f * evaluateTime) && currentTestTime > (0.50f * evaluateTime))
            {
                computer.processedPitch = 0f;
                computer.processedRoll = controlInput;
                computer.processedYaw = 0f;
                computer.processedStabilizerTrim = 0f;
            }
            //--------------------- Yaw
            if (currentTestTime < (0.50f * evaluateTime) && currentTestTime > (0.25f * evaluateTime))
            {
                computer.processedPitch = 0f;
                computer.processedRoll = 0f;
                computer.processedYaw = controlInput;
                computer.processedStabilizerTrim = 0f;
            }
            //--------------------- Trim
            if (currentTestTime < (0.25f * evaluateTime) && currentTestTime > (0.00f * evaluateTime))
            {
                computer.processedPitch = 0f;
                computer.processedRoll = 0f;
                computer.processedYaw = 0f;
                computer.processedStabilizerTrim = controlInput;
            }
        }
    }



    // -------------------------------------------TAXI----------------------------------------------------------------------------------------------------------
    void TaxiMode()
    {
        // ------------------------------------- Clamp
        float thresholdSpeed = maximumTaxiSpeed * 0.1f;
       

        // ------------------------------------- States
        if (taxiState == TaxiState.Stationary)
        {
            if (controller.gearHelper != null && controller.gearHelper.brakeState == SilantroGearSystem.BrakeState.Disengaged && tracker.currentPoint <= tracker.track.pathPoints.Count)
            { taxiState = TaxiState.Moving; isTaxing = true; }
        }
        if (taxiState == TaxiState.Moving)
        {
            if (tracker.currentPoint > tracker.track.pathPoints.Count - 2 && controller.gearHelper != null && controller.gearHelper.brakeState == SilantroGearSystem.BrakeState.Disengaged)
            {
                taxiState = TaxiState.Holding;
                targetTaxiSpeed = 0; brakeInput = 0f;
                if (controller.gearHelper != null) { controller.gearHelper.EngageBrakes(); }
                isTaxing = false;

                //--------------- Set Takeoff Parameters
                takeoffHeading = computer.currentHeading;
            }
            else
            {
                if(tracker.currentPoint > tracker.track.pathPoints.Count - 6 && tracker.currentPoint < tracker.track.pathPoints.Count - 4) { targetTaxiSpeed = (0.6f*maximumTaxiSpeed)/ 1.94384f; } //96
                else if(tracker.currentPoint > tracker.track.pathPoints.Count - 5 && tracker.currentPoint < tracker.track.pathPoints.Count - 3) { targetTaxiSpeed = (0.5f * maximumTaxiSpeed) / 1.94384f; } //97
                else if (tracker.currentPoint > tracker.track.pathPoints.Count - 4 && tracker.currentPoint < tracker.track.pathPoints.Count - 2) { targetTaxiSpeed = (0.25f * maximumTaxiSpeed) / 1.94384f; } //98
                else if (tracker.currentPoint > tracker.track.pathPoints.Count - 3 && tracker.currentPoint < tracker.track.pathPoints.Count - 1) { targetTaxiSpeed = (0.1f * maximumTaxiSpeed) / 1.94384f; } //98
                else { targetTaxiSpeed = steerCurve.Evaluate(steeringAngle) / 1.94384f; }

                isTaxing = true;
            }
        }
        if(taxiState == TaxiState.Holding)
        {
            // -------------------------- Perform function while on hold

            // -------------------------- Receive clearance
            if (clearedForTakeoff) { flightState = FlightState.Takeoff; }
        }


        



        if (isTaxing)
        {
            // ------------------------------------- Speed Control
            float speedError = (targetTaxiSpeed - currentSpeed) * 1.94384f;
            if(computer.autoThrottle == SilantroFlightComputer.ControlState.Off) { computer.autoThrottle = SilantroFlightComputer.ControlState.Active; }
            if(speedError > 5 && tracker.currentPoint < tracker.track.pathPoints.Count - 2)
            {
                // ---------------- Auto Throttle
                float presetSpeed = targetTaxiSpeed;
                computer.speedError = (presetSpeed - currentSpeed);
                computer.processedThrottle = computer.throttleSolver.CalculateOutput(computer.speedError, computer.timeStep);
                if (float.IsNaN(computer.processedThrottle) || float.IsInfinity(computer.processedThrottle)) { computer.processedThrottle = 0f; }
            }
            else { computer.processedThrottle = 0f; }



            // ------------------------------------- Point
            tracker.UpdateTrack();


            // ------------------------------------- Steer
            float taxiSpeed = controller.transform.InverseTransformDirection(controller.aircraft.velocity).z;
            brakeInput = -1 *Mathf.Clamp((targetTaxiSpeed - currentSpeed) * brakeSensitivity, -1, 0);
            Vector3 offsetTargetPos = tracker.target.position;
            Vector3 localTarget = controller.transform.InverseTransformPoint(offsetTargetPos);
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            float steer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(taxiSpeed);
            steeringAngle = Mathf.Lerp(maximumSteeringAngle, minimumSteeringAngle, Mathf.Abs(taxiSpeed) * 0.015f) * steer;


            // ------------------------------------- Send
            if (controller.gearHelper != null)
            {
                controller.gearHelper.brakeInput = brakeInput;
                controller.gearHelper.currentSteerAngle = steeringAngle;
            }
        }
    }



    // -----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TakeoffClearance()
    {
        if(flightState == FlightState.Taxi && taxiState == TaxiState.Holding)
        {
            if (!clearedForTakeoff) { clearedForTakeoff = true; }
            else { Debug.Log(controller.transform.name + " has been cleared for takeoff"); }
        }
        else { Debug.Log(controller.transform.name + " clearance invalid! Aircraft not in holding pattern"); }
    }




    // -------------------------------------------TAKEOFF----------------------------------------------------------------------------------------------------------
    void TakeoffMode()
    {
        if (controller.engineRunning)
        {
            //------Accelerate
            if (controller.gearHelper) { controller.gearHelper.ReleaseBrakes(); }
            computer.processedThrottle = Mathf.Lerp(computer.processedThrottle, 1.05f, Time.deltaTime);
            if (computer.processedThrottle > 1f && controller.input.boostState == SilantroInput.BoostState.Off) { controller.input.EngageBoost(); }

            // ------------------------------------- Send
            if (controller.gearHelper != null)
            {
                controller.gearHelper.brakeInput = brakeInput = 0f;
                controller.gearHelper.currentSteerAngle = steeringAngle = 0f;
            }


            #region Pitch Control
            if (computer.knotSpeed < takeoffSpeed)
            {
                computer.commandPitchRate = 0f;
                computer.pitchRateError = computer.pitchRate - computer.commandPitchRate;
                computer.processedPitch = computer.pitchRateSolver.CalculateOutput(computer.pitchRateError, computer.timeStep);
            }
            else
            {
                // -------------------------------------------- Pitch Rate Required
                computer.pitchAngleError = computer.pitchAngle - (-1f * climboutPitchAngle);
                computer.pitchAngleSolver.minimum = -computer.climboutPitchRate; computer.pitchAngleSolver.maximum = computer.climboutPitchRate;
                computer.commandPitchRate = computer.pitchAngleSolver.CalculateOutput(computer.pitchAngleError, computer.timeStep);

                computer.pitchRateError = computer.pitchRate - computer.commandPitchRate;
                computer.processedPitch = computer.pitchRateSolver.CalculateOutput(computer.pitchRateError, computer.timeStep);
            }
            #endregion Pitch Control

            #region Roll/Yaw Control
            computer.yawAngleError = takeoffHeading - computer.yawAngle;
            computer.yawAngleSolver.minimum = -computer.balanceYawRate; computer.yawAngleSolver.maximum = computer.balanceYawRate;
            computer.commandYawRate = computer.yawAngleSolver.CalculateOutput(computer.yawAngleError, computer.timeStep);
            computer.yawRateError = computer.commandYawRate - computer.yawRate;
            computer.processedYaw = computer.yawRateSolver.CalculateOutput(computer.yawRateError, computer.timeStep);

            computer.commandBankAngle = 0f;
            computer.rollAngleError = computer.rollAngle - (-1f * computer.commandBankAngle);
            computer.rollAngleSolver.minimum = -computer.balanceRollRate; computer.rollAngleSolver.maximum = computer.balanceRollRate;
            computer.commandRollRate = computer.rollAngleSolver.CalculateOutput(computer.rollAngleError, computer.timeStep);
            computer.rollRateError = computer.commandRollRate - computer.rollRate;
            computer.processedRoll = computer.rollRateSolver.CalculateOutput(computer.rollRateError, computer.timeStep);
            #endregion Roll/Yaw Control

            if (computer.knotSpeed > computer.maximumGearSpeed - 10f) { computer.StartCoroutine(PostTakeoffCheckList()); }
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator PostTakeoffCheckList()
    {
        // --------------------------- Flaps
        yield return new WaitForSeconds(checkListTime);
        foreach (SilantroAerofoil foil in controller.wings) { if (foil.flapSetting != 0) { foil.SetFlaps(0, 1); } }

        // --------------------------- Gear
        if (computer.gearSolver)
        {
            yield return new WaitUntil(() => computer.knotSpeed > computer.maximumGearSpeed && computer.ftHeight > 50f);
            computer.gearSolver.DisengageActuator();
        }

        // ---------------------------- Transition
        yield return new WaitForSeconds(transitionTime);
        foreach (SilantroAerofoil foil in computer.wingFoils) { if (foil.slatState == SilantroAerofoil.ControlState.Active) { foil.baseSlat = Mathf.MoveTowards(foil.baseSlat, 0f, foil.slatActuationSpeed * Time.fixedDeltaTime); } }
        flightState = FlightState.Cruise;
    }







    // -------------------------------------------CRUISE----------------------------------------------------------------------------------------------------------
    void CruiseMode()
    {
        if (Mathf.Abs(cruiseSpeed - computer.knotSpeed) > 2) { computer.autoThrottle = SilantroFlightComputer.ControlState.Active; }
        else { computer.autoThrottle = SilantroFlightComputer.ControlState.Off; }


        // ---------------- Auto Throttle
        float presetSpeed = cruiseSpeed / MathBase.toKnots;
        computer.speedError = (presetSpeed - currentSpeed);
        computer.processedThrottle = computer.throttleSolver.CalculateOutput(computer.speedError, computer.timeStep);
        if (float.IsNaN(computer.processedThrottle) || float.IsInfinity(computer.processedThrottle)) { computer.processedThrottle = 0f; }

        // ----------------- Boost e.g Piston Turbo or Turbine Reheat
        if (computer.processedThrottle > 1f && computer.speedError > 5f && controller.input.boostState == SilantroInput.BoostState.Off) { controller.input.EngageBoost(); }
        if (computer.processedThrottle < 1f && computer.speedError < 3f && controller.input.boostState == SilantroInput.BoostState.Active) { controller.input.DisEngageBoost(); }



        // ----------------- Boost Deceleration with Speedbrake
        if (computer.speedBrakeSolver != null && computer.overideSpeedbrake && computer.ftHeight > 500f)
        {
            if (computer.speedBrakeSolver.actuatorState == SilantroActuator.ActuatorState.Disengaged && computer.speedError < -30f) { computer.speedBrakeSolver.EngageActuator(); }
            if (computer.speedBrakeSolver.actuatorState == SilantroActuator.ActuatorState.Engaged && computer.speedError > -20f) { computer.speedBrakeSolver.DisengageActuator(); }
        }


        // ---------------------------------------------------------------------------- Roll
        float presetHeading = cruiseHeading; if (presetHeading > 180) { presetHeading -= 360f; }
        computer.headingError = presetHeading - computer.currentHeading;
        computer.turnSolver.maximum = computer.maximumTurnRate; computer.turnSolver.minimum = -computer.maximumTurnRate;
        computer.commandTurnRate = computer.turnSolver.CalculateOutput(computer.headingError, computer.timeStep);


        // ---------------------------------------------- Calculate Required Bank
        float rollFactor = (computer.commandTurnRate * currentSpeed * MathBase.toKnots) / 1091f;
        computer.commandBankAngle = Mathf.Atan(rollFactor) * Mathf.Rad2Deg;
        if (computer.commandBankAngle > computer.maximumTurnBank) { computer.commandBankAngle = computer.maximumTurnBank; }
        if (computer.commandBankAngle < -computer.maximumTurnBank) { computer.commandBankAngle = -computer.maximumTurnBank; }

        // -------------------------------------------- Roll Rate Required
        computer.rollAngleError = computer.rollAngle - (-1f * computer.commandBankAngle);
        computer.rollAngleSolver.minimum = -computer.balanceRollRate; computer.rollAngleSolver.maximum = computer.balanceRollRate;
        computer.commandRollRate = computer.rollAngleSolver.CalculateOutput(computer.rollAngleError, computer.timeStep);

        computer.rollRateError = computer.commandRollRate - computer.rollRate;
        computer.processedRoll = computer.rollRateSolver.CalculateOutput(computer.rollRateError, computer.timeStep);




        // --------------------------------------------------------------------------- Pitch
        // ------------------------------------------------ Altitude Hold
        computer.altitudeError = cruiseAltitude / MathBase.toFt - computer.currentAltitude;
        float presetClimbLimit = cruiseClimbRate / MathBase.toFtMin;
        computer.altitudeSolver.maximum = presetClimbLimit; computer.altitudeSolver.minimum = -presetClimbLimit;
        computer.commandClimbRate = computer.altitudeSolver.CalculateOutput(computer.altitudeError, computer.timeStep) * MathBase.toFtMin;

        // -------------------------------------------- Pitch Rate Required
        float presetClimbRate = computer.commandClimbRate / MathBase.toFtMin;
        computer.climbRateError = presetClimbRate - computer.currentClimbRate;
        computer.climbSolver.minimum = -computer.balancePitchRate * 0.2f; computer.climbSolver.maximum = computer.balancePitchRate * 0.5f;
        computer.commandPitchRate = computer.climbSolver.CalculateOutput(computer.climbRateError, computer.timeStep);

        computer.pitchRateError = computer.pitchRate - computer.commandPitchRate;
        computer.processedPitch = computer.pitchRateSolver.CalculateOutput(computer.pitchRateError, computer.timeStep);
    }
}
