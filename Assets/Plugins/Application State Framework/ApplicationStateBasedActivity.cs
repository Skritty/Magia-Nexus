using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// Place this script on the UI panel object
    /// </summary>
    public class ApplicationStateBasedActivity : MonoBehaviour
    {
        /// <summary>
        /// The states that enable this panel
        /// </summary>
        [SerializeField, Tooltip("The states that enable this panel")]
        private AppState activeStates;
        [SerializeField]
        private bool controlSelfActivity;
        [SerializeField]
        public UnityEngine.Events.UnityEvent OnEnter;
        [SerializeField]
        public UnityEngine.Events.UnityEvent OnExit;

        private void Awake()
        {
            if (controlSelfActivity)
            {
                OnEnter.AddListener(() => gameObject.SetActive(true));
                OnExit.AddListener(() => gameObject.SetActive(false));
            }
            CheckState(ApplicationState.CurrentState);
            ApplicationState.OnStateChanged += CheckState;
        }

        private void OnDestroy()
        {
            ApplicationState.OnStateChanged -= CheckState;
        }

        private void CheckState(AppState currentState)
        {
            if (((long)activeStates & (long)currentState) != 0)
            {
                OnEnter.Invoke();
            }
            else
            {
                OnExit.Invoke();
            }

        }
    }
}
