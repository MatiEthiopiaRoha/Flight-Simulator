using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using Oyedoyin;


[HelpURL("https://youtu.be/B3mOHT8lsXM")]
public class SilantroTurboProp : MonoBehaviour
{
    //--------------------------------------- Selectibles
    public enum IntakeShape { Rectangular, Circular, Oval }
    public IntakeShape intakeType = IntakeShape.Circular;


    //--------------------------------------- Connections
    public Transform intakePoint, exitPoint;
    public SilantroCore computer;
    public SilantroController controller;
    public Rigidbody connectedAircraft;
    public SilantroEngineCore core;

    public bool initialized; public bool evaluate;
    float inletDiameter, exhaustDiameter;






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
            core.engineType = SilantroEngineCore.EngineType.TurboProp;
            core.turboProp = GetComponent<SilantroTurboProp>();
            core.InitializeEngineCore();

            // --------------------------------- Calculate Engine Areas
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, exhaustPercentage, out inletDiameter, out exhaustDiameter); di = inletDiameter;
            inletArea = (Mathf.PI * inletDiameter * inletDiameter) / 4f; exhaustArea = (Mathf.PI * exhaustDiameter * exhaustDiameter) / 4f;

            // --------------------------------- Plot Factors
            pressureFactor = MathBase.DrawPressureFactor();
            adiabaticFactor = MathBase.DrawAdiabaticConstant();
            initialized = true;

            if (intakeType == IntakeShape.Circular) { intakeFactor = 0.431f; }
            else if (intakeType == IntakeShape.Oval) { intakeFactor = 0.395f; }
            else if (intakeType == IntakeShape.Rectangular) { intakeFactor = 0.32f; }
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




    public float diffuserDrawDiameter, exhaustDrawDiameter;
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
    public float inletArea, di, exhaustArea, intakeFactor = 0.1f, Ma, Uc;


    //-----------------------------CURVES
    public AnimationCurve pressureFactor, adiabaticFactor;

    //-----------------------------VARIABLES
    public float Pa, P02, P03, P04, P05, P06, Pc, PPC;
    public float Ta, T02, T03, T04, T05, T06;
    public float γa, γ1, γ2, γ3, γ4, γ5;
    public float cpa, cp1, cp2, cp3, cp4, cp5;
    public float Ue, Te, Ae, Me;
    public float πc, ρa;
    public float mf, ma, f, Q, TIT = 1000f;
    [Range(70, 95f)] public float nd = 92f;
    [Range(85, 99f)] public float nc = 95f;
    [Range(97, 100f)] public float nb = 98f;
    [Range(90, 100f)] public float nt = 97f;
    [Range(90, 100f)] public float nab = 92f;
    [Range(95, 98f)] public float nn = 96f;
    [Range(50, 90f)] public float ng = 90f;
    public float Wc, alpha, Wt, Wshaft, Pshaft, Tj, Pt, brakePower, Hc;
    [Range(0, 15)] public float pcc = 6f;
    public float PSFC;


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
        ma = ρa * va * inletArea * intakeFactor;

        //1. ----------------------------------- DIFFUSER
        γ1 = γa; cp1 = cpa;
        T02 = Ta + ((Uc * Uc) / (2 * (cp1 * 1000)));
        float p0 = 1 + ((nd / 100f) * ((T02 - Ta) / Ta));
        P02 = Pa * Mathf.Pow(p0, (γ1 / (γ1 - 1f)));


        //2. ----------------------------------- COMPRESSOR
        γ2 = adiabaticFactor.Evaluate(T02);
        cp2 = pressureFactor.Evaluate(T02);
        P03 = P02 * πc * core.coreFactor;
        T03 = T02 * (1 + ((Mathf.Pow((πc * core.coreFactor), ((γ2 - 1) / γ2))) - 1) / (nc / 100));
        Wc = (cp2 * 1000f * (T03 - T02)) / (nc / 100f);


        //3. ----------------------------------- COMBUSTION CHAMBER
        P04 = (1 - (pcc / 100)) * P03;
        T04 = TIT;
        γ3 = adiabaticFactor.Evaluate(T04);
        cp3 = pressureFactor.Evaluate(T04);
        float F1 = (cp3 * T04) - (cp2 * T03);
        float F2 = ((nb / 100) * Q) - (cp3 * T04);
        f = (F1 / F2) * (core.controlInput + 0.01f);


        //4. ----------------------------------- TURBINE
        T05 = T04 - ((cp2 * (T03 - T02)) / (cp3 * (1 + f)));
        γ4 = adiabaticFactor.Evaluate(T05); cp4 = pressureFactor.Evaluate(T05);
        P05 = P04 * (Mathf.Pow((T05 / T04), (γ4 / (γ4 - 1))));
        float p6_p4 = Mathf.Pow((Pa / P04), ((γ4 - 1) / γ4));
        Hc = cp4 * 1000f * T04 * (1 - p6_p4);


        //5. ----------------------------------- NOZZLE
        float pf = (Mathf.Pow(((γ4 + 1) / 2), (γ4 / (γ4 - 1))));
        P06 = P05; Pc = P06 / pf;
        float t6 = T04 / (Mathf.Pow((P04 / Pa), ((γ4 - 1) / γ4)));
        if (!float.IsNaN(t6) && !float.IsInfinity(t6)) { T06 = t6; }
        γ5 = adiabaticFactor.Evaluate(T05);
        cp5 = pressureFactor.Evaluate(T05);


        //6. ----------------------------------- OUTPUT
        if (!float.IsNaN(T06) && !float.IsInfinity(T06) && T06 > 1)
        {
            Ue = Mathf.Sqrt(1.4f * 287f * T06) * 0.5f;
            PPC = ((P06 * 1000) / (287f * T06));
            Me = PPC * Ue * Ae;
            mf = f * ma;
            alpha = 1 - ((Mathf.Pow(Uc, 2)) / (2 * (nt / 100) * Hc));
            Wt = (nt / 100) * alpha * Hc;
            Wshaft = (Wt * (nn / 100)) - Wc;
            Pshaft = ma * Wshaft;
            Tj = ma * ((1 + f) * Ue - Uc);
            Te = T06 - 273.15f;
            Pt = ((ng / 100) * Pshaft) + (Tj / 8.5f);
            Pt /= 1000f;
            core.powerInput = Pt;
            brakePower = (Pt / 0.7457f) * core.coreFactor; if (brakePower < 0) { brakePower = 0; }
            if (brakePower > 1) { PSFC = ((mf * 3600f * 2.20462f) / (brakePower)); }
        }
    }
}
