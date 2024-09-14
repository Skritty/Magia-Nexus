using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

namespace Skritty.Tools.AppStateSystem {

    public enum TimeScaleControlOptions { None, Set, Revert }

    // ---------------------------------------------------------------------------------------------------------

    [CreateAssetMenu(menuName = "Application State", fileName = "Application State")]
    public class ApplicationState : ScriptableObject
    {

        [field: SerializeField, ReadOnly, Tooltip("Controlled and used by the ApplicationStateManager, and is managed only at runtime. Should not be used anywhere else.")]
        public int StateID { get; set; } = -1;

        // ---

        [field: TitleGroup("State Interactions"), SerializeField, Tooltip("Requires all of the following states in order to be active")]
        public List<ApplicationState> SubstateOf { get; private set; }
        [field: SerializeField, Tooltip("Cannot be set if any of these states are active")]
        public List<ApplicationState> BlockedBy { get; private set; }

#region Override

        [field: SerializeField, Tooltip("If true, does not override any state"), FoldoutGroup("Override Controls")]
        public bool DoNotOverrideAny { get; private set; }
        [field: SerializeField, Tooltip("Whether or not the state can be overriden by default. Can still be overwritten by AlwaysOverride"), LabelText("Can\'t Override By Default"), FoldoutGroup("Override Controls")]
        public bool CantOverrideByDefault { get; private set; }

        [field: SerializeField, Tooltip("Does not override the selected states"), HideIf("@DoNotOverrideAny"), FoldoutGroup("Override Controls")]
        public List<ApplicationState> DoNotOverride { get; private set; }
        [field: SerializeField, Tooltip("States to always override, even if CantOverrideByDefault is true"), FoldoutGroup("Override Controls")]
        public List<ApplicationState> AlwaysOverride { get; private set; }

#endregion

        [field: SerializeField, Tooltip("Will toggle states (and scenes) if set multiple times")]
        public bool Toggle { get; private set; } = false;

        // ---

        [field: TitleGroup("Time Scale"), SerializeField]
        public bool ControlTimeScale { get; private set; } = false;

        [field: SerializeField, ShowIf("@ControlTimeScale"), FoldoutGroup("Time Scale Options")]
        public TimeScaleControlOptions UpdateTimeScaleOnEnable { get; private set; } = TimeScaleControlOptions.Set;
        [field: SerializeField, ShowIf("@ControlTimeScale && UpdateTimeScaleOnEnable == TimeScaleControlOptions.Set"), FoldoutGroup("Time Scale Options")]
        public float TimeScaleOnEnable { get; private set; } = 0f;

        [field: SerializeField, ShowIf("@ControlTimeScale"), FoldoutGroup("Time Scale Options")]
        public TimeScaleControlOptions UpdateTimeScaleOnDisable { get; private set; } = TimeScaleControlOptions.None;
        [field: SerializeField, ShowIf("@ControlTimeScale && UpdateTimeScaleOnDisable == TimeScaleControlOptions.Set"), FoldoutGroup("Time Scale Options")]
        public float TimeScaleOnDisable { get; private set; } = 1f;

        // ---

        [field: TitleGroup("Misc"), SerializeField]
        public bool LockCursor { get; private set; } = false;

        // ----------------------------------------------------------------------------------------------------------

        private void OnValidate() {
            if(!DoNotOverrideAny) {
                foreach(ApplicationState state in SubstateOf) {
                    if(!DoNotOverride.Contains(state))
                        DoNotOverride.Add(state);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------------

        public void SetStateActive()
        {
            ApplicationStateManager.Instance.SetState(this);
        }

        public bool IsStateActive => ApplicationStateManager.Instance.IsStateActive(this);

    }
}
#endif