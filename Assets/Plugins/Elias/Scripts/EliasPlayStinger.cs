using System;

[Serializable]
public class EliasPlayStinger
{
    [EliasBitMask(typeof(elias_event_flags))]
    public elias_event_flags flags;
	public uint preWaitTimeMs;
	public string transitionPresetName;
	public string name;
	public int level;

	public elias_event_play_stinger CreatePlayerStingerEvent(EliasHelper elias)
	{
		return new elias_event_play_stinger(
			(uint)flags, 
			preWaitTimeMs, 
			transitionPresetName == "" ? 0 : elias.GetTransitionPresetIndex(transitionPresetName), 
			name, 
			level);
	}
}