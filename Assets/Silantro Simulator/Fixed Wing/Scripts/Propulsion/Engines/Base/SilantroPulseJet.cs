using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using Oyedoyin;

public class SilantroPulseJet : MonoBehaviour
{
    //--------------------------------------- Connections
    public Transform intakePoint, exitPoint;
    public Transform combustionEntry, combustionExit;
    public SilantroCore computer;
    public SilantroController controller;
    public Rigidbody connectedAircraft;
    public SilantroEngineCore core;
    public bool initialized; public bool evaluate;
    float diffuserDrawDiameter, exhaustDrawDiameter;

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

            combustionChamberLength = Vector3.Distance(combustionEntry.position, combustionExit.position);
            engineLength = Vector3.Distance(intakePoint.position, exitPoint.position);
            CombustionVolume = ((3.142f * CombustionDiameter * CombustionDiameter) / 4f) * combustionChamberLength;
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
        // Collect Diameters
        MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, exhaustPercentage, out diffuserDrawDiameter, out exhaustDrawDiameter);

        // Draw
        Handles.color = Color.red;
        if (exitPoint != null) { Handles.DrawWireDisc(exitPoint.position, exitPoint.transform.forward, (exhaustDrawDiameter / 2f)); }
        Handles.color = Color.blue;
        if (intakePoint != null) { Handles.DrawWireDisc(intakePoint.position, intakePoint.transform.forward, (diffuserDrawDiameter / 2f)); }
        Handles.color = Color.cyan;
        if (exitPoint != null && intakePoint != null) { Handles.DrawLine(intakePoint.transform.position, exitPoint.position); }
        Handles.color = Color.yellow;
        if (combustionEntry != null) { Handles.DrawWireDisc(combustionEntry.position, combustionEntry.transform.forward, (CombustionDiameter / 2f)); }
        if (combustionExit != null) { Handles.DrawWireDisc(combustionExit.position, combustionExit.transform.forward, (CombustionDiameter / 2f)); }
        Handles.color = Color.red;
        if (combustionExit != null && combustionEntry != null) { Handles.DrawLine(combustionExit.position, combustionEntry.position); }

        combustionChamberLength = Vector3.Distance(combustionEntry.position, combustionExit.position);
        engineLength = Vector3.Distance(intakePoint.position, exitPoint.position);
        CombustionVolume = ((3.142f * CombustionDiameter * CombustionDiameter) / 4f) * combustionChamberLength;

        // Plot Gas Factors
        pressureFactor = MathBase.DrawPressureFactor();
        adiabaticFactor = MathBase.DrawAdiabaticConstant();
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
        }
    }




    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //----------------------------------------------------------------------
    //------------------------------------------------------THERMODYNAMICS

    //----------------------------ENGINE DIMENSIONS
    public float engineDiameter = 2f;
    [Range(0, 100f)] public float intakePercentage = 90f;
    [Range(0, 100f)] public float exhaustPercentage = 90f;
    public float inletArea, di, exhaustArea, Ma, Uc;

    //-----------------------------CURVES
    public AnimationCurve pressureFactor, adiabaticFactor;

    //-----------------------------VARIABLES
    public float Pa, P02, P03;
    public float Ta, T02, T03, T04;
    public float γa, γ1, γ2, γ3;
    public float cpa, cp1, cp2, cp3;
    public float mf, ma, f, Q, TIT = 1000f;
    public float Ue, Te, Ae, Me;
    public float πc = 10, ρa;
    public float nb = 90f;
    public float engineThrust, TSFC;

    public float CombustionDiameter;
    public float CombustionPercentage = 90f;
    public float engineLength, combustionChamberLength, CombustionVolume;
    public float engineFrequency = 100f;


    //--------------------------------------------------------------ENGINE THERMAL ANALYSIS
    void AnalyseThermodynamics()
    {

        //-------------------------------------- AMBIENT
        Ae = exhaustArea;
        Pa = computer.ambientPressure;
        Ta = computer.ambientTemperature + 273.5f;
        Ma = computer.machSpeed;
        γa = adiabaticFactor.Evaluate(Ta);
        cpa = pressureFactor.Evaluate(Ta);
        Q = controller.combustionEnergy;
        Uc = computer.currentSpeed;


        //0. ----------------------------------- INLET
        float R = 287f;
        ρa = (Pa * 1000) / (R * Ta);
        float va = (3.142f * di * core.coreRPM) / 60f;
        ma = ρa * (engineFrequency * CombustionVolume) * core.coreFactor;


        //1. ----------------------------------- DIFFUSER
        γ1 = γa; cp1 = cpa;
        float T2_Ta = (γ1 - 1) / 2f;
        T02 = Ta * (1 + (T2_Ta * Ma * Ma));
        P02 = Pa * (Mathf.Pow((T02 / Ta), (γ1 / (γ1 - 1))));


        //2. ----------------------------------- COMBUSTION CHAMBER
        T03 = T02 * πc; P03 = πc * P02;
        γ2 = adiabaticFactor.Evaluate(T03);
        cp2 = pressureFactor.Evaluate(T03) * 1000f;
        float F1 = (((cp2 / 1000) * T03) - ((cp1 / 1000) * T02));
        float F2 = (((nb / 100) * Q) - ((cp2 / 1000) * T03));
        f = (F1 / F2) * (core.controlInput + 0.01f);


        //3. ----------------------------------- TAIL PIPE
        float T3_T4 = Mathf.Pow((P03 / Pa), ((γ2 - 1) / γ2));
        T04 = (T03 / T3_T4) * core.coreFactor;
        γ3 = adiabaticFactor.Evaluate(T04);
        cp3 = pressureFactor.Evaluate(T04) * 1000;
        Te = T04 - 273.15f;
        float pa_p3 = Mathf.Pow((Pa / P03), ((γ3 - 1) / γ3));
        Ue = Mathf.Sqrt(2f * cp3 * T04 * (1 - pa_p3)) * core.coreFactor;
        mf = ma * f;
        engineThrust = ma * (((1 + f) * Ue) - Uc);

        float pt = engineThrust * 0.2248f;
        if (pt > 1 && !float.IsInfinity(pt) && !float.IsNaN(pt)) { TSFC = ((mf * 3600f) / (pt * 0.4536f)); }
    }
}
