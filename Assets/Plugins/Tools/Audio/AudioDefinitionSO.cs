using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Skritty.Tools.Audio
{
    [CreateAssetMenu(fileName = "Audio Definition", menuName = "Audio Definition")]
    public class AudioDefinitionSO : ScriptableObject
    {
        public enum AudioClipType { SoundEffect, MusicTrack }
        public enum SFXOrderMethod { Random, Step, Chain }
        private string MissingManagerError => $"{name} SFX could not play because there was no Audio Manager in the scene!";

        [field: SerializeField]
        public AudioClipType ClipType { get; private set; }

        #region SFX Settings
        [field: SerializeField, FoldoutGroup("SFX Settings"), ShowIf("@ClipType == AudioClipType.SoundEffect")]
        public SFXOrderMethod OrderMethod { get; private set; }
        public int CurrentStep { get; set; }
        [field: SerializeField, FoldoutGroup("SFX Settings"), ShowIf("@ClipType == AudioClipType.SoundEffect"), Range(0, 32)]
        public int MaximumSimultaneousAllowed { get; private set; } = 4;
        [field: SerializeField, FoldoutGroup("SFX Settings"), ShowIf("@ClipType == AudioClipType.SoundEffect")]
        public AudioClip[] Sounds { get; private set; }
        [field: SerializeField, FoldoutGroup("SFX Settings"), ShowIf("@ClipType == AudioClipType.SoundEffect"), MinMaxSlider(-2f, 2f, ShowFields = true), Tooltip("In Octaves")]
        public Vector2 RandomPitchVariance { get; private set; }
        [field: SerializeField, FoldoutGroup("SFX Settings"), ShowIf("@ClipType == AudioClipType.SoundEffect"), Min(0f)]
        public float InitialDelay { get; private set; }
        [field: SerializeField, FoldoutGroup("SFX Settings"), ShowIf("@ClipType == AudioClipType.SoundEffect")]
        public Subtitles[] Subtitles { get; private set; }
        #endregion

        #region Song Settings
        [field: SerializeField, FoldoutGroup("Song Settings"), ShowIf("@ClipType == AudioClipType.MusicTrack")]
        public AudioClip Song { get; private set; }
        [field: SerializeField, FoldoutGroup("Song Settings"), ShowIf("@ClipType == AudioClipType.MusicTrack"), Min(0f)]
        public float FadeInTime { get; private set; }
        [field: SerializeField, FoldoutGroup("Song Settings"), ShowIf("@ClipType == AudioClipType.MusicTrack"), Min(0f)]
        public float FadeOutTime { get; private set; }
        #endregion

        #region Audio Source Settings
        [field: SerializeField, FoldoutGroup("Audio Source Settings"), Required]
        public AudioMixerGroup Output { get; private set; }
        [field: SerializeField, FoldoutGroup("Audio Source Settings")]
        public bool Loop { get; private set; } = false;
        [field: SerializeField, Range(0, 256), FoldoutGroup("Audio Source Settings")]
        public int Priority { get; private set; } = 128;
        [field: SerializeField, Range(0f, 1f), FoldoutGroup("Audio Source Settings")]
        public float Volume { get; private set; } = 1f;
        [field: SerializeField, Range(-3f, 3f), FoldoutGroup("Audio Source Settings")]
        public float Pitch { get; private set; } = 1f;
        [field: SerializeField, Range(-1f, 1f), FoldoutGroup("Audio Source Settings")]
        public float StereoPan { get; private set; } = 0f;
        [field: SerializeField, Range(0f, 1f), FoldoutGroup("Audio Source Settings")]
        public float SpatialBlend { get; private set; } = 0f;
        [field: SerializeField, Range(0f, 1.1f), FoldoutGroup("Audio Source Settings")]
        public float ReverbZoneMix { get; private set; } = 1f;
        #endregion

        public void Play()
        {
            if(AudioManager.Instance)
            {
                AudioManager.Instance.Play(this, Camera.main.transform);
            }
            else
            {
                Debug.LogWarning($"{name} SFX could not play because there was no Audio Manager in the scene!");
            }
        }

        public void Play(Transform position)
        {
            if(AudioManager.Instance)
            {
                AudioManager.Instance.Play(this, position.position);
            }
            else
            {
                Debug.LogWarning(MissingManagerError);
            }
        }

        public void Play(Vector3 position)
        {
            if(AudioManager.Instance)
            {
                AudioManager.Instance.Play(this, position);
            }
            else
            {
                Debug.LogWarning(MissingManagerError);
            }
        }

        public void PlayFollowing(Transform target)
        {
            if(AudioManager.Instance)
            {
                AudioManager.Instance.Play(this, target);
            }
            else
            {
                Debug.LogWarning(MissingManagerError);
            }
        }

        public void Stop()
        {
            //TODO:Stop a sound
        }
        public void Pause()
        {
            //TODO:pause a sound
        }

    }
}
