using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Skritty.Tools.Utilities;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace Skritty.Tools.AppStateSystem {

    public class ApplicationStateManager : Singleton<ApplicationStateManager>
    {
    
        [TitleGroup("Settings"), SerializeField, ReadOnly]
        private List<ApplicationState> allStates;
        [SerializeField]
        private ApplicationState defaultState;
        [SerializeField, Tooltip("Folder path searched under the Resources/ folder for ApplicationStates"), ReadOnly]
        private string resourcesPath = "App States";

        [TitleGroup("State Info"), SerializeField, ReadOnly, LabelText("Active States")]
        private List<ApplicationState> activeStatesList = new List<ApplicationState>();
        [SerializeField, ReadOnly, LabelText("Active Substates")]
        private List<ApplicationState> activeSubstatesList = new List<ApplicationState>();
        [field: SerializeField, ReadOnly]
        public bool IsLoadingScene { get; private set; }

        [TitleGroup("Time Scale Values"), SerializeField, ReadOnly]
        private float currentTimeScale = 1f;
        [SerializeField, ReadOnly]
        private float previousTimeScale = 1f;

        // ---

        private Object[] statesAsObject;

        private BitArray ActiveStates { get; set; }

        public Action OnStateChanged;
        public Action OnSceneChange;

        // ----------------------------------------------------------------------------------------------------------

#region Unity Functions

        protected override void Awake() {
            base.Awake();

            FetchStatesFromResourcesFolder();
            InitializeStateIDs();
            ActiveStates = new BitArray(allStates.Count);
        }

        protected void Start()
        {
            InitializeAppStateActivity();
            OnStateChanged += ControlSubstates;
            OnStateChanged += UpdateStateList;
            OnSceneChange += InitializeAppStateActivity;

            SetState(defaultState);
        }

#if UNITY_EDITOR
        private void LateUpdate() {
            currentTimeScale = Time.timeScale;
        }
#endif

#endregion

        // ----------------------------------------------------------------------------------------------------------

#region Initialization

        //[TitleGroup("Buttons")] [Button, HideInPlayMode]
        private void FetchStatesFromResourcesFolder() {
            allStates.Clear();
            statesAsObject = Resources.LoadAll(resourcesPath, typeof(ApplicationState));
            for(int i = 0; i < statesAsObject.Length; i++) {
                allStates.Add((ApplicationState)statesAsObject[i]);
            }
        }

        //[TitleGroup("Buttons")] [Button, HideInPlayMode]
        private void InitializeStateIDs() {
            for(int i = 0; i < allStates.Count; i++) {
                allStates[i].StateID = i;
            }
        }

        private void InitializeAppStateActivity() {
            int count = 0;
            foreach(ApplicationStateBasedActivity activity in FindObjectsOfType<ApplicationStateBasedActivity>(true)) {
                if(activity.Initialize())
                    count++;
            }
            //Debug.Log($"ApplicationStateManager initialized {count} ApplicationStateBasedActivity scripts");
        }

#endregion

        // ----------------------------------------------------------------------------------------------------------

#region State Control

        public void SetState(ApplicationState state) {
            if (state == null) return;
            //Debug.Log($"Setting app state to {state.name}");
            if(!IsBlocked(state)) {
                // Toggle states off if they are already on
                if(state != null && state.Toggle && IsStateActive(state)) {
                    ActiveStates[state.StateID] = false;
                    OnDisableState(state);
                }
                // Otherwise, add the state
                else {
                    // Override
                    CheckStateOverride(state);

                    // Set state
                    ActiveStates[state.StateID] = true;
                    OnEnableState(state);
                    //ControlSubstates();
                }

                // Update
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Adds given state onto current state list, then fully changes states after condition is met
        /// </summary>
        /// <param name="state">State to transition into</param>
        /// <param name="endCondition">Condition on when to end transition</param>
        /// <param name="timeout">If greater than 0, the amount of time before the transition ends on its own</param>
        /// <returns></returns>
        public IEnumerator BlendIntoState(ApplicationState state, Func<bool> endCondition, float timeout = 0f) {
            ActiveStates[state.StateID] = true;
            OnEnableState(state);
            OnStateChanged?.Invoke();

            float timer = 0f;
            while(!endCondition.Invoke()) {
                yield return null;
                if(timeout > 0) {
                    timer += Time.deltaTime;
                    if(timer >= timeout)
                        break;
                }
            }

            CheckStateOverride(state);
            //ControlSubstates();
            OnStateChanged?.Invoke();
        }

        // ---

        public bool IsBlocked(ApplicationState state) {
            if (state == null) return false;
            foreach(ApplicationState otherState in state.BlockedBy) {
                if(ActiveStates[otherState.StateID])
                    return true;
            }
            foreach(ApplicationState otherState in state.SubstateOf) {
                if(!ActiveStates[otherState.StateID])
                    return true;
            }
            return false;
        }

        public void CheckStateOverride(ApplicationState state) {
            if(!state.DoNotOverrideAny) {
                for(int i = 0; i < allStates.Count; i++) {
                    if(i == state.StateID)
                        continue;

                    bool stateActive = ActiveStates[i]; // && state.DoNotOverride.Contains(allStates[i]);
                    if(!allStates[i].CantOverrideByDefault) {
                        stateActive &= state.DoNotOverride.Contains(allStates[i]);
                    } else {
                        if(state.AlwaysOverride.Contains(allStates[i]))
                            stateActive = false;
                    }

                    if(ActiveStates[i] && !stateActive)
                        OnDisableState(allStates[i]);
                    ActiveStates[i] = stateActive;
                }
            }
        }

        // ---

#region On Enable/Disable

        /// <summary>
        /// Enables given state
        /// </summary>
        /// <param name="state">State to enable</param>
        public void OnEnableState(ApplicationState state) {
            if(state.LockCursor) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if(state.ControlTimeScale) {
                switch(state.UpdateTimeScaleOnEnable) {
                    case TimeScaleControlOptions.Set:
                        SetTimeScale(state.TimeScaleOnEnable);
                        break;
                    case TimeScaleControlOptions.Revert:
                        RevertTimeScale();
                        break;
                }
            }
        }

        /// <summary>
        /// Disables given state if that state is set to Toggle on/off
        /// </summary>
        /// <param name="state">State to disable</param>
        public void OnDisableState(ApplicationState state) {
            if(state.ControlTimeScale) {
                switch(state.UpdateTimeScaleOnDisable) {
                    case TimeScaleControlOptions.Set:
                        SetTimeScale(state.TimeScaleOnDisable);
                        break;
                    case TimeScaleControlOptions.Revert:
                        RevertTimeScale();
                        break;
                }
            }
        }

#endregion

        // ---

        /// <summary>
        /// Disables substates if their parent states are no longer active
        /// </summary>
        public void ControlSubstates() {
            foreach(ApplicationState substate in activeSubstatesList) {
                foreach(ApplicationState parentState in substate.SubstateOf) {
                    if(!ActiveStates[parentState.StateID] && ActiveStates[substate.StateID]) {
                        ActiveStates[substate.StateID] = false;
                        OnDisableState(substate);
                    }
                }
            }
        }

        /// <summary>
        /// Updates list of states in inspector to match BitArray and actual state list
        /// </summary>
        private void UpdateStateList() {
            activeStatesList.Clear();
            activeSubstatesList.Clear();
            for(int i = 0; i < allStates.Count; i++) {
                if(ActiveStates[i]) {
                    activeStatesList.Add(allStates[i]);
                    if(allStates[i].SubstateOf.Count > 0)
                        activeSubstatesList.Add(allStates[i]);
                }
            }
        }

#endregion

        // ----------------------------------------------------------------------------------------------------------

#region Check States

        public bool IsStateActive(ApplicationState state) {
            return ActiveStates[state.StateID];
        }

        /// <summary>
        /// Whether or not every ApplicationState given is currently active
        /// </summary>
        /// <param name="states">List of ApplicationStates to check</param>
        /// <returns>Whether or not every ApplicationState given is currently active</returns>
        public bool AreAllStatesActive(List<ApplicationState> states, bool matchExactly = false) {
            if(matchExactly && states.Count != activeStatesList.Count)
                return false;

            foreach(ApplicationState state in states) {
                if(!ActiveStates[state.StateID])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Whether or not at least one of the given ApplicationStates is currently active
        /// </summary>
        /// <param name="states">List of ApplicationStates to check</param>
        /// <returns>Whether or not at least one of the given ApplicationStates is currently active</returns>
        public bool HasAMatchingState(List<ApplicationState> states) {
            foreach(ApplicationState state in states) {
                if(ActiveStates[state.StateID])
                    return true;
            }
            return false;
        }

        public int ActiveStateCount() {
            return activeStatesList.Count;
        }

#endregion

        // ----------------------------------------------------------------------------------------------------------

#region Time Controls

        private void SetTimeScale(float newTimeScale) {
            previousTimeScale = Time.timeScale;
            Time.timeScale = newTimeScale;
        }

        private void RevertTimeScale() {
            (previousTimeScale, Time.timeScale) = (Time.timeScale, previousTimeScale); // Swap values
        }

#endregion

        // ----------------------------------------------------------------------------------------------------------

#region Scene Management

        public IEnumerator LoadScene(int index, LoadSceneMode mode) {
            Debug.Log($"Loading scene index {index}");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(index, mode);
            yield return new WaitUntil(() => asyncLoad.isDone);
            Instance.OnSceneChange?.Invoke();
        }

        public IEnumerator LoadScene(string sceneName, LoadSceneMode mode) {
            Debug.Log($"Loading scene named \"{sceneName}\"");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return new WaitUntil(() => asyncLoad.isDone);
            Instance.OnSceneChange?.Invoke();
        }

        private static bool IsSceneLoaded(int buildIndex) {
            for(int i = 0; i < SceneManager.sceneCount; i++)
                if(SceneManager.GetSceneAt(i).buildIndex == buildIndex)
                    return true;
            return false;
        }

        public void SetIsLoadingScene(bool value) {
            IsLoadingScene = value;
        }

#endregion

        // ----------------------------------------------------------------------------------------------------------

#region Debug
#if UNITY_EDITOR

        public string ActiveStatesAsString() {
            string output = "";
            foreach(ApplicationState state in activeStatesList) {
                output += $"{state.name} / ";
            }
            return output;
        }

        [TitleGroup("Buttons")] [Button]
        private void ManuallySetState(ApplicationState state) {
            state.SetStateActive();
        }

        [TitleGroup("Buttons")] [Button]
        private void ManuallySetTimeScale(float newTimeScale = 1f, bool updatePreviousTimeScale = false) {
            if(updatePreviousTimeScale)
                SetTimeScale(newTimeScale);
            else
                Time.timeScale = newTimeScale;
        }

#endif
#endregion

    }
}
#endif