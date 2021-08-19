using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Handles interaction with ELIAS. Plays ELIAS' output through the attached AudioSource.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EliasPlayer : MonoBehaviour
{
    //The file member matches the "Project" setting in the Inspector.
	[HideInInspector]
	public string file;

    //This is the index of a action preset. The inspector represents this as a list of strings.
	[HideInInspector]
	public int actionPreset = -1;

	[HideInInspector]
	public EliasSetLevel customPreset;

    [HideInInspector]
    public int eliasFramesPerBuffer = 4096;

    [HideInInspector]
    public int eliasCachePageCount = 128;

    [HideInInspector]
    public int eliasCachePageSize = 4096;

    [HideInInspector]
    //This is so that you can set what sample rate the Elias Project is meant to be run at. In the case that Unity uses a different sample rate, resampling will automatically occur.
    public int eliasSampleRate = -1;

    //Set this to 0 to allow Elias to run at the same sample rate as Unity.
    public int eliasChannelCount = 0;

    public bool playOnStart = true;

    //This variable exists to allow the initialization of the Elias Engine and this component without having to load a project when Start is run.
    //Note that this is NOT exposed to the inspector, as if it is set to false, it requires code to interact with the EliasPlayer for usage.
    [HideInInspector]
    public bool deserializeProjectOnStart = true;

    //This variable exists to allow the initialization of the Elias Engine with a different base path then what would normally be used by this plugin. (Streaming Assets).
    // This must be set before Start() is run!
    [HideInInspector]
    public string customBasePath = "";



    /* WARNING: This member is used across multiple threads simultaneously!
     * The EliasHelper is used to setup the Elias Engine, as well as for Utility functionality such as getting indexes of themes from their names.
     */
    public volatile EliasHelper elias = null;

    /* WARNING: This member is used across multiple threads simultaneously!
     * The EliasAudioReader class exists to separate the actual Audio handling away from the EliasPlayer. It's main point of entry is the OnAudioFilterRead function in EliasPlayer.
     */
    protected volatile EliasAudioReader audioReader;

    protected volatile bool isEliasStarted = false;

    //Change this if you want to use the high latency mode, it will allow unity to handle channel and sample rate conversions in exchange for a higher latency with ELIAS.
    protected volatile bool useHighLatencyMode = false;

    protected volatile bool shouldStop = false;
    
    public EliasHelper Elias
	{
		get
		{
			return elias;
		}
    }

    /// <summary>
    /// Stop the rendering of the theme. This function must not be called while any other Elias calls are in progress with the same handle in another thread.
    /// The exception is if the engine has its own background threads; these will automatically be shut down.
    /// IMPORTANT: This function must never use thread unsafe calls, as it can be called from either the audio thread in case of failure to render, or the main thread, (game).
    /// </summary>
    public void Stop()
	{
        shouldStop = false;
        isEliasStarted = false;
        if (elias.Handle != IntPtr.Zero)
        {
            EliasWrapper.elias_result_codes r = EliasWrapper.elias_stop(elias.Handle);
            EliasHelper.LogResult(r, "Problems stopping");
        }
	}

	/// <summary>
	/// Lock and unlock the mixer in order to allow many changes to be made without the risk of running one or more buffers ahead in between your changes.
	/// Note that multiple calls to elias_lock_mixer may be made in succession, as long as the same number of calls to elias_unlock_mixer are made (a recursive mutex, in other words).
	/// If you forget to unlock the mixer, the music will stop and you will get deadlocks.
	/// </summary>
	public void LockMixer()
    {
        if (elias.Handle == IntPtr.Zero)
            return;

        EliasWrapper.elias_result_codes r = EliasWrapper.elias_lock_mixer(elias.Handle);
		EliasHelper.LogResult(r, "Problems locking the mixer");
	}

	/// <summary>
	/// Lock and unlock the mixer in order to allow many changes to be made without the risk of running one or more buffers ahead in between your changes.
	/// Note that multiple calls to elias_lock_mixer may be made in succession, as long as the same number of calls to elias_unlock_mixer are made (a recursive mutex, in other words).
	/// If you forget to unlock the mixer, the music will stop and you will get deadlocks.
	/// </summary>
	public void UnlockMixer()
    {
        if (elias.Handle == IntPtr.Zero)
            return;

        EliasWrapper.elias_result_codes r = EliasWrapper.elias_unlock_mixer(elias.Handle);
		EliasHelper.LogResult(r, "Problems unlocking the mixer");
	}

	/// <summary>
	/// Clear all queued events.
	/// </summary>
	public void ClearEvents()
    {
        if (elias.Handle == IntPtr.Zero)
            return;

        EliasWrapper.elias_result_codes r = EliasWrapper.elias_clear_events(elias.Handle);
		EliasHelper.LogResult(r, "Problems clearing events");
	}
		
	/// <summary>
	/// Run an action preset.
	/// </summary>
	public void RunActionPreset(string preset, bool ignoreRequiredThemeMissmatch = false)
    {
        if (elias.Handle == IntPtr.Zero)
            return;

        EliasWrapper.elias_result_codes r = EliasWrapper.elias_run_action_preset(elias.Handle, preset);
		if (ignoreRequiredThemeMissmatch == false || r != EliasWrapper.elias_result_codes.elias_error_requiredthememismatch)
		{
			EliasHelper.LogResult(r, "Problems running an action preset");
		}
	}

	/// <summary>
	/// Check whether the given action preset can be run at the current time.
	/// </summary>
	public bool CanRunActionPreset(string preset)
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        return EliasWrapper.elias_can_run_action_preset(elias.Handle, preset) == EliasWrapper.elias_result_codes.elias_result_success;
	}

	/// <summary>
	/// Get the current theme.
	/// </summary>
	public string GetActiveTheme()
    {
        if (elias.Handle == IntPtr.Zero)
            return "";

        return elias.GetActiveTheme();

    }

    public bool CheckIfAnyActiveSourcesOrWaitingEvents()
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        if (elias.GetActiveSourceCount() > 0)
        {
            return false;
        }
        else if (elias.GetQueuedEventCount() > 0)
        {
            return false;
        }
        return true;
    }

	/// <summary>
	/// Get greatest Level in theme. Returns -1 on error.
	/// </summary>
	public int GetGreatestLevelInTheme(string themeName)
    {
        if (elias.Handle == IntPtr.Zero)
            return 0;

        return elias.GetGreatestLevelInTheme(themeName);
    }

    /// <summary>
    /// Get greatest Level on track. Returns -1 on error.
    /// </summary>
    public int GetGreatesLevelOnTrack(string themeName, string trackName)
    {
        if (elias.Handle == IntPtr.Zero)
            return 0;

        return EliasWrapper.elias_get_greatest_level_on_track(elias.Handle, themeName, trackName);
    }

    /// <summary>
    /// Get greatest Level on track. Returns -1 on error. OBSOLETE! Use GetGreatesLevelOnTrack instead!
    /// </summary>
    public int GetGreatesLeveltOnTrack(string themeName, string trackName)
    {
        if (elias.Handle == IntPtr.Zero)
            return 0;

        return GetGreatesLevelOnTrack(themeName, trackName);
    }

    /// <summary>
    /// Start ELIAS with an action preset. This function must not be called from more than one thread at the same time.
    /// </summary>
    public bool StartWithActionPreset(string preset)
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        if (AudioSettings.outputSampleRate == 0)
        {
            Debug.LogError("Unity's AudioSettings.outputSampleRate is reporting 0, so the Elias Plugin is unable to determine what sample rate would be correct to use! " +
                "This may cause the speed of the audio to play at the wrong rate. Please set a sample rate for Unity in the Project settings.");
        }
        EliasWrapper.elias_result_codes r = EliasWrapper.elias_start_background_with_action_preset(elias.Handle, preset, AudioSettings.outputSampleRate != 0 ? (uint)AudioSettings.outputSampleRate : (uint)eliasSampleRate, (uint)eliasFramesPerBuffer);
		EliasHelper.LogResult(r, preset);
		return r == EliasWrapper.elias_result_codes.elias_result_success;
	}

	/// <summary>
	/// Start ELIAS with a elias_event_set_level event. This function must not be called from more than one thread at the same time.
	/// </summary>
	public bool StartTheme(elias_event_set_level setLevel)
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        if (AudioSettings.outputSampleRate == 0)
        {
            Debug.LogError("Unity's AudioSettings.outputSampleRate is reporting 0, so the Elias Plugin is unable to determine what sample rate would be correct to use! " +
                "This may cause the speed of the audio to play at the wrong rate. Please set a sample rate for Unity in the Project settings.");
        }
        EliasWrapper.elias_result_codes r = EliasWrapper.elias_start_background_wrapped(elias.Handle, setLevel, AudioSettings.outputSampleRate != 0 ? (uint)AudioSettings.outputSampleRate : (uint)eliasSampleRate, (uint)eliasFramesPerBuffer);
		EliasHelper.LogResult(r, setLevel.ToString());
        return r == EliasWrapper.elias_result_codes.elias_result_success; 
	}

	/// <summary>
	/// Queue an elias_event.
	/// </summary>
	public void QueueEvent(elias_event @event)
    {
        if (elias.Handle == IntPtr.Zero)
            return;

        EliasWrapper.elias_result_codes r = EliasWrapper.elias_queue_event_wrapped(elias.Handle, @event);
		EliasHelper.LogResult(r, "Problems queing an event");
	}

    public void Start()
    {
        if (elias == null)
        {
            if (eliasChannelCount < 0 || eliasChannelCount == 3 || eliasChannelCount == 7 || eliasChannelCount > 8)
            {
                Debug.LogError("Invalid channel count set for Elias, disabling the player: " + eliasChannelCount);
                this.enabled = false;
                return;
            }
            if (AudioSettings.outputSampleRate == 0)
            {
                Debug.LogError("Unity's AudioSettings.outputSampleRate is reporting 0, so the Elias Plugin is unable to determine what sample rate would be correct to use! " +
                    "This may cause the speed of the audio to play at the wrong rate. Please set a sample rate for Unity in the Project settings.");
            }

            // The Elias internal resampling requires that the buffer sizes allocated are large enough to hold enough samples to get the framesPerBuffer set in the elias_start_* calls.
            // It also requires the buffer sizes to be a power of 2.
            int internalEliasBufferSize = eliasFramesPerBuffer;
            if (AudioSettings.outputSampleRate < eliasSampleRate)
            {
                //In case unity's output sample rate is reporting 0, then we double our assumed buffer size to be able to handle resampling if needed.
                int minBufferSizeRequiredForResampling = (int)((float)eliasFramesPerBuffer * (AudioSettings.outputSampleRate != 0 ? ((float)eliasSampleRate / (float)AudioSettings.outputSampleRate) : 2));
                int nextPowerOf2BufferSize = 1;
                while (nextPowerOf2BufferSize < minBufferSizeRequiredForResampling)
                {
                    nextPowerOf2BufferSize *= 2;
                }
                internalEliasBufferSize = nextPowerOf2BufferSize;
            }
            //-1 is the default for the sample rate setting as older projects would not have set it at all.
            int sampleRateToUse = eliasSampleRate == -1 ? AudioSettings.outputSampleRate : eliasSampleRate;

            string basePath = Application.streamingAssetsPath;
#if UNITY_ANDROID && !UNITY_EDITOR
            string baseApkPath = Application.dataPath;
            baseApkPath = baseApkPath.Remove(baseApkPath.LastIndexOf('/') + 1);
            baseApkPath += "base.apk";
            if (File.Exists(baseApkPath) && (Application.dataPath != baseApkPath))
            {
                basePath = "jar:file://" + Application.dataPath;
                basePath = basePath.Remove(basePath.LastIndexOf('/') + 1);
                basePath += "base.apk";
                basePath += "!/assets";
            }
#endif

            if (customBasePath != "") 
            {
                basePath = customBasePath;
            }

            elias = new EliasHelper(Path.Combine(basePath, file), Path.GetDirectoryName(Path.Combine(basePath, file)), (ushort)internalEliasBufferSize, (byte)eliasChannelCount, (uint)sampleRateToUse, null, null, eliasCachePageCount, eliasCachePageSize, deserializeProjectOnStart);

            if (elias.Handle == IntPtr.Zero)
            {
                Debug.LogError("Elias failed to initialize, disabling the EliasPlayer!");
                this.enabled = false;
            }
        }
		if (playOnStart && deserializeProjectOnStart) 
		{
			StartElias ();
		}
	}

    protected void OnDestroy()
    {
        Stop();
        if (elias != null)
        {
            elias.Dispose();
        }
        if (audioReader != null)
        {
            audioReader.Dispose();
        }
	}

    protected void Update()
    {
        if (shouldStop == true)
        {
            Stop();
        }
    }

	public bool StartElias()
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        bool startedSucessfully;
		if (actionPreset >= 0)
		{
			startedSucessfully = StartWithActionPreset();
		}
		else
		{
			startedSucessfully = StartTheme(customPreset.CreateSetLevelEvent(elias));
		}
		if (startedSucessfully)
        {
            if (audioReader == null)
            {
                audioReader = new EliasAudioReader(elias, (uint)eliasFramesPerBuffer, GetComponent<AudioSource>(), useHighLatencyMode);
                audioReader.unityChannelMode = AudioSettings.speakerMode;
                AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            }
            isEliasStarted = true;
            GetComponent<AudioSource> ().Play ();
        }
		return startedSucessfully;
	}

    public bool StartEliasWithActionPreset(string actionPreset)
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        bool startedSucessfully;
        startedSucessfully = StartWithActionPreset(actionPreset);
        if (startedSucessfully)
        {
            if (audioReader == null)
            {
                audioReader = new EliasAudioReader(elias, (uint)eliasFramesPerBuffer, GetComponent<AudioSource>(), useHighLatencyMode);
                audioReader.unityChannelMode = AudioSettings.speakerMode;
                AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            }
            isEliasStarted = true;
            GetComponent<AudioSource>().Play();
        }
        return startedSucessfully;
    }

    void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        AudioConfiguration config = AudioSettings.GetConfiguration();
        if (audioReader != null)
        {
            audioReader.unityChannelMode = config.speakerMode;
        }
    }

    protected bool StartWithActionPreset()
    {
        if (elias.Handle == IntPtr.Zero)
            return false;

        bool success;
		string name;
		EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_action_preset_name_wrapped(elias.Handle, (uint)actionPreset, out name);
		EliasHelper.LogResult(r, "Problems getting an action preset name");
		string theme;
		r = EliasWrapper.elias_get_action_preset_required_initial_theme_wrapped(elias.Handle, name, out theme);
		EliasHelper.LogResult(r, "Problems with getting the required initial theme for an action preset");
		success = StartWithActionPreset(name);
		return success;
    }

    //Due to some problems in Unity where they always pre-buffer 400ms of procedural audio, we use the OnAudioFilterRead to get close to no latency.
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (elias.Handle == IntPtr.Zero && audioReader != null)
            return;

        if (isEliasStarted && useHighLatencyMode == false)
        {
            if (audioReader.ReadCallback(data, channels) == false)
            {
                //Note: We are not directly stopping here, as it seems to be causing crashes on some (ios) devices.
                shouldStop = true;
            }
        }
    }
}
