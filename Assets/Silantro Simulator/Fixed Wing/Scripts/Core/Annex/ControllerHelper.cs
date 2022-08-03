using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class ControllerFunctions
{


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void RestoreFunction(Rigidbody craft, SilantroController controller)
    {
        if (craft != null)
        {
            craft.velocity = Vector3.zero;
            craft.angularVelocity = Vector3.zero;
            craft.transform.position = controller.basePosition;
            craft.transform.rotation = controller.baseRotation;

            controller.TurnOffEngines();
            if (controller.gearActuator != null && controller.gearActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.gearActuator.EngageActuator(); }
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InternalControlSetup(SilantroController controller)
    {
        if (!controller.getOutPosition)
        {
            GameObject getOutPos = new GameObject("Get Out Position");
            getOutPos.transform.SetParent(controller.transform);
            getOutPos.transform.localPosition = new Vector3(-3f, 0f, 0f);
            getOutPos.transform.localRotation = Quaternion.identity;
            controller.getOutPosition = getOutPos.transform;
        }
    }





    // -------------------------- Check Engines
    public bool CheckEngineState(SilantroEngineCore core)
    {
        bool check;
        if (core.CurrentEngineState == SilantroEngineCore.EngineState.Active) { check = true; }
        else { check = false; }
        return check;
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void PositionAircraftFunction(Rigidbody craft, SilantroController controller)
    {
        Vector3 initialPosition = craft.transform.position;
        craft.transform.position = new Vector3(initialPosition.x, controller.startAltitude, initialPosition.z);
        craft.velocity = craft.transform.forward * controller.startSpeed;
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void StartAircraftFunction(Rigidbody craft, SilantroController controller)
    {
        if (craft != null && controller.startMode == SilantroController.StartMode.Hot)
        {
            //POSITION AIRCRAFT
            controller.PositionAircraft();
            //SET ENGINE
            controller.input.TurnOnEngines();
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void RefreshWeaponsFunction(SilantroController controller)
    {
        //SNAPSHOT OF CURRENT POINT
        GameObject oldPod = controller.Armaments.gameObject; int currentWeapon = controller.Armaments.selectedWeapon;
        GameObject newPod = UnityEngine.Object.Instantiate(controller.ArmamentsStorage, controller.Armaments.transform.position, controller.Armaments.transform.rotation, controller.transform);
        SilantroArmament newArmament = newPod.GetComponent<SilantroArmament>(); newPod.name = "Hardpoints";

        //REMOVE AND REPLACE
        controller.Armaments = newArmament; newPod.SetActive(true); controller.CleanupGameobject(oldPod);
        if (controller.radarCore != null) { controller.Armaments.connectedRadar = controller.radarCore; }

        //SET VARIABLES
        controller.Armaments.InitializeWeapons();
        controller.Armaments.SelectWeapon(currentWeapon);
        Debug.Log("Rearm Complete!!");
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void EnterAircraftFunction(SilantroController controller)
    {
        if (!controller.opened && !controller.temp && !controller.pilotOnboard && controller.controlType == SilantroController.ControlType.Internal)
        {
            controller.opened = true; controller.temp = true;
            //OPEN CANOPY
            if (controller.canopyActuator != null && controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged)
            {
                controller.canopyActuator.EngageActuator();
            }
            //SET THINGS UP
            controller.pilotOnboard = true;
            controller.StartCoroutine(EntryProcedure(controller));
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ExitAircraftFunction(SilantroController controller)
    {
        if (controller.pilotOnboard && controller.controlType == SilantroController.ControlType.Internal)
        {
            //EXIT CHECK LIST
            //1. SHUT ENGINES DOWN
            controller.TurnOffEngines();
            //2. ACTIVATE BRAKE
            if (controller.gearHelper != null)
            {
                controller.gearHelper.EngageBrakes();
            }
            //3. TURN OFF LIGHTS
            controller.input.TurnOffLights();
            //OPEN CANOPY
            if (controller.canopyActuator != null && controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged)
            {
                controller.canopyActuator.EngageActuator();
            }
            //ACTUAL EXIT
            controller.pilotOnboard = false;
            controller.StartCoroutine(ExitProcedure(controller));
        }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator RecieveDelay(SilantroController controller)
    {
        yield return new WaitForSeconds(0.5f);
        //CLOSE DOOR
        if (controller.canopyActuator != null && controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.canopyActuator.DisengageActuator();  }
        BeginDisable(controller);
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void BeginDisable(SilantroController controller) { controller.StartCoroutine(DisableController(controller)); }
    IEnumerator DisableController(SilantroController controller)
    {
        if (controller.canopyActuator != null) { yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged); controller.SetControlState(false); }
        else { yield return new WaitForSeconds(5f); controller.SetControlState(false); }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //ENTER AIRCRAFT
    public IEnumerator EntryProcedure(SilantroController controller)
    {

        if (controller.canopyActuator != null) { yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged); }
        else { yield return new WaitForSeconds(1.5f); }

        //PLAYER STATE
        if (controller.player != null)
        {
            controller.view.ActivateExteriorCamera();
            controller.player.SetActive(false); if (controller.interiorPilot != null) { controller.interiorPilot.SetActive(true); }
            controller.player.transform.SetParent(controller.transform);
            controller.player.transform.localPosition = Vector3.zero; controller.player.transform.localRotation = Quaternion.identity;
           
            //ENABLE CONTROLS
            if (controller.playerType == SilantroController.PlayerType.FirstPerson)
            {
                EnableFPControls(controller);
            }
            if (controller.playerType == SilantroController.PlayerType.ThirdPerson)
            {
                controller.ThirdPersonCall();
            }
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //FIRST PERSON
    public void EnableFPControls(SilantroController controller)
    {
        //CLOSE CANOPY
        WaitToClose(controller);

        controller.isControllable = true;
        controller.temp = false;

        //DISABLE MAIN CAMERA USED BY PLAYER NOTE: YOU MIGHT HAVE TO SET YOUR OWN CONDITION DEPENDING ON THE PLAYER CONTROLLER
        if (controller.mainCamera != null)
        {
            controller.mainCamera.gameObject.SetActive(false);
            controller.mainCamera.enabled = false;
            controller.mainCamera.gameObject.GetComponent<AudioListener>().enabled = false;
        }
        //ENABLE CANVAS
        if (controller.canvasDisplay != null)
        {
            controller.canvasDisplay.gameObject.SetActive(true);
            controller.canvasDisplay.connectedAircraft = controller;
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //THIRD PERSON 
    public IEnumerator EnableTPControls(SilantroController controller)
    {
        if (controller.canopyActuator != null) { yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged); }
        else { yield return new WaitForSeconds(0.05f); }
        WaitToClose(controller);
      
        controller.isControllable = true;
        controller.temp = false;

        //DISABLE MAIN CAMERA USED BY PLAYER NOTE: YOU MIGHT HAVE TO SET YOUR OWN CONDITION DEPENDING ON THE PLAYER CONTROLLER
        if (controller.mainCamera != null)
        {
            controller.mainCamera.gameObject.SetActive(false);
            controller.mainCamera.enabled = false;
            controller.mainCamera.gameObject.GetComponent<AudioListener>().enabled = false;
        }
        //ENABLE CANVAS
        if (controller.canvasDisplay != null)
        {
            controller.canvasDisplay.gameObject.SetActive(true);
            controller.canvasDisplay.connectedAircraft = controller;
        }
    }


    public void WaitToClose(SilantroController controller) { controller.StartCoroutine(CloseDoor(controller)); }
    public IEnumerator CloseDoor(SilantroController controller)
    {
        yield return new WaitForSeconds(0.5f);
        if (controller.canopyActuator != null && controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.canopyActuator.DisengageActuator(); }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //EXIT AIRCRAFT
    public IEnumerator ExitProcedure(SilantroController controller)
    {
        if (controller.canopyActuator != null) { yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged); }
        else { yield return new WaitForSeconds(1.3f); }

       

        //PLAYER STATE
        if (controller.player != null)
        {
            if (controller.interiorPilot != null) { controller.interiorPilot.SetActive(false); }
            controller.player.transform.SetParent(null);
            controller.player.transform.position = controller.getOutPosition.position;
            controller.player.transform.rotation = controller.getOutPosition.rotation;
            controller.player.transform.rotation = Quaternion.Euler(0f, controller.player.transform.eulerAngles.y, 0f);
            controller.player.SetActive(true);
            ////SET CAMERA FOR FIRST PERson
            if (controller.playerType == SilantroController.PlayerType.FirstPerson)
            {
                controller.view.ResetCameras();
            }
            //DISABLE CANVAS
            if (controller.canvasDisplay != null)
            {
                controller.canvasDisplay.connectedAircraft = null;
                controller.canvasDisplay.gameObject.SetActive(false);
            }
           
            //ENABLE MAIN CAMERA USED BY PLAYER>>> NOTE: YOU MIGHT HAVE TO SET YOUR OWN CONDITION DEPENDING ON THE PLAYER CONTROLLER
            if (controller.mainCamera != null)
            {
                controller.mainCamera.gameObject.SetActive(true);
                controller.mainCamera.enabled = true;
                controller.mainCamera.gameObject.GetComponent<AudioListener>().enabled = true;
            }

            //
            //DISABLE 1. CAMERA
            if (controller.view != null)
            {
                controller.view.ResetCameras();
            }
            ////DISABLE CONTROLS
            DelayState(controller);
        }
    }

    void DelayState(SilantroController controller)
    {
        controller.StartCoroutine(RecieveDelay(controller));
    }
}
