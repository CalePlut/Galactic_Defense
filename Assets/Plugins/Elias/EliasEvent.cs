using System;
using System.Runtime.InteropServices;

public interface elias_event
{
}

//A special variation of the struct, more in line with how the Engine itself handles these types in C (union).
//However it is harder to use, and easier to make misstakes with, so it's only used in the wrapper when getting elias_events.
[StructLayout(LayoutKind.Explicit)]
public struct elias_event_out
{
    //NOTE: the "type" will be how you know what member to access.
    //From the elias.h file (modified for more obvious numbers!)
    public enum elias_event_types : uint
    {
        elias_event_type_set_level = 1, /**< Perform a transition to a specific level. */
        elias_event_type_play_stinger, /**< Play a standalone stinger. */
        elias_event_type_set_effect_parameter, /**< Set an effect parameter. */
        elias_event_type_set_send_volume, /**< Set the volume of a bus send. */
        elias_event_type_set_level_on_track /**< Tell a single loop track to perform a transition to a specific level. */
    };

    [FieldOffset(0)]
    public System.UInt32 type;
    [FieldOffset(0)]
    public elias_event_set_level set_level;
    [FieldOffset(0)]
    public elias_event_play_stinger play_stinger;
    [FieldOffset(0)]
    public elias_event_set_effect_parameter set_effect_parameter;
    [FieldOffset(0)]
    public elias_event_set_send_volume set_send_volume;
    [FieldOffset(0)]
    public elias_event_set_level_on_track set_level_on_track;
}

[StructLayout(LayoutKind.Sequential)]
public struct elias_event_set_level : elias_event
{
	public uint type;
	public uint flags;
	public uint pre_wait_time_milliseconds;
	public uint transition_preset_index;
	public uint theme_index;
	public int level;
	public int affected_tracks_group_index;
	public UInt16 jump_to_bar;
	public uint suggested_max_time_milliseconds;
	public int stinger_index;

	public elias_event_set_level(uint flags, uint pre_wait_time_milliseconds, uint transition_preset_index, uint theme_index, int level, int affected_tracks_group_index, UInt16 jump_to_bar, uint suggested_max_time_milliseconds, int stinger_index)
	{
		type = (uint)elias_event_types.elias_event_type_set_level;
		this.flags = flags;
		this.pre_wait_time_milliseconds = pre_wait_time_milliseconds;
		this.transition_preset_index = transition_preset_index;
		this.theme_index = theme_index;
		this.level = level;
		this.affected_tracks_group_index = affected_tracks_group_index;
		this.jump_to_bar = jump_to_bar;
		this.suggested_max_time_milliseconds = suggested_max_time_milliseconds;
		this.stinger_index = stinger_index;
	}

	public override string ToString()
	{
		return "type:" + type + "; flags:" + flags + "; wait time:" + pre_wait_time_milliseconds + "; transition:" + transition_preset_index + "; theme:" + theme_index + "; level:" + level + "; track group:" + affected_tracks_group_index + "; bar:" + jump_to_bar + "; max time:" + suggested_max_time_milliseconds + "; stinger:" + stinger_index;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct elias_event_set_level_on_track : elias_event
{
    public uint type;
    public uint flags;
    public uint pre_wait_time_milliseconds;
    public uint transition_preset_index;
    public uint theme_index;
    public int level;
    public uint affected_track_index;
    public UInt16 jump_to_bar;
    public uint suggested_max_time_milliseconds;
    public int stinger_index;

    public elias_event_set_level_on_track(uint flags, uint pre_wait_time_milliseconds, uint transition_preset_index, uint theme_index, int level, uint affected_track_index, UInt16 jump_to_bar, uint suggested_max_time_milliseconds, int stinger_index)
    {
        type = (uint)elias_event_types.elias_event_type_set_level_on_track;
        this.flags = flags;
        this.pre_wait_time_milliseconds = pre_wait_time_milliseconds;
        this.transition_preset_index = transition_preset_index;
        this.theme_index = theme_index;
        this.level = level;
        this.affected_track_index = affected_track_index;
        this.jump_to_bar = jump_to_bar;
        this.suggested_max_time_milliseconds = suggested_max_time_milliseconds;
        this.stinger_index = stinger_index;
    }

    public override string ToString()
    {
        return "type:" + type + "; flags:" + flags + "; wait time:" + pre_wait_time_milliseconds + "; transition:" + transition_preset_index + "; theme:" + theme_index + "; level:" + level + "; track (index):" + affected_track_index + "; bar:" + jump_to_bar + "; max time:" + suggested_max_time_milliseconds + "; stinger:" + stinger_index;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct elias_event_play_stinger : elias_event
{
	public uint type;
	public uint flags;
	public uint pre_wait_time_milliseconds;
	public uint transition_preset_index;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string name;

	public int level;

	public elias_event_play_stinger(uint flags, uint pre_wait_time_milliseconds, uint transition_preset_index, string name, int level)
	{
		type = (uint)elias_event_types.elias_event_type_play_stinger;
		this.flags = flags;
		this.pre_wait_time_milliseconds = pre_wait_time_milliseconds;
		this.transition_preset_index = transition_preset_index;
		this.name = name;
		this.level = level;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct elias_event_set_effect_parameter : elias_event
{
	public uint type;
	public uint flags;
	public uint pre_wait_time_milliseconds;
	public uint bus_index;
	public byte slot;
	public byte parameter_index;
	public EliasWrapper.elias_effect_parameter parameter;
	public uint sweep_time_milliseconds;

	public elias_event_set_effect_parameter(uint flags, uint pre_wait_time_milliseconds, uint bus_index, byte slot, byte parameter_index, EliasWrapper.elias_effect_parameter parameter, uint sweep_time_milliseconds)
	{
		type = (uint)elias_event_types.elias_event_type_set_effect_parameter;
		this.flags = flags;
		this.pre_wait_time_milliseconds = pre_wait_time_milliseconds;
		this.bus_index = bus_index;
		this.slot = slot;
		this.parameter_index = parameter_index;
		this.parameter = parameter;
		this.sweep_time_milliseconds = sweep_time_milliseconds;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct elias_event_set_send_volume : elias_event
{
	public uint type;
	public uint flags;
	public uint pre_wait_time_milliseconds;
	public uint bus_index;
	public byte slot;
	public double volume_db;
	public uint fade_time_milliseconds;

	public elias_event_set_send_volume(uint flags, uint pre_wait_time_milliseconds, uint bus_index, byte slot, double volume_db, uint fade_time_milliseconds)
	{
		type = (uint)elias_event_types.elias_event_type_set_send_volume;
		this.flags = flags;
		this.pre_wait_time_milliseconds = pre_wait_time_milliseconds;
		this.bus_index = bus_index;
		this.slot = slot;
		this.volume_db = volume_db;
		this.fade_time_milliseconds = fade_time_milliseconds;
	}
}
