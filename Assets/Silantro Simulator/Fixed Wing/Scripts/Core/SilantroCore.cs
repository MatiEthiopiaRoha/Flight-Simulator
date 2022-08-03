using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif






/// <summary>
/// Handles core aircraft data collection and processing
/// </summary>
/// <remarks>
/// This component will collect the data required by the aircraft and process them. It also handles the step calculation of the aircraft
/// center of gravity based on the position and weight of the aircraft components
/// </remarks>

public class SilantroCore : MonoBehaviour
{

    // ------------------------------------- Selectibles
    public enum SystemType { Basic, Advanced }
    [Tooltip("Determines if the COG position is static or dynamic")] public SystemType functionality = SystemType.Basic;
    public enum SpeedType { Supersonic, Subsonic }
    public SpeedType speedType = SpeedType.Subsonic;
    public enum TensorMode { Automatic, Manual }
    [Tooltip("Determines if the aircraft rigidbody interia is automatically calculated or will be supplied manually")] public TensorMode tensorMode = TensorMode.Automatic;
   

    // ------------------------------------- Connections
    public SilantroController controller;
    public Rigidbody aircraft;
    public Transform emptyCenterOfMass;
    public Transform currentCOM;
    public Vector3 baseCenter, centerOfMass;

    // ------------------------------------- Pressure Breathing
    public enum PressureBreathing { Active, Off}
    public PressureBreathing breathing = PressureBreathing.Off;
    public AudioClip breathingLoopSound;
    public AudioClip breathingEndSound;
    public float soundVolume = 0.75f;
    public AudioSource breathingLoopSource, breathingEndSource;
    public bool breathingState = false;
    public float breathingThreshold;


    // ------------------------------------- Weights
    public float emptyWeight;
    public float munitionLoad;
    public float componentLoad;
    public float fuelLoad;
    public float totalWeight;
    public int munitionCount;


    // ------------------------------------- Base Data
    public float currentSpeed;
    public float trueSpeed;
    public float groundSpeed;
    public float machSpeed;
    public float currentAltitude;
    public float headingDirection;
    public float acceleration;
    public float speedTimer;
    public float previousSpeed;


    // ------------------------------------- Environmental Data
    public float ambientTemperature;
    public float ambientPressure;
    public float airDensity = 1.225f;
    public float viscocity = 0.1f;


    // ------------------------------------- Dynamics
    public Vector3 earthLinearVelocity, bodyLinearVelocity;
    public Vector3 earthAngularVelocity, bodyAngularVelocity;
    public float verticalSpeed;
    public float pitchRate;
    public float rollRate;
    public float yawRate;
    public float turnRate;
    public float gForce;
    float intermediateValueBuf;
    public float smoothGSpeed = 0.04f;
    public float smoothRateSpeed = 0.05f;
    public float sideSlip, alpha;
    public float bankAngle, soundSpeed;



    // ---------------------------- Inertia
    [Tooltip("Resistance to movement on the pitch axis")] public float xInertia = 10000;
    [Tooltip("Resistance to movement in the roll axis")] public float yInertia = 5000;
    [Tooltip("Resistance to movement in the yaw axis")] public float zInertia = 8000;
    public Vector3 baseInertiaTensor;
    public Vector3 inertiaTensor;
    public Quaternion baseTensorRotation;


    // ---------------------------- Sonic Boom
    public ParticleSystem sonicCone;
    ParticleSystem.EmissionModule sonicModule;
    public AudioClip sonicBoom;
    public AudioSource boom;
    public bool boomPlayed, breathingActive;
    public float sonicEmission = 10f;
    public Rigidbody sampleAircraft;
    bool initialized;

    public float loadFactorThreshold = 4f;
    public float factorChargeSpeed;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeCore()
    {
        //----------------------------COG
        GameObject core = new GameObject("Current Gravity Center"); core.transform.parent = aircraft.transform;
        core.transform.localPosition = Vector3.zero; currentCOM = core.transform;
        if (emptyCenterOfMass == null) { emptyCenterOfMass = this.transform; }


        // ----------------------------- Sonic Boom
        GameObject soundPoint = new GameObject("Sources"); soundPoint.transform.parent = this.transform; soundPoint.transform.localPosition = Vector3.zero;
        if (sonicCone != null && speedType == SpeedType.Supersonic) { sonicModule = sonicCone.emission; sonicModule.rateOverTime = 0f; }
        if (sonicBoom) { Handler.SetupSoundSource(soundPoint.transform, sonicBoom, "Sound Point", 150f, false, false, out boom); boom.volume = 1f; }
        if(breathing == PressureBreathing.Active)
        {
            if (breathingLoopSound) { Handler.SetupSoundSource(this.transform, breathingLoopSound, "Loop Sound Point", 50, true, false, out breathingLoopSource); breathingLoopSource.volume = soundVolume; }
            if (breathingEndSound) { Handler.SetupSoundSource(this.transform, breathingEndSound, "End Sound Point", 50, false, false, out breathingEndSource); breathingEndSource.volume = soundVolume; }
        }

        aircraft.maxAngularVelocity = 10f;

        //----------------------------BALANCE
        baseInertiaTensor = aircraft.inertiaTensor;
        baseTensorRotation = aircraft.inertiaTensorRotation;
        inertiaTensor = new Vector3(xInertia, yInertia, zInertia);
        if (tensorMode == TensorMode.Manual) { aircraft.inertiaTensor = inertiaTensor; }
        initialized = true;
    }







     private void FixedUpdate()
    {
        if (initialized)
        {
            //-----------------DATA
            if (aircraft != null && controller.isControllable) { ProcessData(); }

            //----------------COG
            if (aircraft != null && controller != null) { ProcessCOG(); }

            // ---------------Effects
            if (speedType == SpeedType.Supersonic) { ProcessEffects(); }
        }
    }







    float sonicInput;
    //--------------------------------------------------------------------- Effects
    void ProcessEffects()
    {
        if (sonicCone != null)
        {
            if (!sonicModule.enabled) { sonicModule = sonicCone.emission; }
            if(machSpeed > 0.95f) { sonicInput = machSpeed; if (sonicInput > 1) { sonicInput = 1f; } } else { sonicInput = Mathf.Lerp(sonicInput, 0, 0.8f * Time.deltaTime); }
            sonicModule.rateOverTime = sonicInput * sonicEmission;
        }

        if(boom != null && sonicBoom != null)
        {
            if(machSpeed > 0.98f && !boomPlayed) { boom.Play(); boomPlayed = true; }
            if(machSpeed < 0.95f && boomPlayed) { boomPlayed = false; }
        }
    }



     //---------------------------------------------------------------------COLLECT DATA
    private void ProcessData()
    {
        // ---------------- BASE
        bodyLinearVelocity = MathBase.TransformVelocityToBodyAxis(earthLinearVelocity, aircraft.transform);
        currentSpeed = aircraft.velocity.magnitude;
        currentAltitude = aircraft.transform.position.y;
        headingDirection = aircraft.transform.eulerAngles.y;
        earthAngularVelocity = aircraft.angularVelocity;

        // ---------------- PERFORMANCE
        Vector3 localVelocity = transform.InverseTransformDirection(aircraft.velocity);
        Vector3 localAngularVelocity = transform.InverseTransformDirection(aircraft.angularVelocity);
        pitchRate = (float)Math.Round((-localAngularVelocity.x * Mathf.Rad2Deg), 2);
        yawRate = (float)Math.Round((localAngularVelocity.y * Mathf.Rad2Deg), 2);
        rollRate = (float)Math.Round((-localAngularVelocity.z * Mathf.Rad2Deg), 2);
        verticalSpeed = (float)Math.Round(aircraft.velocity.y, 2);
        acceleration = (currentSpeed - previousSpeed) / Time.fixedDeltaTime; 
        previousSpeed = currentSpeed;


        float U = bodyLinearVelocity.x; float V = bodyLinearVelocity.y; float W = bodyLinearVelocity.z;
        float Vt = Mathf.Sqrt((U * U) + (V * V) + (W * W));
        sideSlip = Mathf.Asin(V / Vt) * Mathf.Rad2Deg;
        Vector3 pointVelocity = transform.InverseTransformDirection(aircraft.velocity);
        alpha = Vector3.Angle(Vector3.forward, pointVelocity) * -Mathf.Sign(pointVelocity.y);


        // ---------------- TURN RADIUS
        float turnRadius = (Mathf.Approximately(localAngularVelocity.x, 0.0f)) ? float.MaxValue : localVelocity.z / localAngularVelocity.x;
        float turnForce = (Mathf.Approximately(turnRadius, 0.0f)) ? 0.0f : (localVelocity.z * localVelocity.z) / turnRadius;
        float baseG = turnForce / -9.81f; baseG += transform.up.y * (Physics.gravity.y / -9.81f);
        float targetG = (baseG * smoothGSpeed) + (intermediateValueBuf * (1.0f - smoothGSpeed));
        intermediateValueBuf = targetG; gForce = (float)Math.Round(targetG, 1);

        bankAngle = aircraft.transform.eulerAngles.z; if (bankAngle > 180.0f) { bankAngle = -(360.0f - bankAngle); }
        turnRate = (1091f * Mathf.Tan(bankAngle * Mathf.Deg2Rad)) / MathBase.ConvertSpeed(currentSpeed + 1, "KTS");

        if (controller.view != null)
        {
            if (gForce > loadFactorThreshold) { controller.view.shakeLevel += Time.deltaTime; }
            else { controller.view.shakeLevel -= Time.deltaTime; }
            if (controller.view.shakeLevel > 1) { controller.view.shakeLevel = 1f; }
            if (controller.view.shakeLevel < 0) { controller.view.shakeLevel = 0f; }
        }


        // ---------------- ----------------AMBIENT 
        float kiloMeter = MathBase.ConvertDistance(currentAltitude, "FT");
        float a = 0.0000004f * kiloMeter * kiloMeter; float b = (0.0351f * kiloMeter);
        ambientPressure = (a - b + 1009.6f) / 10f;
        float a1 = 0.000000003f * kiloMeter * kiloMeter; float a2 = 0.0021f * kiloMeter;
        ambientTemperature = a1 - a2 + 15.443f;
        float kelvinTemperatrue = ambientTemperature + 273.15f;
        airDensity = (ambientPressure * 1000f) / (287.05f * kelvinTemperatrue);
        viscocity = 1.458f / (1000000) * Mathf.Pow(kelvinTemperatrue, 1.5f) * (1 / (kelvinTemperatrue + 110.4f));
        soundSpeed = Mathf.Pow((1.2f * 287f * (273.15f + ambientTemperature)), 0.5f);
        machSpeed = currentSpeed / soundSpeed;

        float densityFactor = 1.225f / airDensity;
        trueSpeed = currentSpeed * Mathf.Sqrt(densityFactor);


        if (breathing == PressureBreathing.Active && controller.view != null && controller.view.cameraState == SilantroCamera.CameraState.Interior)
        {
            if (gForce > 4f) { breathingThreshold += Time.deltaTime; } else { breathingThreshold -= Time.deltaTime; }
            if (breathingThreshold < 0) { breathingThreshold = 0f; }
            if (breathingThreshold > 2f) { breathingThreshold = 2f; }


            if (breathingThreshold > 1.8f) { breathingActive = true; } else { breathingActive = false; }
            if (breathingLoopSound != null && breathingEndSound != null)
            {
                if (breathingActive && breathingLoopSource != null && !breathingLoopSource.isPlaying) { ResetSound(); breathingLoopSource.Play(); }
                if (!breathingActive && breathingLoopSound != null && breathingLoopSource.isPlaying && !breathingEndSource.isPlaying) { ResetSound(); breathingEndSource.Play(); }
            }
        }
    }



    public void ResetSound()
    {
        //breathingThreshold = 0f;
        if (breathingEndSource != null && breathingEndSource.isPlaying) { breathingEndSource.Stop(); }
        if (breathingLoopSource != null && breathingLoopSource.isPlaying) { breathingLoopSource.Stop(); }
    }



    //-------------------------------------------------CENTER OF GRAVITY CALCULATION
    void ProcessCOG()
    {
        //COLLECT WEIGHT DATA
        emptyWeight = controller.emptyWeight; totalWeight = emptyWeight; fuelLoad = componentLoad = 0f;munitionCount = 0;munitionLoad = 0;

        //PROCESS
        centerOfMass = aircraft.transform.TransformDirection(emptyCenterOfMass.position) * emptyWeight;

        //-------------FUEL EFFECT
        if (controller.fuelTanks.Length > 0)
        {
            foreach (SilantroFuelTank tank in controller.fuelTanks)
            {
                if (tank != null)
                {
                    Vector3 tankPosition = tank.transform.position; fuelLoad += tank.CurrentAmount; totalWeight += tank.CurrentAmount;
                    centerOfMass += aircraft.transform.TransformDirection(tankPosition) * tank.CurrentAmount;
                }
            }
        }

        //--------------LOAD EFFECT
        if (controller.payload.Length > 0)
        {
            foreach (SilantroPayload component in controller.payload)
            {
                if (component != null)
                {
                    Vector3 loadPosition = component.transform.position; componentLoad += component.weight; totalWeight += component.weight;
                    centerOfMass += aircraft.transform.TransformDirection(loadPosition) * component.weight;
                }
            }
        }



        //--------------MUNITION EFFECT
        if (controller.Armaments != null && controller.Armaments.munitions != null)
        {
            foreach (SilantroMunition munition in controller.Armaments.munitions)
            {
                if (munition != null)
                {
                    Vector3 loadPosition = munition.transform.position; munitionLoad += munition.munitionWeight; totalWeight += munition.munitionWeight;
                    centerOfMass += aircraft.transform.TransformDirection(loadPosition) * munition.munitionWeight;
                    munitionCount += 1;
                }
            }
        }


        //---------------APPLY
        if (totalWeight > 0) { centerOfMass /= (totalWeight); } else { centerOfMass /= emptyWeight; }
        if (functionality == SystemType.Advanced)
        {
            currentCOM.position = aircraft.transform.InverseTransformDirection(centerOfMass);
            aircraft.centerOfMass = currentCOM.localPosition;
        }
        else
        {
            aircraft.centerOfMass = this.transform.localPosition;
        }



        // ----------------------------- Weight
        controller.currentWeight = totalWeight;
        aircraft.mass = totalWeight;
    }





    //-----------------------------------------------DRAW COG POSITION
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; if (emptyCenterOfMass != null) { Gizmos.DrawSphere(emptyCenterOfMass.position, 0.2f); Gizmos.DrawLine(emptyCenterOfMass.position, (emptyCenterOfMass.transform.up * 3f + emptyCenterOfMass.position)); }
        if (emptyCenterOfMass == null && currentCOM == null) { Gizmos.DrawSphere(this.transform.position, 0.2f); Gizmos.DrawLine(this.transform.position, (this.transform.transform.up * 3f + this.transform.position)); }
        Gizmos.color = Color.red; if (currentCOM != null) { Gizmos.DrawSphere(currentCOM.position, 0.2f); Gizmos.DrawLine(currentCOM.position, (currentCOM.transform.up * 3f + currentCOM.position)); }
    }
}
