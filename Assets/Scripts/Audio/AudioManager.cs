using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("General Music Settings")]
    public AudioSource musicSource;            // Primary music audio source
    public AudioSource[] musicLayers;          // Additional layers/channels for flavor
    public float crossfadeDuration = 2f;       // Duration of crossfade in seconds

    [Header("Spatial Sound Settings")]
    public AudioMixerGroup sfxMixerGroup;      // Mixer group for spatial sound effects
    public float maxDistance = 10f;            // Default maximum distance for spatial drop-off
    public int maxDuplicates = 3;

    [Header("Master Volume Settings")]
    private float _masterMusicVolume = 1f;
    private float _masterSFXVolume = 1f;

    public float MasterMusicVolume
    {
        get => _masterMusicVolume;
        set
        {
            _masterMusicVolume = Mathf.Clamp(value, 0f, 1f);
            UpdateMusicVolume(); // Apply new volume to all music sources
        }
    }

    public float MasterSFXVolume
    {
        get => _masterSFXVolume;
        set
        {
            _masterSFXVolume = Mathf.Clamp(value, 0f, 1f);
            UpdateSFXVolume(); // Apply new volume to all SFX sources
        }
    }

    private Dictionary<AudioClip, int> playingSFXInstances = new Dictionary<AudioClip, int>();

    // List to track active sound effects
    private List<ActiveSFX> activeSFXList = new List<ActiveSFX>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Update and remove finished sound effects
        for (int i = activeSFXList.Count - 1; i >= 0; i--)
        {
            activeSFXList[i].UpdateSFX();

            if (activeSFXList[i].IsFinished)
            {
                DecreaseSFXInstanceCount(activeSFXList[i].Clip);
                activeSFXList.RemoveAt(i);
            }
        }
    }

    #region Music Management

    // Play a music track with optional flavor layers
    public void PlayMusic(AudioClip mainTrack, AudioClip[] flavorLayers = null)
    {
        if (Bootstrap.IsServer()) return;

        musicSource.clip = mainTrack;
        musicSource.loop = true;
        musicSource.Play();
        musicSource.DOFade(_masterMusicVolume, crossfadeDuration); // Fade in the main music track

        if (flavorLayers != null)
        {
            for (int i = 0; i < musicLayers.Length; i++)
            {
                if (i < flavorLayers.Length)
                {
                    musicLayers[i].clip = flavorLayers[i];
                    musicLayers[i].Play();
                    musicLayers[i].DOFade(_masterMusicVolume, crossfadeDuration); // Fade in flavor layers
                }
                else
                {
                    musicLayers[i].Stop();
                }
            }
        }
    }

    public void CrossfadeMusic(AudioClip newTrack)
    {
        if (Bootstrap.IsServer()) return;

        // Check if the new track is the same as the currently playing track
        if (musicSource.clip == newTrack)
        {
            Debug.Log("Attempted to crossfade into the same track. Operation aborted.");
            return; // Early exit if the track is already playing
        }

        // Fade out the current music track
        musicSource.DOFade(0f, crossfadeDuration).OnComplete(() =>
        {
            // Once faded out, stop the current track and assign the new one
            musicSource.Stop();
            musicSource.clip = newTrack;
            musicSource.loop = true;
            musicSource.Play();

            // Fade in the new track
            musicSource.DOFade(_masterMusicVolume, crossfadeDuration);
        });

        // Optionally fade out any flavor layers if they are playing
        foreach (var layer in musicLayers)
        {
            if (layer.isPlaying)
            {
                layer.DOFade(0f, crossfadeDuration).OnComplete(() =>
                {
                    layer.Stop(); // Stop the layer after fade-out
                });
            }
        }
    }

    private void UpdateMusicVolume()
    {
        if (musicSource.isPlaying)
        {
            musicSource.volume = _masterMusicVolume;
        }

        foreach (var layer in musicLayers)
        {
            if (layer.isPlaying)
            {
                layer.volume = _masterMusicVolume;
            }
        }
    }

    #endregion

    #region Sound Effects

    // Play a spatial sound effect with volume drop-off based on distance to the listener
    public void PlaySpatialSFX(AudioClip clip, Vector3 position, bool ignoreAttenuation = false, float volume = 1.0f)
    {
        if (clip == null)
        {
            Debug.LogWarning("NULL AUDIO CLIP");
            return;
        }

        if (Bootstrap.IsServer()) return;

        // Check if the number of playing instances exceeds the max duplicates limit
        if (!playingSFXInstances.ContainsKey(clip))
        {
            playingSFXInstances[clip] = 0;
        }

        if (playingSFXInstances[clip] >= maxDuplicates)
        {
            Debug.Log("Max duplicates reached for clip: " + clip.name);
            return;
        }

        // Increment the instance count for this clip
        playingSFXInstances[clip]++;

        // Create a new GameObject for the spatial sound effect
        GameObject sfxObject = new GameObject("SpatialSFX_" + clip.name);
        AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();

        sfxSource.clip = clip;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        //Debug
        if (ignoreAttenuation)
        {
            // Set the sound to 2D by using spatialBlend = 0 (ignores distance attenuation)
            sfxSource.spatialBlend = 0f; // Fully 2D
        }
        else
        {
            // Set the sound to 3D with attenuation based on distance
            sfxSource.spatialBlend = 1f; // Fully 3D
            sfxSource.minDistance = 1f;
            sfxSource.maxDistance = maxDistance;
            sfxSource.rolloffMode = AudioRolloffMode.Linear; // Linear attenuation
        }

        //sfxSource.volume = _masterSFXVolume; // Apply global SFX volume
        sfxSource.volume = _masterSFXVolume * volume;
        sfxSource.transform.position = position;
        sfxSource.Play();

        // Track this SFX instance
        activeSFXList.Add(new ActiveSFX(clip, sfxObject, sfxSource, clip.length));
    }

    // Decrease the number of instances of a given clip
    private void DecreaseSFXInstanceCount(AudioClip clip)
    {
        if (playingSFXInstances.ContainsKey(clip))
        {
            playingSFXInstances[clip]--;
            if (playingSFXInstances[clip] <= 0)
            {
                playingSFXInstances.Remove(clip);
            }
        }
    }

    private void UpdateSFXVolume()
    {
        // Apply the master volume to all active SFX sources
        foreach (var activeSFX in activeSFXList)
        {
            activeSFX.Source.volume = _masterSFXVolume;
        }
    }

    #endregion

    // Helper class to track active sound effects
    private class ActiveSFX
    {
        public AudioClip Clip { get; private set; }
        public GameObject SFXObject { get; private set; }
        public AudioSource Source { get; private set; }
        public float Duration { get; private set; }
        private float elapsedTime = 0f;

        public bool IsFinished => elapsedTime >= Duration;

        public ActiveSFX(AudioClip clip, GameObject sfxObject, AudioSource source, float duration)
        {
            Clip = clip;
            SFXObject = sfxObject;
            Source = source;
            Duration = duration;
        }

        // Update the elapsed time and destroy the SFX object when finished
        public void UpdateSFX()
        {
            elapsedTime += Time.deltaTime;
            if (IsFinished)
            {
                Destroy(SFXObject);
            }
        }
    }
}
