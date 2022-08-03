using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Oyedoyin;




/// <summary>
/// 
/// Use:		 Handles the processing of inputs and manages the aircraft control state
/// </summary>

public class SilantroFlightComputer : MonoBehaviour
{

    // ------------------------------------------ Selectibles
    public enum AugmentationType { Manual, StabilityAugmentation, CommandAugmentation, Autonomous }
    public AugmentationType operationMode = AugmentationType.Manual;
    public enum CraftState { TakeoffLanding, Cruise }
    public CraftState state = CraftState.TakeoffLanding;
    public enum GainSystem { Static, Dynamic }
    public GainSystem gainSystem = GainSystem.Static;
    public enum ControlType { SingleAxis, TwoAxis, ThreeAxis }
    public ControlType controlType = ControlType.SingleAxis;
    public enum SASInputMode { Basic, Bypass }
    public SASInputMode augmentationMode = SASInputMode.Basic;
    public enum CommandPreset { Airliner, Fighter, Trainer }
    public CommandPreset commandPreset = CommandPreset.Fighter;
    public enum ControlMode { Automatic, Manual }
    [Tooltip("Will the flaps be automatically deflected by the computer or manually by the user")] public ControlMode flapControl = ControlMode.Automatic;
    public ControlMode gearControl = ControlMode.Manual;
    public ControlState autoSlat = ControlState.Active;
    public ControlMode airbrakeControl = ControlMode.Automatic;
    public ControlMode protectorMode = ControlMode.Manual;
    public enum ControlLaw { Normal, AOA }
    public ControlLaw lawState = ControlLaw.Normal;
    public enum CommandInput { RateOnly, LoadFactor}
    public CommandInput longitudinalCommand = CommandInput.RateOnly;

    public enum ControlState { Active, Off }
    public ControlState autoThrottle = ControlState.Off;
    public ControlState machHold = ControlState.Off;
    public ControlState headingHold = ControlState.Off;
    public ControlState bankHold = ControlState.Off;
    public ControlState pitchHold = ControlState.Off;
    public ControlState altitudeHold = ControlState.Off;

    public ControlState bankLimiter = ControlState.Off;
    public ControlState pitchLimiter = ControlState.Off;
    public ControlState trimState = ControlState.Off;

    public ControlState gLimiter = ControlState.Off;
    public ControlState gWarner = ControlState.Off;
    public ControlState stallWarner = ControlState.Off;
    [Tooltip("Stall/Low speed protection system. The FCS will takeover throttle and hold the calculated stall speed or a specified minimum speed")] public ControlState speedProtector = ControlState.Off;

    public ControlState recoveryMode = ControlState.Off;
    public enum BankLimitState { Off, Left, Right }
    [HideInInspector] public BankLimitState bankState = BankLimitState.Off;
    public enum PitchLimitState { Off, Up, Down }
    [HideInInspector] public PitchLimitState pitchState = PitchLimitState.Off;
    public enum CASMode { Manual, Automatic }
    public CASMode commandMode = CASMode.Automatic;
    public enum AirbrakeType { ActuatorOnly,SurfaceOnly,Combined}
    public AirbrakeType airbrakeType = AirbrakeType.ActuatorOnly;



    // ------------------------------------------ AI
    public enum HeadingMethod { BankTurn, FlatTurn}
    public HeadingMethod headingMethod = HeadingMethod.BankTurn;
    public enum PitchMethod { ClimbToPoint, LevelToPoint }
    public PitchMethod pitchMethod = PitchMethod.ClimbToPoint;
    public enum MindState { FlightPoint, HoldLevel}
    public MindState mindState = MindState.FlightPoint;

    public Transform flightPoint;


    // ------------------------------------------ Connections
    public SilantroController controller;
    public SilantroCore core;
    public Transform homePoint;
    public List<SilantroAerofoil> wingFoils = new List<SilantroAerofoil>();
    public SilantroComputerConfig gainBoard;
    public SilantroBrain brain;
   



    // ------------------------------------------ Base Input
    public float basePitchInput;
    public float baseRollInput;
    public float baseYawInput;
    public float baseThrottleInput;
    public float baseStabilizerTrimInput;
    public float baseTillerInput;



    //---------------------------------------------CONTROL DEAD ZONES
    public float pitchDeadZone = 0.05f;
    public float rollDeadZone = 0.05f;
    public float yawDeadZone = 0.05f;

    [Range(0, 0.5f)] public float rollBreakPoint = 0.4f;
    [Range(0, 0.5f)] public float pitchBreakPoint = 0.4f;


    public AnimationCurve pitchInputCurve;
    public AnimationCurve rollInputCurve;
    public AnimationCurve yawInputCurve;
    [Range(1, 3)] public float pitchScale = 1, rollScale = 1, yawScale = 1;



    // ------------------------------------------ Rotation
    public float pitchAngle;
    public float rollAngle;
    public float yawAngle;


    // ------------------------------------------ Rotation
    public float pitchRate;
    public float rollRate;
    public float yawRate;
    public float turnRate;


    // ------------------------------------------ Performance
    public float currentSpeed;
    public float currentAltitude;
    public float currentClimbRate;
    public float currentHeading;
    public float timeStep;



    // ------------------------------------------ Airbrake Config
    [Tooltip("Maximum aileron deflection in speedbrake mode")] public float aileronSet = 30f;
    [Tooltip("Maximum rudder deflection in speedbrake mode")] public float ruddervatorSet = 30f;
    [Tooltip("Maximum flap deflection in speedbrake mode")] public float flapSet = 20f;
    public bool airbrakeActive;
    public float actuationSpeed = 30f;
    public float currentAileronSet, currentFlapSet, currentRudderSet;


    // ------------------------------------------ Control Output
    public float processedPitch;
    public float processedRoll;
    public float processedYaw;
    public float processedThrottle;
    public float processedStabilizerTrim;
    public float processedSteeringInput;
    public float processedTillerInput;

    public float commandRollInput = 0f;
    public float commandYawInput = 0f;
    public float commandPitchInput = 0f;

    public float processedPitchTrim;
    public float processedRollTrim;
    public float processedYawTrim;
    float presetRollInput = 0f;
    float presetYawInput = 0f;
    float presetPitchInput = 0f;



    // -------------------------------------------- Operation Variables
    [HideInInspector] public float computerAngularDrag = 5f;
    private float baseAngularDrag;
    private float baseHeading;


    // -------------------------------------------- Command Variables
    public float pitchAngleError, rollAngleError, yawAngleError;
    public float pitchRateError, rollRateError, yawRateError;
    public float speedError, altitudeError;
    public float climbRateError;
    public float headingError;
    public float machError;
    public float gError;
    public float alphaError;


    // -------------------------------------------- Command Variables
    public float commandSpeed = 150f;//Knots
    public float commandRollRate = 0f;
    public float commandPitchRate = 0f;
    public float commandYawRate = 0f;
    public float commandTurnRate = 0f;

    public float commandBankAngle = 0f;
    public float commandPitchAngle = 0f;
    public float commandAltitude = 1000f;
    public float commandClimbRate = 500f;

    public float commandHeading = 0f;
    public float commandMach = 0.5f;
    public float commandG = 1f, loadCommand = 1f;
    public float commandAlpha = 1f;
    float baseRollRateCommand;
    float basePitchRateCommand;

    public AnimationCurve climbPitchCommandCurve;
    public AnimationCurve cruisePitchCommandCurve;
    public AnimationCurve performancePitchCommandCurve;
    public AnimationCurve lateralRateCurve;
 

    public float presetRollAngle, presetPitchAngle = 0;
    public float balancePitchAngle = 0, balanceRollAngle = 0f;

    // -------------------------------------------- SAS Command
    public float balanceRollRate = 10f;
    public float balancePitchRate = 10f;
    public float balanceYawRate = 10f;
    public float maximumBankAngle = 30f;
    public float maximumTurnBank = 50f;
    public float maximumClimbRate = 500f;
    public float maximumTurnRate = 4f;
    public float maximumPitchAngle = 30f;//Nose Up
    public float minimumPitchAngle = 15f;//Nose Down


    [Tooltip("Speed (knots) above which the gear is automatically retracted")] public float maximumGearSpeed = 190f;
    [Tooltip("Speed (knots) above which the FCS takes over control of the aircraft")] public float minimumControlSpeed = 40f;
    public float maximumLoadFactor = 9f;
    public float minimumLoadFactor = 3f;
   
    


    // ------------------------------------------ Control Bias Points
    [Range(0, 100)] public float balanceRollAuthority = 20f;
    [Range(0, 100)] public float balancePitchAuthority = 30f;
    [Range(0, 100)] public float balanceYawAuthority = 20f;

    [Range(0, 100)] public float commandRollAuthority = 80f;
    [Range(0, 100)] public float commandPitchAuthority = 80f;
    [Range(0, 100)] public float commandYawAuthority = 70f;

    [Range(10, 100)] public float controlCylceRate = 40f; //Control Update Factor...Lower is faster


    // ------------------------------------------ Solvers
    //1. Core
    public SilantroPID throttleSolver;
    public SilantroPID rollRateSolver;
    public SilantroPID pitchRateSolver;
    public SilantroPID yawRateSolver;

    //2. SAS
    public SilantroPID rollAngleSolver;
    public SilantroPID pitchAngleSolver;
    public SilantroPID yawAngleSolver;
    public SilantroPID climbSolver;
    public SilantroPID altitudeSolver;
    public SilantroPID turnSolver;

    //3. CAS
    public SilantroPID loadFactorSolver;
    public bool rollLimitActive;
    public bool pitchLimitActive;
    public bool gLimitActive;
    public SilantroPID pitchAlphaSolver;



    // ------------------------------------------ Equipments
    public SilantroActuator gearSolver;
    public SilantroActuator speedBrakeSolver;
    public bool overideSpeedbrake;
    public bool performanceMode;
    public float basePitchRate;

    


    // ------------------------------------------ Roll Rate
    public float climboutRollRate = 90f;
    public float cruiseRollRate = 100f;
    [Tooltip("Maximum roll rate the aircraft is allowed to reach")] public float maximumRollRate = 200f;


    public float climboutPitchRate = 10f, climboutG = 4f;
    public float cruisePitchRate = 32f, cruiseG = 9f;
    public float maximumPitchRate = 30f;

    // ------------------------------------------ Stall Protection
    public List<float> wingStallAngles;
    public List<float> wingLiftCoefficient;
    public float maximumWingAlpha;
    public float baseStallAngle;
    public float alphaProt = 10f;
    public float alphaFloor = 10f;
    [Range(0,5)]public float alphaThreshold = 2f;
    public bool protectionActive;
    [Tooltip("Lowest speed (knots) the aircraft is allowed to reach before the FCS overides throttle control.")] public float minimumSpeed = 150f;
    public float gThreshold = 9f;


    [Tooltip("Slat deflection for takeoff.")] public float takeOffSlat = 15f;
    [Tooltip("Target slat deflection to augment for lift during manoeuvres.")] public float performanceSlat = 25f;
    public float knotSpeed, ftHeight;
    public float wingArea;
    public float baseliftCoefficient;
    public float wingLift;


    // ----------------------------------------------- Airline Variables
    public float absoluteBankAngle = 67f;
    public float maximumClimbPitchRate = 5f;
    public bool overidePitchCommand;
    public enum PitchOveride { None, AOA, Overspeed }
    public PitchOveride overideControl = PitchOveride.None;

    public float maximumSpeed = 200f;
    public float maximumMachSpeed = 0.6f;


    public Gain basePitchGain;
    public Gain cruisePitchGain;
    public Gain performancePitchGain;
    public GainVector currentPitchGain;

    public Gain baseRollGain;
    public Gain cruiseRollGain;
    public Gain performanceRollGain;
    public GainVector currentRollGain;

    public Gain baseYawGain;
    public Gain cruiseYawGain;
    public Gain performanceYawGain;
    public GainVector currentYawGain;


    // ---------------------------------
    public AudioSource stallWarnerSource, gWarnerSource;
    public AudioClip stallClip, gClip;
    public bool stalling, overging;
    public float stallSpeed = 100f;
    [Range(0, 1f)] public float alarmVolume = 0.75f, gAlarmVolume = 0.75f;


    public string aircraftIdentifier;
    public SilantroGain gainSet;
    public SilantroGain gainSetInput;
    public string saveLocation = "Assets/Silantro Simulator/Fixed Wing/Presets/Gain Sheet/";
    bool allOk;




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SwitchToSAS() { operationMode = AugmentationType.StabilityAugmentation; }
    public void SwitchToCAS() { operationMode = AugmentationType.CommandAugmentation; }
    public void SwitchToManual() { operationMode = AugmentationType.Manual; }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeComputer()
    {
        //----------------------------
        CheckPrerequisites();

        if (allOk)
        {
            baseAngularDrag = controller.aircraft.angularDrag;
            baseHeading = controller.core.headingDirection;
            basePitchRate = cruisePitchRate;
            //controlCylceRate = 60f;

            // --------------------------------------
            PlotInputCurve();

            // --------------------------------------
            if(operationMode == AugmentationType.Autonomous) { brain.computer = this.GetComponent<SilantroFlightComputer>(); brain.controller = controller; brain.InitializeBrain(); }


            GameObject soundPoint = new GameObject("Sources"); soundPoint.transform.parent = this.transform; soundPoint.transform.localPosition = Vector3.zero;
            if (stallWarner == ControlState.Active) { if (stallClip) { Handler.SetupSoundSource(soundPoint.transform, stallClip, "Stall Sound Point", 100f, true, false, out stallWarnerSource); stallWarnerSource.volume = alarmVolume; } }
            if (gLimiter == ControlState.Active && gWarner == ControlState.Active) { if (gClip) { Handler.SetupSoundSource(soundPoint.transform, gClip, "G Sound Point", 100f, true, false, out gWarnerSource); gWarnerSource.volume = gAlarmVolume; } }

            if (controller.input.inputConfigured) { Debug.Log("Flight computer on " + controller.transform.name + " is starting in " + operationMode.ToString() + " mode") ;}
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void FilterWingData()
    {
        // ---------------------------------- Filter Stall
        wingStallAngles = new List<float>();wingArea = 0f;
        foreach (SilantroAerofoil foil in wingFoils)
        {
            if (foil.rootAirfoil != null && foil.tipAirfoil != null)
            {
                wingStallAngles.Add(foil.rootAirfoil.stallAngle);wingLiftCoefficient.Add(foil.rootAirfoil.maxCl);
                wingStallAngles.Add(foil.tipAirfoil.stallAngle); wingLiftCoefficient.Add(foil.tipAirfoil.maxCl);
                wingArea += foil.foilArea;
            }
        }
        if (wingStallAngles.Count > 0) { baseStallAngle = wingStallAngles.Min(); baseliftCoefficient = wingLiftCoefficient.Min(); }
        if (baseStallAngle == 0 || baseStallAngle > 90f) { baseStallAngle = 15f; baseliftCoefficient = 1.5f; }
        alphaProt = baseStallAngle * 0.60f;
        alphaFloor = baseStallAngle - Mathf.Abs(alphaThreshold);
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void CheckPrerequisites()
    {
        //CHECK COMPONENTS
        if (controller != null && core != null) { allOk = true; }
        else if (controller == null)
        {
            Debug.LogError("Prerequisites not met on FCS " + " " + transform.name + "....operational aircraft not assigned");
            allOk = false;
        }
        else if (core == null)
        {
            Debug.LogError("Prerequisites not met on FCS " + " " + transform.name + "....data point not connected");
            allOk = false;
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void PlotInputCurve()
    {
        pitchInputCurve = MathBase.PlotControlInputCurve(pitchScale);
        rollInputCurve = MathBase.PlotControlInputCurve(rollScale);
        yawInputCurve = MathBase.PlotControlInputCurve(yawScale);
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Update()
    {
        if (allOk && controller != null && controller.isControllable)
        {
            timeStep = Time.deltaTime; if (timeStep > Time.fixedDeltaTime) { timeStep = Time.fixedDeltaTime; }//STUTTER PROTECTION
            knotSpeed = currentSpeed * MathBase.toKnots;
            ftHeight = currentAltitude * MathBase.toFt;


            // --------------------------------------------- Collect Control Inputs
            basePitchInput = controller.input.pitchInput;
            baseRollInput = controller.input.rollInput;
            baseYawInput = controller.input.yawInput;
            baseThrottleInput = controller.input.rawThrottleInput;
            baseStabilizerTrimInput = -controller.input.stabilizerTrimInput;
            baseTillerInput = controller.input.tillerInput;


            // ----------------------------------------------- Noise Filter
            if (Mathf.Abs(baseRollInput) > rollDeadZone) { presetRollInput = baseRollInput; } else { presetRollInput = 0f; }
            if (Mathf.Abs(basePitchInput) > pitchDeadZone) { presetPitchInput = basePitchInput; } else { presetPitchInput = 0f; }
            if (Mathf.Abs(baseYawInput) > yawDeadZone) { presetYawInput = baseYawInput; } else { presetYawInput = 0f; }

            commandPitchInput = ((Mathf.Abs(presetPitchInput) - pitchDeadZone) / (1 - pitchDeadZone)) * presetPitchInput;
            commandRollInput = ((Mathf.Abs(presetRollInput) - rollDeadZone) / (1 - rollDeadZone)) * presetRollInput;
            commandYawInput = ((Mathf.Abs(presetYawInput) - yawDeadZone) / (1 - yawDeadZone)) * presetYawInput;


            // --------------------------------------------- Collect Performance Data
            currentAltitude = core.currentAltitude;
            currentClimbRate = core.verticalSpeed;
            currentSpeed = core.currentSpeed;
            currentHeading = core.headingDirection; if (currentHeading > 180) { currentHeading -= 360f; }


            //--------------------------------------------- Estimate Rotation Angles
            float rawPitchAngle = controller.transform.eulerAngles.x; if (rawPitchAngle > 180) { rawPitchAngle -= 360f; }
            float rawRollAngle = controller.transform.eulerAngles.z; if (rawRollAngle > 180) { rawRollAngle -= 360f; }
            float rawYawAngle = controller.transform.eulerAngles.y; if (rawYawAngle > 180) { rawYawAngle -= 360f; }

            rollAngle = (float)Math.Round(rawRollAngle, 2);
            pitchAngle = (float)Math.Round(rawPitchAngle, 2);
            yawAngle = (float)Math.Round(rawYawAngle, 2);
            maximumWingAlpha = -90f; foreach (SilantroAerofoil foil in wingFoils) { if (foil.maximumAOA > maximumWingAlpha) { maximumWingAlpha = foil.maximumAOA; } }


            pitchRate = core.pitchRate;
            yawRate = core.yawRate;
            rollRate = core.rollRate;
            turnRate = core.turnRate;



            //---------------------------------------------OPERATION MODE
            switch (operationMode)
            {

                case AugmentationType.Manual: ManualControl(); break;
                case AugmentationType.StabilityAugmentation: AssitedControl(); break;
                case AugmentationType.CommandAugmentation: CommandControl(); break;
                case AugmentationType.Autonomous: brain.UpdateBrain(); break;
            }


            /// ----------------------------------------- General Functions
            if (operationMode != AugmentationType.Autonomous) { GeneralControl(); }


            //------------------------------------------- Gain Scheduling
            if (gainSystem == GainSystem.Dynamic && operationMode != AugmentationType.Manual) { AnalyseGains(); }


            //------------------------------------------- Recover Aircraft
            if (recoveryMode == ControlState.Active) { RecoverAircraft(); }
        }
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void GeneralControl()
    {

        // ------------------------------------------------------------------------------------------------------ Steering Control
        processedSteeringInput = commandYawInput;
        processedTillerInput = baseTillerInput;



        // -------------- Stall Warning System
        //1. Speed
        wingLift = Mathf.Abs(controller.core.gForce) * controller.currentWeight * 9.81f; ;
        float w = core.airDensity * wingArea * baseliftCoefficient;
        stallSpeed = Mathf.Sqrt(wingLift / w);


        //2. AOA
        float stallThreshold = stallSpeed; if(currentAltitude > 100 && currentSpeed > 10f && !controller.isGrounded) { 
        if (Mathf.Abs(maximumWingAlpha) >= Mathf.Abs(alphaFloor) || Mathf.Approximately(currentSpeed,stallThreshold)) { stalling = true; } else { stalling = false; }} else{ stalling = false; }
        if (stallWarnerSource != null && stallWarner == ControlState.Active)
        {
            if (stalling && !stallWarnerSource.isPlaying) { stallWarnerSource.Play(); }
            if (!stalling && stallWarnerSource.isPlaying) { stallWarnerSource.Stop(); }
        }

        // -------------- G Warning System
        if (gLimiter == ControlState.Active && gWarner == ControlState.Active && operationMode != AugmentationType.Manual)
        {
            if (maximumLoadFactor < gThreshold) { gThreshold = maximumLoadFactor; }
            if(core.gForce < -3f || core.gForce > gThreshold) { overging = true; } else { overging = false; }
            if (gWarnerSource)
            {
                if (overging && !gWarnerSource.isPlaying) { gWarnerSource.Play(); }
                if (!overging && gWarnerSource.isPlaying) { gWarnerSource.Stop(); }
                gWarnerSource.volume = gAlarmVolume;
            }
        }


        // ---------------------------------------------------------------- Speed Protector
        if (speedProtector == ControlState.Active)
        {
            if (controller.isGrounded == false && currentAltitude > 10f)
            {
                if (protectorMode == ControlMode.Manual) { if (currentSpeed < (minimumSpeed / MathBase.toKnots)) { protectionActive = true; commandSpeed = minimumSpeed; } else { protectionActive = false; } }
                else if (protectorMode == ControlMode.Automatic) { if (currentSpeed < stallSpeed) { protectionActive = true; commandSpeed = stallSpeed * MathBase.toKnots; } else { protectionActive = false; } }
            }
            else { protectionActive = false; }

            if(protectionActive)
            {
                // ---------------- Auto Throttle
                float presetSpeed = commandSpeed / MathBase.toKnots;
                speedError = (presetSpeed - currentSpeed);
                processedThrottle = throttleSolver.CalculateOutput(speedError, timeStep);
                if (float.IsNaN(processedThrottle) || float.IsInfinity(processedThrottle)) { processedThrottle = 0f; }
            }
        }




        // ------------------------------------------------------------------------------------------------------ Surface Airbrake
        if(airbrakeType == AirbrakeType.SurfaceOnly || airbrakeType == AirbrakeType.Combined)
        {
            if (airbrakeActive)
            {
                currentAileronSet = Mathf.MoveTowards(currentAileronSet, aileronSet, actuationSpeed * 1/controlCylceRate);
                currentFlapSet = Mathf.MoveTowards(currentFlapSet, flapSet, actuationSpeed * 1 / controlCylceRate);
                currentRudderSet = Mathf.MoveTowards(currentRudderSet, ruddervatorSet, actuationSpeed * 1 / controlCylceRate);
            }
            else
            {
                currentAileronSet = Mathf.MoveTowards(currentAileronSet, 0, actuationSpeed * 1 / controlCylceRate);
                currentFlapSet = Mathf.MoveTowards(currentFlapSet, 0, actuationSpeed * 1 / controlCylceRate);
                currentRudderSet = Mathf.MoveTowards(currentRudderSet, 0, actuationSpeed * 1 / controlCylceRate);
            }

            // -------------------------- Send to wing
            foreach (SilantroAerofoil foil in controller.wings)
            {
                foil.airbrakeActive = airbrakeActive;
            }
        }



        if (operationMode != AugmentationType.Autonomous)
        {
            // ------------------------------------------------------------------------------------------------------ Throttle/Power
            if (autoThrottle == ControlState.Off && !protectionActive) { processedThrottle = baseThrottleInput; }
            else
            {

                if (machHold == ControlState.Off)
                {
                    // ---------------- Auto Throttle
                    float presetSpeed = commandSpeed / MathBase.toKnots;
                    speedError = (presetSpeed - currentSpeed);
                    processedThrottle = throttleSolver.CalculateOutput(speedError, timeStep);
                    if (float.IsNaN(processedThrottle) || float.IsInfinity(processedThrottle)) { processedThrottle = 0f; }
                }
                else
                {
                    machError = commandMach - core.machSpeed;
                    float presetSpeed = commandMach * core.soundSpeed;
                    speedError = (presetSpeed - currentSpeed);
                    processedThrottle = throttleSolver.CalculateOutput(speedError, timeStep);
                    if (float.IsNaN(processedThrottle) || float.IsInfinity(processedThrottle)) { processedThrottle = 0f; }
                }

                // ----------------- Boost e.g Piston Turbo or Turbine Reheat
                if (processedThrottle > 1f && speedError > 5f && controller.input.boostState == SilantroInput.BoostState.Off) { controller.input.EngageBoost(); }
                if (processedThrottle < 1f && speedError < 3f && controller.input.boostState == SilantroInput.BoostState.Active) { controller.input.DisEngageBoost(); }



                if (airbrakeControl == ControlMode.Automatic)
                {
                    // ----------------- Boost Deceleration with Speedbrake
                    if (speedBrakeSolver != null && overideSpeedbrake && ftHeight > 500f)
                    {
                        if (speedBrakeSolver.actuatorState == SilantroActuator.ActuatorState.Disengaged && speedError < -30f) { speedBrakeSolver.EngageActuator(); }
                        if (speedBrakeSolver.actuatorState == SilantroActuator.ActuatorState.Engaged && speedError > -20f) { speedBrakeSolver.DisengageActuator(); }
                    }
                }
            }
        }


        //--------------------------------------------------------------------------------------------------------- Gear Handling
        if (gearSolver != null && gearControl == ControlMode.Automatic)
        {

            if (gearSolver.actuatorState == SilantroActuator.ActuatorState.Engaged && !controller.isGrounded && knotSpeed > 100f && ftHeight > 50f)
            {
                //1. ----------------------- Check Speed
                if (knotSpeed >= maximumGearSpeed) { gearSolver.DisengageActuator(); }

                //2.------------------------ Check Acceleration
                if (speedError > 50f) { gearSolver.DisengageActuator(); }
            }
        }


        //--------------------------------------------------------------------------------------------------------- Gain State
        if (gearSolver != null)
        {
            if(gearSolver.actuatorState == SilantroActuator.ActuatorState.Engaged) { state = CraftState.TakeoffLanding; }
            else { state = CraftState.Cruise; }
        }
        else
        {
            if(knotSpeed > 180f) { state = CraftState.Cruise; }
            else { state = CraftState.TakeoffLanding; }
        }


        // ------------------------------------------------------------------------------------------------------ Slats
        if (autoSlat == ControlState.Active)
        {
            // ----------------------------------------- Alpha Protection During Takeoff
            if (ftHeight < 500f && knotSpeed > 100)
            {
                foreach (SilantroAerofoil foil in wingFoils)
                {
                    // if (foil.slatState == SilantroAerofoil.ControlState.Active) { foil.baseSlat = Mathf.MoveTowards(foil.baseSlat, takeOffSlat, foil.slatActuationSpeed * Time.fixedDeltaTime); }
                }
            }



            // -------------------------------------- Alpha Protection During Flight
            if (state == CraftState.Cruise)
            {
                if (Mathf.Abs(maximumWingAlpha) > Mathf.Abs(alphaProt) || core.gForce > 4f)
                {
                    foreach (SilantroAerofoil foil in wingFoils)
                    {
                        if (foil.slatState == SilantroAerofoil.ControlState.Active) { foil.baseSlat = Mathf.MoveTowards(foil.baseSlat, performanceSlat, foil.slatActuationSpeed * Time.fixedDeltaTime); }
                    }
                }
                else
                {
                    foreach (SilantroAerofoil foil in wingFoils)
                    {
                        if (foil.slatState == SilantroAerofoil.ControlState.Active) { foil.baseSlat = Mathf.MoveTowards(foil.baseSlat, 0, foil.slatActuationSpeed * Time.fixedDeltaTime); }
                    }
                }
            }
        }




        // ------------------------------------------------------------------------------------------------------ Flaps
        if (flapControl == ControlMode.Automatic)
        {
            // ----------------------------- Cruise Config
            if (ftHeight > 100 && !controller.isGrounded && knotSpeed >= maximumGearSpeed)
            {
                foreach (SilantroAerofoil foil in controller.wings) { if (foil.flapSetting != 0) { foil.SetFlaps(0, 1); } }
            }

            // ----------------------------- Takeoff Config
            if (controller.isGrounded && knotSpeed < 50)
            {
                foreach (SilantroAerofoil foil in controller.wings) { if (foil.flapSetting != 1) { foil.SetFlaps(1, 1); } }
            }
        }
    }










    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Control surface deflection is directly proportional to the control inputs i.e the surfaces are driven
    /// directly from the unfilter inputs
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void ManualControl()
    {
        processedPitch = commandPitchInput;
        processedRoll = commandRollInput;
        processedYaw = commandYawInput;
        processedStabilizerTrim = -baseStabilizerTrimInput;
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void AssitedControl()
    {
        // ----------------------- Stabilization Bias
        float balancePitchBias = balancePitchAuthority / 100f;
        float balanceYawBias = balanceYawAuthority / 100f;
        float balanceRollBias = balanceRollAuthority / 100f;

        float commandPitchBias = commandPitchAuthority / 100f;
        float commandRollBias = commandRollAuthority / 100f;
        float commandYawBias = commandYawAuthority / 100f;



        #region Single Axis
        //1. Wing Leveler // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        if (controlType == ControlType.SingleAxis)
        {
            // -------------------------------------------------- Bank Limiter
            if (bankLimiter == ControlState.Active)
            {
                if (baseRollInput > rollDeadZone && rollAngle < -(maximumBankAngle)) { presetRollAngle = maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Right; }
                else if (baseRollInput < -rollDeadZone && rollAngle > (maximumBankAngle)) { presetRollAngle = -maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Left; }

                if (rollLimitActive)
                {
                    if (bankState == BankLimitState.Left) { if (baseRollInput >= -rollDeadZone) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                    if (bankState == BankLimitState.Right) { if (baseRollInput <= rollDeadZone) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                }
            }
            else { presetRollAngle = 0f; }

            // -------------------------------------------- Roll Rate Required
            rollAngleError = rollAngle - (-1f * presetRollAngle);
            rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
            commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

            // -------------------------------------------- Send
            rollRateError = commandRollRate - rollRate;
            if (!rollLimitActive)
            {
                rollRateSolver.minimum = -balanceRollBias; rollRateSolver.maximum = balanceRollBias;
                float balanceRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                processedRoll = balanceRoll + commandRollInput;
            }
            else
            {
                rollRateSolver.minimum = -commandRollBias; rollRateSolver.maximum = commandRollBias;
                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
            }

            // -------------------------------------------- Others
            processedPitch = commandPitchInput;
            processedYaw = commandYawInput;
            processedStabilizerTrim = -baseStabilizerTrimInput;
        }
        #endregion Single Axis

        #region Two Axis
        //2. Wing and Nose Leveler // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        if (controlType == ControlType.TwoAxis)
        {

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------ROLL AXIS------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (bankHold == ControlState.Off)
            {
                // -------------------------------------------------- Bank Limiter
                if (bankLimiter == ControlState.Active)
                {
                    if (baseRollInput > rollDeadZone && rollAngle < -(maximumBankAngle)) { presetRollAngle = maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Right; }
                    else if (baseRollInput < -rollDeadZone && rollAngle > (maximumBankAngle)) { presetRollAngle = -maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Left; }

                    if (rollLimitActive)
                    {
                        if (bankState == BankLimitState.Left) { if (baseRollInput > rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                        if (bankState == BankLimitState.Right) { if (baseRollInput < -rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                    }
                }
                else { presetRollAngle = 0f; }

                // -------------------------------------------- Roll Rate Required
                rollAngleError = rollAngle - (-1f * presetRollAngle);
                rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                // -------------------------------------------- Send
                rollRateError = commandRollRate - rollRate;
                if (!rollLimitActive)
                {
                    rollRateSolver.minimum = -balanceRollBias; rollRateSolver.maximum = balanceRollBias;
                    float balanceRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);


                    if (augmentationMode == SASInputMode.Bypass)
                    {
                        if (balanceRoll < 0)
                        {
                            if (commandRollInput < 0) { if (commandRollInput < balanceRoll) { processedRoll = commandRollInput; } }
                            else { processedRoll = commandRollInput; }
                        }
                        else if (balanceRoll > 0)
                        {
                            if (commandRollInput > 0) { if (commandRollInput > balanceRoll) { processedRoll = commandRollInput; } }
                            else { processedRoll = commandRollInput; }
                        }
                        if (commandRollInput == 0) { processedRoll = balanceRoll; }
                    }
                    else { processedRoll = balanceRoll + commandRollInput; }
                }
                else
                {
                    rollRateSolver.minimum = -commandRollBias; rollRateSolver.maximum = commandRollBias;
                    processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                }
            }
            else
            {
                // -------------------------------------------- Roll Rate Required
                rollAngleError = rollAngle - (-1f * commandBankAngle);
                rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                rollRateError = commandRollRate - rollRate;
                rollRateSolver.minimum = -commandRollBias; rollRateSolver.maximum = commandRollBias;
                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
            }



            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------PITCH AXIS-----------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------- Pitch Limiter
            if (pitchLimiter == ControlState.Active)
            {

                if (basePitchInput > pitchDeadZone && pitchAngle > minimumPitchAngle) { presetPitchAngle = -minimumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Down; }
                else if (basePitchInput < -pitchDeadZone && pitchAngle < -maximumPitchAngle) { presetPitchAngle = maximumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Up; }

                if (pitchLimitActive)
                {
                    if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                    if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                }
            }
            else { presetPitchAngle = 0f; }


            // -------------------------------------------- Pitch Rate Required
            pitchAngleError = pitchAngle - (-1f * presetPitchAngle);
            pitchAngleSolver.minimum = -balancePitchRate; pitchAngleSolver.maximum = balancePitchRate;
            commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

            // --------------------------------------------- Send
            pitchRateError = pitchRate - commandPitchRate;

            if (!pitchLimitActive)
            {
                pitchRateSolver.minimum = -balancePitchBias; pitchRateSolver.maximum = balancePitchBias;
                float balancepitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);

                if (augmentationMode == SASInputMode.Bypass)
                {
                    if (balancepitch < 0)
                    {
                        if (commandPitchInput < 0) { if (commandPitchInput < balancepitch) { processedPitch = commandPitchInput; } }
                        else { processedPitch = commandPitchInput; }if (commandPitchInput == 0) { processedPitch = balancepitch; }
                    }
                    else if (balancepitch > 0)
                    {
                        if (commandPitchInput > 0) { if (commandPitchInput > balancepitch) { processedPitch = commandPitchInput; } }
                        else { processedPitch = commandPitchInput; }if (commandPitchInput == 0) { processedPitch = balancepitch; }
                    }
                    if (commandPitchInput == 0) { processedPitch = balancepitch; }

                    //-------------------Autotrim
                    if (trimState == ControlState.Active && knotSpeed > 50f) { balancePitchAngle = -pitchAngle; } else { balancePitchAngle = 0f; }
                }
                else { processedPitch = balancepitch + commandPitchInput; }
            }
            else
            {
                pitchRateSolver.minimum = -commandPitchBias; pitchRateSolver.maximum = commandPitchBias;
                processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
            }



            // -------------------------------------------- Others
            processedYaw = commandYawInput;
            processedStabilizerTrim = -baseStabilizerTrimInput;
        }
        #endregion Two Axis

        #region Three Axis
        //3. Three Axis Control with Autopilot // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        if (controlType == ControlType.ThreeAxis)
        {

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------ROLL AXIS------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (headingHold == ControlState.Off)
            {
                if (bankHold == ControlState.Off)
                {
                    // -------------------------------------------------- Bank Limiter
                    if (bankLimiter == ControlState.Active)
                    {
                        if (baseRollInput > rollDeadZone && rollAngle < -(maximumBankAngle)) { presetRollAngle = maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Right; }
                        else if (baseRollInput < -rollDeadZone && rollAngle > (maximumBankAngle)) { presetRollAngle = -maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Left; }

                        if (rollLimitActive)
                        {
                            if (bankState == BankLimitState.Left) { if (baseRollInput > rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = balanceRollAngle; } }
                            if (bankState == BankLimitState.Right) { if (baseRollInput < -rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = balanceRollAngle; } }
                        }
                    }
                    else { presetRollAngle = balanceRollAngle; }


                    // -------------------------------------------- Roll Rate Required
                    rollAngleError = rollAngle - (-1f * presetRollAngle);
                    rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                    commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                    // -------------------------------------------- Send
                    rollRateError = commandRollRate - rollRate;
                    if (!rollLimitActive)
                    {
                        rollRateSolver.minimum = -balanceRollBias; rollRateSolver.maximum = balanceRollBias;
                        float balanceRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);

                        if (augmentationMode == SASInputMode.Bypass)
                        {
                            if (balanceRoll < 0)
                            {
                                if (commandRollInput < 0) { if (commandRollInput < balanceRoll) { processedRoll = commandRollInput; } }
                                else { processedRoll = commandRollInput; }
                            }
                            else if (balanceRoll > 0)
                            {
                                if (commandRollInput > 0) { if (commandRollInput > balanceRoll) { processedRoll = commandRollInput; } }
                                else { processedRoll = commandRollInput; }
                            }
                            if (commandRollInput == 0) { processedRoll = balanceRoll; }

                            // ------------------------------Autotrim
                            if (trimState == ControlState.Active && knotSpeed > 50f) { balanceRollAngle = -rollAngle; } else { balanceRollAngle = 0f; }
                        }
                        else { processedRoll = balanceRoll + commandRollInput; }
                    }
                    else
                    {
                        rollRateSolver.minimum = -commandRollBias; rollRateSolver.maximum = commandRollBias;
                        processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                    }
                }
                else
                {
                    // -------------------------------------------- Roll Rate Required
                    rollAngleError = rollAngle - (-1f * commandBankAngle);
                    rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                    commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                    rollRateError = commandRollRate - rollRate;
                    rollRateSolver.minimum = -commandRollBias; rollRateSolver.maximum = commandRollBias;
                    processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                }
            }
            else
            {
                float presetHeading = commandHeading; if (presetHeading > 180) { presetHeading -= 360f; }
                headingError = presetHeading - currentHeading;
                turnSolver.maximum = maximumTurnRate; turnSolver.minimum = -maximumTurnRate;
                commandTurnRate = turnSolver.CalculateOutput(headingError, timeStep);


                // ---------------------------------------------- Calculate Required Bank
                float rollFactor = (commandTurnRate * currentSpeed * MathBase.toKnots) / 1091f;
                commandBankAngle = Mathf.Atan(rollFactor) * Mathf.Rad2Deg;
                if (commandBankAngle > maximumTurnBank) { commandBankAngle = maximumTurnBank; }
                if (commandBankAngle < -maximumTurnBank) { commandBankAngle = -maximumTurnBank; }

                // -------------------------------------------- Roll Rate Required
                rollAngleError = rollAngle - (-1f * commandBankAngle);
                rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                rollRateError = commandRollRate - rollRate;
                rollRateSolver.minimum = -commandRollBias; rollRateSolver.maximum = commandRollBias;
                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------PITCH AXIS-----------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (altitudeHold == ControlState.Off)
            {
                if (pitchHold == ControlState.Off)
                {
                    // -------------------------------------------------- Pitch Limiter
                    if (pitchLimiter == ControlState.Active)
                    {
                        if (basePitchInput > pitchDeadZone && pitchAngle > minimumPitchAngle) { presetPitchAngle = -minimumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Down; }
                        else if (basePitchInput < -pitchDeadZone && pitchAngle < -maximumPitchAngle) { presetPitchAngle = maximumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Up; }

                        if (pitchLimitActive)
                        {
                            if (pitchState == PitchLimitState.Up) { if (basePitchInput > pitchBreakPoint) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = balancePitchAngle; } }
                            if (pitchState == PitchLimitState.Down) { if (basePitchInput < -pitchBreakPoint) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = balancePitchAngle; } }
                        }
                    }
                    else { presetPitchAngle = balancePitchAngle; }


                    // -------------------------------------------- Pitch Rate Required
                    pitchAngleError = pitchAngle - (-1f * presetPitchAngle);
                    pitchAngleSolver.minimum = -balancePitchRate; pitchAngleSolver.maximum = balancePitchRate;
                    commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                    // --------------------------------------------- Send
                    pitchRateError = pitchRate - commandPitchRate;

                    if (!pitchLimitActive)
                    {
                        pitchRateSolver.minimum = -balancePitchBias; pitchRateSolver.maximum = balancePitchBias;
                        float balancepitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);


                        if (augmentationMode == SASInputMode.Bypass)
                        {
                            if(balancepitch < 0)
                            {
                                if (commandPitchInput < 0) { if (commandPitchInput < balancepitch) { processedPitch = commandPitchInput; } }
                                else { processedPitch = commandPitchInput; } if (commandPitchInput == 0) { processedPitch = balancepitch; }
                            }
                            else if (balancepitch > 0)
                            {
                                if (commandPitchInput > 0) { if (commandPitchInput > balancepitch) { processedPitch = commandPitchInput; } }
                                else { processedPitch = commandPitchInput; } if (commandPitchInput == 0) { processedPitch = balancepitch; }
                            }
                            if(commandPitchInput == 0) { processedPitch = balancepitch; }

                            //-------------------Autotrim
                            if (trimState == ControlState.Active && knotSpeed > 50f) { balancePitchAngle = -pitchAngle; } else { balancePitchAngle = 0f; }
                        }
                        else { processedPitch = balancepitch + commandPitchInput; }
                    }
                    else
                    {
                        pitchRateSolver.minimum = -commandPitchBias; pitchRateSolver.maximum = commandPitchBias;
                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                    }
                }
                else
                {
                    // -------------------------------------------- Pitch Rate Required
                    pitchAngleError = pitchAngle - (-1f * commandPitchAngle);
                    pitchAngleSolver.minimum = -balancePitchRate; pitchAngleSolver.maximum = balancePitchRate;
                    commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                    pitchRateError = pitchRate - commandPitchRate;
                    pitchRateSolver.minimum = -commandPitchBias; pitchRateSolver.maximum = commandPitchBias;
                    processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                }
            }
            else
            {
                // ------------------------------------------------ Altitude Hold
                altitudeError = commandAltitude / MathBase.toFt - currentAltitude;
                float presetClimbLimit = maximumClimbRate / MathBase.toFtMin;
                altitudeSolver.maximum = presetClimbLimit; altitudeSolver.minimum = -presetClimbLimit;
                commandClimbRate = altitudeSolver.CalculateOutput(altitudeError, timeStep) * MathBase.toFtMin;

                // -------------------------------------------- Pitch Rate Required
                float presetClimbRate = commandClimbRate / MathBase.toFtMin;
                climbRateError = presetClimbRate - currentClimbRate;
                climbSolver.minimum = -balancePitchRate; climbSolver.maximum = balancePitchRate;
                commandPitchRate = climbSolver.CalculateOutput(climbRateError, timeStep);

                pitchRateError = pitchRate - commandPitchRate;
                pitchRateSolver.minimum = -commandPitchBias; pitchRateSolver.maximum = commandPitchBias;
                processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
            }




            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------YAW AXIS-------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (controller.isGrounded)
            {
                yawAngleError = baseHeading - yawAngle;
                yawAngleSolver.minimum = -balanceYawRate; yawAngleSolver.maximum = balanceYawRate;
                commandYawRate = yawAngleSolver.CalculateOutput(yawAngleError, timeStep);
            }
            else { commandYawRate = 0f; }

            if(headingHold != ControlState.Active)
            {
                yawRateError = commandYawRate - yawRate;
                yawRateSolver.minimum = -balanceYawBias; yawRateSolver.maximum = balanceYawBias;
                float balanceYaw = yawRateSolver.CalculateOutput(yawRateError, timeStep);
                processedYaw = balanceYaw + commandYawInput;
            }
            else
            {
                processedYaw = 0f;
            }
            processedStabilizerTrim = -baseStabilizerTrimInput;
        }

        #endregion Three Axis
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void RecoverAircraft()
    {
        if (operationMode == AugmentationType.CommandAugmentation || operationMode == AugmentationType.StabilityAugmentation)
        {
            autoThrottle = ControlState.Active; commandSpeed = 200f;
            altitudeHold = ControlState.Active; commandAltitude = 2000f; maximumClimbRate = 2000f;
            bankHold = ControlState.Active; commandBankAngle = 0f;
        }



        StartCoroutine(ReturnControls());
    } 


    IEnumerator ReturnControls()
    {
        float combinedError = Mathf.Abs(altitudeError) + Mathf.Abs(rollAngleError);// + Mathf.Abs(speedError);
        yield return new WaitUntil(() => combinedError < 5f);

        yield return new WaitForSeconds(3f);
        // ------------------------------- Normal Controls
        autoThrottle = ControlState.Off;
        altitudeHold = ControlState.Off;
        bankHold = ControlState.Off;
        recoveryMode = ControlState.Off;
    }







   
  






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Control surface deflection is driven based on the output commands from the flight computer e.g roll rate, pitch rate e.t.c
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void CommandControl()
    {

        // ---------------------------- Fighter
        if (commandPreset == CommandPreset.Fighter) { FighterCommandControl(); }

        // ---------------------------- Airlinier
        if (commandPreset == CommandPreset.Airliner) { AirlineCommandControl(); }
    }








   


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void AirlineCommandControl()
    {

        if (knotSpeed > minimumControlSpeed)
        {

            #region Roll Axis
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------ROll AXIS------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------

            if (headingHold == ControlState.Off)
            {
                if (bankHold == ControlState.Off)
                {
                    // -------------------------------------------------- Bank Limiter
                    if (baseRollInput > rollDeadZone && rollAngle < -(maximumBankAngle)) { presetRollAngle = maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Right; }
                    else if (baseRollInput < -rollDeadZone && rollAngle > (maximumBankAngle)) { presetRollAngle = -maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Left; }

                    if (rollLimitActive)
                    {
                        if (bankState == BankLimitState.Left) { if (baseRollInput > rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                        if (bankState == BankLimitState.Right) { if (baseRollInput < -rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                    }


                    if (!rollLimitActive)
                    {
                        if (knotSpeed > 50f && knotSpeed < 100f) { baseRollRateCommand = maximumRollRate * 0.3f; }
                        if (knotSpeed > 100f && knotSpeed < 200f) { baseRollRateCommand = maximumRollRate * 0.6f; }
                        if (knotSpeed > 200f) { baseRollRateCommand = maximumRollRate; }

                        commandRollRate = baseRollRateCommand * commandRollInput;
                        rollRateError = commandRollRate - rollRate;
                        processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                    }
                    else
                    {

                        float bankMarker = Mathf.Abs(rollAngle);
                        float absRollInput = Mathf.Abs(commandRollInput);

                        if (absRollInput > 0.9f)
                        {
                            if (bankMarker > absoluteBankAngle)
                            {
                                // -------------------------------------------- Roll Rate Required
                                rollAngleError = rollAngle - (-1f * absoluteBankAngle);
                                rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                                commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                                rollRateError = commandRollRate - rollRate;
                                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                            }
                            else
                            {
                                commandRollRate = baseRollRateCommand * Mathf.Sign(commandRollInput);
                                rollRateError = commandRollRate - rollRate;
                                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                            }
                        }
                        else
                        {
                            // -------------------------------------------- Roll Rate Required
                            rollAngleError = rollAngle - (-1f * presetRollAngle);
                            rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                            commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                            rollRateError = commandRollRate - rollRate;
                            processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                        }
                    }
                }
                else
                {
                    // -------------------------------------------- Roll Rate Required
                    rollAngleError = rollAngle - (-1f * commandBankAngle);
                    rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                    commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                    rollRateError = commandRollRate - rollRate;
                    processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                }
            }

            else
            {
                float presetHeading = commandHeading; if (presetHeading > 180) { presetHeading -= 360f; }
                headingError = presetHeading - currentHeading;
                turnSolver.maximum = maximumTurnRate; turnSolver.minimum = -maximumTurnRate;
                if (headingError < 1) { commandTurnRate = 0f; } else { commandTurnRate = turnSolver.CalculateOutput(headingError, timeStep); }


                // ---------------------------------------------- Calculate Required Bank
                float rollFactor = (commandTurnRate * currentSpeed * MathBase.toKnots) / 1091f;
                commandBankAngle = Mathf.Atan(rollFactor) * Mathf.Rad2Deg;
                if (commandBankAngle > maximumTurnBank) { commandBankAngle = 33f; }
                if (commandBankAngle < -maximumTurnBank) { commandBankAngle = -33f; }

                // -------------------------------------------- Roll Rate Required
                rollAngleError = rollAngle - (-1f * commandBankAngle);
                rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                rollRateError = commandRollRate - rollRate;
                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
            }

            #endregion Roll Axis

            #region Pitch Axis

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------Protection-----------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------



            // -------------------------------------------------- Overspeed
            float absoluteSpeed = maximumSpeed + 16f;//
            float absoluteMach = maximumMachSpeed + 0.04f;
            if (knotSpeed > absoluteSpeed || core.machSpeed > absoluteMach && overideControl == PitchOveride.None) { overideControl = PitchOveride.Overspeed; overidePitchCommand = true; }
            else if (knotSpeed < absoluteSpeed || core.machSpeed < absoluteMach && overideControl == PitchOveride.Overspeed) { overideControl = PitchOveride.None; overidePitchCommand = false; }



            // -------------------------------------------------- AOA
            if (maximumWingAlpha > alphaProt && overideControl == PitchOveride.None) { overideControl = PitchOveride.AOA; overidePitchCommand = true; }
            else if (maximumWingAlpha < alphaProt && overideControl == PitchOveride.AOA) { overideControl = PitchOveride.None; overidePitchCommand = false; }



            if (altitudeHold == ControlState.Off)
            {
                if (pitchHold == ControlState.Off)
                {

                    // -------------------------------------------------- Pitch Limiter
                    if (pitchLimiter == ControlState.Active)
                    {
                        if (basePitchInput > pitchDeadZone && pitchAngle > minimumPitchAngle) { presetPitchAngle = -minimumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Down; }
                        else if (basePitchInput < -pitchDeadZone && pitchAngle < -maximumPitchAngle) { presetPitchAngle = maximumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Up; }

                        if (pitchLimitActive)
                        {
                            if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                            if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                        }
                    }

                    // -------------------------------------------------- G Limiter
                    if (gLimiter == ControlState.Active)
                    {
                        if (basePitchInput > pitchDeadZone && core.gForce < -minimumLoadFactor) { commandG = -minimumLoadFactor; gLimitActive = true; pitchState = PitchLimitState.Down; }
                        else if (basePitchInput < -pitchDeadZone && core.gForce > maximumLoadFactor) { commandG = maximumLoadFactor; gLimitActive = true; pitchState = PitchLimitState.Up; }

                        if (gLimitActive)
                        {
                            if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { gLimitActive = false; pitchState = PitchLimitState.Off; commandG = 0f; } }
                            if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { gLimitActive = false; pitchState = PitchLimitState.Off; commandG = 0f; } }
                        }
                    }


                    if (!gLimitActive)
                    {
                        if (!pitchLimitActive)
                        {
                            if (knotSpeed > 0 && knotSpeed < 200) { basePitchRateCommand = climboutPitchRate; }
                            if (knotSpeed > 200) { basePitchRateCommand = cruisePitchRate; }

                            commandPitchRate = basePitchRateCommand * commandPitchInput;
                            pitchRateError = pitchRate - (-1f * commandPitchRate);
                            processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                        }
                        else
                        {
                            // -------------------------------------------- Pitch Rate Required
                            pitchAngleError = pitchAngle - (-1f * presetPitchAngle);
                            pitchAngleSolver.maximum = maximumPitchRate; pitchAngleSolver.minimum = 0.5f * -maximumPitchRate;
                            commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                            pitchRateError = pitchRate - commandPitchRate;
                            processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                        }
                    }
                    else
                    {
                        commandPitchRate = (1845f * (commandG - 1)) / (currentSpeed * MathBase.toFt);
                        pitchRateError = pitchRate - (commandPitchRate);
                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                    }
                }
                else
                {
                    // -------------------------------------------- Pitch Rate Required
                    pitchAngleError = pitchAngle - (-1f * commandPitchAngle);
                    pitchAngleSolver.minimum = -balancePitchRate; pitchAngleSolver.maximum = balancePitchRate;
                    commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                    pitchRateError = pitchRate - commandPitchRate;
                    processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                }
            }
            else
            {
                // ------------------------------------------------ Altitude Hold
                altitudeError = commandAltitude / MathBase.toFt - currentAltitude;
                float presetClimbLimit = maximumClimbRate / MathBase.toFtMin;
                altitudeSolver.maximum = presetClimbLimit; altitudeSolver.minimum = -presetClimbLimit;
                commandClimbRate = altitudeSolver.CalculateOutput(altitudeError, timeStep) * MathBase.toFtMin;

                // -------------------------------------------- Pitch Rate Required
                float presetClimbRate = commandClimbRate / MathBase.toFtMin;
                climbRateError = presetClimbRate - currentClimbRate;
                climbSolver.minimum = -balancePitchRate * 0.2f; climbSolver.maximum = balancePitchRate * 0.5f;
                commandPitchRate = climbSolver.CalculateOutput(climbRateError, timeStep);


                pitchRateError = pitchRate - commandPitchRate;
                processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
            }


            #endregion Pitch Axis

            #region Yaw Axis

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------YAW AXIS------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------

            float yawFilter = Mathf.Abs(commandYawInput);

            if (yawFilter > 0.3f)
            {
                processedYaw = commandYawInput;
            }
            else
            {
                yawRateError = commandYawRate - yawRate;
                processedYaw = yawRateSolver.CalculateOutput(yawRateError, timeStep);
            }

            #endregion Yaw Axis
        }
        else
        {
            // - --------------------------- Proportional Control Law
            processedPitch = commandPitchInput;
            processedRoll = commandRollInput;
            processedYaw = commandYawInput;
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void FighterCommandControl()
    {
        if (knotSpeed > minimumControlSpeed)
        {
            if (commandMode == CASMode.Manual)
            {
                #region Roll Axis
                // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                // -------------------------------------------------------------ROll AXIS------------------------------------------------------------------------------------
                // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                if (headingHold == ControlState.Off)
                {
                    if (bankHold == ControlState.Off)
                    {
                        // -------------------------------------------------- Bank Limiter
                        if (bankLimiter == ControlState.Active)
                        {
                            if (baseRollInput > rollDeadZone && rollAngle < -(maximumBankAngle)) { presetRollAngle = maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Right; }
                            else if (baseRollInput < -rollDeadZone && rollAngle > (maximumBankAngle)) { presetRollAngle = -maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Left; }

                            if (rollLimitActive)
                            {
                                if (bankState == BankLimitState.Left) { if (baseRollInput > rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                                if (bankState == BankLimitState.Right) { if (baseRollInput < -rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                            }
                        }

                        if (!rollLimitActive)
                        {
                            if(state == CraftState.Cruise) { commandRollRate = cruiseRollRate * commandRollInput; }
                            else { commandRollRate = climboutRollRate * commandRollInput; }
                           
                            if (commandRollRate > maximumRollRate) { commandRollRate = maximumRollRate; }
                            rollRateError = commandRollRate - rollRate;
                            processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                        }
                        else
                        {
                            // -------------------------------------------- Roll Rate Required
                            rollAngleError = rollAngle - (-1f * presetRollAngle);
                            rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                            commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                            rollRateError = commandRollRate - rollRate;
                            processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                        }
                    }
                    else
                    {
                        // -------------------------------------------- Roll Rate Required
                        rollAngleError = rollAngle - (-1f * commandBankAngle);
                        rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                        commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                        rollRateError = commandRollRate - rollRate;
                        processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                    }
                }

                else
                {
                    float presetHeading = commandHeading; if (presetHeading > 180) { presetHeading -= 360f; }
                    headingError = presetHeading - currentHeading;
                    turnSolver.maximum = maximumTurnRate; turnSolver.minimum = -maximumTurnRate;
                    commandTurnRate = turnSolver.CalculateOutput(headingError, timeStep);


                    // ---------------------------------------------- Calculate Required Bank
                    float rollFactor = (commandTurnRate * currentSpeed * MathBase.toKnots) / 1091f;
                    commandBankAngle = Mathf.Atan(rollFactor) * Mathf.Rad2Deg;
                    if (commandBankAngle > maximumTurnBank) { commandBankAngle = maximumTurnBank; }
                    if (commandBankAngle < -maximumTurnBank) { commandBankAngle = -maximumTurnBank; }

                    // -------------------------------------------- Roll Rate Required
                    rollAngleError = rollAngle - (-1f * commandBankAngle);
                    rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                    commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                    rollRateError = commandRollRate - rollRate;
                    processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
                }


                #endregion Roll Axis

                #region Pitch Axis
                // ----------------------------------------------------------------------------------------------------------------------------------------------------------
                // -------------------------------------------------------------PITCH AXIS-----------------------------------------------------------------------------------
                // ----------------------------------------------------------------------------------------------------------------------------------------------------------


                if (altitudeHold == ControlState.Off)
                {
                    if (pitchHold == ControlState.Off)
                    {

                        // -------------------------------------------------- Pitch Limiter
                        if (pitchLimiter == ControlState.Active)
                        {
                            if (basePitchInput > pitchDeadZone && pitchAngle > minimumPitchAngle) { presetPitchAngle = -minimumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Down; }
                            else if (basePitchInput < -pitchDeadZone && pitchAngle < -maximumPitchAngle) { presetPitchAngle = maximumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Up; }

                            if (pitchLimitActive)
                            {
                                if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                                if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                            }
                        }

                        // -------------------------------------------------- G Limiter
                        if (gLimiter == ControlState.Active)
                        {
                            if (basePitchInput > pitchDeadZone && core.gForce < -minimumLoadFactor) { commandG = -minimumLoadFactor; gLimitActive = true; pitchState = PitchLimitState.Down; }
                            else if (basePitchInput < -pitchDeadZone && core.gForce > maximumLoadFactor) { commandG = maximumLoadFactor; gLimitActive = true; pitchState = PitchLimitState.Up; }

                            if (gLimitActive)
                            {
                                if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { gLimitActive = false; pitchState = PitchLimitState.Off; commandG = 0f; } }
                                if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { gLimitActive = false; pitchState = PitchLimitState.Off; commandG = 0f; } }
                            }
                        }



                        if (!gLimitActive)
                        {
                            if (!pitchLimitActive)
                            {
                                // ---------------------------- Takeoff/Landing Mode
                                if (state == CraftState.TakeoffLanding)
                                {
                                    if (longitudinalCommand == CommandInput.RateOnly)
                                    {
                                        float baseCommand = 0f;
                                        if (knotSpeed > 0 && knotSpeed < 200) { baseCommand = 0.5f * climboutPitchRate; }
                                        if (knotSpeed > 200 && knotSpeed < 250) { baseCommand = 0.65f * climboutPitchRate; }
                                        if (knotSpeed > 250 && knotSpeed < 300) { baseCommand = 0.75f * climboutPitchRate; }
                                        if (knotSpeed > 300 && knotSpeed < 350) { baseCommand = 0.85f * climboutPitchRate; }
                                        if (knotSpeed > 350) { baseCommand = climboutPitchRate; }

                                       

                                        commandPitchRate = commandPitchInput > 0f ? commandPitchInput * (baseCommand / 2f) : commandPitchInput * baseCommand;
                                        pitchRateError = pitchRate - (-1f * commandPitchRate);
                                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                                    }
                                    else
                                    {
                                        if (knotSpeed > 0 && knotSpeed < 200) { commandG = -0.35f * climboutG * commandPitchInput; }
                                        if (knotSpeed > 200 && knotSpeed < 250) { commandG = -0.5f * climboutG * commandPitchInput; }
                                        if (knotSpeed > 250 && knotSpeed < 300) { commandG = -0.65f * climboutG * commandPitchInput; }
                                        if (knotSpeed > 300 && knotSpeed < 350) { commandG = -0.75f * climboutG * commandPitchInput; }
                                        if (knotSpeed > 350) { commandG = climboutG * commandPitchInput; }

                                        // --------------------------------- Limit negative G command
                                        float commandCap = (knotSpeed / 250f) * 3f;
                                        if (knotSpeed > 250) { if (commandG > 3) { commandG = 3f; } }
                                        else { if (commandG > commandCap) { commandG = commandCap; } }
                                        if (commandG > 3) { commandG = 3f; }

                                        if (commandG > 9) { commandG = 9f; }
                                        if (commandG < -3f) { commandG = -3f; }
                                        // --------------------------------- Translate G to rate command
                                        loadCommand = commandG;
                                        commandPitchRate = (1845f * loadCommand) / (currentSpeed * MathBase.toFt);
                                        pitchRateError = pitchRate - (commandPitchRate);
                                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                                    }
                                }

                                // ---------------------------- Cruise Mode
                                if (state == CraftState.Cruise)
                                {
                                    if (longitudinalCommand == CommandInput.RateOnly)
                                    {
                                        float baseCommand = 0f;

                                        // --------------------- Set Rate
                                        float negativeFactor = 2f;
                                        if (performanceMode) { negativeFactor = 9f; }
                                        baseCommand = cruisePitchRate;
                                        commandPitchRate = commandPitchInput > 0f ? commandPitchInput * (baseCommand / negativeFactor) : commandPitchInput * baseCommand;
                                        pitchRateError = pitchRate - (-1f * commandPitchRate);
                                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                                    }
                                    else
                                    {
                                        commandG = commandPitchInput > 0f ? commandPitchInput * (-cruiseG / 3f) : commandPitchInput * -cruiseG;
                                        if (commandG > 9) { commandG = 9f; }
                                        if (commandG < -3f) { commandG = -3f; }

                                        // --------------------------------- Translate G to rate command
                                        loadCommand = commandG;
                                        commandPitchRate = (1845f * loadCommand) / (currentSpeed * MathBase.toFt);
                                        pitchRateError = pitchRate - (commandPitchRate);
                                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                                    }
                                }
                            }
                            else
                            {
                                // -------------------------------------------- Pitch Rate Required
                                pitchAngleError = pitchAngle - (-1f * presetPitchAngle);
                                pitchAngleSolver.maximum = maximumPitchRate; pitchAngleSolver.minimum = 0.5f * -maximumPitchRate;
                                commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                                pitchRateError = pitchRate - commandPitchRate;
                                processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                            }
                        }
                        else
                        {
                            commandPitchRate = (1845f * (commandG)) / (currentSpeed * MathBase.toFt);
                            pitchRateError = pitchRate - (commandPitchRate);
                            processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                        }
                    }
                    else
                    {
                        // -------------------------------------------- Pitch Rate Required
                        pitchAngleError = pitchAngle - (-1f * commandPitchAngle);
                        pitchAngleSolver.minimum = -balancePitchRate; pitchAngleSolver.maximum = balancePitchRate;
                        commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                        pitchRateError = pitchRate - commandPitchRate;
                        processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                    }
                }
                else
                {
                    // ------------------------------------------------ Altitude Hold
                    altitudeError = commandAltitude / MathBase.toFt - currentAltitude;
                    float presetClimbLimit = maximumClimbRate / MathBase.toFtMin;
                    if (headingHold == ControlState.Active && Mathf.Abs(headingError) > 5) { presetClimbLimit = Mathf.Abs(core.verticalSpeed * 1.5f); }
                    altitudeSolver.maximum = presetClimbLimit; altitudeSolver.minimum = -presetClimbLimit;
                    commandClimbRate = altitudeSolver.CalculateOutput(altitudeError, timeStep) * MathBase.toFtMin;

                    // -------------------------------------------- Pitch Rate Required
                    float presetClimbRate = commandClimbRate / MathBase.toFtMin;
                    climbRateError = presetClimbRate - currentClimbRate;
                    climbSolver.minimum = -balancePitchRate * 0.2f; climbSolver.maximum = balancePitchRate * 0.5f;
                    commandPitchRate = climbSolver.CalculateOutput(climbRateError, timeStep);

                    pitchRateError = pitchRate - commandPitchRate;
                    processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                }
            }


            #endregion Pitch Axis

                #region Yaw Axis

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // -------------------------------------------------------------YAW AXIS------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------

            if (headingHold == ControlState.Off)
            {
                float yawFilter = Mathf.Abs(commandYawInput);
                if (yawFilter > 0.3f)
                {
                    processedYaw = commandYawInput;
                }
                else
                {
                    if (commandPitchInput < 0.5f && commandRollInput < 0.4f)
                    {
                        yawRateError = commandYawRate - yawRate;
                        processedYaw = yawRateSolver.CalculateOutput(yawRateError, timeStep);
                    }
                }
            }
            else
            {
                processedYaw = 0f;
            }

            #endregion Yaw Axis
        }
        else
        {
            // - --------------------------- Proportional Control Law
            processedPitch = commandPitchInput;
            processedRoll = commandRollInput;
            processedYaw = commandYawInput;
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseGains()
    {
        float baseMach = core.machSpeed;
        float baseKnot = core.currentSpeed * MathBase.toKnots;
        float pitchGainFactor = 0f, rollGainFactor = 0f, yawGainFactor = 0f;


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------PITCH AXIS-----------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        if (basePitchGain.factorType == Gain.FactorType.Mach) { pitchGainFactor = baseMach; }
        else if (basePitchGain.factorType == Gain.FactorType.KnotSpeed) { pitchGainFactor = baseKnot; }

        if (pitchGainFactor <= cruisePitchGain.factor) { currentPitchGain = MathBase.AnalyseGain(basePitchGain, cruisePitchGain, pitchGainFactor); }
        else if (pitchGainFactor > cruisePitchGain.factor && pitchGainFactor <= performancePitchGain.factor) { currentPitchGain = MathBase.AnalyseGain(cruisePitchGain, performancePitchGain, pitchGainFactor); }
        else if (pitchGainFactor > performancePitchGain.factor) { currentPitchGain = new GainVector(performancePitchGain.Kp, performancePitchGain.Ki, performancePitchGain.Kd); }

        pitchRateSolver.Kp = currentPitchGain.Kp;
        pitchRateSolver.Ki = currentPitchGain.Ki;
        pitchRateSolver.Kd = currentPitchGain.Kd;



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------ROll AXIS------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        if (baseRollGain.factorType == Gain.FactorType.Mach) { rollGainFactor = baseMach; }
        else if (baseRollGain.factorType == Gain.FactorType.KnotSpeed) { rollGainFactor = baseKnot; }

        if (rollGainFactor <= cruiseRollGain.factor) { currentRollGain = MathBase.AnalyseGain(baseRollGain, cruiseRollGain, rollGainFactor); }
        else if (rollGainFactor > cruiseRollGain.factor && rollGainFactor <= performanceRollGain.factor) { currentRollGain = MathBase.AnalyseGain(cruiseRollGain, performanceRollGain, rollGainFactor); }
        else if (rollGainFactor > performanceRollGain.factor) { currentRollGain = new GainVector(performanceRollGain.Kp, performanceRollGain.Ki, performanceRollGain.Kd); }

        rollRateSolver.Kp = currentRollGain.Kp;
        rollRateSolver.Ki = currentRollGain.Ki;
        rollRateSolver.Kd = currentRollGain.Kd;


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------YAW AXIS------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        if (baseYawGain.factorType == Gain.FactorType.Mach) { yawGainFactor = baseMach; }
        else if (baseYawGain.factorType == Gain.FactorType.KnotSpeed) { yawGainFactor = baseKnot; }

        if (yawGainFactor <= cruiseYawGain.factor) { currentYawGain = MathBase.AnalyseGain(baseYawGain, cruiseYawGain, yawGainFactor); }
        else if (yawGainFactor > cruiseYawGain.factor && yawGainFactor <= performanceYawGain.factor) { currentYawGain = MathBase.AnalyseGain(cruiseYawGain, performanceYawGain, yawGainFactor); }
        else if (yawGainFactor > performanceYawGain.factor) { currentYawGain = new GainVector(performanceYawGain.Kp, performanceYawGain.Ki, performanceYawGain.Kd); }

        yawRateSolver.Kp = currentYawGain.Kp;
        yawRateSolver.Ki = currentYawGain.Ki;
        yawRateSolver.Kd = currentYawGain.Kd;
    }





#if UNITY_EDITOR
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void OnDrawGizmos()
    {
        // -----------------------
        PlotInputCurve();

        // -----------------------
        if(brain != null && brain.tracker != null && operationMode == AugmentationType.Autonomous) { brain.tracker.DrawTrackingBeacon(); brain.PlotSteerCurve(); }

        // ----------------------
        if (gainBoard == null) { SetupGainBoard(); } if (gainBoard != null && gainBoard.fcs == null) { gainBoard.fcs = gameObject.GetComponent<SilantroFlightComputer>(); }
    }
#endif








#if UNITY_EDITOR

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SaveGain()
    {
        if (Directory.Exists(saveLocation))
        {
            gainSet = ScriptableObject.CreateInstance<SilantroGain>(); gainSet.InitializeGains();

            // ------------------------------------------------------------------------------- Base
            gainSet.throttleGain.Kp = throttleSolver.Kp; gainSet.throttleGain.Ki = throttleSolver.Ki; gainSet.throttleGain.Kd = throttleSolver.Kd; gainSet.throttleGain.minimum = throttleSolver.minimum; gainSet.throttleGain.maximum = throttleSolver.maximum;
            gainSet.rollRateGain.Kp = rollRateSolver.Kp; gainSet.rollRateGain.Ki = rollRateSolver.Ki; gainSet.rollRateGain.Kd = rollRateSolver.Kd; gainSet.rollRateGain.minimum = rollRateSolver.minimum; gainSet.rollRateGain.maximum = rollRateSolver.maximum;
            gainSet.pitchRateGain.Kp = pitchRateSolver.Kp; gainSet.pitchRateGain.Ki = pitchRateSolver.Ki; gainSet.pitchRateGain.Kd = pitchRateSolver.Kd; gainSet.pitchRateGain.minimum = pitchRateSolver.minimum; gainSet.pitchRateGain.maximum = pitchRateSolver.maximum;
            gainSet.yawRateGain.Kp = yawRateSolver.Kp; gainSet.yawRateGain.Ki = yawRateSolver.Ki; gainSet.yawRateGain.Kd = yawRateSolver.Kd; gainSet.yawRateGain.minimum = yawRateSolver.minimum; gainSet.yawRateGain.maximum = yawRateSolver.maximum;

            gainSet.rollAngleGain.Kp = rollAngleSolver.Kp; gainSet.rollAngleGain.Ki = rollAngleSolver.Ki; gainSet.rollAngleGain.Kd = rollAngleSolver.Kd; gainSet.rollAngleGain.minimum = rollAngleSolver.minimum; gainSet.rollAngleGain.maximum = rollAngleSolver.maximum;
            gainSet.pitchAngleGain.Kp = pitchAngleSolver.Kp; gainSet.pitchAngleGain.Ki = pitchAngleSolver.Ki; gainSet.pitchAngleGain.Kd = pitchAngleSolver.Kd; gainSet.pitchAngleGain.minimum = pitchAngleSolver.minimum; gainSet.pitchAngleGain.maximum = pitchAngleSolver.maximum;
            gainSet.yawAngleGain.Kp = yawAngleSolver.Kp; gainSet.yawAngleGain.Ki = yawAngleSolver.Ki; gainSet.yawAngleGain.Kd = yawAngleSolver.Kd; gainSet.yawAngleGain.minimum = yawAngleSolver.minimum; gainSet.yawAngleGain.maximum = yawAngleSolver.maximum;

            gainSet.altitudeGain.Kp = altitudeSolver.Kp; gainSet.altitudeGain.Ki = altitudeSolver.Ki; gainSet.altitudeGain.Kd = altitudeSolver.Kd; gainSet.altitudeGain.minimum = altitudeSolver.minimum; gainSet.altitudeGain.maximum = altitudeSolver.maximum;
            gainSet.climbGain.Kp = climbSolver.Kp; gainSet.climbGain.Ki = climbSolver.Ki; gainSet.climbGain.Kd = climbSolver.Kd; gainSet.climbGain.minimum = climbSolver.minimum; gainSet.climbGain.maximum = climbSolver.maximum;
            gainSet.turnGain.Kp = turnSolver.Kp; gainSet.turnGain.Ki = turnSolver.Ki; gainSet.turnGain.Kd = turnSolver.Kd; gainSet.turnGain.minimum = turnSolver.minimum; gainSet.turnGain.maximum = turnSolver.maximum;


            // ------------------------------------------------------------------------------- Schedule



            // ------------------------------------------------------------------------------- Save
            AssetDatabase.CreateAsset(gainSet, saveLocation + aircraftIdentifier + ".asset");
            AssetDatabase.SaveAssets();

            Debug.Log(aircraftIdentifier + " gain sheet saved successfully");
        }
        else { Debug.Log("Save location not available"); }
    }




    public void LoadGain()
    {
        if (gainSetInput != null)
        {
            // ------------------------------------------------------------------------------- Base
            throttleSolver.Kp = gainSetInput.throttleGain.Kp; throttleSolver.Ki = gainSetInput.throttleGain.Ki; throttleSolver.Kd = gainSetInput.throttleGain.Kd; throttleSolver.minimum = gainSetInput.throttleGain.minimum; throttleSolver.maximum = gainSetInput.throttleGain.maximum;
            rollRateSolver.Kp = gainSetInput.rollRateGain.Kp; rollRateSolver.Ki = gainSetInput.rollRateGain.Ki; rollRateSolver.Kd = gainSetInput.rollRateGain.Kd; rollRateSolver.minimum = gainSetInput.rollRateGain.minimum; rollRateSolver.maximum = gainSetInput.rollRateGain.maximum;
            pitchRateSolver.Kp = gainSetInput.pitchRateGain.Kp; pitchRateSolver.Ki = gainSetInput.pitchRateGain.Ki; pitchRateSolver.Kd = gainSetInput.pitchRateGain.Kd; pitchRateSolver.minimum = gainSetInput.pitchRateGain.minimum; pitchRateSolver.maximum = gainSetInput.pitchRateGain.maximum;
            yawRateSolver.Kp = gainSetInput.yawRateGain.Kp; yawRateSolver.Ki = gainSetInput.yawRateGain.Ki; yawRateSolver.Kd = gainSetInput.yawRateGain.Kd; yawRateSolver.minimum = gainSetInput.yawRateGain.minimum; yawRateSolver.maximum = gainSetInput.yawRateGain.maximum;

            rollAngleSolver.Kp = gainSetInput.rollAngleGain.Kp; rollAngleSolver.Ki = gainSetInput.rollAngleGain.Ki; rollAngleSolver.Kd = gainSetInput.rollAngleGain.Kd; rollAngleSolver.minimum = gainSetInput.throttleGain.minimum; rollAngleSolver.maximum = gainSetInput.throttleGain.maximum;
            pitchAngleSolver.Kp = gainSetInput.pitchAngleGain.Kp; pitchAngleSolver.Ki = gainSetInput.pitchAngleGain.Ki; pitchAngleSolver.Kd = gainSetInput.pitchAngleGain.Kd; pitchAngleSolver.minimum = gainSetInput.throttleGain.minimum; pitchAngleSolver.maximum = gainSetInput.throttleGain.maximum;
            yawAngleSolver.Kp = gainSetInput.yawAngleGain.Kp; yawAngleSolver.Ki = gainSetInput.yawAngleGain.Ki; yawAngleSolver.Kd = gainSetInput.yawAngleGain.Kd; yawAngleSolver.minimum = gainSetInput.throttleGain.minimum; yawAngleSolver.maximum = gainSetInput.throttleGain.maximum;

            altitudeSolver.Kp = gainSetInput.altitudeGain.Kp; altitudeSolver.Ki = gainSetInput.altitudeGain.Ki; altitudeSolver.Kd = gainSetInput.altitudeGain.Kd; altitudeSolver.minimum = gainSetInput.altitudeGain.minimum; altitudeSolver.maximum = gainSetInput.altitudeGain.maximum;
            climbSolver.Kp = gainSetInput.climbGain.Kp; climbSolver.Ki = gainSetInput.climbGain.Ki; climbSolver.Kd = gainSetInput.climbGain.Kd; climbSolver.minimum = gainSetInput.climbGain.minimum; climbSolver.maximum = gainSetInput.climbGain.maximum;
            turnSolver.Kp = gainSetInput.turnGain.Kp; turnSolver.Ki = gainSetInput.turnGain.Ki; turnSolver.Kd = gainSetInput.turnGain.Kd; turnSolver.minimum = gainSetInput.turnGain.minimum; turnSolver.maximum = gainSetInput.turnGain.maximum;


            // ------------------------------------------------------------------------------- Schedule
            Debug.Log(aircraftIdentifier + " gain sheet loaded successfully");

            gainSetInput = null;
        }
    }

#endif




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SetupGainBoard()
    {
        SilantroComputerConfig testBoard = GetComponentInChildren<SilantroComputerConfig>();
        if (testBoard == null)
        {
            GameObject board = new GameObject("Gain Board");
            board.transform.parent = transform; board.transform.localPosition = Vector3.zero;
            gainBoard = board.AddComponent<SilantroComputerConfig>();
        }
        else
        {
            gainBoard = testBoard;
        }
    }
}
