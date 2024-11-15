using Skritty.Tools.Utilities;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using DG.Tweening;

namespace Skritty.Tools.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField, Required]
        private AudioMixer mixer;
        [SerializeField]
        private TMPro.TextMeshProUGUI subtitles;
        [SerializeField]
        private UnityEngine.UI.Image subsBackground;

        [System.Serializable]
        public class AudioGroup
        {
            [Required]
            public PooledObject defaultSFXSource;
            [ReadOnly]
            public List<AudioSource> audioSources = new();
            [ReadOnly]
            public List<AudioDefinitionSO> audioDefinitions = new();
        }

        [SerializeField, ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout, KeyLabel = "Group", ValueLabel = "Settings")]
        private SerializedDictionary<AudioMixerGroup, AudioGroup> audioGroups = new();

        private void OnValidate()
        {
            if(mixer)
            {
                foreach(AudioMixerGroup group in mixer.FindMatchingGroups(string.Empty))
                {
                    if(audioGroups.ContainsKey(group)) continue;
                    audioGroups.Add(group, new AudioGroup());
                }
            }
        }

        private void Start()
        {
            if (subtitles)
            {
                subtitles.text = "";
                subsBackground.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Tries to get an audio source on a gameObject.
        /// </summary>
        /// <returns>If it was successful</returns>
        private bool TryGetAudioSource(out AudioSource source, AudioDefinitionSO audioDefinition)
        {
            AudioGroup group = audioGroups[audioDefinition.Output];
            if(audioDefinition.ClipType == AudioDefinitionSO.AudioClipType.SoundEffect && group.audioSources.Count >= audioDefinition.MaximumSimultaneousAllowed)
            {
                ConsoleLog.Log($"Audio Definition {audioDefinition.name} has too many active instances, cannot play.");
                source = null;
                return false;
            }

            if(audioDefinition.ClipType == AudioDefinitionSO.AudioClipType.MusicTrack && group.audioDefinitions.Contains(audioDefinition))
            {
                source = group.audioSources[group.audioDefinitions.IndexOf(audioDefinition)];
                return true;
            }

            GameObject sourceGameObject = group.defaultSFXSource.RequestObject<PooledObject>().gameObject;

            if(sourceGameObject == null)
            {
                ConsoleLog.Log($"Audio Definition {audioDefinition.name} has too many active instances, cannot play.");
                source = null;
                return false;
            }

            source = sourceGameObject.GetComponent<AudioSource>();
            group.audioSources.Add(source);
            group.audioDefinitions.Add(audioDefinition);
            source.outputAudioMixerGroup = audioDefinition.Output;
            source.loop = audioDefinition.Loop;
            source.volume = audioDefinition.ClipType == AudioDefinitionSO.AudioClipType.MusicTrack ? 0.01f : audioDefinition.Volume;
            source.priority = audioDefinition.Priority;
            source.pitch = audioDefinition.Pitch;
            source.panStereo = audioDefinition.StereoPan;
            source.spatialBlend = audioDefinition.SpatialBlend;
            source.reverbZoneMix = audioDefinition.ReverbZoneMix;
            return true;
        }

        public void ToggleAllSounds(bool paused)
        {
            foreach(AudioGroup group in audioGroups.Select(g => g.Value))
            {
                group.audioSources.ForEach(x =>
                {
                    if(x == null) return;
                    if(paused) x.Pause();
                    else x.UnPause();
                });
            }
        }

        public AudioSource Play(AudioDefinitionSO audioDefinition, Vector3 position)
        {
            if(audioDefinition.Sounds.Length == 0)
            {
                ConsoleLog.Log($"Audio Definition {audioDefinition.name} has no sounds to play, but is being told to play!");
                return null;
            }

            if(TryGetAudioSource(out AudioSource source, audioDefinition))
            {
                source.transform.position = position;
                StartCoroutine(DoPlaySound(audioDefinition, source));
                StartCoroutine(DoSubtitles(audioDefinition, source));
                return source;
            }
            else
            {
                return null;
            }
        }

        public AudioSource Play(AudioDefinitionSO audioDefinition, Transform position)
        {
            if((audioDefinition.ClipType == AudioDefinitionSO.AudioClipType.SoundEffect && audioDefinition.Sounds.Length == 0) 
                || (audioDefinition.ClipType == AudioDefinitionSO.AudioClipType.MusicTrack && audioDefinition.Song == null))
            {
                ConsoleLog.Log($"Audio Definition {audioDefinition.name} has no sounds to play, but is being told to play!");
                return null;
            }

            if(TryGetAudioSource(out AudioSource source, audioDefinition))
            {
                source.transform.position = position.position;
                StartCoroutine(DoPlaySound(audioDefinition, source));
                StartCoroutine(DoSubtitles(audioDefinition, source));
                StartCoroutine(DoFollowTransform(source, position));
                return source;
            }
            else
            {
                return null;
            }
        }

        private IEnumerator DoFollowTransform(AudioSource source, Transform position)
        {
            while(source && source.isPlaying && position != null)
            {
                if(source)
                {
                    source.transform.position = position.position;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator DoPlaySound(AudioDefinitionSO audioDefinition, AudioSource source)
        {
            AudioGroup group = audioGroups[audioDefinition.Output];
            bool alreadyPlaying = false;

            if (audioDefinition.InitialDelay > 0)
            {
                yield return new WaitForSeconds(audioDefinition.InitialDelay);
            }

            switch(audioDefinition.ClipType)
            {
                case AudioDefinitionSO.AudioClipType.MusicTrack:
                {
                    foreach(AudioSource track in group.audioSources)
                    {
                        if(track == source)
                        {
                            track.DOKill();
                            float percentComplete = track.volume / audioDefinition.Volume;
                            track.DOFade(audioDefinition.Volume, audioDefinition.FadeInTime * percentComplete);
                            if(!source.isPlaying)
                            {
                                source.clip = audioDefinition.Song;
                                source.Play();
                            }
                            else
                            {
                                alreadyPlaying = true;
                            }
                        }
                        else
                        {
                            track.DOKill();
                            float percentComplete = track.volume / group.audioDefinitions[group.audioSources.IndexOf(track)].Volume;
                            track.DOFade(0, group.audioDefinitions[0].FadeOutTime * percentComplete);
                        }
                    }
                    break;
                }
                case AudioDefinitionSO.AudioClipType.SoundEffect:
                {
                    switch(audioDefinition.OrderMethod)
                    {
                        case AudioDefinitionSO.SFXOrderMethod.Random:
                        {
                            source.clip = audioDefinition.Sounds[Random.Range(0, audioDefinition.Sounds.Length)];
                            source.pitch = Mathf.Pow(2f, Random.Range(audioDefinition.RandomPitchVariance.x, audioDefinition.RandomPitchVariance.y));
                            source.Play();
                            break;
                        }
                        case AudioDefinitionSO.SFXOrderMethod.Step:
                        {
                            source.clip = audioDefinition.Sounds[++audioDefinition.CurrentStep];
                            source.pitch = Mathf.Pow(2f, Random.Range(audioDefinition.RandomPitchVariance.x, audioDefinition.RandomPitchVariance.y));
                            source.Play();
                            break;
                        }
                        case AudioDefinitionSO.SFXOrderMethod.Chain:
                        {
                            foreach(AudioClip c in audioDefinition.Sounds)
                            {
                                source.clip = c;
                                source.pitch = Mathf.Pow(2f, Random.Range(audioDefinition.RandomPitchVariance.x, audioDefinition.RandomPitchVariance.y));
                                source.Play();
                                yield return new WaitWhile(() => source.isPlaying);
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            if(!alreadyPlaying)
            {
                yield return new WaitUntil(() => !source || (Application.isFocused && !source.isPlaying) || source.volume == 0);
                if(source)
                {
                    if(audioDefinition.ClipType == AudioDefinitionSO.AudioClipType.MusicTrack)
                    {
                        source.Pause();
                    }
                    else
                    {
                        source.Stop();
                    }
                    source.GetComponent<PooledObject>().ReleaseObject();
                    group.audioSources.Remove(source);
                    group.audioDefinitions.Remove(audioDefinition);
                }
            }
        }

        private IEnumerator DoSubtitles(AudioDefinitionSO SFX, AudioSource source)
        {
            if (SFX.InitialDelay > 0)
                yield return new WaitForSeconds(SFX.InitialDelay);
            float maxDuration = 0;
            float currentTime = 0f;
            switch(SFX.OrderMethod)
            {
                case AudioDefinitionSO.SFXOrderMethod.Random:
                {
                    // Figure out how long to keep the coroutine running for
                    if(source && source.clip)
                        maxDuration = source.clip.length;
                    foreach(Subtitles subs in SFX.Subtitles)
                        if(subs.startTime + subs.duration > maxDuration)
                            maxDuration = subs.startTime + subs.duration;
                    break;
                }
                case AudioDefinitionSO.SFXOrderMethod.Step:
                {
                    break;
                }
                case AudioDefinitionSO.SFXOrderMethod.Chain:
                {
                    // Figure out how long to keep the coroutine running for
                    foreach(AudioClip c in SFX.Sounds)
                        maxDuration += c.length;
                    foreach(Subtitles subs in SFX.Subtitles)
                        if(subs.startTime + subs.duration > maxDuration)
                            maxDuration = subs.startTime + subs.duration;
                    break;
                }
            }

            while (currentTime < maxDuration)
            {
                foreach (Subtitles subs in SFX.Subtitles)
                {
                    // Check to see if its time to play this subtitle
                    if (subs.startTime > currentTime - Time.deltaTime && subs.startTime <= currentTime)
                    {
                        if (subs.subtitles != "" && (subtitles.text == "" || subs.overrideOtherSubs))
                        {
                            subtitles.text = subs.subtitles;
                            subsBackground.gameObject.SetActive(true);
                        }
                    }

                    // Check to see if its time to end this subtitle
                    if (subs.customDuration && subs.startTime + subs.duration > currentTime - Time.deltaTime && subs.startTime + subs.duration <= currentTime)
                    {
                        if (subtitles.text == subs.subtitles)
                        {
                            subtitles.text = "";
                            subsBackground.gameObject.SetActive(false);
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
                currentTime += Time.deltaTime;
            }

            // Clear any active subtitles
            foreach (Subtitles subs in SFX.Subtitles)
                if (subtitles.text == subs.subtitles)
                {
                    subtitles.text = "";
                    subsBackground.gameObject.SetActive(false);
                }
        }
    }
}