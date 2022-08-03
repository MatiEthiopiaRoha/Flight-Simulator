
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Oyedoyin;



public class SilantroTurboFan : MonoBehaviour
{
    //--------------------------------------- Selectibles
    public enum EngineType { Unmixed, Mixed }
    public EngineType engineType;
    public enum IntakeShape { Rectangular, Circular, Oval }
    public IntakeShape intakeType = IntakeShape.Circular;
    public enum ReheatSystem { Afterburning, noReheat }
    public ReheatSystem reheatSystem = ReheatSystem.noReheat;
    public enum ReverseThrust { Available, Absent }
    public ReverseThrust reverseThrustMode = ReverseThrust.Absent;

    //--------------------------------------- Connections
    public Transform intakePoint, exitPoint;
    public Transform fanExhaustPoint;
    public SilantroController controller;
    public SilantroCore computer;
    public Rigidbody connectedAircraft;
    public SilantroEngineCore core;
    public bool initialized; public bool evaluate;
    float cEd, inletDiameter, coreExhaustDiameter, fanExhaustDiameter, nullFloat;





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
    public void InitializeEngine()
    {
        // --------------------------------- CHECK SYSTEMS
        _checkPrerequisites();


        if (allOk)
        {
            // --------------------------------- Run Core
            core.engine = this.transform;
            core.controller = controller;
            core.engineType = SilantroEngineCore.EngineType.Turbofan;
            if (reheatSystem == ReheatSystem.Afterburning && engineType == EngineType.Mixed) { core.canUseAfterburner = true; }
            if (reverseThrustMode == ReverseThrust.Available) { core.reverseThrustAvailable = true; }
            core.turboFan = GetComponent<SilantroTurboFan>();
            core.intakeFan = intakePoint;
            core.InitializeEngineCore();

            // --------------------------------- Calculate Engine Areas
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, coreExhaustPercentage, out inletDiameter, out coreExhaustDiameter);
            MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, fanExhaustPercentage, out nullFloat, out fanExhaustDiameter); di = inletDiameter;
            inletArea = (Mathf.PI * inletDiameter * inletDiameter) / 4f; cEd = coreExhaustDiameter;
            coreExhaustArea = (Mathf.PI * coreExhaustDiameter * coreExhaustDiameter) / 4f;
            fanExhaustArea = (Mathf.PI * fanExhaustDiameter * fanExhaustDiameter) / 4f;

            // --------------------------------- Plot Factors
            pressureFactor = MathBase.DrawPressureFactor();
            adiabaticFactor = MathBase.DrawAdiabaticConstant();
            initialized = true;

            if (intakeType == IntakeShape.Circular) { intakeFactor = 0.431f; }
            else if (intakeType == IntakeShape.Oval) { intakeFactor = 0.395f; }
            else if (intakeType == IntakeShape.Rectangular) { intakeFactor = 0.32f; }
        }
    }











    public float diffuserDrawDiameter, fanExhaustDrawDiameter, coreExhaustDrawDiameter;
    //------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Collect Diameters
        MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, coreExhaustPercentage, out diffuserDrawDiameter, out coreExhaustDrawDiameter);
        MathBase.AnalyseEngineDimensions(engineDiameter, intakePercentage, fanExhaustPercentage, out nullFloat, out fanExhaustDrawDiameter);

        // Draw
        Handles.color = Color.red;
        if (exitPoint != null)
        {
            Handles.DrawWireDisc(exitPoint.position, exitPoint.transform.forward, (coreExhaustDrawDiameter / 2f));
            Handles.color = Color.yellow; Handles.ArrowHandleCap(0, exitPoint.position, exitPoint.rotation * Quaternion.LookRotation(-Vector3.forward), 0.3f, EventType.Repaint);
        }
        Handles.color = Color.blue;
        if (intakePoint != null) { Handles.DrawWireDisc(intakePoint.position, intakePoint.transform.forward, (diffuserDrawDiameter / 2f)); }

        if (engineType == EngineType.Unmixed)
        {
            Handles.color = Color.cyan;
            if (fanExhaustPoint != null) { Handles.DrawWireDisc(fanExhaustPoint.position, fanExhaustPoint.transform.forward, (fanExhaustDrawDiameter / 2f)); }
        }

        // Plot Gas Factors
        pressureFactor = MathBase.DrawPressureFactor();
        adiabaticFactor = MathBase.DrawAdiabaticConstant();
        core.EvaluateRPMLimits();
    }
#endif


    private void LateUpdate()
    {
        if (controller != null && controller.view != null) { core.cameraSector = controller.view.CalculateCameraAngle(transform); }
    }



    public float liftFactor = 1f; float thrustFactor = 1f;
    //------------------------------------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        if (initialized)
        {
            // ----------------- //Core
            core.UpdateCore();

            // ----------------- //Power
            AnalyseThermodynamics();

            if (reverseThrustMode == ReverseThrust.Available && core.reverseThrustEngaged) { thrustFactor = -1f * core.reverseThrustFactor * core.reverseThrustPercentage / 100f; }
            else { thrustFactor = 1f; }


            // ----------------- //Thrust
            if (engineThrust > 0 && !float.IsInfinity(engineThrust) && !float.IsNaN(engineThrust))
            {
                if (connectedAircraft != null && exitPoint != null)
                {
                    Vector3 thrustForce = exitPoint.forward * engineThrust * thrustFactor * liftFactor;
                    connectedAircraft.AddForceAtPosition(thrustForce, exitPoint.position,ForceMode.Force);
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
    [Range(0, 100f)] public float coreExhaustPercentage = 90f, fanExhaustPercentage = 90f;
    public float inletArea, di, coreExhaustArea, fanExhaustArea, intakeFactor = 0.1f, Ma, Uc;


    //-----------------------------CURVES
    public AnimationCurve pressureFactor, adiabaticFactor;



    //-----------------------------VARIABLES
    public float Pa, P02, P03, P04, P05, P06, P7, P08, P09, P10, Pc, pf;
    public float Ta, T02, T03, T04, T05, T06, T7, T08, T09, T10;
    public float πc = 10f, β = 0.5f, ρa, ed;
    [Range(1f, 3f)]public float πf = 1f;
    public float γa, γ1, γ2, γ3, γ4, γ5, γ6, γ7, γ8, γ9;
    public float cpa, cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9;
    public float mf, ma, mh, f, fab, Q, TIT = 1000f, MaximumTemperature = 2000f;
    public float ppc;
    [Range(70, 95f)] public float nd = 92f;
    [Range(90, 99f)] public float nf = 95f;
    [Range(85, 99f)] public float nc = 95f;
    [Range(97, 100f)] public float nb = 98f;
    [Range(90, 100f)] public float nhpt = 97f;
    [Range(90, 100f)] public float nab = 92f;
    [Range(95, 98f)] public float nn = 96f;
    [Range(90, 100f)] public float nlpt = 97f;
    [Range(0, 15f)] public float pcc = 6f;
    [Range(0, 15f)] public float pcab = 3f;
    public float Ue, Te, Aeb, Ae, Me;
    public float coreThrust, pressureThrust, engineThrust, TSFC, fanThrust;
    public float cAe, fAe, Wc, Uef;
    public float fanPressureThrust, fanCoreThrust, coreCoreThrust, corePressureThrust;
    public float pcc8b, pcc8a, P8c, p6cp5, p7c, p7cp6;
    public bool showPerformance;
    public float baseThrust, maxThrust; float baseMf, maxMf;


    //--------------------------------------------------------------ENGINE THERMAL ANALYSIS
    void AnalyseThermodynamics()
    {

        //-------------------------------------- AMBIENT
        if (engineType == EngineType.Mixed) { Ae = Aeb = coreExhaustArea; } else { cAe = Aeb = coreExhaustArea; fAe = fanExhaustArea; }
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

        if (engineType == EngineType.Mixed)
        {
            //2. ----------------------------------- FAN
            γ2 = adiabaticFactor.Evaluate(T02);
            cp2 = pressureFactor.Evaluate(T02);
            P03 = P02 * πf * core.coreFactor + 0.01f;
            T03 = T02 * (1 + ((Mathf.Pow((πf * core.coreFactor), ((γ2 - 1) / γ2))) - 1) / (nc / 100));


            //3. ----------------------------------- COMPRESSOR
            P04 = P03 * πc;
            float t4_t3 = Mathf.Pow((πc), ((γ2 - 1) / γ2));
            T04 = T03 * (1 + ((t4_t3 - 1) / (nc / 100)));
            γ3 = adiabaticFactor.Evaluate(T04); cp3 = pressureFactor.Evaluate(T04);


            //4. ----------------------------------- COMBUSTION CHAMBER
            P05 = (1 - (pcc / 100)) * P04;
            T05 = TIT;
            γ4 = adiabaticFactor.Evaluate(T04);
            cp4 = pressureFactor.Evaluate(T04);
            float F1 = (cp4 * T05) - (cp3 * T04);
            float F2 = ((nb / 100) * Q) - (cp3 * T05);
            f = (F1 / F2) * (core.controlInput + 0.01f); fab = 0;


            //5. ----------------------------------- LOW PRESSURE TURBINE
            float p6A = cp3 * (T04 - T03);
            float p6B = cp4 * (1 + f);
            T06 = T05 - (p6A / p6B);
            float p6_p5 = 1 - (1 - (T06 / T05)) * (1 / (nlpt / 100));
            P06 = P05 * Mathf.Pow(p6_p5, (γ4 / (γ4 - 1)));


            //6. ----------------------------------- HIGH PRESSURE TURBINE
            γ5 = adiabaticFactor.Evaluate(T06);
            cp5 = pressureFactor.Evaluate(T06);
            P7 = P03;
            float p7_p6 = Mathf.Pow((P7 / P06), ((γ5 - 1) / γ5));
            T7 = T06 * (1 - ((nhpt / 100) * (1 - p7_p6)));
            Wc = ((cp5 * (T06 - T7)) / (nhpt / 100f) * ma) / 745f;


            //7. ----------------------------------- NOZZLE
            if (T7 > 0 && T7 < 10000) { γ6 = adiabaticFactor.Evaluate(T7); cp6 = pressureFactor.Evaluate(T7); }
            P08 = P03;
            float t8A = (β * cp5 * T03) + ((1 + f) * cp6 * T7);
            float t8B = ((1 + f) + β) * cp6;
            T08 = t8A / t8B;
            if (T08 > 0 && T08 < 10000) { γ7 = adiabaticFactor.Evaluate(T08); cp7 = pressureFactor.Evaluate(T08); }
            float pcc8 = 1 / (1 - (1 / (nn / 100)) * ((γ7 - 1) / (γ7 + 1)));
            float pc_p8 = Mathf.Pow(pcc8, (γ7 / (γ7 - 1)));
            Pc = P08 / (pc_p8);

            //float Aea;

            // ------ Check if Chocked
            float p8_pa = P08 / Pa;
            float p8_pc = P08 / Pc;
            if (p8_pa > p8_pc)
            {
                P09 = Pc;
                T09 = T08 / ((γ7 + 1) / 2);
                float p9 = (P09 * 1000) / (287 * T09);
                γ8 = adiabaticFactor.Evaluate(T09);
                Ue = Mathf.Sqrt(γ8 * 287 * T09) * core.coreFactor;
                Me = p9 * Ue * Ae;
                //Aea = (Ma * (1 + f)) / (p9 * Ue);
            }
            else
            {
                P09 = Pa;
                T09 = T08 / ((γ7 + 1) / 2);//T9 = T8 * Mathf.Pow ((P8 / Pa), a);
                float p9 = (P09 * 1000) / (287 * T09);
                Ue = Mathf.Sqrt(2f * cp7 * 1000f * (T08 - T09)) * core.coreFactor;
                Me = p9 * Ue * Ae;
                //Aea = (Ma * (1 + f)) / (P09 * Ue);
            }

            Te = (T08 - 273.15f) * core.coreFactor;
            mf = (f) * ma;


            //8. ----------------------------------- AFTERBURNER
            if (core.afterburnerOperative)
            {
                P10 = (1 - (pcab / 100)) * (P06 / (1.905f * pc_p8));
                T10 = MaximumTemperature;
                γ9 = adiabaticFactor.Evaluate(T10);
                cp9 = pressureFactor.Evaluate(T10);

                fab = ((cp7 * (T10 - T7)) / (((nab / 100) * Q) - (cp7 * T10)));
                float a = (γ9 + 1) / 2;
                float t11 = T10 / a;
                Ue = Mathf.Sqrt(2f * cp9 * 1000f * (T10 - t11));
                Te = (t11 - 273.15f) * core.coreFactor;

                float Acf = (287f * t11 * Ma * (1 + f + fab)) / ((P08 / pc_p8) * 1000 * Ue);
                if (Acf > 0) { ed = Mathf.Sqrt((Acf * 4f) / (3.142f)); }
                if (ed < cEd) { ed = cEd; }


                float p9 = (P09 * 1000) / (287 * T09);
                float mc = p9 * Ue * Ae;
                Me = (mc + (ma*fab)) *core.coreFactor;
            }

           
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (!core.afterburnerOperative)
            {
                float ct = (((1 + f) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
                float ptt = Ae * (((P06 / (1.905f * pc_p8)) - Pa) * 1000); if (!float.IsInfinity(ptt) && !float.IsNaN(ptt) && ptt > 0) { pressureThrust = ptt; }
                baseThrust = (coreThrust + pressureThrust); baseMf = ma * (f);
            }
            else
            {
                float ct = (((1 + f + fab) * Ue) - (Uc)) * ma; if (ct > 0 && !float.IsInfinity(ct) && !float.IsNaN(ct)) { coreThrust = ct; }
                float ptt = (Aeb * ((P10 - Pa) * 1000)); if (!float.IsInfinity(ptt) && !float.IsNaN(ptt) && ptt > 0) { pressureThrust = ptt; }
                maxThrust = (coreThrust + pressureThrust); maxMf = ma * (f + fab);
            }
            engineThrust = (baseThrust) + (maxThrust - baseThrust) * core.burnerFactor; mf = baseMf + (maxMf - baseMf) * core.burnerFactor;
            if (engineThrust < 0) { engineThrust = 0; } if (engineThrust > (controller.currentWeight * 9.8f * 1.5f)) { engineThrust = (controller.currentWeight * 9.8f); }
        }


        // --------------------- UNMIXED
        else
        {
            //2. ----------------------------------- FAN
            mh = ma / (1 + β);
            γ2 = adiabaticFactor.Evaluate(T02);
            cp2 = pressureFactor.Evaluate(T02);
            P03 = P02 * πf * core.coreFactor;
            T03 = T02 * (1 + ((Mathf.Pow((πf), ((γ2 - 1) / γ2))) - 1) / (nc / 100));
            float pcc3a = 1 / (1 - (1 / (nn / 100)) * ((γ2 - 1) / (γ2 + 1)));
            float pcc3b = Mathf.Pow(pcc3a, (γ2 / (γ2 - 1)));



            //3. ----------------------------------- FAN NOZZLE
            Pc = P03 / pcc3b;
            if (Pc > Pa)
            {
                P09 = Pc;
                T09 = T03 / pcc3a;
                float p9 = (P09 * 1000) / (287 * T09);
                Uef = Mathf.Sqrt(γ2 * 287f * T09);
                fanCoreThrust = β * core.coreFactor * mh * (Uef - Uc);
                fanPressureThrust = fAe * (P09 - Pa) * 1000f;
            }
            else
            {
                P09 = Pa;
                float p9c = 1 - Mathf.Pow((P09 / P03), ((γ2 - 1) / γ2));
                T09 = T03 * (1 - ((nn / 100) * p9c));
                float p9 = (P09 * 1000) / (287 * T09);
                Uef = Mathf.Sqrt(γ2 * 287f * T09);
                fanCoreThrust = β * core.coreFactor * mh * (Uef - Uc);
            }

            if (fanCoreThrust > 0 && !float.IsInfinity(fanCoreThrust) && !float.IsNaN(fanCoreThrust) &&
               fanPressureThrust > 0 && !float.IsInfinity(fanPressureThrust) && !float.IsNaN(fanPressureThrust)) { fanThrust = fanCoreThrust + fanPressureThrust; }


            //4. ----------------------------------- COMPRESSOR
            P04 = P03 * πc;
            float t4_t3 = Mathf.Pow((πc), ((γ2 - 1) / γ2));
            T04 = T03 * (1 + ((t4_t3 - 1) / (nc / 100)));
            γ3 = adiabaticFactor.Evaluate(T04); cp3 = pressureFactor.Evaluate(T04);


            //5. ----------------------------------- COMBUSTION CHAMBER
            P05 = (1 - (pcc / 100)) * P04;
            T05 = TIT;
            γ4 = adiabaticFactor.Evaluate(T04);
            cp4 = pressureFactor.Evaluate(T04);
            float F1 = (cp4 * T05) - (cp3 * T04);
            float F2 = ((nb / 100) * Q) - (cp3 * T05);
            f = (F1 / F2) * (core.controlInput + 0.01f); fab = 0;


            //6. ----------------------------------- HIGH PRESSURE TURBINE
            T06 = T05 - ((cp3 * (T04 - T03)) / ((1 + f) * cp4));
            γ5 = adiabaticFactor.Evaluate(T06); cp5 = pressureFactor.Evaluate(T06);
            float p6c = 1 - ((1 / (nhpt / 100)) * (1 - (T06 / T05)));
            p6cp5 = Mathf.Pow(p6c, (γ5 / (γ5 - 1)));
            P06 = P05 * p6cp5;


            //7. ----------------------------------- LOW PRESSURE TURBINE
            float t7_t6 = ((1 + β) * cp5 * (T03 - T02)) / ((1 + f) * cp4);
            T7 = T06 - t7_t6;
            γ6 = adiabaticFactor.Evaluate(T7); cp6 = pressureFactor.Evaluate(T7) * 1000f;
            p7c = 1 - ((1 / (nlpt / 100)) * (1 - (T7 / T06)));
            p7cp6 = Mathf.Pow(p7c, (γ6 / (γ6 - 1)));
            P7 = P06 * p7cp6;
            Wc = ((cp5 * (T06 - T7)) / (nhpt / 100f) * mh) / 745f;



            //8. ----------------------------------- NOZZLE
            pcc8a = 1 / (1 - (1 / (nn / 100)) * ((γ6 - 1) / (γ6 + 1)));
            pcc8b = Mathf.Pow(pcc8a, (γ6 / (γ6 - 1)));
            P8c = P7 / pcc8b;

            if (P8c > Pa)
            {
                P08 = P8c;
                T08 = T7 / pcc8a;
                float p88 = (P08 * 1000) / (287f * T08);
                Ue = Mathf.Sqrt(1.33f * 287 * T08);
                coreCoreThrust = mh * ((1 + f) * Ue - Uc);
                corePressureThrust = cAe * (P08 - Pa) * 1000f;
            }
            else
            {
                P08 = Pa;
                float p9c = 1 - Mathf.Pow((P08 / P7), ((γ6 - 1) / γ6));
                T08 = T7 * (1 - ((nn / 100) * p9c));
                Ue = Mathf.Sqrt(1.33f * 287 * T08);
                coreCoreThrust = mh * ((1 + f) * Ue - Uc);
                corePressureThrust = 0f;
            }

            mf = ma * f;
            if (coreCoreThrust > 0 && !float.IsInfinity(coreCoreThrust) && !float.IsNaN(coreCoreThrust)
            && fanCoreThrust > 0 && !float.IsInfinity(fanCoreThrust) && !float.IsNaN(fanCoreThrust)) { coreThrust = fanCoreThrust + coreCoreThrust; }

            if (!float.IsInfinity(corePressureThrust) && !float.IsNaN(corePressureThrust)
            && !float.IsInfinity(fanPressureThrust) && !float.IsNaN(fanPressureThrust)) { pressureThrust = fanPressureThrust + corePressureThrust; }

            engineThrust = coreThrust + pressureThrust;
            if (engineThrust < 0) { engineThrust = 0; }
        }

        float pt = engineThrust * 0.2248f;
        if (pt > 1 && !float.IsInfinity(pt) && !float.IsNaN(pt)) { TSFC = ((mf * 3600f) / (pt * 0.4536f)); }
        if (core.afterburnerOperative && core.controlInput < 0.5f) { core.afterburnerOperative = false; }
    }
}






