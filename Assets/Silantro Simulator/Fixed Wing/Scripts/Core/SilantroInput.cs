using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityStandardAssets.CrossPlatformInput;
#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Controls;
#endif



/// <summary>
/// Handles the input variable and state collection plus processing	 
/// </summary>
/// /// <remarks>
/// This component will collect the inputs from various sources e.g Keyboard, Joystick, VR or custom
/// and process them into control variables for the flight computer. It also contains all the control and
/// command functions for the aircraft operation
/// </remarks>




[Serializable]
public class SilantroInput
{

    // ----------------------------------- States
    public enum LightState { On, Off }
    /// <summary>
    /// Current light state either On or Off
    /// </summary>
    public LightState lightState = LightState.Off;
    public enum FlapState { Up, Middle, Down }
    /// <summary>
    /// Current flap setting or position
    /// </summary>
    public FlapState flapState = FlapState.Up;
    public enum BoostState { Active, Off }
    /// <summary>
    /// Current boost/afterburner state either On or Off
    /// </summary>
    public BoostState boostState = BoostState.Off;


    // ---------------------------------- Connections
    public SilantroController controller;


    // ---------------------------------- Bools
    /// <summary>
    /// Is the brake lever button held down?
    /// </summary>
    public bool brakeLeverHeld;
    public bool flapDownSwitch, flapUpSwitch = true;
    /// <summary>
    /// Is the trigger button held down?
    /// </summary>
    public bool triggerHeld;
    public bool afterburnerSwitch;
    /// <summary>
    /// Is the boost or afterburnner currently running?
    /// </summary>
    public bool boostRunning;


    // --------------------------------- Variables
    public Vector2 scrollInput;
    public float rollInput, yawInput, pitchInput, rawThrottleInput, stabilizerTrimInput, tillerInput;
    public float rawPitchInput, rawRollInput, rawYawInput;
    public bool inputConfigured = true;




    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void AwakeInput()
    {


#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE

        // ------------------------- Create Instance
        controlSource = new SilantroControl();
        controlSource.Enable();

        // ------------------------- Map
        controlSource.General.StartEngineGlobal.performed += ctx => TurnOnEngines();
        controlSource.General.StopEngineGlobal.performed += ctx => TurnOffEngines();
        controlSource.General.ParkingBrake.performed += ctx => ToggleBrakeState();
        controlSource.General.CameraSwitch.performed += ctx => ToggleCameraState();
        controlSource.General.LightSwitch.performed += ctx => ToggleLightState();
        controlSource.General.GearSwitch.performed += ctx => ToggleGearState();
        controlSource.General.SpeedBrakeSwitch.performed += ctx => ToggleSpeedBrakeState();

        controlSource.General.TriggerPress.performed += ctx => FireWeapons();
        controlSource.General.WeaponSwitch.performed += ctx => SwitchWeapons();
        controlSource.General.TargetUp.performed += ctx => CycleTargetUpwards();
        controlSource.General.TargetDown.performed += ctx => CycleTargetDownwards();
        controlSource.General.TargetLock.performed += ctx => LockTarget();
        controlSource.General.TargetRelease.performed += ctx => ReleaseTarget();


        controlSource.General.BrakeHoldLever.performed += ctx =>
        {
            var control = ctx.control;
            var button = control as ButtonControl;
            brakeLeverHeld = button.isPressed;
        };
        controlSource.General.TriggerHold.performed += ctx =>
        {
            var control = ctx.control;
            var button = control as ButtonControl;
            triggerHeld = button.isPressed;
        };

        controlSource.General.CockpitLook.performed += ctx =>
        {
            var control = ctx.control;
            var button = control as Vector2Control;
            scrollInput = button.ReadValue();
        };
        controlSource.General.FlapDownSwitch.performed += ctx =>
        {
            var control = ctx.control;
            var button = control as ButtonControl;
            flapDownSwitch = button.isPressed;
        };
        controlSource.General.FlapUpSwitch.performed += ctx =>
        {
            var control = ctx.control;
            var button = control as ButtonControl;
            flapUpSwitch = button.isPressed;
        };
        controlSource.General.AfterburnerSwitch.performed += ctx =>
        {
            var control = ctx.control;
            var button = control as ButtonControl;
            afterburnerSwitch = button.isPressed;
        };

        controlSource.General.RollLever.performed += ctx => { rawRollInput = ctx.ReadValue<float>(); };
        controlSource.General.PitchLever.performed += ctx => { rawPitchInput = ctx.ReadValue<float>(); };
        controlSource.General.YawLever.performed += ctx => { rawYawInput = ctx.ReadValue<float>(); };
        controlSource.General.PitchTrim.performed += ctx => { stabilizerTrimInput = ctx.ReadValue<float>(); };
        controlSource.General.ThrottleLever.performed += ctx => { if (ctx.ReadValue<float>() != 0) { throttleInput = (1 - ctx.ReadValue<float>()) / 2; } };
#endif
    }






    /// <summary>
    /// Collect the control inputs from the set source
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void CollectInputs()
    {
        if(controller.inputType == SilantroController.InputType.Default)
        {

#if ENABLE_LEGACY_INPUT_MANAGER

            if (Input.GetButtonDown("Start Engine")) { TurnOnEngines(); }
            if (Input.GetButtonDown("Stop Engine")) { TurnOffEngines(); }
            if (Input.GetButtonDown("Start Engine BandL")) { TurnOnLeftEngines(); }
            if (Input.GetButtonDown("Start Engine BandR")) { TurnOnRightEngines(); }
            if (Input.GetButtonDown("Stop Engine BandL")) { TurnOffLeftEngines(); }
            if (Input.GetButtonDown("Stop Engine BandR")) { TurnOffRightEngines(); }

            if (controller.flightComputer != null && controller.flightComputer.operationMode != SilantroFlightComputer.AugmentationType.Autonomous)
            {
                // ----------------------------------------- Base
                rawPitchInput = -Input.GetAxis("Pitch");
                rawRollInput = Input.GetAxis("Roll");
                rawYawInput = Input.GetAxis("Rudder");
                float baseInput = (Input.GetAxis("Throttle"));
                rawThrottleInput = (baseInput + 1) / 2;

                // ----------------------------------------- Toggles
                if (Input.GetButtonDown("Parking Brake")) { ToggleBrakeState(); }
                if (Input.GetKeyDown(KeyCode.C)) { ToggleCameraState(); }
                if (Input.GetButtonDown("Actuate Gear")) { ToggleGearState(); }
                if (Input.GetButtonDown("Speed Brake")) { ToggleSpeedBrakeState(); }
                if (Input.GetButtonDown("LightSwitch")) { ToggleLightState(); }
                if (Input.GetButtonDown("Afterburner")) { if (!boostRunning) { EngageBoost(); } else if (boostRunning) { DisEngageBoost(); } }
                if (Input.GetButtonDown("Fire")) { FireWeapons(); }
                if (Input.GetButtonDown("Target Up")) { CycleTargetUpwards(); }
                if (Input.GetButtonDown("Target Down")) { CycleTargetDownwards(); }
                if (Input.GetButtonDown("Target Lock")) { LockTarget(); }
                if (Input.GetKeyDown(KeyCode.Backspace)) { ReleaseTarget(); }
                if (Input.GetButtonDown("Weapon Select")) { SwitchWeapons(); }
                if (Input.GetButtonDown("Extend Flap")) { LowerFlaps(); }
                if (Input.GetButtonDown("Retract Flap")) { RaiseFlaps(); }
                if (Input.GetKeyDown(KeyCode.J)) { ToggleWingState(); }
                if (Input.GetButtonDown("Actuate Slat")) { ToggleSlatState(); }
                if (Input.GetButton("DropSwitch")) { InitiateBombDrop(); }
                if (Input.GetButtonDown("Spoiler")) { ToggleSpoilerState(); }
                if (Input.GetKeyDown(KeyCode.F)) { ExitAircraftSwitch(); }
                if (Input.GetButtonDown("Transition To STOL")) { TransitionToVTOL(); }
                if (Input.GetButtonDown("Transition To Normal")) { TransitionToNormal(); }
                if (Input.GetKeyDown(KeyCode.R)) { controller.ResetScene(); }

                // ----------------------------------------- Keys
                if (Input.GetButton("Brake Lever")) { if (!brakeLeverHeld) { brakeLeverHeld = true; } } else { if (brakeLeverHeld) { brakeLeverHeld = false; } }
                if (Input.GetButton("Fire")) { if (!triggerHeld) { triggerHeld = true; } } else { if (triggerHeld) { triggerHeld = false; } }
            }
#endif
        }

        else if (controller.inputType == SilantroController.InputType.Mobile)
        {
            // ----------------------------------------- Base
            rawRollInput = CrossPlatformInputManager.GetAxis("Horizontal");
            rawPitchInput = CrossPlatformInputManager.GetAxis("Vertical");
        }


        else if(controller.inputType == SilantroController.InputType.VR)
        {
            rawPitchInput = controller.controlStick.rollOutput;
            rawPitchInput = controller.controlStick.pitchOutput;
            rawThrottleInput = controller.throttleLever.leverOutput;
        }

        else
        {

        }
    }






    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateInput()
    {

        #if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
        // -------------------------------------- Re-initialize Inputs
        if (controlSource == null)
        {
            controlSource = new SilantroControl();
            controlSource.Enable();
        }
        #endif
        
       

        if (Application.isFocused)
        {

            // ---------------------------------------- Collect Input
            CollectInputs(); 
          


            // ---------------------------------------- Base Input
            pitchInput = controller.flightComputer.pitchInputCurve.Evaluate(rawPitchInput);
            rollInput = controller.flightComputer.rollInputCurve.Evaluate(rawRollInput);
            yawInput = controller.flightComputer.yawInputCurve.Evaluate(rawYawInput);

            // ---------------------------------------- Struct
            if (controller.gearHelper != null && controller.flightComputer.operationMode != SilantroFlightComputer.AugmentationType.Autonomous)
            {
                if (brakeLeverHeld) { controller.gearHelper.brakeInput = Mathf.Lerp(controller.gearHelper.brakeInput, 1f, 0.1f); }
                else { controller.gearHelper.brakeInput = Mathf.Lerp(controller.gearHelper.brakeInput, 0f, 0.1f); }
            }


            // --------------------------------------- Camera
            if (controller.view != null) { controller.view.scrollValue = scrollInput.y / 120f; }


            // --------------------------------------- Weapons
            if (controller.Armaments != null)
            {

                if (controller.guns != null && controller.guns.Length > 0 && controller.Armaments.currentWeapon == "Gun")
                {
                    if (triggerHeld)
                    {
                        controller.Armaments.FireGuns(); 

                        foreach (SilantroGun gun in controller.Armaments.attachedGuns)
                        { if (gun.currentAmmo > 0 && gun.canFire) { gun.running = true; } }
                    }
                    else
                    {
                        foreach (SilantroGun gun in controller.Armaments.attachedGuns)
                        { if (gun.running) { gun.running = false; } }
                    }
                }
            }



            // --------------------------------------- Control Engines
            AnalyseEngines();


            // --------------------------------------- Flaps
            if (flapUpSwitch && flapState != FlapState.Up) { SetFlapUp(); }
            else if (flapDownSwitch && flapState != FlapState.Down) { SetFlapDown(); }
            else if (!flapUpSwitch && !flapDownSwitch && flapState != FlapState.Middle) { SetFlapMiddle(); }
        }
    }







    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void AnalyseEngines()
    {
        if (controller.flightComputer.autoThrottle == SilantroFlightComputer.ControlState.Off)
        {
            if (afterburnerSwitch && boostState == BoostState.Off) { EngageBoost(); }
            if (!afterburnerSwitch && boostState == BoostState.Active) { DisEngageBoost(); }
        }
        else
        {
            if (boostState == BoostState.Active && !boostRunning) { EngageBoost(); }
            if (boostState == BoostState.Off && boostRunning) { DisEngageBoost(); }
        }
    }






    /// <summary>
    /// Send the processed input variables to the proper components
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void AnalyseInputs()
    {
        if (controller != null && controller.allOk)
        {

            // ------------------------------ Send Engine Data
            if (controller.engineType == SilantroController.EngineType.TurboJet) { foreach (SilantroTurboJet engine in controller.turboJets) { engine.core.controlInput = controller.flightComputer.processedThrottle; } }
            if (controller.engineType == SilantroController.EngineType.TurboFan) { foreach (SilantroTurboFan engine in controller.turboFans) { engine.core.controlInput = controller.flightComputer.processedThrottle; } }
            if (controller.engineType == SilantroController.EngineType.TurboProp) { foreach (SilantroTurboProp engine in controller.propEngines) { engine.core.controlInput = controller.flightComputer.processedThrottle; } }
            if (controller.engineType == SilantroController.EngineType.Piston) { foreach (SilantroPistonEngine engine in controller.pistons) { engine.core.controlInput = controller.flightComputer.processedThrottle; } }
            if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { engine.controlInput = controller.flightComputer.processedThrottle; } }


            // ---------------------------------------- Wings
            foreach (SilantroAerofoil foil in controller.wings)
            {
                foil.pitchInput = -controller.flightComputer.processedPitch;
                foil.rollInput = controller.flightComputer.processedRoll;
                foil.yawInput = controller.flightComputer.processedYaw;
                foil.stabilizerInput = controller.flightComputer.processedStabilizerTrim;

                foil.pitchTrimInput = -controller.flightComputer.processedPitchTrim;
                foil.rollTrimInput = controller.flightComputer.processedRollTrim;
                foil.yawTrimInput = controller.flightComputer.processedYawTrim;
            }

            // ------------------------------------------ Gear
            if(controller.gearHelper != null)
            {
                controller.gearHelper.tillerInput = controller.flightComputer.processedTillerInput;
                controller.gearHelper.rudderInput = controller.flightComputer.processedSteeringInput;
            }
        }
    }































    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------CONTROL FUNCTIONS--------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// Activate boost or afterburner
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void EngageBoost()
    {
        if (controller.isControllable)
        {
            if (controller.engineType == SilantroController.EngineType.TurboJet)
            {
                foreach (SilantroTurboJet engine in controller.turboJets) { if (!engine.core.afterburnerOperative && engine.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning && engine.core.active) { engine.core.afterburnerOperative = true; boostRunning = true; } }
            }
            if (controller.engineType == SilantroController.EngineType.TurboFan)
            {
                foreach (SilantroTurboFan engine in controller.turboFans) { if (!engine.core.afterburnerOperative && engine.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning && engine.core.active) { engine.core.afterburnerOperative = true; boostRunning = true; } }
            }
        }
    }

    /// <summary>
    /// deactivate boost or afterburner
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void DisEngageBoost()
    {
        if (controller.engineType == SilantroController.EngineType.TurboJet) { foreach (SilantroTurboJet engine in controller.turboJets) { engine.core.afterburnerOperative = false; boostRunning = false; } }
        if (controller.engineType == SilantroController.EngineType.TurboFan) { foreach (SilantroTurboFan engine in controller.turboFans) { engine.core.afterburnerOperative = false; boostRunning = false; } }
    }



    /// <summary>
    /// Fire the rockets or missiles connected to the aircraft weapons system
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void FireWeapons()
    {
        if(controller.Armaments != null)
        {
            if(controller.Armaments.currentWeapon == "Missile") { controller.Armaments.FireMissile(); }
            else if(controller.Armaments.currentWeapon == "Rocket") { controller.Armaments.FireRocket(); }
        }
    }




    /// <summary>
    /// Start up the aircraft engines
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOnEngines()
    {
        if (controller.isControllable)
        {
            if (controller.engineStartMode == SilantroController.EngineStartMode.Sequential && controller.baseCore != null) { controller.StartCoroutine(SequentialEngineStart()); }
            else
            {
                if (controller.engineType == SilantroController.EngineType.TurboJet) { foreach (SilantroTurboJet engine in controller.turboJets) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                if (controller.engineType == SilantroController.EngineType.TurboFan) { foreach (SilantroTurboFan engine in controller.turboFans) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                if (controller.engineType == SilantroController.EngineType.TurboProp) { foreach (SilantroTurboProp engine in controller.propEngines) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                if (controller.engineType == SilantroController.EngineType.Piston) { foreach (SilantroPistonEngine engine in controller.pistons) { if (!engine.core.active) { engine.core.StartEngine(); } } }
                if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Running) { engine.StartEngine(); } } }
            }
        }
    }



    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator SequentialEngineStart()
    {
        TurnOnLeftEngines();
        //yield return new WaitForSeconds(3f);
        yield return new WaitUntil(() => controller.baseCore.CurrentEngineState == SilantroEngineCore.EngineState.Active);
        TurnOnRightEngines();
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator SequentialEngineStop()
    {
        TurnOffLeftEngines();
        //yield return new WaitForSeconds(3f);
        yield return new WaitUntil(() => controller.baseCore.CurrentEngineState == SilantroEngineCore.EngineState.Off);
        TurnOffRightEngines();
    }


    /// <summary>
    /// Start up the aircraft engines on the left
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOnLeftEngines()
    {
        if (controller.isControllable)
        {
            if (controller.engineType == SilantroController.EngineType.TurboJet) {controller.baseCore = controller.turboJets[0].core; foreach (SilantroTurboJet engine in controller.turboJets) 
                { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.TurboFan) {controller.baseCore = controller.turboFans[0].core; foreach (SilantroTurboFan engine in controller.turboFans) 
               { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.TurboProp) {controller.baseCore = controller.propEngines[0].core; foreach (SilantroTurboProp engine in controller.propEngines) 
                 { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.Piston) {controller.baseCore = controller.pistons[0].core; foreach (SilantroPistonEngine engine in controller.pistons) 
                { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Running) { engine.StartEngine(); } } }
        }
    }


     /// <summary>
    /// Start up the aircraft engines on the left
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOnRightEngines()
    {
        if (controller.isControllable)
        {
            if (controller.engineType == SilantroController.EngineType.TurboJet) { foreach (SilantroTurboJet engine in controller.turboJets) 
                { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.TurboFan) { foreach (SilantroTurboFan engine in controller.turboFans) 
               { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.TurboProp) { foreach (SilantroTurboProp engine in controller.propEngines) 
                 { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.Piston) { foreach (SilantroPistonEngine engine in controller.pistons) 
                { if (!engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.StartEngine(); } } } }
            if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Running) { engine.StartEngine(); } } }
        }
    }

    /// <summary>
    /// Shutdown the aircraft engines
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOffEngines()
    {
        if (controller.isControllable)
        {
            if (controller.isControllable)
            {
                if (controller.engineStartMode == SilantroController.EngineStartMode.Sequential && controller.baseCore != null) { controller.StartCoroutine(SequentialEngineStop()); }
                else
                {
                    if (controller.engineType == SilantroController.EngineType.TurboJet) { foreach (SilantroTurboJet engine in controller.turboJets) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                    if (controller.engineType == SilantroController.EngineType.TurboFan) { foreach (SilantroTurboFan engine in controller.turboFans) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                    if (controller.engineType == SilantroController.EngineType.TurboProp) { foreach (SilantroTurboProp engine in controller.propEngines) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                    if (controller.engineType == SilantroController.EngineType.Piston) { foreach (SilantroPistonEngine engine in controller.pistons) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
                    if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Off) { engine.ShutDownEngine(); } } }
                }
            }
        }
    }



    /// <summary>
    /// Shutdown the aircraft engines on the left
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOffLeftEngines()
    {
        if (controller.isControllable)
        {
            if (controller.engineType == SilantroController.EngineType.TurboJet)
            {
                foreach (SilantroTurboJet engine in controller.turboJets)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.TurboFan)
            {
                foreach (SilantroTurboFan engine in controller.turboFans)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.TurboProp)
            {
                foreach (SilantroTurboProp engine in controller.propEngines)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.Piston)
            {
                foreach (SilantroPistonEngine engine in controller.pistons)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N1 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N3 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N5) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Off) { engine.ShutDownEngine(); } } }
        }
    }





    /// <summary>
    /// Shutdown the aircraft engines on the right
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOffRightEngines()
    {
        if (controller.isControllable)
        {
            if (controller.engineType == SilantroController.EngineType.TurboJet)
            {
                foreach (SilantroTurboJet engine in controller.turboJets)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.TurboFan)
            {
                foreach (SilantroTurboFan engine in controller.turboFans)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.TurboProp)
            {
                foreach (SilantroTurboProp engine in controller.propEngines)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.Piston)
            {
                foreach (SilantroPistonEngine engine in controller.pistons)
                { if (engine.core.active) { if (engine.core.engineNumber == SilantroEngineCore.EngineNumber.N2 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N4 || engine.core.engineNumber == SilantroEngineCore.EngineNumber.N6) { engine.core.ShutDownEngine(); } } }
            }
            if (controller.engineType == SilantroController.EngineType.Electric) { foreach (SilantroElectricMotor engine in controller.motors) { if (engine.engineState != SilantroElectricMotor.EngineState.Off) { engine.ShutDownEngine(); } } }
        }
    }



    /// <summary>
    /// Engage or disengage the gear actuator
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleGearState()
    {
        if (controller.isControllable)
        {
            if (controller != null && controller.gearActuator != null)
            {
                if (controller.gearActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.gearActuator.EngageActuator(); }
                else { controller.gearActuator.DisengageActuator(); }
            }
        }
    }



    /// <summary>
    /// Engage or disengage the wing actuator
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleWingState()
    {
        if (controller.isControllable)
        {
            if (controller != null && controller.wingActuator != null)
            {
                if (controller.flapAngle < 5)
                {
                    if (controller.wingActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.wingActuator.EngageActuator(); }
                    else { controller.wingActuator.DisengageActuator(); }
                }
                else
                {
                    Debug.Log("Lower Flap angle to swing wings");
                }
            }
        }
    }



    /// <summary>
    /// Engage or disengage the speedbrake actuator
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void ToggleSpeedBrakeState()
    {
        if (controller.isControllable)
        {
            if (controller != null && controller.speedBrakeActuator != null)
            {
                if (controller.speedBrakeActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.speedBrakeActuator.EngageActuator(); }
                else { controller.speedBrakeActuator.DisengageActuator(); }
            }
            if(controller.flightComputer.airbrakeType != SilantroFlightComputer.AirbrakeType.ActuatorOnly) { controller.flightComputer.airbrakeActive = !controller.flightComputer.airbrakeActive; }
        }
    }


    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Engage or disengage the aircraft parking brakes
    /// </summary>
    public void ToggleBrakeState() { if (controller.isControllable) { if (controller != null && controller.gearHelper != null) { controller.gearHelper.ToggleBrakes(); } } }
    /// <summary>
    /// Switch the connected aircraft lights on or off
    /// </summary>
    public void ToggleLightState() { if (controller != null && controller.isControllable) { foreach (SilantroLight light in controller.lightBulbs) { if (light.state == SilantroLight.CurrentState.On) { light.SwitchOff(); if (controller.lightState == SilantroController.LightState.On) { controller.lightState = SilantroController.LightState.Off; } } else { light.SwitchOn(); if (controller.lightState == SilantroController.LightState.Off) { controller.lightState = SilantroController.LightState.On; } } } } }
    /// <summary>
    /// Switch off the connected aircraft lights
    /// </summary>
    public void TurnOffLights() { if (controller != null && controller.isControllable) { foreach (SilantroLight light in controller.lightBulbs) { if (light.state == SilantroLight.CurrentState.On) { light.SwitchOff();  if (controller.lightState == SilantroController.LightState.On) { controller.lightState = SilantroController.LightState.Off; } } } } }
    /// <summary>
    /// Switch on the connected aircraft lights
    /// </summary>
    public void TurnOnLights() { if (controller != null && controller.isControllable) { foreach (SilantroLight light in controller.lightBulbs) { if (light.state == SilantroLight.CurrentState.Off) { light.SwitchOn(); if (controller.lightState == SilantroController.LightState.Off) { controller.lightState = SilantroController.LightState.On; } } } } }
    /// <summary>
    /// Cycle through the available camera modes
    /// </summary>
    public void ToggleCameraState() { if(controller != null && controller.isControllable && controller.view != null){ controller.view.ToggleCamera(); } }
    /// <summary>
    /// Set the flap surfaces to fully deflected mode
    /// </summary>
    public void SetFlapUp() { foreach (SilantroAerofoil foil in controller.wings) { foil.SetFlaps(0, 1); } flapState = FlapState.Up; }
    /// <summary>
    /// Set the flap surfaces to partially deflected mode
    /// </summary>
    public void SetFlapMiddle() { foreach (SilantroAerofoil foil in controller.wings) { if (flapState == FlapState.Up) { foil.SetFlaps(1, 2); } else if (flapState == FlapState.Down) { foil.SetFlaps(1, 1); } } flapState = FlapState.Middle; }
    /// <summary>
    /// Set the flap surfaces to fully retracted mode
    /// </summary>
    public void SetFlapDown() { foreach (SilantroAerofoil foil in controller.wings) { foil.SetFlaps(2, 2); } flapState = FlapState.Down; }
    /// <summary>
    /// Disengage the flap actuator and retract them
    /// </summary>
    public void RaiseFlaps() { foreach (SilantroAerofoil foil in controller.wings) { foil.RaiseFlap(); } if (controller.flightComputer != null) { controller.flightComputer.flapControl = SilantroFlightComputer.ControlMode.Manual; } }
    /// <summary>
    /// Deflect the flap surface(s) downwards
    /// </summary>
    public void LowerFlaps() { foreach (SilantroAerofoil foil in controller.wings) { foil.LowerFlap(); } if (controller.flightComputer != null) { controller.flightComputer.flapControl = SilantroFlightComputer.ControlMode.Manual; } }
    /// <summary>
    /// Deflect or retract the slat surface(s)
    /// </summary>
    public void ToggleSlatState() { foreach (SilantroAerofoil foil in controller.wings) { foil.ActuateSlat(); } }
    /// <summary>
    /// Deflect or retract the spoiler surface(s)
    /// </summary>
    public void ToggleSpoilerState() { foreach (SilantroAerofoil foil in controller.wings) { foil.ActuateSpoiler(); } }
    /// <summary>
    /// Start the bomb drop sequence
    /// </summary>
    public void InitiateBombDrop() { if(controller.Armaments != null) { controller.Armaments.DropBomb(); } }
    /// <summary>
    /// Transition the aircraft from the normal state to the VTOL flight mode
    /// </summary>
    public void TransitionToVTOL() { if(controller.aircraftType == SilantroController.AircraftType.VTOL && controller.core.currentSpeed < 130f && !controller.hoverSystem.transitionToVTOL) { controller.hoverSystem.transitionToVTOL = true; } }
    /// <summary>
    /// Transition the aircraft from the VTOL state to the normal flight mode
    /// </summary>
    public void TransitionToNormal() { if (controller.aircraftType == SilantroController.AircraftType.VTOL && !controller.hoverSystem.transitionToNormal) { controller.hoverSystem.transitionToNormal = true; } }
    /// <summary>
    /// Cycle through the weapons connected to the aircraft
    /// </summary>
    public void SwitchWeapons() { if (controller.Armaments != null) { controller.Armaments.ChangeWeapon(); } }
    /// <summary>
    /// Scroll up through the discovered/tracked object list
    /// </summary>
    public void CycleTargetUpwards() { if (controller.radarCore != null) { controller.radarCore.SelectedUpperTarget(); } }
    /// <summary>
    /// Scroll down through the discovered/tracked object list
    /// </summary>
    public void CycleTargetDownwards() { if (controller.radarCore != null) { controller.radarCore.SelectLowerTarget(); } }
    /// <summary>
    /// Lock the selected target object
    /// </summary>
    public void LockTarget() { if (controller.radarCore != null) { controller.radarCore.LockSelectedTarget(); } }
    /// <summary>
    /// Unlock the selected/locked target object
    /// </summary>
    public void ReleaseTarget() { if (controller.radarCore != null) { controller.radarCore.ReleaseLockedTarget(); } }


    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ExitAircraftSwitch()
    {
        if (controller.controlType == SilantroController.ControlType.Internal && !controller.temp)
        {
            if (controller.isGrounded && controller.core.currentSpeed < 10) {  controller.ExitAircraft(); controller.opened = false; controller.temp = false; }
        }
    }


#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
         private void OnDisable() { controlSource.Disable(); }
#endif
}
