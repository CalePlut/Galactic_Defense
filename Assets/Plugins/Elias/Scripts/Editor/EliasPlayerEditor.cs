using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EliasPlayer))]
public class EliasPlayerEditor : Editor
{
	private const string ELIAS_SUFFIX = ".mepro";
	private static string[] projects;
	private static string[] empty = new string[] { "None" };
	private int selectedProject;
	private int oldProject;
	private string[] actionPresets;
	private IList<string> transitionPresets;
	private IList<string> themes;
	private IList<string> trackGroups;
	private IList<string> stingers;
	private EliasHelper elias;
	private EliasPlayer player;

	private string ProjectsPath
	{
		get
		{
			return Application.streamingAssetsPath;
		}
	}

	public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("If Elias uses 4 or more channels (quad), please set channel count to 0, allowing the Elias Engine to match Unity's Channel count internally.", MessageType.Info);

        DrawDefaultInspector();

        EditorGUILayout.Space();

        GUIContent[] framesPerBufferDisplayStrings = {new GUIContent("1024"), new GUIContent("2048"), new GUIContent("4096"), new GUIContent("8192")};
		int[] framesPerBufferOptions = {1024, 2048, 4096, 8192};
		player = (EliasPlayer)target;
		player.eliasFramesPerBuffer = EditorGUILayout.IntPopup (new GUIContent("Frames per buffer", "A higher value will generally save performance by doing less disk reads, in exchange for a (slightly) increased latency"), player.eliasFramesPerBuffer, framesPerBufferDisplayStrings, framesPerBufferOptions);

        EditorGUILayout.Space();

        player.eliasCachePageCount = Math.Max(4, EditorGUILayout.IntField(new GUIContent("Cache page count", "This determines the number of pages in the cache, min 4. For memory limited targets 128 is recommended."), player.eliasCachePageCount));

        GUIContent[] bytesPerPageDisplayStrings = { new GUIContent("256"), new GUIContent("512"), new GUIContent("1024"), new GUIContent("2048"), new GUIContent("4096"), new GUIContent("8192"), new GUIContent("16384"), new GUIContent("32768")};
        int[] bytesPerPageOptions = { 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
        player.eliasCachePageSize = EditorGUILayout.IntPopup(new GUIContent("Bytes per cache page", "This determines the size of each cache page. Min 256, 4096 is recommended."), player.eliasCachePageSize, bytesPerPageDisplayStrings, bytesPerPageOptions);
        EditorGUILayout.LabelField("Memory used for cache: " + ((player.eliasCachePageCount * player.eliasCachePageSize) / 1024) + "kb");

        EditorGUILayout.Space();

        GUIContent[] avalibleSampleRateStrings = { new GUIContent("Match Unity Project Settings (Not recomended!)"), new GUIContent("16000"), new GUIContent("22050"), new GUIContent("24000"), new GUIContent("32000"), new GUIContent("44100"), new GUIContent("48000") };
        int[] avalibleSampleRates = { -1, 16000, 22050, 24000, 32000, 44100, 48000 };
        player.eliasSampleRate = EditorGUILayout.IntPopup(new GUIContent("Sample Rate", "The sample rate of the files used in the Elias Project (.mepro)"), player.eliasSampleRate, avalibleSampleRateStrings, avalibleSampleRates);

        EditorGUILayout.Space();

        LoadProjects();
		DrawProjectSelector();
		InitializeElias();

        EditorGUILayout.Space();

        DrawActionPresets();
		player.file = GetSelectedProjectPath();

        EditorGUILayout.Space();

        if (player.actionPreset == -1)
		{
			transitionPresets = elias.GetTransitionPresets();
			themes = elias.GetThemes().ToArray();
			EliasSetLevel setLevel = player.customPreset;
			if (setLevel == null)
			{
				setLevel = new EliasSetLevel();
				player.customPreset = setLevel;
			}
			setLevel.preWaitTimeMs = Mathf.Clamp(EditorGUILayout.IntField("Wait Time (ms)", setLevel.preWaitTimeMs), 0, int.MaxValue);
			DrawTransitionPreset(setLevel);
			DrawThemeName(setLevel);
			stingers = empty.Concat(elias.GetStingers(setLevel.themeName)).ToList();
			trackGroups = empty.Concat(elias.GetTracksGroups()).ToList();
			setLevel.jumpToBar = (ushort)Mathf.Clamp(EditorGUILayout.IntField("Jump To Bar", setLevel.jumpToBar), 0, (int)elias.GetBasicInfo(setLevel.themeName).bars);
			DrawTrackGroupName(setLevel);
			setLevel.level = Mathf.Clamp(EditorGUILayout.IntField("Level", setLevel.level), 0, elias.GetGreatestLevelInTheme(setLevel.themeName));
			setLevel.suggestedMaxTimeMs = EditorGUILayout.IntField("Suggested Max Time", setLevel.suggestedMaxTimeMs);
			DrawStinger(setLevel);
		}
		else
		{
			player.customPreset = null;
		}
	}

	private void DrawTransitionPreset(EliasSetLevel setLevel)
	{
		int id = transitionPresets.IndexOf(setLevel.transitionPresetName);
		id = Mathf.Clamp(id, 0, transitionPresets.Count - 1);
		id = EditorGUILayout.Popup("Transition Preset", id, transitionPresets.ToArray());
		setLevel.transitionPresetName = transitionPresets[id];
	}

	private void DrawThemeName(EliasSetLevel setLevel)
	{
		int id = themes.IndexOf(setLevel.themeName);
		id = Mathf.Clamp(id, 0, themes.Count - 1);
		id = EditorGUILayout.Popup("Theme", id, themes.ToArray());
		setLevel.themeName = themes[id];
        setLevel.stingerRequiredTheme = themes[id];
    }

	private void DrawTrackGroupName(EliasSetLevel setLevel)
	{
		int id = trackGroups.IndexOf(setLevel.trackGroupName);
		id = Mathf.Clamp(id, 0, trackGroups.Count - 1);
		id = EditorGUILayout.Popup("Track Group", id, trackGroups.ToArray());
		setLevel.trackGroupName = trackGroups[id];
	}

	private void DrawStinger(EliasSetLevel setLevel)
	{
		int id = stingers.IndexOf(setLevel.stingerName);
		id = Mathf.Clamp(id, 0, stingers.Count - 1);
		id = EditorGUILayout.Popup("Stinger", id, stingers.ToArray());
		setLevel.stingerName = stingers[id];
	}

	private void DrawProjectSelector()
	{
		if (projects != null)
		{
			oldProject = selectedProject;
            GUIContent[] projectsListed = new GUIContent[projects.Length];
            for (int i = 0; i < projects.Length; i++)
            {
                projectsListed[i] = new GUIContent( projects[i]);
            } 
            selectedProject = EditorGUILayout.Popup(new GUIContent("Project", "This sets the \"file\" property of the EliasPlayer"), selectedProject, projectsListed);
		}
		else
		{
			EditorGUILayout.HelpBox("No Themes found.", MessageType.Error);
		}
	}

	private void DrawActionPresets()
	{
		if (elias != null)
		{
			actionPresets = new string[] { "None" }.Concat(elias.GetActionPresets()).ToArray();
			player.actionPreset = EditorGUILayout.Popup("Action Preset", player.actionPreset + 1, actionPresets) - 1;
		}
	}

	private void InitializeElias()
	{
		if ((elias == null || oldProject != selectedProject) && IsProjectSelected())
		{
			elias = new EliasHelper(GetSelectedProjectPath(), (uint)AudioSettings.outputSampleRate);
		}
	}

	private string GetSelectedProjectPath()
	{
		return projects[selectedProject] + ELIAS_SUFFIX;
	}

	private bool IsProjectSelected()
	{
		return selectedProject >= 0;
	}

	private void LoadProjects()
	{
		try
		{
			projects = Directory.GetFiles(ProjectsPath, "*" + ELIAS_SUFFIX, SearchOption.AllDirectories)
				.Select(s => s.Replace(ProjectsPath, "").Replace(ELIAS_SUFFIX, "").Replace("\\", "/").Remove(0, 1)).ToArray();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		for (int i = 0; i < projects.Count(); i++) 
		{		
			if ((projects[i] + ELIAS_SUFFIX) == player.file) 
			{
				selectedProject = i;
				oldProject = i;
			}
		}
	}
}