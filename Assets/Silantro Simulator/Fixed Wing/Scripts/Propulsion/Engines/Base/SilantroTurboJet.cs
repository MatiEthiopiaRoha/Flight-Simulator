using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using Oyedoyin;

[HelpURL("https://youtu.be/wFQngghaBoE")]
public class SilantroTurboJet : MonoBehaviour
{
    //--------------------------------------- Selectibles
    public enum IntakeShape { Rectangular, Circular, Oval }
    public IntakeShape intakeType = IntakeShape.Circular;
    public enum ReheatSystem { Afterburning, noReheat }
    public ReheatSystem reheatSystem = ReheatSystem.noReheat;
    public enum ReverseThrust { Available, Absent }
    public ReverseThrust reverseThrustMode = ReverseThrust.Absent;

    //--------------------------------------- Connections
    public Transform intakePoint, exitPoint;
    public SilantroController controller;
    public SilantroCore computer;
    public Rigidbody connectedAircraft;
    public SilantroEngineCore core;
    float inletDiameter, exhaustDiameter;

    public bool initialized; public bool evaluate;



    /// <summary>
    /// For testing purposes only
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        if (evaluate) { InitializeEngine(); }
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
            allOk = false;
        }
        else if (connectedAircraft == null)
        {
            Debug.LogError("Prerequisites not met on Engine " + transform.name + "....Aircraft not connected");
            allOk = false;
        }
    }


    // ---------------------------------
    public Gain rollAngleGain;
    public Gain pitchAngleGain;
    public Gain yawAngleGain;

    // ---------------------------------
    public Gain altitudeGain;
    public Gain climbGain;
    public Gain turnGain;




    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void ReturnIgnitionCall()
    {
        StartCoroutine(ReturnIgnition());
    }
    public IEnumerator ReturnIgnition() { yield return new WaitForSeconds(0.5f); core.start = false; core.shutdown = false; }







    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //AFTERBURNER CONTROL
    public void ToggleAfterburner()
    {
        if (reheatSystem == ReheatSystem.Afterburning && core.corePower > 0.5f && core.controlInput > 0.5f) { core.afterburnerOperative = !core.afterburnerOperative; }
    }

    public void EngageAfterburner() { if (reheatSystem == ReheatSystem.Afterburning && core.corePower > 0.5f && core.controlInput > 0.5f && !core.reverseThrustEngaged) { core.afterburnerOperative = true; } }
    public void DisEngageAfterburner() { if (core.afterburnerOperative) { core.afterburnerOperative = false; } }










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
            core.engineType = SilantroEngineCore.EngineType.Turbojet;
            if (reheatSystem == ReheatSystem.Afterburning) { core.canUseAfterburner = true; }
            if (reverseThrustMode == ReverseThrust.Available) { core.reverseThrustAvailable = true; }
            core.turboJet = GetComponent<SilantroTurboJet>();
            core.intakeFan = intakePoint;
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








   
    //------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Collect Diameters
        MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, exhaustPercentage, out diffuserDrawDiameter, out exhaustDrawDiameter);

        // Draw
        Handles.color = Color.red;
        if (exitPoint != null)
        {
            Handles.DrawWireDisc(exitPoint.position, exitPoint.transform.forward, (exhaustDrawDiameter / 2f));
            Handles.color = Color.yellow; Handles.ArrowHandleCap(0, exitPoint.position, exitPoint.rotation * Quaternion.LookRotation(-Vector3.forward), 0.3f, EventType.Repaint);
        }
        Handles.color = Color.blue;
        if (intakePoint != null) { Handles.DrawWireDisc(intakePoint.position, intakePoint.transform.forward, (diffuserDrawDiameter / 2f)); }

        // Plot Gas Factors
        pressureFactor = MathBase.DrawPressureFactor();
        adiabaticFactor = MathBase.DrawAdiabaticConstant();
        core.EvaluateRPMLimits();
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


            // ----------------- //Thrust
            if (engineThrust > 0 && !float.IsInfinity(engineThrust) && !float.IsNaN(engineThrust))
            {
                if (connectedAircraft != null && exitPoint != null)
                {
                    Vector3 thrustForce = exitPoint.forward * engineThrust;
                    connectedAircraft.AddForceAtPosition(thrustForce, exitPoint.position, ForceMode.Force);
                }
            }
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
    public float Pa, P02, P03, P04, P05, P06, P7, Pc, pf;
    public float Ta, T02, T03, T04, T05, T06, T7;
    public float πc = 5, ρa, ed;
    public float γa, γ1, γ2, γ3, γ4, γ5, γ6;
    public float cpa, cp1, cp2, cp3, cp4, cp5, cp6, cp7;
    public float mf, ma, f, fab, Q, TIT = 1000f, MaximumTemperature = 2000f;
    [Range(0, 15)] public float pcc = 6f, ppc, pcab = 3f;
    [Range(70, 95f)] public float nd = 92f;
    [Range(85, 99f)] public float nc = 95f;
    [Range(97, 100f)] public float nb = 98f;
    [Range(90, 100f)] public float nt = 97f;
    [Range(90, 100f)] public float nab = 92f;
    [Range(95, 98f)] public float nn = 96f;
    public float Ue, Te, Aeb, Ae, Me;
    public float coreThrust, pressureThrust, engineThrust, TSFC;
    public float diffuserDrawDiameter, exhaustDrawDiameter;
    public float baseThrust, maxThrust; float baseMf, maxMf;

    //--------------------------------------------------------------ENGINE THERMAL ANALYSIS
    void AnalyseThermodynamics()
    {

        //-------------------------------------- AMBIENT
        Ae = Aeb = exhaustArea;
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
        T02 = Ta * (1 + (((γ1 - 1) / 2) * Ma * Ma));
        float p0 = 1 + (0.5f * Ma * Ma * (nd / 100f) * (γ1 - 1));
        P02 = Pa * Mathf.Pow(p0, (γ1 / (γ1 - 1f)));


        //2. ----------------------------------- COMPRESSOR
        γ2 = adiabaticFactor.Evaluate(T02);
        cp2 = pressureFactor.Evaluate(T02);
        P03 = P02 * πc * core.coreFactor;
        T03 = T02 * (1 + ((Mathf.Pow((πc * core.coreFactor), ((γ2 - 1) / γ2))) - 1) / (nc / 100));


        //3. ----------------------------------- COMBUSTION CHAMBER
        P04 = (1 - (pcc / 100)) * P03;
        T04 = TIT;
        γ3 = adiabaticFactor.Evaluate(T04);
        cp3 = pressureFactor.Evaluate(T04);
        float F1 = (cp3 * T04) - (cp2 * T03);
        float F2 = ((nb / 100) * Q) - (cp3 * T04);
        f = (F1 / F2) * (core.controlInput + 0.01f); fab = 0;


        //4. ----------------------------------- TURBINE
        T05 = T04 - ((cp2 * (T03 - T02)) / (cp3 * (1 + f)));
        γ4 = adiabaticFactor.Evaluate(T05);
        cp4 = pressureFactor.Evaluate(T05);
        float p5_4 = 1 - ((T04 - T05) / ((nt / 100) * T04));
        P05 = P04 * (Mathf.Pow(p5_4, (γ4 / (γ4 - 1))));


        //5. ----------------------------------- NOZZLE
        pf = (Mathf.Pow(((γ4 + 1) / 2), (γ4 / (γ4 - 1))));
        P06 = P05; Pc = P06 / pf; T06 = T05;
        γ5 = adiabaticFactor.Evaluate(T05);
        cp5 = pressureFactor.Evaluate(T05);

        // ------ Check if Chocked
        if (Pc >= Pa) { P7 = Pc * core.coreFactor; }
        else
        {
            P7 = Pa;
            float T7Factor1 = (2 * cp5 * T06);
            float T7factor2 = (1 - ((Mathf.Pow((Pa / P06), (γ5 - 1) / γ5))));
            Ue = Mathf.Sqrt(T7Factor1 * T7factor2);
        }
        T7 = (T06 / ((γ5 + 1) / 2f)) * core.coreFactor;
        mf = (f) * ma;
        Ue = Mathf.Sqrt(1.4f * 287f * T7);



        //7. ----------------------------------- AFTERBURNER
        if (core.afterburnerOperative)
        {
            P06 = (1 - (pcab / 100)) * P05;
            T06 = MaximumTemperature;
            γ6 = adiabaticFactor.Evaluate(T06);
            cp6 = pressureFactor.Evaluate(T06);
            fab = ((cp5 * (T06 - T05)) / (((nab / 100) * Q) - (cp5 * T06)));


            //CHECK IF CHOCKED
            if (Pc >= Pa)
            {
                P7 = Pc;
                T7 = T06 / ((γ6 + 1) / 2f);
                cp7 = pressureFactor.Evaluate(T7);
                Ue = Mathf.Sqrt(1.4f * 287f * T7);
            }
            else
            {
                P7 = Pa;
                float T7Factor1 = (2 * cp6 * T06);
                float T7factor2 = (1 - ((Mathf.Pow((Pa / P06), (γ6 - 1) / γ6))));
                Ue = Mathf.Sqrt(T7Factor1 * T7factor2);
            }
            Aeb = (287f * T7 * ma * (1 + f + fab)) / (P7 * 1000 * Ue);
            if (Aeb < Ae) { Aeb = Ae; }
            if (Aeb > 0) { ed = Mathf.Sqrt((Aeb * 4f) / (3.142f)); }
            mf = ma * (f + fab);
        }



        //8. ----------------------------------- OUTPUT
        if (T7 > 0 && !float.IsInfinity(T7) && !float.IsNaN(T7)) { Te = T7 - 273.15f; }
        if (T7 > 0 && !float.IsInfinity(T7) && !float.IsNaN(T7))
        {
            cp6 = pressureFactor.Evaluate(T7);
            if (P7 > 0 && !float.IsInfinity(P7) && !float.IsNaN(P7)) { ppc = ((P7 * 1000) / (287f * T7)); }
        }
        if (ppc > 0 && !float.IsInfinity(ppc) && !float.IsNaN(ppc)) { Me = ppc * Ue * Ae; }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        if (!core.afterburnerOperative)
        {
            float ct = (((1 + f) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
            float ptt = (Ae * ((P7 - Pa) * 1000)); if (ptt > 0 && !float.IsInfinity(ptt) && !float.IsNaN(ptt)) { pressureThrust = ptt; }
            baseThrust = (coreThrust + pressureThrust); baseMf = ma * (f);
        }
        else
        {
            float ct = (((1 + f + fab) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
            float ptt = (Aeb * ((P7 - Pa) * 1000)); if (ptt > 0 && !float.IsInfinity(ptt) && !float.IsNaN(ptt)) { pressureThrust = ptt; }
            maxThrust = (coreThrust + pressureThrust); maxMf = ma * (f + fab);
        }

        engineThrust = (baseThrust) + (maxThrust - baseThrust) * core.burnerFactor;
        if (engineThrust < 0) { engineThrust = 0; } mf = baseMf + (maxMf - baseMf) * core.burnerFactor;
        if (engineThrust > (controller.currentWeight * 9.8f * 1.5f)) { engineThrust = (controller.currentWeight * 9.8f); }


        float pt = engineThrust * 0.2248f;
        if (pt > 0 && !float.IsInfinity(pt) && !float.IsNaN(pt)) { TSFC = ((mf * 3600f) / (pt * 0.4536f)); }
        if (core.afterburnerOperative && core.controlInput < 0.5f) { core.afterburnerOperative = false; }
    }
}
