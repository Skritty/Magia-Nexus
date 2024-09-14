//Created by: Julian Noel
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// Toggles the specified scripts on or off depending on whether or not we're in range of a given transform or bounding box
    /// </summary>
    public class ProximityToggler : MonoBehaviour
    {
        public Transform proximityTarget;
        public float toggleRange = 10f;
        [Tooltip("How long, in seconds, to go between state updates. If <= 0, runs every frame.")]
        public float updateInterval;

        [Tooltip("Whether or not to ignore the toggle state when out of range or to enforce the opposite toggle state.")]
        public bool switchTogglesOutOfRange;
        public List<Behaviour> enableInRange;
        public List<Behaviour> disableInRange;
        public List<GameObject> activateInRange;
        public List<GameObject> deactivateInRange;

        private Coroutine _proximityCheckerRoutine;

        private void OnEnable()
        {
            //TODO @ Julian: Import your RestartCoroutine extension
            if (_proximityCheckerRoutine != null) StopCoroutine(_proximityCheckerRoutine);
            _proximityCheckerRoutine = StartCoroutine(_UpdateToggleRoutine());
        }

        private void OnDisable()
        {
            if (_proximityCheckerRoutine != null) StopCoroutine(_proximityCheckerRoutine);
        }

        /// <summary>
        /// Coroutine handling toggler updates.
        /// </summary>
        private IEnumerator _UpdateToggleRoutine()
        {
            while (true)
            {
                _UpdateToggleStates();

                //Runs every frame if update <= 0
                if (updateInterval > 0)
                {
                    yield return new WaitForSeconds(updateInterval);
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Logic for applying toggle updates
        /// </summary>
        private void _UpdateToggleStates()
        {
            float currentSqrDist = (proximityTarget.position - transform.position).sqrMagnitude;
            float currentSqrRange = toggleRange * toggleRange;

            if (currentSqrDist <= currentSqrRange)
            {
                _UpdateToToggleState(true);
            }
            else if (switchTogglesOutOfRange)
            {
                _UpdateToToggleState(false);
            }
        }

        private void _UpdateToToggleState(bool inToggleRange)
        {
            foreach (Behaviour behav in enableInRange)
            {
                behav.enabled = inToggleRange;
            }

            foreach (GameObject go in activateInRange)
            {
                go.SetActive(inToggleRange);
            }

            foreach (Behaviour behav in disableInRange)
            {
                behav.enabled = !inToggleRange;
            }

            foreach (GameObject go in deactivateInRange)
            {
                go.SetActive(!inToggleRange);
            }
        }
    }
}
