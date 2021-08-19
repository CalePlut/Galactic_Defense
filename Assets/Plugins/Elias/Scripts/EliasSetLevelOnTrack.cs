using System;

/// <summary>
/// Editor utility class. Encapsulates elias_event_set_level in an easy to store way.
/// </summary>
[Serializable]
public class EliasSetLevelOnTrack
{
    [EliasBitMask(typeof(elias_event_flags))]
    public elias_event_flags flags;
	public int preWaitTimeMs;
	public int level;
	public ushort jumpToBar;
	public int suggestedMaxTimeMs;
	public string transitionPresetName;
	public string themeName;
	public string trackName;
    [UnityEngine.Tooltip("Leave empty if it should use the active theme when added to the queue")]
    public string stingerRequiredTheme;
    public string stingerName;

	public elias_event_set_level_on_track CreateSetLevelOnTrackEvent(EliasHelper elias)
	{
		return new elias_event_set_level_on_track(
			(uint)flags,
			(uint)preWaitTimeMs,
			transitionPresetName == "" ? 0 : elias.GetTransitionPresetIndex(transitionPresetName),
			elias.GetThemeIndex(themeName),
			level,
			elias.GetTrackIndex(themeName, trackName),
			jumpToBar,
			(uint)suggestedMaxTimeMs,
			elias.GetStingerIndex(stingerRequiredTheme, stingerName)); 
            //Note: By not passing in a theme name, we are getting the stinger index of the currently playing theme.
            //This is required when changing themes, as the stinger that plays is from the theme that is transitioned away from.
	}
}