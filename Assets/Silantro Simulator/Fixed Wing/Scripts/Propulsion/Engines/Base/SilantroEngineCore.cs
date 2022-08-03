using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;


[System.Serializable]
public class SilantroEngineCore
{

    //---------------------------------------VARIABLES
    public string engineIdentifier = "Default Engine";
    public float cameraSector;
    public Transform engine;
    public SilantroController controller;
    public enum EngineNumber { N1, N2, N3, N4, N5, N6 }
    public EngineNumber engineNumber = EngineNumber.N1;
    public enum EngineType { Turbojet, Turbofan, TurboProp, Piston, LiftFan }
    public EngineType engineType = EngineType.Turbojet;
    public enum EnginePosition { Left, Right, Center}
    public EnginePosition enginePosition = EnginePosition.Center;
    public enum RotationAxis { X, Y, Z }
    public RotationAxis rotationAxis = RotationAxis.Z;
    public RotationAxis pitchAxis = RotationAxis.X;
    public RotationAxis rollAxis = RotationAxis.Y;
    public RotationAxis yawAxis = RotationAxis.Z;
    public enum RotationDirection { CW, CCW }
    public RotationDirection rotationDirection = RotationDirection.CCW;
    public RotationDirection pitchDirection = RotationDirection.CW;
    public RotationDirection rollDirection = RotationDirection.CW;
    public RotationDirection yawDirection = RotationDirection.CW;
    public enum ThrustVectoring { None, PitchOnly, TwoAxis, ThreeAxis}
    public ThrustVectoring vectoringType = ThrustVectoring.None;
    public enum FlameType { Circular, Rectangular}
    public FlameType flameType = FlameType.Circular;

    public enum PitchLabel { PitchAxis} public PitchLabel pitchLabel = PitchLabel.PitchAxis;
    public enum RollLabel { RollAxis} public RollLabel rollLabel = RollLabel.RollAxis;
    public enum YawLabel { YawAxis} public YawLabel yawLabel = YawLabel.YawAxis;

    //Engine
    public SilantroTurboJet turboJet;
    public SilantroTurboFan turboFan;
    public SilantroTurboProp turboProp;
    public SilantroPistonEngine piston;
    public SilantroLiftFan liftFan;
    public Transform intakeFan;
    public Transform nozzlePivot;


    public enum SoundMode { Basic, Advanced}
    public SoundMode soundMode = SoundMode.Basic;
    public enum InteriorMode { Off, Active}
    public InteriorMode interiorMode = InteriorMode.Off;

    //--------------------------------------SOUND
    public float sideVolume;
    public float frontVolume;
    public float backVolume;
    public float interiorVolume, exteriorVolume;
    public float overideExteriorVolume, overideInteriorVolume;
    public float overidePitch,basePitch;

    public AudioSource frontSource;
    public AudioSource sideSource;
    public AudioSource backSource;
    public AudioSource interiorSource;
    public AudioSource exteriorSource;
    public AudioSource interiorBase;

    public AudioClip frontIdle;
    public AudioClip sideIdle;
    public AudioClip backIdle;
    public AudioClip interiorIdle;
    public AudioClip ignitionInterior, ignitionExterior;
    public AudioClip shutdownInterior, shutdownExterior;
    public bool baseEffects = true, baseEmission;


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeEngineCore()
    {
        //----------------------------------------SET SOUND SOURCES
        GameObject soundPoint = new GameObject("Sources"); soundPoint.transform.parent = engine; soundPoint.transform.localPosition = Vector3.zero;
        if (soundMode == SoundMode.Advanced)
        {
            if (frontIdle) { Handler.SetupSoundSource(soundPoint.transform, frontIdle, "Front Sound Point", 150f, true, true, out frontSource); }
            if (sideIdle) { Handler.SetupSoundSource(soundPoint.transform, sideIdle, "Side Sound Point", 150f, true, true, out sideSource); }
        }
        if (backIdle) { Handler.SetupSoundSource(soundPoint.transform, backIdle, "Rear Sound Point", 150f, true, true, out backSource); }
        if (interiorIdle && interiorMode == InteriorMode.Active) { Handler.SetupSoundSource(soundPoint.transform, interiorIdle, "Interior Base Point", 80f, true, true, out interiorBase); }


        if (ignitionInterior && interiorMode == InteriorMode.Active) { Handler.SetupSoundSource(soundPoint.transform, ignitionInterior, "Interior Sound Point", 50f, false, false, out interiorSource); }
        if (ignitionExterior) { Handler.SetupSoundSource(soundPoint.transform, ignitionExterior, "Exterior Sound Point", 150f, false, false, out exteriorSource); }
        if (controller.startMode == SilantroController.StartMode.Hot) { trueCoreAcceleration = 10f; }

       // if(ignitionExterior != null && shutdownExterior != null && backIdle != null) { soundConfigured = true; }

        // ---------------------------------------- Set up Afterburner Flame
        if (flameObject != null && flameMaterial != null)
        {
            baseDiameter = flameObject.transform.localScale.x;
            baseLength = flameObject.transform.localScale.z;
            baseFlameColor = flameMaterial.GetColor("_TintColor");

            if(controller.startMode == SilantroController.StartMode.Hot) { actualDiameter = targetDiameter = normalDiameter;actualLength = targetLength = normalLength; }
        }
        baseColor = Color.white;
        EvaluateRPMLimits();
        gFactor = 1f;

        if(vectoringType != ThrustVectoring.None)
        {
            if(nozzlePivot != null) { baseNormalRotation = nozzlePivot.transform.localRotation; }
            pitchAxisRotation = Handler.EstimateModelProperties(pitchDirection.ToString(), pitchAxis.ToString());
            rollAxisRotation = Handler.EstimateModelProperties(rollDirection.ToString(), rollAxis.ToString());
            yawAxisRotation = Handler.EstimateModelProperties(yawDirection.ToString(), yawAxis.ToString());
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void EvaluateRPMLimits()
    {
        minimumRPM = (1 - (overspeedAllowance / 100)) * functionalRPM;
        maximumRPM = (1 + (overspeedAllowance / 100)) * functionalRPM;
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void StartEngine()
    {
        if (controller != null && controller.isControllable)
        {
            //MAKE SURE SOUND IS SET PROPERLY
            if (backIdle == null || ignitionExterior == null || shutdownExterior == null)
            {
                Debug.Log("Engine " + engine.name + " cannot start due to incorrect Audio configuration");
            }
            else
            {
                //MAKE SURE THERE IS FUEL TO START THE ENGINE
                if (controller && controller.fuelLevel > 1f)
                {
                    //ACTUAL START ENGINE
                    if (controller.startMode == SilantroController.StartMode.Cold)
                    {
                        start = true;
                    }
                    if (controller.startMode == SilantroController.StartMode.Hot)
                    {
                        //JUMP START ENGINE
                        active = true;
                        StateActive(); clutching = false; CurrentEngineState = EngineState.Active;
                    }
                }
                else
                {
                    Debug.Log("Engine " + engine.name + " cannot start due to low fuel");
                }
            }
        }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ShutDownEngine() { shutdown = true; }


   










    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //------------------------------------------------------STATE MANAGEMENT
    public enum EngineState { Off, Starting, Active }
    public EngineState CurrentEngineState;

    //--------------------------------------VARIABLES
    [Tooltip("Engine throttle up and down speed.")] [Range(0.01f,1f)]public float baseCoreAcceleration = 0.25f;
    public float corePower, trueCoreAcceleration;
    public bool start, shutdown, clutching, active;
    public float coreRPM, factorRPM, norminalRPM, functionalRPM = 1000f;
    public float controlInput;
    public float idlePercentage = 10f, coreFactor, fuelFactor;
    [Tooltip("Percentage of RPM allowed over or under the functional RPM.")] [Range(0,10)]public float overspeedAllowance = 3f;
    public bool afterburnerOperative, canUseAfterburner;
    public float pitchTarget;
    public float gControl, gForce, pitchFactor, totalLoad, engineLoad = 10f;
    public float inputLoad, shaftRPM, powerInput;
    public bool torqueConnected, torqueEngaged;
    public float minimumRPM = 900, maximumRPM = 1000;

    public bool reverseThrustAvailable, reverseThrustEngaged;
    public float reverseThrustFactor;
    [Range(0, 60f)] public float reverseThrustPercentage = 50f;
    public float burnerFactor;
    public float gFactor = 1f;


    //-------------------------------------------------------ENGINE CORE
    void AnalyseCore()
    {

        // ----------------- Check Control state
        if (controller.startMode != SilantroController.StartMode.Hot)
        {
            if (controller.flightComputer != null && corePower > 0.95f && CurrentEngineState == EngineState.Active && controller.core.currentAltitude > 15f)
            {
                if(controller.flightComputer.autoThrottle == SilantroFlightComputer.ControlState.Active || controller.flightComputer.protectionActive) { trueCoreAcceleration = 10f; }
            }
            else { trueCoreAcceleration = baseCoreAcceleration; }
        }


        if(active && controller.fuelLevel < 5) { ShutDownEngine(); }
        if(active && controller.isControllable == false) { ShutDownEngine(); }


        //------------------ POWER
        if (active) { if (corePower < 1f) { corePower += Time.deltaTime * trueCoreAcceleration; } }
        else if (corePower > 0f) { corePower -= Time.deltaTime * trueCoreAcceleration; }
        if (controlInput > 1) { controlInput = 1f; }
        if (corePower > 1) { corePower = 1f; }
        if (!active && corePower < 0) { corePower = 0f; }
        if(active && controller.fuelExhausted) { shutdown = true; }
        fuelFactor = 1f;

        if(active && canUseAfterburner && afterburnerOperative) { burnerFactor += Time.deltaTime * trueCoreAcceleration; }
        else if (burnerFactor > 0f) { burnerFactor -= Time.deltaTime * trueCoreAcceleration; }
        if (burnerFactor > 1) { burnerFactor = 1f; }if(burnerFactor < 0) { burnerFactor = 0f; }


        //------------------ FUEL
        if (active && controller.lowFuel)
        {
            float startRange = 0.2f; float endRange = 0.85f; float cycleRange = (endRange - startRange) / 2f;
            float offset = cycleRange + startRange; fuelFactor = offset + Mathf.Sin(Time.time * 3f) * cycleRange;
        }

        //------------------- STATES
        switch (CurrentEngineState) { case EngineState.Off: StateOff(); break; case EngineState.Starting: StateStart(); break; case EngineState.Active: StateActive(); break; }

        //------------------- RPM
        if (active) { factorRPM = Mathf.Lerp(factorRPM, norminalRPM, trueCoreAcceleration * Time.fixedDeltaTime * 2); }
        else { factorRPM = Mathf.Lerp(factorRPM, 0, trueCoreAcceleration * Time.fixedDeltaTime * 2f); }
        float limitRPM = (functionalRPM * (100 + overspeedAllowance)) / 100f; if (factorRPM > limitRPM) { factorRPM = limitRPM; }


        if (torqueConnected)
        {
            //TORQUE CONFIGURATION
            totalLoad = engineLoad + inputLoad + 0.0001f;
            shaftRPM = (30f * powerInput * 1000) / (totalLoad * 3.142f);
            shaftRPM = Mathf.Clamp(shaftRPM, 0, maximumRPM);
        }

        coreRPM = factorRPM * corePower*fuelFactor*gFactor; coreFactor = coreRPM / functionalRPM;

        if (intakeFan)
        {
            if (rotationDirection == RotationDirection.CCW)
            {
                if (rotationAxis == RotationAxis.X) { intakeFan.Rotate(new Vector3(coreRPM * Time.deltaTime, 0, 0)); }
                if (rotationAxis == RotationAxis.Y) { intakeFan.Rotate(new Vector3(0, coreRPM * Time.deltaTime, 0)); }
                if (rotationAxis == RotationAxis.Z) { intakeFan.Rotate(new Vector3(0, 0, coreRPM * Time.deltaTime)); }
            }
            //
            if (rotationDirection == RotationDirection.CW)
            {
                if (rotationAxis == RotationAxis.X) { intakeFan.Rotate(new Vector3(-1f * coreRPM * Time.deltaTime, 0, 0)); }
                if (rotationAxis == RotationAxis.Y) { intakeFan.Rotate(new Vector3(0, -1f * coreRPM * Time.deltaTime, 0)); }
                if (rotationAxis == RotationAxis.Z) { intakeFan.Rotate(new Vector3(0, 0, -1f * coreRPM * Time.deltaTime)); }
            }
        }

        //-------------------SOUND
        if (controller.cameraState == SilantroCamera.CameraState.Exterior) { overideExteriorVolume = corePower; overideInteriorVolume = 0f; }
        if (controller.cameraState == SilantroCamera.CameraState.Interior) { overideInteriorVolume = corePower; overideExteriorVolume = 0f; }
        if(afterburnerOperative && controlInput < 0.5f) { afterburnerOperative = false; }
        if(reverseThrustAvailable && reverseThrustEngaged && afterburnerOperative) { afterburnerOperative = false; }

        if(active && reverseThrustAvailable)
        {
            if (reverseThrustEngaged) { reverseThrustFactor += Time.deltaTime * baseCoreAcceleration; }
            else { reverseThrustFactor -= Time.deltaTime * baseCoreAcceleration; }
        }
        if(reverseThrustFactor > 1) { reverseThrustFactor = 1f; }
        if(reverseThrustFactor < 0) { reverseThrustFactor = 0f; }
    }








    //------------------------------------------------ANALYSE SOUND SECTOR
    void AnalyseSound()
    {

        if (controller.cameraState == SilantroCamera.CameraState.Exterior)
        {
            //RESET
            interiorVolume = 0f; exteriorVolume = 1f;

            if (soundMode == SoundMode.Advanced && controller.view != null)
            {
                //------------------------------------------FRONT || RIGHT
                if (cameraSector > 0 && cameraSector < 90) { frontVolume = cameraSector / 90f; sideVolume = 1 - frontVolume; backVolume = 0f; }

                //------------------------------------------FRONT || LEFT
                if (cameraSector >= 90 && cameraSector < 180) { sideVolume = (cameraSector - 90) / 90f; frontVolume = 1 - sideVolume; backVolume = 0f; }

                //------------------------------------------BACK || LEFT
                if (cameraSector >= 180 && cameraSector < 270) { backVolume = (cameraSector - 180) / 90f; sideVolume = 1 - backVolume; frontVolume = 0f; }

                //------------------------------------------BACK || RIGHT
                if (cameraSector >= 270 && cameraSector < 360) { sideVolume = (cameraSector - 270) / 90f; backVolume = 1 - sideVolume; frontVolume = 0f; }
            }
            else { backVolume = 1f; }
        }
        else { backVolume = sideVolume = frontVolume = 0f; interiorVolume = 1f; exteriorVolume = 0f; }

        //-------------------PITCH
        float speedFactor = ((coreRPM + (controller.aircraft.velocity.magnitude * 1.943f) + 10f) - functionalRPM * (idlePercentage / 100f)) / (functionalRPM - functionalRPM * (idlePercentage / 100f));

        basePitch = 0.35f + (0.7f * speedFactor);
        if (afterburnerOperative && canUseAfterburner) { pitchTarget = 0.5f + (1.35f * speedFactor); } else { pitchTarget = basePitch; }
        if(fuelFactor < 1) { overidePitch = pitchTarget; } else { overidePitch = fuelFactor * Mathf.Lerp(overidePitch, pitchTarget, Time.deltaTime * 0.5f); }
        basePitch *= fuelFactor; backSource.pitch = overidePitch;
        if (interiorMode == InteriorMode.Active && interiorBase != null) { interiorBase.pitch = overidePitch; } if (soundMode == SoundMode.Advanced) { frontSource.pitch = basePitch; sideSource.pitch = basePitch; }


        //-------------------SET VOLUMES
        backSource.volume = overideExteriorVolume * backVolume; if (soundMode == SoundMode.Advanced) { frontSource.volume = overideExteriorVolume * frontVolume; sideSource.volume = overideExteriorVolume * sideVolume; }
        exteriorSource.volume = exteriorVolume; if (interiorMode == InteriorMode.Active && interiorBase != null && interiorSource != null) { interiorSource.volume = interiorVolume;
        if (controller != null && controller.view != null ) { interiorBase.volume = overideInteriorVolume * controller.view.maximumInteriorVolume; } else { interiorBase.volume = overideInteriorVolume; } }
    }






    //--------------------------------------------------------ENGINE STATES
    public void StateActive()
    {
        if (exteriorSource.isPlaying) { exteriorSource.Stop(); }
        if (interiorSource != null && interiorSource.isPlaying) { interiorSource.Stop(); }

        //------------------STOP ENGINE
        if (shutdown)
        {
            exteriorSource.clip = shutdownExterior; exteriorSource.Play();
            if (interiorSource != null) { interiorSource.clip = shutdownInterior; interiorSource.Play(); }
            CurrentEngineState = EngineState.Off;
            active = false;

            if (engineType == EngineType.Turbojet) { turboJet.ReturnIgnitionCall(); }
            if (engineType == EngineType.Turbofan) { turboFan.ReturnIgnitionCall(); }
            if (engineType == EngineType.Piston) { piston.ReturnIgnitionCall(); }
            if (engineType == EngineType.LiftFan) { liftFan.ReturnIgnitionCall(); }
            if (engineType == EngineType.TurboProp) { turboProp.ReturnIgnitionCall(); }
        }

        //------------------RUN
        if (torqueEngaged) { norminalRPM = (functionalRPM * (idlePercentage / 100f)) + (shaftRPM - (functionalRPM * (idlePercentage / 100f))) * controlInput; }
        else { norminalRPM = (functionalRPM * (idlePercentage / 100f)) + (maximumRPM - (functionalRPM * (idlePercentage / 100f))) * controlInput; }  
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void StateStart()
    {
        if (clutching) { if (!exteriorSource.isPlaying) { CurrentEngineState = EngineState.Active; clutching = false; StateActive(); } }
        else { exteriorSource.Stop(); if (interiorSource != null) { interiorSource.Stop(); } CurrentEngineState = EngineState.Off; }

        //------------------RUN
        norminalRPM = functionalRPM * (idlePercentage / 100f);
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void StateOff()
    {
        if (exteriorSource.isPlaying && corePower < 0.01f) { exteriorSource.Stop(); }
        if (interiorSource != null && interiorSource.isPlaying && corePower < 0.01f) { interiorSource.Stop(); }


        //------------------START ENGINE
        if (start)
        {
            exteriorSource.clip = ignitionExterior; exteriorSource.Play();
            if (interiorSource != null) { interiorSource.clip = ignitionInterior;  interiorSource.Play(); }
            CurrentEngineState = EngineState.Starting; clutching = true;
            active = true;
            if (engineType == EngineType.Turbojet) { turboJet.ReturnIgnitionCall(); }
            if (engineType == EngineType.Turbofan) { turboFan.ReturnIgnitionCall(); }
            if (engineType == EngineType.Piston) { piston.ReturnIgnitionCall(); }
            if (engineType == EngineType.LiftFan) { liftFan.ReturnIgnitionCall(); }
            if (engineType == EngineType.TurboProp) { turboProp.ReturnIgnitionCall(); }
        }


        //------------------RUN
        norminalRPM = 0f;
    }

  







    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //------------------------------------------------------EFFECT MANAGEMENT
    public ParticleSystem exhaustSmoke;
    public ParticleSystem.EmissionModule smokeModule;
    public ParticleSystem exhaustDistortion;
    ParticleSystem.EmissionModule distortionModule;


    // -------------------------- Emission
    public Color baseColor, finalColor;
    public float smokeEmissionLimit = 50f;
    public float distortionEmissionLimit = 20f;
    public Material engineMaterial;
    public Material afterburnerTubeMaterial; public float emissionCore;
    public float maximumNormalEmission = 1f; public float maximumAfterburnerEmission = 2f;

    // --------------------------- Afterburner
    public SilantroActuator dataSource;
    public float targetDiameter, currentDiameter, baseDiameter;
    public float targetLength, currentLength, baseLength;

    public float normalDiameter = 1000f, normalLength = 1000f;
    public float wetDiameter = 1200f, wetLength = 1200f;
    [Range(0,1f)]public float wetAlpha = 0.5f, normalAlpha = 0.01f;
    public float actualDiameter, actualLength;

    // ---------------------------- Flame
    public GameObject flameObject;
    public Material flameMaterial;
    Color baseFlameColor, currentFlameColor;

    public float currentLevel, flameAlpha, targetAlpha;
    public float alphaSpeed = 0.1f, scaleSpeed = 100f,value;
    public bool coreFlame;






    //-------------------------------------------------------ENGINE EFFECTS
    void AnalyseEffects()
    {
        // Collect Modules
        if (!smokeModule.enabled && exhaustSmoke != null) { smokeModule = exhaustSmoke.emission; }
        if (!distortionModule.enabled && exhaustDistortion != null) { distortionModule = exhaustDistortion.emission; }

        // ------------------------- Nozzle Driver
        if(dataSource != null)
        {
            if (afterburnerOperative) { dataSource.targetActuationLevel = coreFactor; }
            else { dataSource.targetActuationLevel = coreFactor * controlInput / 3f; }
            if (!active) { dataSource.targetActuationLevel = 0f; }
        }

        
        // Control Amount
        if (smokeModule.enabled) { smokeModule.rateOverTime = smokeEmissionLimit * coreFactor; }
        if (distortionModule.enabled) { distortionModule.rateOverTime = distortionEmissionLimit * coreFactor; }


        // ------------------------------ Engine Emission
        if(canUseAfterburner && afterburnerOperative) { value = Mathf.Lerp(value, maximumAfterburnerEmission, 0.025f); }
        else { value = Mathf.Lerp(value, maximumNormalEmission, 0.02f); }
        finalColor = baseColor * Mathf.LinearToGammaSpace(value * coreFactor);
        if (engineMaterial != null) { engineMaterial.SetColor("_EmissionColor", finalColor); }
        if (afterburnerTubeMaterial != null) { afterburnerTubeMaterial.SetColor("_EmissionColor", finalColor); }
     


        // -------------------------------Afterburner Flame
        if (flameObject != null && flameMaterial != null)
        {
            if (active)
            {
                if (canUseAfterburner && afterburnerOperative) { targetAlpha = wetAlpha * coreFactor; targetDiameter = wetDiameter; targetLength = wetLength; }
                else { targetAlpha = normalAlpha * coreFactor; targetDiameter = normalDiameter; targetLength = normalLength; }
            }
            else { targetAlpha = 0f; targetDiameter = baseDiameter; targetLength = baseLength; }


            // --------------------------- Flame Color
            flameAlpha = Mathf.MoveTowards(flameAlpha, targetAlpha, Time.deltaTime * alphaSpeed);
            currentFlameColor = new Color(baseFlameColor.r, baseFlameColor.b, baseFlameColor.g, flameAlpha);

            // --------------------------- Flame Scale
            currentDiameter = baseDiameter + ((targetDiameter - baseDiameter) * coreFactor);
            currentLength = baseLength + ((targetLength - baseLength) * coreFactor);
            actualDiameter = Mathf.MoveTowards(actualDiameter, currentDiameter, Time.deltaTime * scaleSpeed);
            actualLength = Mathf.MoveTowards(actualLength, currentLength, Time.deltaTime * scaleSpeed);

            //Set
            if(flameType == FlameType.Rectangular) { flameObject.transform.localScale = new Vector3(actualDiameter, baseDiameter, actualLength*coreFactor); }
            else { flameObject.transform.localScale = new Vector3(actualDiameter, actualDiameter, actualLength); }
            flameMaterial.SetColor("_TintColor", currentFlameColor);
        }
    }





    Quaternion baseNormalRotation, angleEffect;
    Vector3 pitchAxisRotation, yawAxisRotation, rollAxisRotation;
    public float maximumPitchDeflection = 15f, maximumRollDeflection = 15f, maximumYawDeflection = 15f;
    //-------------------------------------------------------ENGINE NOZZLE
    public void AnalyseNozzle()
    {
        var yaw = controller.flightComputer.processedYaw * maximumYawDeflection;
        var pitch = controller.flightComputer.processedPitch * maximumPitchDeflection;
        var roll = controller.flightComputer.processedRoll * maximumRollDeflection;

        var pitchEffect = Quaternion.AngleAxis(pitch, pitchAxisRotation);
        var rollEffect = Quaternion.AngleAxis(roll, rollAxisRotation);
        var yawEffect = Quaternion.AngleAxis(yaw, yawAxisRotation);

        if(vectoringType == ThrustVectoring.PitchOnly) { angleEffect = pitchEffect; }
        else if(vectoringType == ThrustVectoring.TwoAxis) {angleEffect = pitchEffect * rollEffect; }
        else { angleEffect = yawEffect * pitchEffect * rollEffect; }
        nozzlePivot.localRotation = baseNormalRotation * angleEffect;
    }







    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateCore()
    {
        //-----------------//SOUND
        AnalyseSound();

        //-----------------//CORE
        AnalyseCore();

        //----------------//EFFECTS
        AnalyseEffects();

        // -------------- NOZZLE
        if(vectoringType != ThrustVectoring.None && nozzlePivot != null) { AnalyseNozzle(); }
    }
}
