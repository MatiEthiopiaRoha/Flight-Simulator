using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class HoverController
{
    public enum Configuration { F35Liftfan }
    public Configuration configuration = Configuration.F35Liftfan;
    public enum Mode { Normal, VTOL }
    public Mode mode = Mode.Normal;
  

    // ------------------------------------------ Connections
    public SilantroController controller;
    public SilantroLiftFan liftfan;
    public SilantroTurboFan mainEngine;
    public SilantroActuator vtolMechanism;


    // ------------------------------------------ Variables
    public float VTOLMultiplier;
    public LayerMask surfaceMask;
    public float BrakingTorque = 2000f;
    public float maximumHoverHeight = 30f;
    public float hoverDamper = 9000f;
    public float hoverAngleDrift = 25f;
    public float PitchCompensationTorque = 1000;
    public float RollCompensationTorque = 1000;
    public float maximumTorque = 500f;
    public float LiftForce;
    public bool transitionToVTOL, transitionToNormal; public bool transitioning;
    public float yawControlTorque, rollControlTorque, pitchControlTorque;
    public Vector3 GeneratedLift;
    public float pitchTorque, rollTorque, yawTorque;

    public float liftPercentage;
    public float transitionSpeed = 0.5f;
    public float actuationSpeed = 0.5f;
    public float deteranceFactor;
    public bool enginePush;

    public float thrustFactor;

    // ----------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateHover()
    {

        // ------------------------------ Main
        HoverControl();
        CalculateHover();
        HoverBalance();


        if (mainEngine.core.active)
        {
            switch (mode)
            {
                case Mode.Normal: NormalState();  break;
                case Mode.VTOL: VTOLState(); break;
            }

            if(mode == Mode.Normal) { liftPercentage -= Time.deltaTime * transitionSpeed; }
            else { liftPercentage += Time.deltaTime * transitionSpeed*5; if (mainEngine != null && mainEngine.core.afterburnerOperative) { mainEngine.DisEngageAfterburner(); } }
            if(liftPercentage > 1) { liftPercentage = 1f; } if(liftPercentage < 0) { liftPercentage = 0f; }
        }

        if(vtolMechanism.currentActuationLevel > 0.5f) { deteranceFactor -= Time.deltaTime * actuationSpeed; }
        else { if (controller.core.currentSpeed > 80f) { deteranceFactor += Time.deltaTime * actuationSpeed; } if (controller.core.currentSpeed > 81f && thrustFactor > 0f) { thrustFactor = 0f; controller.aircraft.angularDrag = 0.05f; } }
        if (deteranceFactor > 1) { deteranceFactor = 1f; } if (deteranceFactor < 0) { deteranceFactor = 0f; }

        
        Vector3 thrustForce = mainEngine.exitPoint.forward * mainEngine.engineThrust * thrustFactor;
        mainEngine.connectedAircraft.AddForce(thrustForce, ForceMode.Force);


        // ---------------------------------- F-35 Setup
        if (configuration == Configuration.F35Liftfan)
        {
            LiftForce = mainEngine.engineThrust + liftfan.fanThrust;
            mainEngine.liftFactor = deteranceFactor;
        }
    }



   
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //HOVER YAW CONTROL
    void HoverControl()
    {
        if (mode == Mode.VTOL && controller.transform.position.y > 5f)
        {
            float yawControl = controller.flightComputer.processedYaw * mainEngine.core.coreFactor;
            float rollControl = controller.flightComputer.processedRoll * mainEngine.core.coreFactor;
            float pitchControl = controller.flightComputer.processedPitch * mainEngine.core.coreFactor;

            yawControlTorque = (yawControl * maximumTorque);
            rollControlTorque = (-rollControl * 0.5f * maximumTorque);

            //BALANCE AIRCRAFT IS YAW INPUT IS NEAR ZERO
            if (Mathf.Abs(yawControl) < 0.05f)
            { 
                var ControlAxis = Vector3.Dot(controller.aircraft.angularVelocity, controller.transform.up); 
                controller.aircraft.AddRelativeTorque(0, -ControlAxis * BrakingTorque, 0); 
            }
            else { controller.aircraft.AddRelativeTorque(0, yawControlTorque, rollControlTorque); }
        }
    }




   
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void CalculateHover()
    {
        //CALCULATE DIRECTION OF FORCE
        Vector3 up = controller.aircraft.transform.up;
        Vector3 gravity = Physics.gravity.normalized;
        up = Vector3.RotateTowards(up, -gravity, hoverAngleDrift * Mathf.Deg2Rad, 1);

        float verticalSpeed = controller.aircraft.velocity.y;
        float dampForce = -verticalSpeed * Mathf.Abs(verticalSpeed) * hoverDamper;
        GeneratedLift = up * (LiftForce + dampForce) * liftPercentage;
        if (!float.IsNaN(GeneratedLift.magnitude)) { controller.aircraft.AddForce(GeneratedLift, ForceMode.Force); }
    }






    
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void HoverBalance()
    {
        if (mode == Mode.VTOL)
        {
            float yawFactor = Vector3.Dot(controller.aircraft.angularVelocity, controller.transform.up);
            yawTorque = (-yawFactor * BrakingTorque);
            controller.aircraft.AddRelativeTorque(0, yawTorque, 0);

            //PITCH AND ROLL BALALNCE
            if (controller.transform.position.y > 5f)
            {
                Vector3 gravity = -Physics.gravity.normalized;
                float pitch = Mathf.Asin(Vector3.Dot(controller.transform.forward, gravity)) * Mathf.Rad2Deg;
                float roll = Mathf.Asin(Vector3.Dot(controller.transform.right, gravity)) * Mathf.Rad2Deg;
                pitch = Mathf.DeltaAngle(pitch, 0);
                roll = Mathf.DeltaAngle(roll, 0);
                //TORQUES
                pitchTorque = -pitch * PitchCompensationTorque;
                rollTorque = roll * RollCompensationTorque;
                controller.aircraft.AddRelativeTorque(pitchTorque, 0, rollTorque);
            }
        }
    }


    
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void NormalState()
    {
        //FOR F-35 LIGHTNING
        if (configuration == Configuration.F35Liftfan)
        {
            if (transitionToVTOL && !transitioning && vtolMechanism.actuatorState == SilantroActuator.ActuatorState.Disengaged)
            {
                vtolMechanism.EngageActuator();
                transitioning = true;
                controller.StartCoroutine(TransitionToVTOL());
            }
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void VTOLState()
    {
        if (configuration == Configuration.F35Liftfan)
        {
            if (transitionToNormal && !transitioning && vtolMechanism.actuatorState == SilantroActuator.ActuatorState.Engaged)
            {
                thrustFactor = 1f;enginePush = true;
                vtolMechanism.DisengageActuator();
                transitioning = true;
                controller.StartCoroutine(TransitionToNormal());
            }
        }
    }



    IEnumerator TransitionToVTOL()
    {
        yield return new WaitUntil(() => vtolMechanism.actuatorState == SilantroActuator.ActuatorState.Engaged);
        if (configuration == Configuration.F35Liftfan) { liftfan.core.start = true; }
        mode = Mode.VTOL; transitioning = false;
        thrustFactor = 0f;controller.aircraft.angularDrag = 5f;
        transitionToNormal = false; transitionToVTOL = false;
    }


    private IEnumerator TransitionToNormal()
    {
        yield return new WaitUntil(() => vtolMechanism.actuatorState == SilantroActuator.ActuatorState.Disengaged);
        mode = Mode.Normal; transitioning = false;
        mainEngine.EngageAfterburner();
        transitionToNormal = false;transitionToVTOL = false;
        liftfan.core.shutdown = true;
    }


    public void ResetKeys()
    {
        transitioning = false;
        transitionToNormal = false;
        transitionToVTOL = false;
    }
}
