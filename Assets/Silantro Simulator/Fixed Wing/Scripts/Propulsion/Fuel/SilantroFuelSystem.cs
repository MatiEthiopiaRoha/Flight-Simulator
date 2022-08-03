using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;

[System.Serializable]
public class SilantroFuelSystem
{
    // ----------------------------------- Selectibles
    public enum FuelSelector { Left, Right, External, Automatic }
    public FuelSelector fuelSelector = FuelSelector.Automatic;
    public enum FuelFactor { DeltaTime, FixedDeltaTime }
    public FuelFactor usageFactor = FuelFactor.DeltaTime;

    // ----------------------------------- Connections
    public SilantroController controller;
    public SilantroFuelTank[] fuelTanks;
    public List<SilantroFuelTank> internalFuelTanks;
    public List<SilantroFuelTank> externalTanks;


    public List<SilantroFuelTank> RightTanks;
    public List<SilantroFuelTank> LeftTanks;
    public List<SilantroFuelTank> CentralTanks;


    public bool dumpFuel = false;
    public float fuelDumpRate = 1f;//Rate at which fuel will be dumped in kg/s
    public float actualFlowRate;

    public bool refillTank = false;
    public float refuelRate = 1f;
    public float actualrefuelRate;

    // ---------------------------------- Alert System
    AudioSource FuelAlert;
    public AudioClip fuelAlert;
    public float minimumFuelAmount = 50f;
    bool fuelAlertActivated;


    // ---------------------------------- Fuel Data
    public float RightFuelAmount;
    public float LeftFuelAmount;
    public float CenterFuelAmount;
    public float ExternalFuelAmount;
    public float engineConsumption;

    public string fuelType;
    public float timeFactor = 1;
    public float fuelFlow;





    // ---------------------------------------------------------------------CONTROLS-------------------------------------------------------------------------------------
    //START/STOP FUEL DUMP
    public void ActivateFuelDump()
    {
        if (!refillTank) { dumpFuel = !dumpFuel; }
    }
    //START/STOP TANK REFILL
    public void ActivateTankRefill()
    {
        if (!dumpFuel) { refillTank = !refillTank; }
    }
    //STOP ALERT SOUND
    public void StopAlertSound()
    {
        if (fuelAlertActivated) { FuelAlert.Stop(); controller.lowFuel = false; }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeDistributor()
    {

        if (fuelTanks == null || fuelTanks.Length < 1) { Debug.LogError("No fuel tank is assigned to aircraft!!"); return; }
        // ------------------------- Setup Containers
        controller.TotalFuelCapacity = 0f; controller.fuelLevel = 0f;
        externalTanks = new List<SilantroFuelTank>();
        LeftTanks = new List<SilantroFuelTank>();
        RightTanks = new List<SilantroFuelTank>();
        CentralTanks = new List<SilantroFuelTank>();
        fuelType = fuelTanks[0].fuelType.ToString();
        if (fuelAlert) { Handler.SetupSoundSource(fuelTanks[0].transform, fuelAlert, "Alert Point", 50f, true, false, out FuelAlert); FuelAlert.volume = 1f; }


        // ------------------------ Filter
        foreach (SilantroFuelTank tank in fuelTanks)
        {
            controller.TotalFuelCapacity += tank.actualAmount;
            controller.fuelLevel += tank.CurrentAmount;
            if (tank.tankType == SilantroFuelTank.TankType.Internal) { internalFuelTanks.Add(tank); }
            if (tank.tankType == SilantroFuelTank.TankType.External) { externalTanks.Add(tank); }
            if (tank.fuelType.ToString() != fuelType) { Debug.LogError("Fuel Type Selection not uniform"); return; }
        }
        foreach (SilantroFuelTank tank in internalFuelTanks)
        {
            if (tank.tankPosition == SilantroFuelTank.TankPosition.Center) { CentralTanks.Add(tank); }
            if (tank.tankPosition == SilantroFuelTank.TankPosition.Left) { LeftTanks.Add(tank); }
            if (tank.tankPosition == SilantroFuelTank.TankPosition.Right) { RightTanks.Add(tank); }
        }
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateFuel()
    {
        controller.fuelLevel = 0f;
        if (fuelTanks.Length > 0) { foreach (SilantroFuelTank tank in fuelTanks) { controller.fuelLevel += tank.CurrentAmount; } }
        foreach (SilantroFuelTank tank in fuelTanks) { if (tank.CurrentAmount < 0) { tank.CurrentAmount = 0f; } }

        if (controller.fuelLevel <= minimumFuelAmount) { controller.lowFuel = true; fuelAlertActivated = true; LowFuelAction(); }
        if (controller.fuelLevel > minimumFuelAmount && fuelAlertActivated) { fuelAlertActivated = false; FuelAlert.Stop(); }

        if (dumpFuel) { DumpFuel(); }
        if (refillTank) { RefuelTank(); }
        if (usageFactor == FuelFactor.DeltaTime) { timeFactor = Time.deltaTime; } else { timeFactor = Time.fixedDeltaTime; }

        // ---------------- Actual Usage
        DepleteTanks();
    }










    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void DepleteTanks()
    {
        engineConsumption = controller.totalConsumptionRate; fuelFlow = engineConsumption * timeFactor;
        LeftFuelAmount = RightFuelAmount = CenterFuelAmount = ExternalFuelAmount = 0f;

        foreach (SilantroFuelTank tank in LeftTanks) { LeftFuelAmount += tank.CurrentAmount; }
        foreach (SilantroFuelTank tank in RightTanks) { RightFuelAmount += tank.CurrentAmount; }
        foreach (SilantroFuelTank tank in CentralTanks) { CenterFuelAmount += tank.CurrentAmount; }
        foreach (SilantroFuelTank tank in externalTanks) { ExternalFuelAmount += tank.CurrentAmount; }



        // -------------------------------- Use External Tanks
        if (fuelSelector == FuelSelector.External)
        {
            if (externalTanks != null && externalTanks.Count > 0)
            {
                if (ExternalFuelAmount <= 0) { fuelSelector = FuelSelector.Automatic; }
                float individualRate = engineConsumption / externalTanks.Count;
                foreach (SilantroFuelTank tank in externalTanks) { tank.CurrentAmount -= individualRate * timeFactor; }
            }
            else { fuelSelector = FuelSelector.Automatic; }
        }


        // -------------------------------- Use Left Tanks
        if (fuelSelector == FuelSelector.Left)
        {
            if (LeftTanks != null && LeftTanks.Count > 0)
            {
                float individualRate = engineConsumption / LeftTanks.Count;
                foreach (SilantroFuelTank tank in LeftTanks) { tank.CurrentAmount -= individualRate * timeFactor; }
                if (LeftFuelAmount <= 0) { fuelSelector = FuelSelector.Automatic; }
            }
            else { fuelSelector = FuelSelector.Automatic; }
        }



        // -------------------------------- Use Right Tanks
        if (fuelSelector == FuelSelector.Right)
        {
            if (RightTanks != null && RightTanks.Count > 0)
            {
                float individualRate = engineConsumption / RightTanks.Count;
                foreach (SilantroFuelTank tank in RightTanks) { tank.CurrentAmount -= individualRate * timeFactor; }
                if (RightFuelAmount <= 0) { fuelSelector = FuelSelector.Automatic; }
            }
            else { fuelSelector = FuelSelector.Automatic; }
        }




        // -------------------------------- Automatic
        if (fuelSelector == FuelSelector.Automatic)
        {
            //A> USE CENTRAL TANKS FIRST
            if (CentralTanks != null && CentralTanks.Count > 0 && CenterFuelAmount > 0)
            {
                //DEPLETE
                float individualRate = engineConsumption / CentralTanks.Count;
                foreach (SilantroFuelTank tank in CentralTanks)
                {
                    tank.CurrentAmount -= individualRate * timeFactor;
                }
            }
            else
            {
                //B> USE EXTERNAL TANKS
                if (externalTanks != null && externalTanks.Count > 0 && ExternalFuelAmount > 0)
                {
                    //DEPLETE
                    float individualRate = engineConsumption / externalTanks.Count;
                    foreach (SilantroFuelTank tank in externalTanks)
                    {
                        tank.CurrentAmount -= individualRate * timeFactor;
                    }
                }
                //C> USE OTHER TANKS
                else
                {
                    int usefulTanks = LeftTanks.Count + RightTanks.Count;
                    float individualRate = engineConsumption / usefulTanks;
                    //LEFT
                    foreach (SilantroFuelTank tank in LeftTanks)
                    {
                        tank.CurrentAmount -= individualRate * timeFactor;
                    }
                    //RIGHT
                    foreach (SilantroFuelTank tank in RightTanks)
                    {
                        tank.CurrentAmount -= individualRate * timeFactor;
                    }
                }
            }
        }
    }




    // ---------------------------------------------------------- FUEL ACTION
    void LowFuelAction()
    {
        if (FuelAlert != null)
        {
            if (!FuelAlert.isPlaying) { FuelAlert.Play(); }
            else { FuelAlert.Stop(); }
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    ////REFUEL TANKS
    public void RefuelTank()
    {
        actualrefuelRate = refuelRate * Time.deltaTime;
        if (internalFuelTanks != null && internalFuelTanks.Count > 0)
        {
            float indivialRate = actualrefuelRate / internalFuelTanks.Count;
            foreach (SilantroFuelTank tank in internalFuelTanks)
            {
                tank.CurrentAmount += indivialRate;
            }
        }
        //CONTROL AMOUNT
        foreach (SilantroFuelTank tank in internalFuelTanks)
        {
            if (tank.CurrentAmount > tank.Capacity)
            {
                tank.CurrentAmount = tank.Capacity;
            }
        }
        if (controller.fuelLevel >= controller.TotalFuelCapacity)
        {
            refillTank = false;
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    ////REFUEL TANKS
    void DumpFuel()
    {
        actualFlowRate = fuelDumpRate * Time.deltaTime;
        if (internalFuelTanks != null && internalFuelTanks.Count > 0)
        {
            float indivialRate = actualFlowRate / internalFuelTanks.Count;
            foreach (SilantroFuelTank tank in internalFuelTanks)
            {
                tank.CurrentAmount -= indivialRate;
            }
        }
        //CONTROL AMOUNT
        foreach (SilantroFuelTank tank in fuelTanks)
        {
            if (tank.CurrentAmount <= 0)
            {
                tank.CurrentAmount = 0;
            }
        }
        if (controller.fuelLevel <= 0)
        {
            dumpFuel = false;
        }
    }
}
