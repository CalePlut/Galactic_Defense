using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Encapsulates one ELIAS project and provides utility functions.
/// </summary>
public class EliasHelper
{
	public const int ABI_VERSION = 1;

	public IntPtr Handle
	{
		get;
        protected set;
	}

	public int ChannelCount
	{
		get;
        protected set;
	}

	public int SampleRate
	{
		get;
        protected set;
	}

	public int FramesPerBuffer
	{
		get;
        protected set;
	}

    /// <summary>
    /// Initialize ELIAS and deserializes "file" with default values. Expects the audio files in the same folder.
    /// </summary>
    protected static string GetPath(string file)
	{
		return Path.GetDirectoryName(file);
	}

	/// <summary>
	/// Initializes ELIAS and deserializes "file" with all provided parameters.
	/// </summary>
	public EliasHelper(string file, uint eliasSampleRate, bool loadProject = true) : 
        this(Path.Combine(Application.streamingAssetsPath, file), GetPath(Path.Combine(Application.streamingAssetsPath, file)), 2048, 0, eliasSampleRate, null, null, 128, 4096, true)
	{
	}

	/// <summary>
	/// Initializes ELIAS and deserializes "file" with all provided parameters. This overload also takes channels, framesPerBuffer and the cache settings.
	/// </summary>
	public EliasHelper(string file, int framesPerBuffer, int channels, uint eliasSampleRate, int cachePageCount, int cachePageSize, bool loadProject = true) : 
        this(Path.Combine(Application.streamingAssetsPath, file), GetPath(Path.Combine(Application.streamingAssetsPath, file)), (ushort)framesPerBuffer, (byte)channels, eliasSampleRate, null, null, cachePageCount, cachePageSize, true)
	{
	}

	public EliasHelper(string file, string basePath, ushort framesPerBuffer, byte channels, uint sampleRate, EliasWrapper.elias_data_reader_functions? readerFunctions, EliasWrapper.elias_memory_functions? memoryFunctions, int cachePageCount, int cachePageSize, bool loadProject = true)
	{

		SampleRate = (int)sampleRate;
		FramesPerBuffer = (int)framesPerBuffer;
        //If no channel count is set for the Elias Object, Unity's channel count setting is used.
        if (channels == 0)
        {
            AudioSpeakerMode unityChannelMode = AudioSettings.speakerMode;
            switch (unityChannelMode)
            {
                case AudioSpeakerMode.Mono:
                    channels = 1;
                    break;
                case AudioSpeakerMode.Stereo:
                    channels = 2;
                    break;
                case AudioSpeakerMode.Quad:
                    channels = 4;
                    break;
                case AudioSpeakerMode.Surround:
                    channels = 5;
                    break;
                case AudioSpeakerMode.Mode5point1:
                    channels = 6;
                    break;
                case AudioSpeakerMode.Mode7point1:
                    channels = 8;
                    break;
                default: //If Unity is set to "Raw", or some new/unknown AudioSpeakerMode is used, we default Elias to use Stereo (As 0 is not a vaid channel count).
                    Debug.LogError("No channel count set for Elias, and unknown channel count is set for Unity. Defaulting Elias to use Stereo!");
                    channels = 2;
                    break;
            }
        }
        ChannelCount = channels;
        Initialize(readerFunctions, basePath, sampleRate, channels, framesPerBuffer, memoryFunctions, cachePageCount, cachePageSize);
        if (loadProject && Handle != IntPtr.Zero)
        {
            Deserialize(file);
        }
	}

	public static void LogResult(EliasWrapper.elias_result_codes result, string errorMessage = "")
	{
		if (result != EliasWrapper.elias_result_codes.elias_result_success)
		{
			Debug.LogError(result + "\n" + errorMessage);
		}
	}

	public IList<string> GetActionPresets()
	{
        if (Handle == IntPtr.Zero)
            return new List<string>();

		string name;
		List<string> presets = new List<string>();
		for (uint i = 0; i < EliasWrapper.elias_get_action_preset_count(Handle); i++)
		{
			EliasWrapper.elias_result_codes result = EliasWrapper.elias_get_action_preset_name_wrapped(Handle, i, out name);
			LogResult(result, i.ToString());
			presets.Add(name);
		}
		return presets;
	}

	public IList<string> GetTransitionPresets()
    {
        if (Handle == IntPtr.Zero)
            return new List<string>();

        List<string> presets = new List<string>();
		for (uint i = 0; i < EliasWrapper.elias_get_transition_preset_count(Handle); i++)
		{
			string name;
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_transition_preset_name_wrapped(Handle, i, out name);
			LogResult(r, i.ToString());
			presets.Add(name);
		} 
		return presets;
	}

	public IList<string> GetThemes()
    {
        if (Handle == IntPtr.Zero)
            return new List<string>();

        List<string> themes = new List<string>();
		for (uint i = 0; i < EliasWrapper.elias_get_theme_count(Handle); i++)
		{
			string name;
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_theme_name_wrapped(Handle, i, out name);
			LogResult(r, i.ToString());
			themes.Add(name);
		}
		return themes;
	}

	public IList<string> GetTracks(string theme)
    {
        if (Handle == IntPtr.Zero)
            return new List<string>();

        List<string> tracks = new List<string>();
		for (uint i = 0; i < EliasWrapper.elias_get_track_count(Handle, theme); i++)
		{
			string name;
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_track_name_wrapped(Handle, theme, i, out name);
			LogResult(r, theme);
			tracks.Add(name);
		}
		return tracks;
	}

	public IList<string> GetTracksGroups()
    {
        if (Handle == IntPtr.Zero)
            return new List<string>();

        List<string> tracks = new List<string>();
		for (uint i = 0; i < EliasWrapper.elias_get_track_group_count(Handle); i++)
		{
			string name;
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_track_group_name_wrapped(Handle, i, out name);
			LogResult(r, name + " " + i);
			tracks.Add(name);
		}
		return tracks;
	}

	public IList<string> GetStingers(string theme)
    {
        if (Handle == IntPtr.Zero)
            return new List<string>();

        List<string> tracks = new List<string>();
		for (uint i = 0; i < EliasWrapper.elias_get_track_count(Handle, theme); i++)
		{
			string name;
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_track_name_wrapped(Handle, theme, i, out name);
			LogResult(r, theme);
			uint type;
			r = EliasWrapper.elias_get_track_type(Handle, theme, name, out type);
			LogResult(r, theme);
			if (type == (int)elias_track_types.elias_track_audio_stinger)
			{
				tracks.Add(name);
			}
		}
		return tracks;
	}

	public int GetGreatestLevelInTheme(string theme)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        return EliasWrapper.elias_get_greatest_level_in_theme(Handle, theme);
	}

	public EliasBasicInfo GetBasicInfo(string theme)
    {
        if (Handle == IntPtr.Zero)
            return new EliasBasicInfo();

        EliasBasicInfo info;
		EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_theme_basic_info(Handle, theme, out info.initial_bpm, out info.initial_timesig_numerator, out info.initial_timesig_denominator, out info.bars);
		LogResult(r, theme);
		return info;
	}
	
	public uint GetBusIndex(string busName)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        uint busIndex;
		EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_bus_index(Handle, busName, out busIndex);
		LogResult(r, "Failed to get bus index for " + busName);
		return busIndex;
	}

	public uint GetTransitionPresetIndex(string transitionPresetName)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        uint transitionPresetIndex;
		EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_transition_preset_index(Handle, transitionPresetName, out transitionPresetIndex);
		LogResult(r, "Failed to get transition preset index for " + transitionPresetName);
		if (r != EliasWrapper.elias_result_codes.elias_result_success) 
		{
			transitionPresetIndex = 0;
		}
		return transitionPresetIndex;
	}

	public uint GetThemeIndex(string themeName)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        uint themeIndex;
		EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_theme_index(Handle, themeName, out themeIndex);
		LogResult(r, themeName);
		return themeIndex;
	}

    public uint GetTrackIndex(string themeName, string trackName)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        uint trackIndex;
        EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_track_index(Handle, themeName, trackName, out trackIndex);
        LogResult(r, trackName);
        return trackIndex;
    }

	public int GetGroupIndex(string trackGroupName)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        uint tmp;
		int affectedTracksGroupIndex;
		if (IsValidName(trackGroupName))
		{
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_track_group_index(Handle, trackGroupName, out tmp);
			affectedTracksGroupIndex = (int)tmp;
			LogResult(r, trackGroupName);
		}
		else
		{
			affectedTracksGroupIndex = -1;
		}
		return affectedTracksGroupIndex;
    }

    /// <summary>
    /// Get the current theme.
    /// </summary>
    public string GetActiveTheme()
    {
        if (Handle == IntPtr.Zero)
            return "";

        uint id = (uint)EliasWrapper.elias_get_active_theme_index(Handle, null);
        string name;
        EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_theme_name_wrapped(Handle, id, out name);
        EliasHelper.LogResult(r, "Failed to get active theme");
        return name;
    }

    public uint GetActiveSourceCount()
    {
        if (Handle == IntPtr.Zero)
            return 0;

        return EliasWrapper.elias_get_active_source_count(Handle);
    }

    public uint GetQueuedEventCount()
    {
        if (Handle == IntPtr.Zero)
            return 0;

        return EliasWrapper.elias_get_event_count(Handle);
    }

    public int GetStingerIndex(string themeName, string stingerName)
    {
        if (Handle == IntPtr.Zero)
            return 0;

        int stingerIndex;
		uint tmp;
		if (IsValidName(stingerName))
		{
            if (themeName == "")
            {
                themeName = GetActiveTheme();
            }
			EliasWrapper.elias_result_codes r = EliasWrapper.elias_get_track_index(Handle, themeName, stingerName, out tmp);
			stingerIndex = (int)tmp;
			LogResult(r, "Failed to get stinger index");
		}
		else
		{
			stingerIndex = -1;
		}
		return stingerIndex;
	}

	/// <summary>
	/// Disposes the ELIAS reference. This instance of EliasHelper can't be used after this method is called.
	/// </summary>
	public void Dispose()
	{
        if (Handle != IntPtr.Zero)
        {
            EliasWrapper.elias_free(Handle);
        }
	}

    protected void Initialize(EliasWrapper.elias_data_reader_functions? readerFunctions, string basePath, uint sampleRate, byte channels, ushort framesPerBuffer, EliasWrapper.elias_memory_functions? memoryFunctions, int cachePageCount, int cachePageSize)
    {
        string innerPath = basePath;
#if UNITY_ANDROID && !UNITY_EDITOR
        string pathToArchive = "";
        int splitIndex = basePath.IndexOf('!');
        if (splitIndex != -1)
        {
	        innerPath = basePath.Substring(splitIndex + 2);
        }
        if (basePath.Contains("jar:file:/"))
        {
	        pathToArchive = basePath.Remove(splitIndex);
	        pathToArchive = pathToArchive.Replace("jar:file:", "");
        }
#endif
        EliasWrapper.elias_result_codes result;
		Handle = EliasWrapper.elias_initialize_wrapped(out result, ABI_VERSION, readerFunctions, innerPath, sampleRate, channels, framesPerBuffer, memoryFunctions);
		LogResult(result, basePath);

#if UNITY_ANDROID && !UNITY_EDITOR
        if (pathToArchive != "" && result == EliasWrapper.elias_result_codes.elias_result_success)
        {
            result = EliasWrapper.elias_set_archive(Handle, pathToArchive, 0);
		    LogResult(result, pathToArchive);
        }
#endif
        if (result == EliasWrapper.elias_result_codes.elias_result_success)
        {
            result = EliasWrapper.elias_configure_cache(Handle, (uint)cachePageCount, (uint)cachePageSize);
            LogResult(result, "Attempted to configure cache");
        }
    } 

    public void DerserializeString(string meproContent)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(meproContent);
        EliasWrapper.elias_result_codes r = EliasWrapper.elias_deserialize(Handle, bytes, (uint)bytes.Length + 1, 0);
        LogResult(r, meproContent);
    }

    public void Deserialize(string file)
    {
        EliasWrapper.elias_result_codes r = EliasWrapper.elias_deserialize_from_file(Handle, Path.GetFileName(file), 0);
        LogResult(r, file);
    }

    protected bool IsValidName(string name)
	{
		return !string.IsNullOrEmpty(name) && !name.Equals("None");
	}
}
