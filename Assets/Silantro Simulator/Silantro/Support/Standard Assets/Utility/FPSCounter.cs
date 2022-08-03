using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof (Text))]
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "{0} FPS";
        private Text m_Text;


        private void Stagrt()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
           
        }


      
        string label = "";
        float count;

        IEnumerator Start()
        {
            m_Text = GetComponent<Text>();
            GUI.depth = 2;
            while (true)
            {
                if (Time.timeScale == 1)
                {
                    yield return new WaitForSeconds(0.1f);
                    count = (1 / Time.deltaTime);
                    label = "FPS :" + (Mathf.Round(count));
                    m_Text.text = label;
                }
                else
                {
                    label = "Pause";
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
