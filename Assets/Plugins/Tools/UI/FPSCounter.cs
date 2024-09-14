// Created by: Unknown
// Edited by: Bill D.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Skritty.Tools.Utilities
{
    public class FPSCounter : MonoBehaviour
    {
        private TextMeshProUGUI displayText;

        [field: SerializeField]
#if ODIN_INSPECTOR
        [field: Sirenix.OdinInspector.ReadOnly]
#endif
        public int AvgFrameRate { get; private set; }
        [SerializeField, Tooltip("Amount of time between each update of the counter")]
        private float updateTick = 0.2f;
        [SerializeField]
        private string textSuffix = " FPS";

        // ---

        private float timer_UpdateTick = 0f;

        // ---------------------------------------------------------------------------

        private void Awake()
        {
            displayText = GetComponent<TextMeshProUGUI>();
        }

        public void LateUpdate()
        {
            timer_UpdateTick += Time.deltaTime;
            if(timer_UpdateTick > updateTick)
            {
                AvgFrameRate = (int)(1f / Time.unscaledDeltaTime);
                displayText.text = $"{AvgFrameRate}{textSuffix}";
                timer_UpdateTick = 0f;
            }

        }
    }
}