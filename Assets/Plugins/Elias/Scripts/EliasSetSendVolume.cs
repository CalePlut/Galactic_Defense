using System;

/// <summary>
/// Editor utility class. Encapsulates elias_event_set_send_volume in an easy to store way.
/// </summary>
[Serializable]
public class EliasSetSendVolume
{
    [EliasBitMask(typeof(elias_event_flags))]
    public elias_event_flags flags;
	public int preWaitTimeMs;
	public string busName;
	public byte slot = 9;
	public double volume;
	public int fadeTimeMS;
	public elias_event_set_send_volume CreateSetSendVolumeEvent(EliasHelper elias)
	{
		return new elias_event_set_send_volume(
			(uint)flags,
			(uint)preWaitTimeMs,
			(uint)elias.GetBusIndex(busName),
			slot,
			volume,
			(uint)fadeTimeMS);
	}
}