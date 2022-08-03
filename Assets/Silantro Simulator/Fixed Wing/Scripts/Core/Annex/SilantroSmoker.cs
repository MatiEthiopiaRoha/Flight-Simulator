using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroSmoker : MonoBehaviour
{
    //Connections
    public ParticleSystem smoke;
    ParticleSystem.EmissionModule smokeModule;
    public float smokeEmission = 10000f;
    public float smokeInput = 0f;
   

    void Start() { if (smoke != null) { smokeModule = smoke.emission; smokeModule.rateOverTime = 0f; }  }

  
    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetButton("Brake Lever")) { smokeInput = 1f; }
        else { smokeInput = Mathf.Lerp(smokeInput, 0, 5f * Time.deltaTime); }
#endif
        if (!smokeModule.enabled && smoke != null) { smokeModule = smoke.emission; }
        smokeModule.rateOverTime = smokeInput * smokeEmission;
    }
}
