using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

namespace Skritty.Tools.Utilities
{
    [Flags]
    public enum AppState
    {
        All = -1,
        None = 0,
        TitleScreen = 1,
        Options = 2,
        CharacterSelect = 4,
        CharacterInfo = 8,
        Pause = 16,
        Map = 32
    };

    [CreateAssetMenu(menuName = "Application State", fileName = "Application State")]
    public class ApplicationState : ScriptableObject
    {
        #region Static
        // Change this to the initial state!
        private static AppState _currentState = AppState.TitleScreen;
        public static AppState CurrentState
        {
            get
            {
                return _currentState;
            }
            private set
            {
                _currentState = value;
                OnStateChanged?.Invoke(value);
            }
        }

        public static Action<AppState> OnStateChanged;

        static ApplicationState()
        {
            OnStateChanged = null;
        }

        /// <summary>
        /// Will enable passed in UI panel and any others assigned to that state
        /// </summary>
        private static void SetState(ApplicationState state)
        {
            // If this app state can coexist with what states are existing
            if ((CurrentState & state.BlockedBy) == AppState.None)
            {
                SetStateInternal(state);
            }
            Debug.Log($"Current Application States: {CurrentState}");
        }

        private static void SetStateInternal(ApplicationState state)
        {
            // Toggle states off if they are all already on
            if (state.Toggle && CurrentState.HasFlag(state.States))
            {
                CurrentState &= ~state.States;
                StateDisabled(state);

            }
            // Otherwise, add the states
            else
            {
                CurrentState &= state.IgnoreOverride;
                CurrentState |= state.States;
                StateEnabled(state);
            }
        }

        private static void StateEnabled(ApplicationState state)
        {
            LoadScene(state);

            if (state.LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (state.Pause)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;

            state.OnActivate?.Invoke();
        }

        private static void StateDisabled(ApplicationState state)
        {
            LoadScene(state, true);

            if (state.Pause)
                Time.timeScale = 1;
        }

        private static void LoadScene(ApplicationState state, bool unload = false)
        {
            foreach (SceneToLoad s in state.Scenes)
            {
                int index = s.Scene;
                if (s.UseDynamicScene && s.DataStore && s.DataStore.Has(s.Key))
                {
                    index = s.DataStore.Get<int>(s.Key);
                }

                if (!unload && index >= 0 && index < SceneManager.sceneCountInBuildSettings && !IsSceneLoaded(index))
                {
                    if (s.ConcurrentLoad)
                        SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
                    else
                        SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
                }

                if (unload && s.ConcurrentLoad && index >= 0 && index < SceneManager.sceneCountInBuildSettings && IsSceneLoaded(index))
                {
                    SceneManager.UnloadSceneAsync(index);
                }
            }
        }

        private static bool IsSceneLoaded(int buildIndex)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).buildIndex == buildIndex)
                    return true;
            return false;
        }
        #endregion

        #region Scriptable Object
        [field: SerializeField, Tooltip("States to be set"), Header("State Info")]
        public AppState States { get; private set; }
        [field: SerializeField, Tooltip("Cannot be set if any of these states are active")]
        public AppState BlockedBy { get; private set; }
        [field: SerializeField, Tooltip("Does not override the selected states")]
        public AppState IgnoreOverride { get; private set; }
        [field: SerializeField, Tooltip("Will toggle states (and scenes) if set multiple times")]
        public bool Toggle { get; private set; } = false;

        [field: SerializeField, Tooltip("Build index. -1 is do not load"), Header("Scene Loading")]
        public SceneToLoad[] Scenes { get; private set; } = new SceneToLoad[0];
        [Serializable]
        public class SceneToLoad
        {
            [field: SerializeField]
            public bool UseDynamicScene { get; private set; } = false;
            [field: SerializeField, Tooltip("Build index. -1 is do not load"), ShowIf("@!UseDynamicScene")]
            public int Scene { get; private set; } = -1;
            [field: SerializeField, Tooltip("The Key in the Data Store to check"), ShowIf("@UseDynamicScene")]
            public string Key { get; private set; }
            [field: SerializeField, Tooltip("The Data Store to check"), ShowIf("@UseDynamicScene")]
            public PersistantDataStore DataStore { get; private set; }
            [field: SerializeField, Tooltip("When this state is toggled, it will unload the scene")]
            public bool ConcurrentLoad { get; private set; } = false;
        }

        public UnityEngine.Events.UnityAction OnActivate;
        [field: SerializeField, Header("Misc")]
        public bool LockCursor { get; private set; } = false;
        [field: SerializeField]
        public bool Pause { get; private set; } = false;

        public void SetStateActive()
        {
            SetState(this);
        }
        #endregion
    }
}