using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
///
/// 
/// Use:		 Handles mobile controls
/// </summary>


public class SilantroMobile : MonoBehaviour
{
    // --------------------------- Components
    SilantroController controller;
    SilantroCore core;
    SilantroArmament hardpoints;
    SilantroRadar radar;

    // --------------------------- Throttle
    public Scrollbar ThrottleSlider;
    private Text throttleText;
    private void Start()  { StartCoroutine(SetupMobileControls()); }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator SetupMobileControls()
    {
        yield return new WaitForSeconds(0.002f);//JUST LAG A BIT BEHIND CONTROLLER SCRIPT

        // ----------------------------- Collect Components
        controller = GetComponent<SilantroController>();
        if(controller != null)
        {
            core = controller.core;
            hardpoints = controller.Armaments;
            radar = controller.radarCore;
            throttleText = ThrottleSlider.gameObject.GetComponentInChildren<Text>();
        }
        else
        {
            Debug.LogError("Mobile aircraft not setup properly");
        }
    }



    // -------------------------------------------------Control Functions-----------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void BrakeControl() { if (controller) { controller.input.ToggleBrakeState(); } }
    public void GearControl() { if (controller) { controller.input.ToggleGearState(); } }
    public void LightControl() { if (controller) { controller.input.ToggleLightState(); } }
    public void StartupEngines() { if (controller) { controller.input.TurnOnEngines(); } }
    public void ShutdownEngines() { if (controller) { controller.input.TurnOffEngines(); } }
    public void Exit() { Application.Quit(); }
    public void ToggleCamera() { if (controller) { controller.view.ToggleCamera(); } }
    public void SwitchWeapon(){ if (controller && hardpoints) { controller.Armaments.ChangeWeapon(); } }
    public void TargetUp() { if (controller && radar) { radar.SelectedUpperTarget(); } }
    public void TargetDown() { if (controller && radar) { radar.SelectLowerTarget(); } }
    public void LockTarget() { if (controller && radar) { radar.LockSelectedTarget(); } }
    public void ReleaseTarget() { if (controller && radar) { radar.ReleaseLockedTarget(); } }
    public void FireWeapon()
    {
        if (controller && hardpoints)
        {
            //MISSILE
            if (hardpoints.currentWeapon == "Missile") { hardpoints.FireMissile();}
            //ROCKETS
            if (hardpoints.currentWeapon == "Rocket") { hardpoints.FireRocket(); }
            //FIRE GUN IF SELECTED
            if (hardpoints.currentWeapon == "Gun") { hardpoints.FireGuns(); }
            //BOMB
            if (hardpoints.currentWeapon == "Bomb") { hardpoints.DropBomb(); }
        }
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void Update()
    {
        if (controller)
        {
            controller.input.rawThrottleInput = ThrottleSlider.value;
            throttleText.text = (ThrottleSlider.value * 100f).ToString("0") + " %";
        }
    }
}
