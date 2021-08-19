// copied from elias.h

public enum elias_data_reader_results
{
    elias_data_reader_result_endofdata = 0,
    elias_data_reader_result_trylater = -1, /* Tells the engine to try the request later. Does not modify the cursor position. */
    elias_data_reader_result_horribleerror = -2 /* An irrecoverable reading error occurred. */
}

public enum elias_serialization_flags
{
    elias_serialization_flag_default = 0, /* Defaults to compact mode. */
    elias_serialization_flag_compact = 1, /* Make the output as compact as possible to save space. */
    elias_serialization_flag_formatted = 2 /* Format the output so that it is easier to read. */
}

public enum elias_track_types
{
    elias_track_audio_loop = 1,
    elias_track_audio_stinger,
    elias_track_midi_loop,
    elias_track_midi_stinger
}

public enum elias_analysis_operations
{
    elias_analysis_basic = 0, /* Retrieves the length of the file (this should be done if the file is updated after it was added. */
    elias_analysis_amplitude_scan /* In addition to the basic analysis, this also scans the file for silent sections which enables optimizations. */
}

public enum elias_event_types
{
    elias_event_type_set_level = 1,
    elias_event_type_play_stinger,
    elias_event_type_set_effect_parameter,
    elias_event_type_set_send_volume,
    elias_event_type_set_level_on_track
}

public enum elias_event_flags
{
    elias_event_flag_urgent = 1,
    elias_event_flag_wait_for_prior = 2
}

public enum elias_source_status
{
    elias_source_status_waiting_to_start,
    elias_source_status_fading_in,
    elias_source_status_playing_normally,
    elias_source_status_waiting_to_fade_out,
    elias_source_status_fading_out
}

public enum elias_effect_parameter_types
{
    elias_effect_parameter_double,
    elias_effect_parameter_int32,
    elias_effect_parameter_bool
}

public enum elias_level_selection_strategies
{
    elias_level_selection_closest_above,
    elias_level_selection_closest_below,
    elias_level_selection_closest_to_target,
    elias_level_selection_least_possible_change
}

public enum elias_progression_modes
{
    elias_progression_random,
    elias_progression_sequential,
    elias_progression_shuffle
}

public enum elias_stinger_selection_strategies
{
    elias_stinger_selection_progression,
    elias_stinger_selection_closest_in_time
}

public enum elias_transition_options
{
    elias_transition_option_fade_in = 1, /* Double in milliseconds. */
    elias_transition_option_crossfade, /* Double in milliseconds. */
    elias_transition_option_fade_out, /* Double in milliseconds. */
    elias_transition_option_agility, /* Double in beats - may not be used in themes that have time signature changes. */
    elias_transition_option_agility_beat_points, /* A double array in beats. */
    elias_transition_option_disable_smart_transitions, /* Boolean. */
    elias_transition_option_pickup_beats, /* Double in beats - will calculate beat lengths from right before the */
                                          /* point at which the remainder of the audio starts, wherever it is triggered. This works because regular agility cannot */
                                          /* be used in themes with time signature changes. */
    elias_transition_option_level_selection_strategy, /* Int - corresponds to one of the constants in the elias_level_selection_strategies enum. */
    elias_transition_option_progression_mode, /* Int - corresponds to one of the constants in the elias_level_selection_modes enum. */
    elias_transition_option_stinger_selection_strategy /* Int - corresponds to one of the constants in the elias_stinger_selection_strategies enum. */
}

public enum elias_transition_option_data_types
{
    elias_transition_option_type_int32 = 1,
    elias_transition_option_type_double,
    elias_transition_option_type_bool,
    elias_transition_option_type_int32_array,
    elias_transition_option_type_double_array,
    elias_transition_option_type_bool_array
}

internal enum elias_bus_slot_types
{
    elias_bus_slot_type_empty = 0,
    elias_bus_slot_type_effect,
    elias_bus_slot_type_bus_send,
    elias_bus_slot_type_effect_send
}