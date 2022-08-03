using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


/// <summary>
/// Handles the collection and organization of all the connected aircraft components
/// </summary>
/// <remarks>
/// This component will collect the components connected to the aircraft root and set them up with the variables and components they
/// need to function properly. It also runs the core control functions in the dependent child components
/// </remarks>

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SilantroController : MonoBehaviour
{

    // -------------------------------- Selectibles
    public enum SoundState { Exterior, Interior }
    /// <summary>
    /// The current camera sound state
    /// </summary>
    [Tooltip("A float using the Range attribute")]
    public SoundState currentSoundState = SoundState.Exterior;
    public enum AircraftType { CTOL, VTOL }
    /// <summary>
    /// The form of the aircraft. Whether it is a conventional or vertical takeoff aircraft
    /// </summary>
    public AircraftType aircraftType = AircraftType.CTOL;
    public enum EngineType { Unpowered, Electric, Piston, TurboJet, TurboFan, TurboProp }
    /// <summary>
    /// The type of engine powering the aircraft. Can be set as "Unpowered" for gliders.
    /// </summary>
    public EngineType engineType = EngineType.Piston;
    public enum ControlType { External, Internal }
    [Tooltip("Switch to Internal for 3rd or First person Entery-Exit setup")] public ControlType controlType = ControlType.External;
    public enum StartMode { Cold, Hot }
    /// <summary>
    /// The aircraft start mode. Cold and dark or Hot and running
    /// </summary>
    public StartMode startMode = StartMode.Cold;
    /// <summary>
    /// The source of input variable that will be supplied to the aircraft.
    /// </summary>
    public enum InputType { Default, Custom, VR, Mobile }
    public InputType inputType = InputType.Default;
    public enum GearState { Stowed, Open }
    public GearState gearState = GearState.Open;
    public enum LightState { On, Off }
    public LightState lightState = LightState.Off;
    public enum FlapState { Up, Middle, Down }
    public FlapState flapState = FlapState.Up;
    public enum JetType { JetB, JetA1, JP6, JP8 }
    public JetType jetType = JetType.JetB;
    public enum GasType { AVGas100, AVGas100LL, AVGas82UL }
    public GasType gasType = GasType.AVGas100;
    public SilantroCamera.CameraState cameraState = SilantroCamera.CameraState.Exterior;
    public enum PlayerType { ThirdPerson, FirstPerson }
    public PlayerType playerType = PlayerType.ThirdPerson;
    public enum WingType { Biplane, Monoplane }
    public WingType wingType = WingType.Monoplane;


    // -------------------------------- Components
    public Rigidbody aircraft;
    public SilantroController controller;
    public SilantroInput input;
    public SilantroFlightComputer flightComputer;
    public SilantroGearSystem gearHelper;
    public SilantroCamera view;
    public SilantroCore core;
    public ControllerFunctions helper;
    public HoverController hoverSystem;

    public SilantroLight[] lightBulbs;
    public SilantroFuelTank[] fuelTanks;
    public SilantroPayload[] payload;
    public SilantroLever[] levers;
    public SilantroDial[] dials;
    public SilantroAerofoil[] wings;
    public SilantroBody[] dragForms;
    public SilantroFuelSystem fuelSystem;
    public SilantroRadar radarCore;
    public SilantroHydraulicSystem[] hydraulics;

    // ---------- Actuators
    public SilantroActuator[] actuators;
    public SilantroActuator gearActuator;
    public SilantroActuator canopyActuator;
    public SilantroActuator speedBrakeActuator;
    public SilantroActuator wingActuator;
    public SilantroActuator liftSystem;

    public SilantroTurboJet[] turboJets;
    public SilantroTurboFan[] turboFans;
    public SilantroPropeller[] propellers;
    public SilantroPistonEngine[] pistons;
    public SilantroTurboProp[] propEngines;
    public SilantroLiftFan liftFan;
    public SilantroElectricMotor[] motors;

    // -------------- Weapons
    public SilantroArmament Armaments;
    public GameObject ArmamentsStorage;
    public SilantroGun[] guns;

    // ---------------Player
    public Transform getOutPosition;
    public GameObject player;
    public GameObject interiorPilot;
    public SilantroDisplay canvasDisplay;
    public Camera mainCamera;

    // ---------------VR Controls
    public SilantroVirtualLever controlStick;
    public SilantroVirtualLever throttleLever;

    // -------------------------------- Data
    public string aircraftName = "Default";
    public float totalWingArea = 0f;
    public int AvailableEngines = 0;
    public float totalThrustGenerated = 0f;
    public float totalConsumptionRate;
    public float wingLoading;
    public float thrustToWeightRatio;
    public bool isGrounded;
    public bool silantroActive;
    public bool opened = false;
    public bool temp = false;
    public float silantroEnginePower;
    public float baseAcceleration;
    public enum EngineCount { Single, Double, _3Engines, _4Engines, _5Engines, _6Engines }
    public EngineCount engineCount = EngineCount.Single;
    public enum EngineStartMode { Collective, Sequential }
    public EngineStartMode engineStartMode = EngineStartMode.Collective;
    public SilantroEngineCore baseCore;

    // -------------------------------- Weight
    public float emptyWeight = 1000f;
    public float currentWeight;
    public float maximumWeight = 5000f;


    // -------------------------------- Fuel
    public float combustionEnergy;
    public float fuelLevel;
    public bool lowFuel, fuelExhausted;
    public float TotalFuelCapacity;

    public float initialDrag;
    public float currentDrag;
    public float actuatorDrag;



    // -------------------------------- Hot Start
    public float startSpeed;
    public float startAltitude;
    public float upperWingSpan, lowerWingSpan, wingGap;

    public float flapAngle;
    public float slatAngle;
    public float spoilerAngle;
    public SilantroAerofoil coreFlapWing, coreSlatWing, coreSpoilerWing;


    public bool isControllable;
    public bool pilotOnboard;
    public bool allOk;
    public Vector3 basePosition;
    public Quaternion baseRotation;
    public float maximumThrust;
    public bool engineRunning;
    public List<string> inputList;


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start() { InitializeAircraft(); }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //ACTIVATE AIRCRAFT CONTROLS
    /// <summary>
    /// Sets the state of the aircraft control.
    /// </summary>
    /// <param name="state">If set to <c>true</c> aircraft is controllable.</param>
    public void SetControlState(bool state) { isControllable = state; }
    public void TurnOnEngines() { input.TurnOnEngines(); }
    public void TurnOffEngines() { input.TurnOffEngines(); }
    public void ToggleCameraState() { input.ToggleCameraState(); }
    public void ToggleGearState() { input.ToggleGearState(); }
    public void ToggleBrakeState() { input.ToggleBrakeState(); }
    public void ToggleLightState() { input.ToggleLightState(); }
    public void LowerFlaps() { input.LowerFlaps(); }
    public void RaiseFlaps() { input.RaiseFlaps(); }
    public void RestoreAircraft() { helper.RestoreFunction(aircraft, controller); }
    public void ResetScene() { UnityEngine.SceneManagement.SceneManager.LoadScene(this.gameObject.scene.name); }
    public void PositionAircraft() { helper.PositionAircraftFunction(aircraft, controller); }
    public void StartAircraft() { helper.StartAircraftFunction(aircraft, controller); } 
    public void ThirdPersonCall() { StartCoroutine(helper.EnableTPControls(controller));}
    public void CleanupGameobject(GameObject trash) { Destroy(trash); }
    public void RefreshWeapons(){ helper.RefreshWeaponsFunction(controller); }
    public void EnterAircraft() { helper.EnterAircraftFunction(controller); }
    public void ExitAircraft() { helper.ExitAircraftFunction(controller); }
    public void InitialteFlight() { flightComputer.brain.flightInitiated = true; }
    public void ClearTakeoff() { flightComputer.brain.TakeoffClearance(); }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void _checkPrerequisites()
    {
        //CHECK COMPONENTS
        if (core != null && aircraft && wings.Length > 0 && flightComputer != null)
        {
            allOk = true;
        }
        else if (aircraft == null)
        {
            Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... rigidbody not assigned");
            allOk = false; return;
        }
        else if (core == null)
        {
            Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... control module not assigned");
            allOk = false; return;
        }
        else if (wings.Length <= 0)
        {
            Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... aerofoil System not assigned");
            allOk = false; return;
        }

        if (flightComputer == null)
        {
            Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... flight computer not connected to aircraft");
            allOk = false;
            return;
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void InitializeAircraft()
    {
        aircraft = GetComponent<Rigidbody>();
        controller = GetComponent<SilantroController>();
        gearHelper = GetComponentInChildren<SilantroGearSystem>();
        view = GetComponentInChildren<SilantroCamera>();
        core = GetComponentInChildren<SilantroCore>();
        actuators = GetComponentsInChildren<SilantroActuator>();
        lightBulbs = GetComponentsInChildren<SilantroLight>();
        turboJets = GetComponentsInChildren<SilantroTurboJet>();
        turboFans = GetComponentsInChildren<SilantroTurboFan>();
        flightComputer = GetComponentInChildren<SilantroFlightComputer>();
        fuelTanks = GetComponentsInChildren<SilantroFuelTank>();
        payload = GetComponentsInChildren<SilantroPayload>();
        levers = GetComponentsInChildren<SilantroLever>();
        dials = GetComponentsInChildren<SilantroDial>();
        wings = GetComponentsInChildren<SilantroAerofoil>();
        dragForms = GetComponentsInChildren<SilantroBody>();
        propellers = GetComponentsInChildren<SilantroPropeller>();
        propEngines = GetComponentsInChildren<SilantroTurboProp>();
        pistons = GetComponentsInChildren<SilantroPistonEngine>();
        motors = GetComponentsInChildren<SilantroElectricMotor>();
        radarCore = GetComponentInChildren<SilantroRadar>();
        hydraulics = GetComponentsInChildren<SilantroHydraulicSystem>();
        liftFan = GetComponentInChildren<SilantroLiftFan>();

        guns = GetComponentsInChildren<SilantroGun>();
        Armaments = GetComponentInChildren<SilantroArmament>();
        mainCamera = Camera.main;
        basePosition = aircraft.transform.position; 
        baseRotation = aircraft.transform.rotation;
        aircraftName = transform.gameObject.name;

        //--------------------------CONFIRM NEEDED COMPONENTS
        _checkPrerequisites();



        if (allOk)
        {
            
            // ------------------------- Setup Camera
            if (view != null)
            {
                view.aircraft = aircraft;
                view.controller = controller;
                if(interiorPilot != null) { view.pilotObject = interiorPilot; interiorPilot.SetActive(false); }
                view.InitializeCamera();
            }
#if UNITY_EDITOR
            if (input != null) { CheckInputCofig(false); }
#endif


            // ------------------------- Setup Core
            if (core != null)
            {
                core.aircraft = aircraft;
                core.controller = controller;
                initialDrag = aircraft.drag;
                core.InitializeCore();
            }



            // ------------------------- Setup Gear
            if (gearHelper != null)
            {
                gearHelper.aircraft = aircraft;
                gearHelper.controller = controller;
                gearHelper.InitializeStruct();
            }


            // ------------------------- Setup Lever
            if (levers != null)
            {
                foreach (SilantroLever lever in levers)
                {
                    lever.controller = controller;
                    lever.InitializeLever();
                }
            }

            // ------------------------- Setup Dial
            if (dials != null)
            {
                foreach (SilantroDial dial in dials)
                {
                    dial.controller = controller;
                    dial.dataLog = core;
                    dial.InitializeDial();
                }
            }






            // ------------------------- Setup Fuel
            if (engineType != EngineType.Electric && engineType != EngineType.Unpowered)
            {
                foreach (SilantroFuelTank tank in fuelTanks) { tank.controller = controller; }
                fuelSystem.fuelTanks = fuelTanks;
                string tankFuelType = fuelTanks[0].fuelType.ToString();
                
                if(engineType == EngineType.Piston)
                {
                    if(tankFuelType != gasType.ToString())
                    {
                        Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... fuel selected on controller (" + gasType.ToString() + ") not a match with tank fuel (" + tankFuelType + ")");
                        allOk = false; return;
                    }
                }
                if(engineType == EngineType.TurboFan || engineType == EngineType.TurboJet || engineType == EngineType.TurboProp)
                {
                    if (tankFuelType != jetType.ToString())
                    {
                        Debug.LogError("Prerequisites not met on Aircraft " + transform.name + ".... fuel selected on controller (" +jetType.ToString() + ") not a match with loaded tank fuel (" +tankFuelType + ")");
                        allOk = false; return;
                    }
                }

                // --------------------------------
                fuelSystem.controller = controller;
                fuelSystem.InitializeDistributor();
            }



            // ------------------------- Setup Drag Body
            foreach (SilantroBody body in dragForms)
            {
                body.controller = controller;
                body.aircraft = aircraft;
                body.InitializeBody();
            }


            // ------------------------- Setup Actuators
            if (actuators != null)
            {
                foreach (SilantroActuator actuator in actuators)
                {
                    if (actuator.initialized) { Debug.LogWarning("Actuator for " + actuator.transform.name + " is still in evaluation mode."); }
                    else { actuator.InitializeActuator(); }

                    // ------------- Filter
                    if (actuator.actuatorType == SilantroActuator.ActuatorType.Canopy) { canopyActuator = actuator; }
                    if (actuator.actuatorType == SilantroActuator.ActuatorType.LandingGear) { gearActuator = actuator; }
                    if (actuator.actuatorType == SilantroActuator.ActuatorType.SpeedBrake) { speedBrakeActuator = actuator; }
                    if (actuator.actuatorType == SilantroActuator.ActuatorType.SwingWings) { wingActuator = actuator; }
                    if (actuator.actuatorType == SilantroActuator.ActuatorType.LiftSystem) { liftSystem = actuator; }
                }
            }




            // --------------------------- Bulbs
            foreach (SilantroLight bulb in lightBulbs) { bulb.InitializeBulb(); if (bulb.lightType == SilantroLight.LightType.Landing && gearActuator != null) { gearActuator.landingBulbs.Add(bulb); } }




            // ------------------------- Setup Flight Computer
            if (flightComputer != null)
            {
                flightComputer.core = core;
                flightComputer.controller = controller;
                if (gearActuator != null) { flightComputer.gearSolver = gearActuator; }
                if (speedBrakeActuator != null) { flightComputer.speedBrakeSolver = speedBrakeActuator; }

                flightComputer.InitializeComputer();
            }



            // ------------------------- Setup Aerofoils
            if (wings != null)
            {
                foreach (SilantroAerofoil foil in wings)
                {
                    foil.connectedAircraft = controller;
                    foil.coreSystem = core;
                    foil.InitializeFoil();

                    // --- Filter
                    if (foil.aerofoilType == SilantroAerofoil.AerofoilType.Wing)
                    {
                        if (flightComputer != null) { flightComputer.wingFoils.Add(foil); }

                        // ---------- Flap
                        if (foil.flapState == SilantroAerofoil.ControlState.Active) { coreFlapWing = foil; }
                        // ---------- Slat
                        if (foil.slatState == SilantroAerofoil.ControlState.Active) { coreSlatWing = foil; }
                        // ---------- Soiler
                        if (foil.spoilerState == SilantroAerofoil.ControlState.Active) { coreSpoilerWing = foil; }
                    }
                }

                if (flightComputer != null) { flightComputer.FilterWingData(); }
            }



            // ------------------------- Setup Engines
            if (engineType == EngineType.TurboFan && turboFans != null) { baseCore = turboFans[0].core; foreach (SilantroTurboFan engine in turboFans) { engine.controller = controller; engine.computer = core; engine.connectedAircraft = aircraft; engine.InitializeEngine(); } }
            if (engineType == EngineType.TurboJet && turboJets != null) { baseCore = turboJets[0].core; foreach (SilantroTurboJet engine in turboJets) { engine.controller = controller; engine.computer = core; engine.connectedAircraft = aircraft; engine.InitializeEngine(); } }
            if (engineType == EngineType.Piston && pistons != null) { baseCore = pistons[0].core; foreach (SilantroPistonEngine engine in pistons) { engine.controller = controller; engine.computer = core; engine.connectedAircraft = aircraft; engine.InitializeEngine(); } }
            if (engineType == EngineType.TurboProp && propEngines != null) { baseCore = propEngines[0].core; foreach (SilantroTurboProp engine in propEngines) { engine.controller = controller; engine.computer = core; engine.connectedAircraft = aircraft; engine.InitializeEngine(); } }
            if (engineType == EngineType.Electric && motors != null) { foreach (SilantroElectricMotor engine in motors) { engine.controller = controller; engine.InitializeMotor(); } }


            // ------------------------- Setup Propellers
            if (propellers != null)
            {
                foreach (SilantroPropeller prop in propellers)
                {
                    prop.controller = controller;
                    prop.computer = core;
                    prop.InitializePropeller();
                }
            }


            // ------------------------- Setup Lift System
            if (aircraftType == AircraftType.VTOL)
            {
                hoverSystem.controller = controller;
                hoverSystem.mainEngine = turboFans[0];
                hoverSystem.liftfan = liftFan;
                hoverSystem.thrustFactor = 1f;
                if (liftSystem != null) { hoverSystem.vtolMechanism = liftSystem; }
          

                if(liftFan != null) {
                    liftFan.attachedEngine = turboFans[0];
                    liftFan.controller = controller;
                    liftFan.InitializeEngine(); 
                }
            }


            // ------------------------- Setup Radar
            if (radarCore != null)
            {
                radarCore.connectedAircraft = controller;
                radarCore.InitializeRadar();
            }


            // --------------------------------------------------------------------------------------- Weapons
            if (Armaments != null)
            {
                //STORE FOR REARMING
                GameObject armamentBox = Armaments.gameObject;
                ArmamentsStorage = GameObject.Instantiate(armamentBox, Armaments.transform.position, Armaments.transform.rotation, this.transform);
                ArmamentsStorage.SetActive(false); ArmamentsStorage.name = "Hardpoints(Storage)";
                if (radarCore != null)
                {
                    Armaments.connectedRadar = radarCore;
                }
                Armaments.controller = controller;
                Armaments.InitializeWeapons();
            }




            // --------------------------------------------------------------------------------------- Fuel System
            if (engineType == EngineType.Piston)
            {
                if (gasType == GasType.AVGas100) { combustionEnergy = 42.8f; }
                if (gasType == GasType.AVGas100LL) { combustionEnergy = 43.5f; }
                if (gasType == GasType.AVGas82UL) { combustionEnergy = 43.6f; }
            }
            else
            {
                if (jetType == JetType.JetB) { combustionEnergy = 42.8f; }
                if (jetType == JetType.JetA1) { combustionEnergy = 43.5f; }
                if (jetType == JetType.JP6) { combustionEnergy = 43.02f; }
                if (jetType == JetType.JP8) { combustionEnergy = 43.28f; }
            }
            combustionEnergy *= 1000f;


            // --------------------------------------------------------------------------------------- Control State
            if (controlType == ControlType.Internal)
            {
                helper.InternalControlSetup(controller);
                SetControlState(false);

                // ------------------------ Disable Canvas
                if (canvasDisplay != null) { canvasDisplay.gameObject.SetActive(false); }
            }
            else
            {
                SetControlState(true);
                // ------------------------ Disable Main Camera
                if (mainCamera != null) { mainCamera.gameObject.SetActive(false); }
            }



            // ------------------------- Setup Input
            if (input != null)
            {
                input.controller = controller;
                input.AwakeInput();
#if UNITY_EDITOR
                CheckInputCofig(true);
#endif
            }
        }
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Update()
    {
        if (isControllable && allOk)
        {
            // -------------------------- Update Inputs
            if (input != null) { input.UpdateInput(); input.AnalyseInputs(); }
            if (view != null) { cameraState = view.cameraState; }


            // -------------------------- Collect Data
            AnalyseData();
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (isControllable && allOk)
        {
            // ---------------------------- Fuel Usage
            if (engineType != EngineType.Electric && engineType != EngineType.Unpowered) { fuelSystem.UpdateFuel(); }

            if(aircraftType == AircraftType.VTOL) { hoverSystem.UpdateHover(); }
        }
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void AnalyseData()
    {
        if (engineType == EngineType.TurboJet && turboJets.Length > 0)
        {
            engineRunning = helper.CheckEngineState(turboJets[0].core);
            totalThrustGenerated = 0; totalConsumptionRate = 0; AvailableEngines = turboJets.Length;
            if (turboJets != null && turboJets[0] != null) { silantroActive = turboJets[0].core.active; silantroEnginePower = turboJets[0].core.corePower; }
            foreach (SilantroTurboJet turbojet in turboJets)
            { if (turbojet.core.active) { totalThrustGenerated += turbojet.engineThrust; totalConsumptionRate += turbojet.mf; } }
        }
        if (engineType == EngineType.TurboFan && turboFans.Length > 0)
        {
            engineRunning = helper.CheckEngineState(turboFans[0].core);
            totalThrustGenerated = 0; totalConsumptionRate = 0; AvailableEngines = turboFans.Length;
            if (turboFans != null && turboFans[0] != null) { silantroActive = turboFans[0].core.active; silantroEnginePower = turboFans[0].core.corePower; }
            foreach (SilantroTurboFan turbofan in turboFans)
            { if (turbofan.core.active) { totalThrustGenerated += turbofan.engineThrust; totalConsumptionRate += turbofan.mf; } }
        }
        if (engineType == EngineType.Piston && pistons.Length > 0)
        {
            engineRunning = helper.CheckEngineState(pistons[0].core);
            totalConsumptionRate = 0; AvailableEngines = pistons.Length;
            if (pistons != null && pistons[0] != null) { silantroActive = pistons[0].core.active; silantroEnginePower = pistons[0].core.corePower; }
            foreach (SilantroPistonEngine piston in pistons)
            { if (piston.core.active) { totalConsumptionRate += piston.Mf; } }
            if (propellers != null && propellers.Length > 0) { totalThrustGenerated = 0; foreach (SilantroPropeller prop in propellers) { totalThrustGenerated += prop.thrust; } }
        }
        if (engineType == EngineType.TurboProp && propEngines.Length > 0)
        {
            engineRunning = helper.CheckEngineState(propEngines[0].core);
            totalConsumptionRate = 0; AvailableEngines = propEngines.Length;
            if (propEngines != null && propEngines[0] != null) { silantroActive = propEngines[0].core.active; silantroEnginePower = propEngines[0].core.corePower; }
            foreach (SilantroTurboProp prop in propEngines)
            { if (prop.core.active) { totalConsumptionRate += prop.mf; } }
            if (propellers != null && propellers.Length > 0) { totalThrustGenerated = 0; foreach (SilantroPropeller prop in propellers) { totalThrustGenerated += prop.thrust; } }
        }
        if (engineType == EngineType.Electric && motors.Length > 0)
        {
            AvailableEngines = motors.Length;
            if (propEngines != null && motors[0] != null) { silantroActive = motors[0].engineState == SilantroElectricMotor.EngineState.Running; silantroEnginePower = motors[0].coreFactor; }
            if (propellers != null && propellers.Length > 0) { totalThrustGenerated = 0; foreach (SilantroPropeller prop in propellers) { totalThrustGenerated += prop.thrust; } }
        }

        if (totalThrustGenerated > maximumThrust) { maximumThrust = totalThrustGenerated; }
        thrustToWeightRatio = maximumThrust / (currentWeight*9.81f);
        if(flightComputer.wingArea > 0 && currentWeight > 0) { wingLoading = currentWeight / flightComputer.wingArea; }


        // -------------------------------------------- Wings
        if (coreFlapWing != null) { flapAngle = Mathf.Abs(coreFlapWing.flapDeflection); }
        if (coreSlatWing != null) { slatAngle = Mathf.Abs(coreSlatWing.slatDeflection); }
        if (coreSpoilerWing != null) { spoilerAngle = Mathf.Abs(coreSpoilerWing.spoilerDeflection); }


        // -------------------------------------------- Drag
        actuatorDrag = 0f;
        foreach (SilantroActuator actuator in actuators) { actuatorDrag += actuator.currentDragFactor; }
        currentDrag = (initialDrag + actuatorDrag); aircraft.drag = currentDrag;
        if (gearHelper != null && core.currentAltitude < 5) { isGrounded = gearHelper.GroundCheck(); } else { isGrounded = false; }
    }





#if UNITY_EDITOR
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void CheckInputCofig(bool truncate)
    {
        if (Application.isEditor)
        {
            inputList = new List<string>();
            // ------------------------------------- Check Input Config
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            SerializedObject obj = new SerializedObject(inputManager);
            SerializedProperty axisArray = obj.FindProperty("m_Axes");
            if (axisArray.arraySize == 0) Debug.Log("No Axes");

            for (int i = 0; i < axisArray.arraySize; ++i) { inputList.Add(axisArray.GetArrayElementAtIndex(i).displayName); }

            if (inputList.Contains("Start Engine") && inputList.Contains("Stop Engine") && inputList.Contains("Pitch") && inputList.Contains("Roll"))
            {
                if (inputList.Contains("Start Engine BandL") && inputList.Contains("Stop Engine BandL"))
                {
                    input.inputConfigured = true;
                }
                else
                {
                    input.inputConfigured = false;
                    Debug.LogError("Input needs to be reconfigured. Go to Oyedoyin/Miscellaneous/Setup Input");
                    allOk = false;
                    if (truncate) { return; }
                }
            }
            else
            {
                input.inputConfigured = false;
                Debug.LogError("Input needs to be configured. Go to Oyedoyin/Miscellaneous/Setup Input");
                allOk = false;
                if (truncate) { return; }
            }
        }
    }
#endif
}
