using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif





public class SilantroPropeller : MonoBehaviour
{

    //--------------------------------------- Selectibles
    public enum AnalysisMethod {InflowModel, MomentumTheory, BEM }
    public AnalysisMethod analysisMethod = AnalysisMethod.MomentumTheory;
    public enum EngineType { TurbopropEngine, PistonEngine, ElectricMotor }
    public EngineType engineType = EngineType.PistonEngine;
    public enum RotationAxis { X, Y, Z }
    public RotationAxis rotationAxis = RotationAxis.X;
    public enum RotationDirection { CW, CCW }
    public RotationDirection rotationDirection = RotationDirection.CCW;
    public enum PropellerTorqueEffect { Neglect, Consider }
    public PropellerTorqueEffect torqueEffect = PropellerTorqueEffect.Neglect;



    //--------------------------------------- Connections
    public SilantroController controller;
    public SilantroCore computer;
    public Transform Propeller;
    public SilantroTurboProp propEngine;
    public SilantroPistonEngine pistonEngine;
    public SilantroElectricMotor electricMotor;
    public Transform thrustPoint;


    //--------------------------------------- Extras
    public Material[] normalRotor;
    public Material[] blurredRotor;

    public Color blurredRotorColor;
    public Color normalRotorColor;

    public float alphaSettings;
    public enum PropellerBlendMode { None, Partial, Complete }
    public PropellerBlendMode blendMode = PropellerBlendMode.None;



    //-----------------------------VARIABLES
    public float availablePower;
    [Range(2,8)]public int Nb;//Blade Count
    public float functionalRPM = 1000f;
    public float currentRPM;
    public float Ω, J, σ;
    public float bladeDiameter = 1f;
    [Range(0,100f)]public float hubRatio;
    public float hubDiameter = 0.2f;
    [Range(5,10)]public int bladeSubdivisions;
    public float bladeArea;
    public float bladeSolidity;

    public float bladePitch = 1f;
    public float bladePitchAngle = 0f;
    public float bladeRootChord;
    [Range(0,100)]public float bladeTaperRatio = 100;

    public float tipChord, rootChord;

    public SilantroAirfoil bladeAirfoil;

    public float n;
    public float hubRadius, bladeRadius;
    public float Δr, r0, x0;
    public float Uc, ρ;
    public float Cq0 = 0, Cqi = 0, CQ, CT, a = 5.7f;
    public float discLoading, powerCorrectionFactor, inducedTorqueCorrection, CT_solidity;
    public AnimationCurve A1, A2;
    public float VT;

    // --------------------------------- Table
    public float[] r;
    public float[] x, A, B, b, C;
    public float[] c, φi, VR, βi, αi, αr;
    public float φ, β, α, λ, CL, CD;
    public float[] θr;

    public float lift, drag, thrust, torque;
    public float engineFactor;
    public float engineRPM, gearRatio, cutoutRatio;
    Vector3 torqueAxis;
    [Range(0.01f, 1)]public float normalBalance = 0.2f;




    public void InitializePropeller()
    {
        if (engineType == EngineType.PistonEngine && pistonEngine != null) { gearRatio = pistonEngine.core.functionalRPM / functionalRPM; thrustPoint = pistonEngine.exitPoint; if (analysisMethod == AnalysisMethod.InflowModel) { pistonEngine.core.torqueConnected = true; } }
        if (engineType == EngineType.TurbopropEngine && propEngine != null) { gearRatio = propEngine.core.functionalRPM / functionalRPM; thrustPoint = propEngine.exitPoint; if (analysisMethod == AnalysisMethod.InflowModel) { propEngine.core.torqueConnected = true; } }
        if (engineType == EngineType.ElectricMotor && electricMotor != null) { gearRatio = electricMotor.ratedRPM / functionalRPM; }

        // ----------------------------------- Base Dimensions
        rootChord = bladeRootChord;
        tipChord = bladeRootChord * (bladeTaperRatio / 100f);
        hubDiameter = (hubRatio / 100f) * bladeDiameter;
        hubRadius = hubDiameter / 2f;
        bladeRadius = bladeDiameter / 2f;
        bladeArea = 0.5f * bladeDiameter / 2f * (rootChord + tipChord);
        bladeSolidity = bladeArea / (3.142f * bladeRadius * bladeRadius);
        Δr = (bladeRadius - hubRadius) / bladeSubdivisions;
        cutoutRatio = hubRadius / bladeRadius;
        MathBase.DrawCorrectionCurves(out A2, out A1);

        if (blendMode == PropellerBlendMode.Complete || blendMode == PropellerBlendMode.Partial)
        {
            blurredRotorColor = blurredRotor[0].color; alphaSettings = 0;
            if (normalRotor.Length > 0) { normalRotorColor = normalRotor[0].color; }
            ProcessBlur();
        }

        //Initialize
        ShapePropeller();
        if(analysisMethod == AnalysisMethod.BEM) { analysisMethod = AnalysisMethod.MomentumTheory; }
    }





    private void FixedUpdate()
    {
        if (controller != null && computer != null)
        {
            // ----------------------------------- Collect Run Data
            n = currentRPM / 60f;
            Ω = (2 * 3.142f * currentRPM) / 60f;
            J = Uc / (Ω * bladeRadius);
            if (J <= 0.01f) { J = 0.01f; }
            Uc = computer.currentSpeed;
            ρ = computer.airDensity;
            if (bladeAirfoil != null && bladeAirfoil.centerLiftSlope > 4f) { a = bladeAirfoil.centerLiftSlope; }

            AnalyseTransmission();

            // --------------------------------- Send Data
            if (currentRPM > 0)
            {
                AnalysePropeller(); AnalyseThrust();
            }
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalysePropeller()
    {
        // ----------------------- CALCULATE AIR INDUCED RPM
        float inducedAirSpeed = 17f * controller.aircraft.velocity.magnitude;
        float omega = (inducedAirSpeed) / (3.14264f * bladeDiameter);
        float inducedRPM = (60f * omega) / (2f * 3.14265f);
        float propellerRPM = currentRPM + (inducedRPM);
        alphaSettings = currentRPM / functionalRPM;


        // ----------------------------------- Rotate
        if (Propeller != null)
        {
            if (rotationDirection == RotationDirection.CCW)
            {
                if (rotationAxis == RotationAxis.X) { Propeller.Rotate(new Vector3(propellerRPM / 2f * Time.deltaTime, 0, 0)); }
                if (rotationAxis == RotationAxis.Y) { Propeller.Rotate(new Vector3(0, propellerRPM / 2f * Time.deltaTime, 0)); }
                if (rotationAxis == RotationAxis.Z) { Propeller.Rotate(new Vector3(0, 0, propellerRPM / 2f * Time.deltaTime)); }
            }
            if (rotationDirection == RotationDirection.CW)
            {
                if (rotationAxis == RotationAxis.X) { Propeller.Rotate(new Vector3(-1f * propellerRPM / 2f * Time.deltaTime, 0, 0)); }
                if (rotationAxis == RotationAxis.Y) { Propeller.Rotate(new Vector3(0, -1f * propellerRPM / 2f * Time.deltaTime, 0)); }
                if (rotationAxis == RotationAxis.Z) { Propeller.Rotate(new Vector3(0, 0, -1f * propellerRPM / 2f * Time.deltaTime)); }
            }
        }


        // --------------------------------- Apply Torque
        if (torqueEffect == PropellerTorqueEffect.Consider)
        {
            if (rotationDirection == RotationDirection.CCW) { torqueAxis = new Vector3(0, 0, -1); }
            else if (rotationDirection == RotationDirection.CW) { torqueAxis = new Vector3(0, 0, 1); }

            Vector3 torqueEffect = torqueAxis * torque;
            if(!float.IsInfinity(torque) && !float.IsNaN(torque)) { controller.aircraft.AddTorque(torqueEffect, ForceMode.Force); }
        }


        // --------------------------------- Apply Thrust
        if (thrust > 1 && thrustPoint != null)
        {
            Vector3 thrustForce = thrustPoint.forward * thrust;
            if (!float.IsInfinity(thrust) && !float.IsNaN(thrust)) { controller.aircraft.AddForceAtPosition(thrustForce, thrustPoint.position, ForceMode.Force); }
        }

        ProcessBlur();
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void ProcessBlur()
    {
        // ---------------------------------- Prop Visuals
        if (blendMode == PropellerBlendMode.Complete)
        {
            if (blurredRotor != null && normalRotor != null)
            {
                foreach (Material brotor in blurredRotor) { brotor.color = new Color(blurredRotorColor.r, blurredRotorColor.g, blurredRotorColor.b, alphaSettings); }
                foreach (Material nrotor in normalRotor) { nrotor.color = new Color(normalRotorColor.r, normalRotorColor.g, normalRotorColor.b, (1 - alphaSettings) + normalBalance); }
            }
        }
        if (blendMode == PropellerBlendMode.Partial)
        {
            if (blurredRotor != null)
            {
                foreach (Material brotor in blurredRotor) { brotor.color = new Color(blurredRotorColor.r, blurredRotorColor.g, blurredRotorColor.b, alphaSettings); }
            }
        }
    }




   
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseThrust()
    {

        // ----------------------------------- Reset
        thrust = torque = 0f; lift = 0; drag = 0f;
        CQ = CT = 0; Cq0 = Cqi = 0f;


        //1. ----------------------------------------- Convert Power To Thrust Proportionally
        if (analysisMethod == AnalysisMethod.MomentumTheory)
        {
            float propellerArea = (3.142f * Mathf.Pow((3.28084f * bladeDiameter), 2f)) / 4f;
            float dynamicPower = Mathf.Pow((availablePower * 550f), 2 / 3f);
            float dynamicArea = engineFactor * Mathf.Pow((2f * computer.airDensity * 0.0624f * propellerArea), 1 / 3f);
            thrust = dynamicArea * dynamicPower;
            if (currentRPM > 1f) { torque = ((availablePower * 60 * 746) / (2 * 3.142f * (currentRPM / 60f) * 1000f)) * bladeDiameter * engineFactor; }
        }

        

        if(analysisMethod == AnalysisMethod.BEM)
        {
            for (int i = 0; i < bladeSubdivisions; i++)
            {
                // ----------------------------------- Initial Data
                if (i == 0) { r0 = hubRadius + (Δr / 2); r[0] = r0; x0 = r0 / bladeRadius; x[0] = x0; }
                else { r[i] = r[i - 1] + Δr; x[i] = r[i] / bladeRadius; }

                c[i] = rootChord + (r[i] / bladeRadius) * (tipChord - rootChord); 
                float Ap = c[i] * Δr;
                β = Mathf.Atan(bladePitch / (2 * 3.142f * r[i])) * Mathf.Rad2Deg;
                θr[i] = (β + bladePitchAngle);

                VT = Ω * bladeRadius;
                λ = Uc / (VT);
                φi[i] = Mathf.Atan(λ / x[i]) * 0.1f;
                VR[i] = VT * Mathf.Sqrt((λ* λ) + (x[i]* x[i]));
                βi[i] = Mathf.Atan((bladePitch / bladeDiameter) / (3.142f * x[i]));

                A[i] = ((bladeSolidity * 5 * VR[i]) / (2 * x[i] * x[i] * VT))*(βi[i]- φi[i]);
                b[i] = (λ / x[i]) + ((bladeSolidity * 5 * VR[i]) / (8 * x[i] * x[i] * VT));
                B[i] = Mathf.Pow(b[i], 2f)+A[i];
                C[i] = -b[i] + Mathf.Sqrt(B[i]);
                αi[i] = 0.5f * C[i];
                αr[i] = βi[i] - φi[i] - αi[i];
                α = αr[i] * Mathf.Rad2Deg;

                float cl = bladeAirfoil.liftCurve.Evaluate(α);
                float cd = bladeAirfoil.dragCurve.Evaluate(α);
                float dL = 0.5f * computer.airDensity * VR[i] * VR[i] * c[i] * cl * r[i];
                float dD = 0.5f * computer.airDensity * VR[i] * VR[i] * c[i] * cd * r[i];
                lift += dL;drag += dD;

                float dT = (dL * Mathf.Cos(φi[i] + αi[i])) - (dD * Mathf.Sin(φi[i] + αi[i]));
                //float dT = (Nb * computer.airDensity * 0.5f) * VR[i] * VR[i] * c[i] * cl * Mathf.Cos(φi[i]) * r[i];
                float dQ = r[i] * ((dL * Mathf.Sin(φi[i] + αi[i])) + (dD * Mathf.Cos(φi[i] + αi[i])));

                torque += dQ;
                thrust += dT;
            }
        }


        if (analysisMethod == AnalysisMethod.InflowModel)
        {
            for (int i = 0; i < bladeSubdivisions; i++)
            {
                // ----------------------------------- Initial Data
                if (i == 0) { r0 = hubRadius + (Δr / 2); r[0] = r0; x0 = r0 / bladeRadius; x[0] = x0; }
                else { r[i] = r[i - 1] + Δr; x[i] = r[i] / bladeRadius; }

                c[i] = rootChord + (r[i] / bladeRadius) * (tipChord - rootChord); float Ap = c[i] * Δr;
                β = Mathf.Atan(bladePitch / (2 * 3.142f * r[i])) * Mathf.Rad2Deg;
                θr[i] = (β + bladePitchAngle);
                float y = x[i];

                float λi = (a * bladeSolidity) / (16 * x[i]) * (-1 + Mathf.Sqrt(1 + ((32 * x[i] * Mathf.Abs(β) * Mathf.Deg2Rad) / (a * bladeSolidity))));
                float λc = J * Mathf.Tan(Mathf.Deg2Rad * α);
                λ = λc + λi;
                φ = Mathf.Atan(λ) * Mathf.Rad2Deg;
                α = Mathf.Abs(θr[i]) - φ;
                α *= Mathf.Sign(bladePitchAngle);
                if (α > 89) { α = 89f; }
                if (α < -89) { α = -89f; }
                if (float.IsNaN(α)) { α = 0f; }
                if (float.IsInfinity(α)) { α = 0f; }

                CL = bladeAirfoil.liftCurve.Evaluate(α);
                CD = bladeAirfoil.dragCurve.Evaluate(α);

                if (float.IsNaN(a) || Mathf.Approximately(a, 0)) { a = 5.7f; }
                else { a = (Mathf.Abs(CL) * y) / ((Mathf.Abs(θr[i] + 0.1f) * y)); a *= 57.2958f; }

                // ----------------------------------- Thrust
                float deltaCTdy = (Mathf.Pow(y, 2) * bladeSolidity * Mathf.Abs(CL)) / 2f;
                float panelCT = deltaCTdy * y;
                float B = 1 - (Mathf.Sqrt(2 * panelCT) / 2);
                float dCT = (deltaCTdy) - (deltaCTdy * B);
                panelCT -= dCT; CT += (panelCT * Mathf.Sign(bladePitchAngle)*2f);

                // ----------------------------------- Torque
                float dCQ0dy = (Mathf.Pow(y, 3) * bladeSolidity * CD) / 2f;
                Cq0 += dCQ0dy * y;
                float dCQidy = (Mathf.Pow(y, 3) * bladeSolidity * CL * λ) / 2f;
                Cqi += ((dCQidy * B) - (dCQidy* cutoutRatio));
            }


            // --------------------------------------------- Extract Coefficients
            CT /= bladeSubdivisions;
            if (!float.IsNaN(CT)) { inducedTorqueCorrection = A1.Evaluate(CT); 
            discLoading = ρ * CT * Mathf.Pow((Ω * bladeRadius), 2); 
            CT_solidity = CT / bladeSolidity; }
            float powerFactor = discLoading * CT_solidity; if (!float.IsNaN(powerFactor)) { powerCorrectionFactor = A2.Evaluate(powerFactor); }
            if (!float.IsNaN(powerCorrectionFactor)) { powerCorrectionFactor = 1f; }
            CQ = ((Cq0 / bladeSubdivisions) + (Cqi / bladeSubdivisions) + ((Cqi / bladeSubdivisions) * inducedTorqueCorrection)) * powerCorrectionFactor;

            thrust = CT * ρ * Nb * (3.142f * bladeRadius * bladeRadius) * Mathf.Pow((Ω * bladeRadius), 2);
            torque = (CQ * ρ * Nb * (3.142f * bladeRadius * bladeRadius) * Mathf.Pow((Ω * bladeRadius), 2) * bladeRadius);
        }
    }



   



    public float internalFriction = 0.05f;
    public float rotorLoad, hydraulicsLoad, totalLoad;
    public float requestedPower, excessPower, enginePower;
    public float speedFactor;
    public bool torqueEngaged;
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseTransmission()
    {
        // --------------------------------- Collect Variables
        if (engineType == EngineType.PistonEngine && pistonEngine != null)
        {
            availablePower = pistonEngine.brakePower;
            engineFactor = pistonEngine.core.coreFactor;
            engineRPM = pistonEngine.core.coreRPM;
            enginePower = pistonEngine.core.powerInput;
            pistonEngine.core.torqueEngaged = torqueEngaged;
        }
        if (engineType == EngineType.TurbopropEngine && propEngine != null)
        {
            availablePower = propEngine.brakePower;
            engineFactor = propEngine.core.coreFactor;
            engineRPM = propEngine.core.coreRPM;
            enginePower = propEngine.core.powerInput;
            propEngine.core.torqueEngaged = torqueEngaged;
        }
        if (engineType == EngineType.ElectricMotor && electricMotor != null)
        {
            availablePower = electricMotor.horsePower;
            engineFactor = electricMotor.coreFactor;
            engineRPM = electricMotor.coreRPM;
        }



        if (analysisMethod == AnalysisMethod.InflowModel)
        {
            // ----------------------------------- Calculate Load
            hydraulicsLoad = internalFriction * Ω;
            rotorLoad = Mathf.Abs(torque) / gearRatio;
            totalLoad = rotorLoad + hydraulicsLoad;
            requestedPower = (totalLoad * Ω) / 1000;
            excessPower = enginePower - requestedPower;

            if (totalLoad > 1)
            {
                if (engineType == EngineType.PistonEngine && pistonEngine != null) { pistonEngine.core.inputLoad = totalLoad; }
                if (engineType == EngineType.TurbopropEngine && propEngine != null) { propEngine.core.inputLoad = totalLoad; }
            }
            if (speedFactor > 0.85f) { torqueEngaged = true; } else { torqueEngaged = false; }
        }

        // ----------------------------------------
        currentRPM = engineRPM / gearRatio;
        speedFactor = currentRPM / functionalRPM;
    }









#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        hubDiameter = (hubRatio / 100f) * bladeDiameter;
        tipChord = bladeRootChord * (bladeTaperRatio / 100f);
        // -------------------------------------------- Draw Propeller
        if (Propeller != null)
        {
            float propRadius = bladeDiameter / 2f;
            float hub = hubDiameter / 2f;
            if (Propeller != null)
            {
                Handles.color = Color.red; Handles.DrawWireDisc(Propeller.position, this.transform.forward, propRadius);
                Handles.color = Color.cyan; Handles.DrawWireDisc(Propeller.position, this.transform.forward, hub);
            }

            // -------------------------------------------- Draw Disc and Blades
            if (Nb < 2) { Nb = 2; }
            float sectorAngle = 360 / Nb;
            for (int i = 0; i < Nb; i++)
            {
                float currentSector = sectorAngle * (i + 1);
                Quaternion sectorRotation = Quaternion.AngleAxis(currentSector, Propeller.transform.forward);
                Vector3 sectorTipPosition = Propeller.position + (sectorRotation * (transform.right * propRadius));
                Debug.DrawLine(Propeller.position, sectorTipPosition, Color.yellow);
                Handles.color = Color.yellow; Handles.ArrowHandleCap(0, sectorTipPosition, transform.rotation * Quaternion.LookRotation(-Vector3.forward), 0.3f, EventType.Repaint);
            }
        }

        // ------------------------------------------------
        ShapePropeller();
    }
#endif


    public void ShapePropeller()
    {
        if (bladeSubdivisions < 5) { bladeSubdivisions = 5; }
        if (Nb < 2) { Nb = 2; }

        if (r == null || bladeSubdivisions != r.Length) { r = new float[bladeSubdivisions]; }
        if (x == null || bladeSubdivisions != x.Length) { x = new float[bladeSubdivisions]; }
        if (c == null || bladeSubdivisions != c.Length) { c = new float[bladeSubdivisions]; }
        if (θr == null || bladeSubdivisions != θr.Length) { θr = new float[bladeSubdivisions]; }
        if (φi == null || bladeSubdivisions != φi.Length) { φi = new float[bladeSubdivisions]; }
        if (VR == null || bladeSubdivisions != VR.Length) { VR = new float[bladeSubdivisions]; }
        if (βi == null || bladeSubdivisions != βi.Length) { βi = new float[bladeSubdivisions]; }
        if (αi == null || bladeSubdivisions != αi.Length) { αi = new float[bladeSubdivisions]; }
        if (αr == null || bladeSubdivisions != αr.Length) { αr = new float[bladeSubdivisions]; }

        if (A == null || bladeSubdivisions != A.Length) { A = new float[bladeSubdivisions]; }
        if (B == null || bladeSubdivisions != B.Length) { B = new float[bladeSubdivisions]; }
        if (b == null || bladeSubdivisions != b.Length) { b = new float[bladeSubdivisions]; }
        if (C == null || bladeSubdivisions != C.Length) { C = new float[bladeSubdivisions]; }
    }
}
