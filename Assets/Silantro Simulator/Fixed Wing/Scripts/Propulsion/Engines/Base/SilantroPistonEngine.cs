using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using Silantro;


[HelpURL("https://youtu.be/8pDn_PxCPjI")]
public class SilantroPistonEngine : MonoBehaviour
{
    //--------------------------------------- Selectibles
    public enum CarburatorType { RAECorrected, SUCarburettor }
    public CarburatorType carburettorType = CarburatorType.RAECorrected;
    public enum DisplacementUnit { Liter, CubicMeter, CubicInch, CubicCentimeter, CubicFoot }
    public DisplacementUnit displacementUnit = DisplacementUnit.Liter;


    //--------------------------------------- Connections
    public Transform exitPoint;
    public SilantroCore computer;
    public SilantroController controller;
    public Rigidbody connectedAircraft;
    public SilantroEngineCore core;
    public bool initialized; public bool evaluate;



    /// <summary>
    /// For testing purposes only
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        if (evaluate) { InitializeEngine(); }
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void ReturnIgnitionCall()
    {
        StartCoroutine(ReturnIgnition());
    }
    public IEnumerator ReturnIgnition() { yield return new WaitForSeconds(0.5f); core.start = false; core.shutdown = false; }




    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeEngine()
    {
        // --------------------------------- CHECK SYSTEMS
        _checkPrerequisites();


        if (allOk)
        {
            // --------------------------------- Run Core
            core.engine = this.transform;
            core.controller = controller;
            core.engineType = SilantroEngineCore.EngineType.Piston;
            core.piston = GetComponent<SilantroPistonEngine>();
            core.InitializeEngineCore();

            initialized = true;

            if (displacementUnit == DisplacementUnit.CubicCentimeter) { actualDisplacement = displacement / 1000000; }
            if (displacementUnit == DisplacementUnit.CubicFoot) { actualDisplacement = displacement / 35.315f; }
            if (displacementUnit == DisplacementUnit.CubicInch) { actualDisplacement = displacement / 61023.744f; }
            if (displacementUnit == DisplacementUnit.CubicMeter) { actualDisplacement = displacement; }
            if (displacementUnit == DisplacementUnit.Liter) { actualDisplacement = displacement / 1000; }
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    bool allOk;
    protected void _checkPrerequisites()
    {
        //CHECK COMPONENTS
        if (computer != null && connectedAircraft != null)
        {
            allOk = true;
        }
        else if (computer == null)
        {
            Debug.LogError("Prerequisites not met on Engine " + transform.name + "....Core not connected");
            allOk = false; return;
        }
        else if (connectedAircraft == null)
        {
            Debug.LogError("Prerequisites not met on Engine " + transform.name + "....Aircraft not connected");
            allOk = false; return;
        }

        if (core.ignitionExterior != null && core.backIdle != null && core.shutdownExterior != null) { } else { Debug.LogError("Prerequisites not met on Engine " + transform.name + "....sound clips not assigned properly"); allOk = false; return; }
    }





    //------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (displacementUnit == DisplacementUnit.CubicCentimeter) { actualDisplacement = displacement / 1000000; }
        if (displacementUnit == DisplacementUnit.CubicFoot) { actualDisplacement = displacement / 35.315f; }
        if (displacementUnit == DisplacementUnit.CubicInch) { actualDisplacement = displacement / 61023.744f; }
        if (displacementUnit == DisplacementUnit.CubicMeter) { actualDisplacement = displacement; }
        if (displacementUnit == DisplacementUnit.Liter) { actualDisplacement = displacement / 1000; }
    }
#endif





    //------------------------------------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        if (initialized)
        {
            if (controller.view != null) { core.cameraSector = controller.view.CalculateCameraAngle(transform); }


            // ----------------- //Core
            core.UpdateCore();

            // ----------------- //Power
            AnalyseThermodynamics();

            // ----------------- //Carb Type
            if(carburettorType == CarburatorType.SUCarburettor) { AnalyseCarburator(); }
        }
    }




    //------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseCarburator()
    {
        if (controller.core.gForce < 0) {
            core.gFactor = Mathf.Lerp(core.gFactor, 0f, Time.deltaTime * 0.25f);
            if (core.gFactor <= 0.45f) { core.ShutDownEngine(); }
        }
        else { core.gFactor = Mathf.Lerp(core.gFactor, 1f, Time.deltaTime * 0.8f); }
    }










    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //------------------------------------------------------THERMODYNAMICS


    public float stroke = 5;
    public float bore = 6;
    public float displacement = 1000, actualDisplacement;
    public float compressionRatio = 10;
    [Range(0, 15)] public float residue = 4f;
    public int numberOfCylinders = 1;
    [Range(40, 90)] public float nm = 80f;
    public float brakePower;
    public bool showPerformance;



    //-----------------------------VARIABLES
    public float Pa, P02, P03, P04;
    public float Ta, T02, T03, T04;
    public float Ue, Te;
    public float mm, me, ma, mf;
    public float V1, V2, V3, vc, vd;
    public float Qin, W3_4, W1_2, Wnet, Wb, Pb;
    public float PSFC, AF = 15f, Q;
    public float Ma, Mf;


    //--------------------------------------------------------------ENGINE THERMAL ANALYSIS
    void AnalyseThermodynamics()
    {
        //-------------------------------------- AMBIENT
        Pa = computer.ambientPressure;
        Ta = computer.ambientTemperature + 273.5f;
        Q = controller.combustionEnergy;



        //-------------------------------------- STAGE 1
        vd = actualDisplacement / numberOfCylinders;
        vc = vd / (compressionRatio - 1);
        V1 = vc + vd;
        mm = (Pa * 1000 * V1) / (287f * Ta);


        //-------------------------------------- STAGE 2
        float supremeRatio = compressionRatio * core.controlInput;
        P02 = Pa * Mathf.Pow(supremeRatio, 1.35f);
        T02 = Ta * Mathf.Pow(supremeRatio, 0.35f);
        V2 = V1 / compressionRatio;

        me = (residue / 100) * mm;
        mf = ((mm - me) / (AF + 1)) * core.controlInput;
        ma = AF * mf;


        //-------------------------------------- STAGE 3
        Qin = mf * Q;
        T03 = (Qin / (mm * 0.821f)) + T02;
        V3 = V1;
        P03 = P02 * (T03 / T02);


        //-------------------------------------- STAGE 4
        P04 = P03 * Mathf.Pow((1 / supremeRatio), 1.35f);
        T04 = T03 * Mathf.Pow((1 / supremeRatio), 0.35f);
        W3_4 = (mm * 0.287f * (T04 - T03)) / (-0.35f);
        W1_2 = (mm * 0.287f * (T02 - Ta)) / (-0.35f);
        Wnet = W3_4 + W1_2;


        //-------------------------------------- OUTPUT
        Wb = (nm / 100) * Wnet;
        float pb = (Wb * (core.coreRPM / 60f) * 0.5f) * numberOfCylinders;
        if(!float.IsNaN(pb) && !float.IsInfinity(pb)) { Pb = pb; }
        core.powerInput = Pb;
        brakePower = (Pb * 1000) / 745.6f;
        if (brakePower < 0) { brakePower = 0; }

        Mf = (mf * (core.coreRPM / 60) * 0.5f) * numberOfCylinders;
        Ma = (ma * (core.coreRPM / 60) * 0.5f) * numberOfCylinders;
        AF = (Ma * 3600) / (numberOfCylinders * 15.56f);
        AF = Mathf.Clamp(AF, 10, 20);
        if (brakePower > 0) { PSFC = (Mf * 3600f * 2.2046f) / (brakePower); }
        Ue = Mathf.Sqrt(1.4f * 287f * T04) * 0.5f;
        //Te = T04 - 273.15f;

        float omega = core.coreRPM * 0.1047f;
        float massFlow = mf + (computer.airDensity * 0.5f * actualDisplacement * omega);
        float specHeat = 1300;
        float corr = 1.0f / (Mathf.Pow(compressionRatio, 0.4f) - 1.0f);
        Te = corr * (brakePower * 1.1f) / (massFlow * specHeat);
        // if (Te < temp) Te = temp;
    }
}
