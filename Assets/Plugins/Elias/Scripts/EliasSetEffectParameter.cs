using System;

/// <summary>
/// Editor utility class. Encapsulates elias_event_set_effect_parameter in an easy to store way.
/// </summary>
[Serializable]
public abstract class EliasSetEffectParameter
{
    [EliasBitMask(typeof(elias_event_flags))]
    public elias_event_flags flags;
	public int preWaitTimeMs;
	public string busName;
	public byte slot;
	public byte parameterIndex;
	public int sweepTimeMS;

	abstract public elias_event_set_effect_parameter CreateSetEffectParameterEvent(EliasHelper elias);
}

/// <summary>
/// Editor utility class. Encapsulates elias_event_set_effect_parameter in an easy to store way for doubles.
/// </summary>
[Serializable]
public class EliasSetEffectParameterDouble : EliasSetEffectParameter
{
	public double value;
	public override elias_event_set_effect_parameter CreateSetEffectParameterEvent(EliasHelper elias)
	{
		EliasWrapper.elias_effect_parameter effectParam = new EliasWrapper.elias_effect_parameter();
		effectParam.type = (uint)elias_effect_parameter_types.elias_effect_parameter_double;
		effectParam.value.double_value = value;
		return new elias_event_set_effect_parameter(
			(uint)flags,
			(uint)preWaitTimeMs,
			(uint)elias.GetBusIndex(busName),
			slot,
			parameterIndex,
			effectParam,
			(uint)sweepTimeMS);
	}
}

/// <summary>
/// Editor utility class. Encapsulates elias_event_set_effect_parameter in an easy to store way for integers.
/// </summary>
[Serializable]
public class EliasSetEffectParameterInt : EliasSetEffectParameter
{
	public int value;
	public override elias_event_set_effect_parameter CreateSetEffectParameterEvent(EliasHelper elias)
	{
		EliasWrapper.elias_effect_parameter effectParam = new EliasWrapper.elias_effect_parameter();
		effectParam.type = (uint)elias_effect_parameter_types.elias_effect_parameter_int32;
		effectParam.value.int32_value = value;
		return new elias_event_set_effect_parameter(
			(uint)flags,
			(uint)preWaitTimeMs,
			(uint)elias.GetBusIndex(busName),
			slot,
			parameterIndex,
			effectParam,
			(uint)sweepTimeMS);
	}
}

/// <summary>
/// Editor utility class. Encapsulates elias_event_set_effect_parameter in an easy to store way for booleans.
/// </summary>
[Serializable]
public class EliasSetEffectParameterBool : EliasSetEffectParameter
{
	public bool value;
	public override elias_event_set_effect_parameter CreateSetEffectParameterEvent(EliasHelper elias)
	{
		EliasWrapper.elias_effect_parameter effectParam = new EliasWrapper.elias_effect_parameter();
		effectParam.type = (uint)elias_effect_parameter_types.elias_effect_parameter_bool;
		effectParam.value.bool_value = (byte)(value ? 1 : 0);
		return new elias_event_set_effect_parameter(
			(uint)flags,
			(uint)preWaitTimeMs,
			(uint)elias.GetBusIndex(busName),
			slot,
			parameterIndex,
			effectParam,
			(uint)sweepTimeMS);
	}
}