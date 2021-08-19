using System;

/// <summary>
/// Utility class. Encapsulates elias_event_set_level in an easy to store and use way.
/// </summary>
[Serializable]
public class EliasSetLevel
{
    [EliasBitMask(typeof(elias_event_flags))]
    public elias_event_flags flags;
	public int preWaitTimeMs;
	public int level;
	public ushort jumpToBar = 0; //Set to 0 to not jump
	public int suggestedMaxTimeMs;
	public string transitionPresetName; //Set to "" to use the default
    public string themeName;
    public string trackGroupName; //Set to "" to ignore
    [UnityEngine.Tooltip("Leave empty if it should use the active theme when added to the queue")]
    public string stingerRequiredTheme; //Set to "" to use the currently active theme
    public string stingerName; //Set to "" to ignore

    public elias_event_set_level CreateSetLevelEvent(EliasHelper elias)
	{
		return new elias_event_set_level(
			(uint)flags,
			(uint)preWaitTimeMs,
			transitionPresetName == "" ? 0 : elias.GetTransitionPresetIndex(transitionPresetName),
			elias.GetThemeIndex(themeName),
			level,
			elias.GetGroupIndex(trackGroupName),
			jumpToBar,
			(uint)suggestedMaxTimeMs,
			elias.GetStingerIndex(stingerRequiredTheme, stingerName)); 
            //Note: By not passing in a theme name, we are getting the stinger index of the currently playing theme.
            //This is required when changing themes, as the stinger that plays is from the theme that is transitioned away from.
	}
}