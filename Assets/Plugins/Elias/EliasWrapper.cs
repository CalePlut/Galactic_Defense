using System;
using System.Text;
using System.Runtime.InteropServices;
using elias_handle = System.IntPtr;
using AOT;
using UnityEngine;

public class EliasWrapper
{
    private const int STRING_LENGTH = 64;   // Length for strings which are returned from C++ native plugin

    public static string StringFromNativeUtf8(IntPtr nativeUtf8)
    {
        int len = 0;
        while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
        byte[] buffer = new byte[len];
        Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer);
    }

#if UNITY_IOS && !UNITY_EDITOR
	private const string DllName = "__Internal";
#elif UNITY_ANDROID && !UNITY_EDITOR
    private const string DllName = "elias";
#else
    private const string DllName = "libelias";
#endif

#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate IntPtr elias_memory_functions_malloc(UIntPtr bytes, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_memory_functions_free(IntPtr pointer, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate IntPtr elias_memory_functions_realloc(IntPtr pointer, UIntPtr bytes, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate IntPtr elias_data_reader_functions_create_instance(IntPtr memory_functions, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate byte elias_data_reader_functions_open(IntPtr instance, string filename);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_data_reader_functions_close(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate int elias_data_reader_functions_read(IntPtr instance, byte[] buffer, int bytes_to_read, byte urgent);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate byte elias_data_reader_functions_seek(IntPtr instance, uint offset_in_bytes);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate uint elias_data_reader_functions_get_size(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate uint elias_data_reader_functions_get_position(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_data_reader_functions_free_instance(IntPtr instance, IntPtr memory_functions);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate IntPtr elias_decoder_create_instance(IntPtr memory_functions, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate elias_result_codes elias_decoder_open(IntPtr instance, IntPtr reader_functions, IntPtr reader_instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_decoder_close(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate elias_result_codes elias_decoder_read(IntPtr instance, float[] buffer, uint[] frames);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate elias_result_codes elias_decoder_seek(IntPtr instance, uint frame);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate uint elias_decoder_get_position(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate uint elias_decoder_get_length(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate uint elias_decoder_get_sample_rate(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate byte elias_decoder_get_channels(IntPtr instance);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_decoder_free_instance(IntPtr instance, IntPtr memory_functions);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate byte elias_effect_info_versions_compatible(uint effect_version, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_effect_info_get_parameter_descriptive_name(byte parameter_index, [In, Out] IntPtr name, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_effect_info_get_parameter_value_unit_label(byte parameter_index, [In, Out] IntPtr unit, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate uint elias_effect_info_get_parameter_type(byte parameter_index, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_effect_info_get_double_parameter_range(byte parameter_index, out double min_value, out double max_value, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_effect_info_get_int32_parameter_range(byte parameter_index, out int min_value, out int max_value, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_effect_info_get_int32_option_parameter_descriptive_name(byte parameter_index, int value, [In, Out] IntPtr name, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_effect_info_get_input_descriptive_name(byte input_index, [In, Out] IntPtr name, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate byte elias_effect_info_supports_sample_rate(uint sample_rate, IntPtr user);
#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate byte elias_effect_info_supports_channels(byte channels, IntPtr user);
    public enum elias_result_codes
    {
        elias_result_success = 0, /**< All is well. */
        elias_error_unknown, /**< An unknown error condition. */
        elias_error_badhandle, /**< An invalid handle was given. */
        elias_error_incompatibleversion, /**< The given version is not compatible. */
        elias_error_invalidinput, /**< One or more invalid parameters and/or invalid input data. */
        elias_error_outofmemory, /**< Out of memory. */
        elias_error_forbidden, /**< This action is forbidden. */
        elias_error_overlap, /**< No overlap is allowed. */
        elias_error_segmentoutofrange, /**< One or more segments are outside the bar boundaries of the theme. */
        elias_error_transitionoptionnotset, /**< The specified option was not set. */
        elias_error_transitionpresetnotfound, /**< The specified transition preset was not found. */
        elias_error_trackgroupnotfound, /**< The specified track group was not found. */
        elias_error_trackgroupempty, /**< The specified track group is empty. */
        elias_error_actionpresetnotfound, /**< The specified action preset was not found. */
        elias_error_actionpresetempty, /**< The specified action preset is empty. */
        elias_error_themenotfound, /**< The specified theme was not found. */
        elias_error_tracknotfound, /**< The specified track was not found. */
        elias_error_levelnotfound, /**< The specified level was not found. */
        elias_error_variationnotfound, /**< The specified variation was not found. */
        elias_error_decodernotfound, /**< The specified decoder was not found. */
        elias_error_effectnotfound, /**< The specified effect was not found. */
        elias_error_effectinputnotfound, /**< The specified input was not found for the given effect. */
        elias_error_busnotfound, /**< The specified bus was not found or the bus was of the wrong type. */
        elias_error_nodecoders, /**< No decoders were found. */
        elias_error_effecterror, /**< An effect reported an unexpected error. */
        elias_error_cyclic, /**< This operation would cause a cyclic dependency. */
        elias_error_slotempty, /**< The specified slot is empty. */
        elias_error_slotnotempty, /**< The specified slot is not empty. */
        elias_error_notaneffectslot, /**< The specified slot does not contain an effect. */
        elias_error_notasendslot, /**< The specified slot does not contain a send (either to a bus or to an effect input). */
        elias_error_notaneffectsendslot, /**< The specified slot does not contain a send to an effect input. */
        elias_error_requiredthememismatch, /**< The theme at the transition point does not match what is required. */
        elias_error_actionpresetneedsexplicittheme, /**< The action preset must set an initially required theme. */
        elias_error_running, /**< Cannot perform this operation while the engine is running. */
        elias_error_notrunning, /**< Cannot perform this operation while the engine isn't running. */
        elias_error_nothemes, /**< No themes have been added. */
        elias_error_notastingertrack, /**< This is not a stinger track. */
        elias_error_invalidfortracktype, /**< The given operation cannot be performed for this type of track. */
        elias_error_invaliddatatype, /**< An invalid data type was used for the given transition option. */
        elias_error_threading, /**< Could not start one or more of the internal thread/synchronization subsystems. */
        elias_error_duplicate, /**< Duplicate entries are not allowed. */
        elias_error_usercancelled, /**< The operation was cancelled by the user. */
        elias_error_filenotfound, /**< File not found. */
        elias_error_unknownformat, /**< Unknown format. */
        elias_error_invalidformat, /**< Invalid format. */
        elias_error_reading, /**< Reading error. */
        elias_error_unsupportedchannelmapping, /**< Unsupported channel mapping. */
        elias_error_unsupportedsamplerate, /**< Unsupported sample rate. */
        elias_error_unsupportedbitdepth, /**< Unsupported bit depth. */
        elias_error_mustsupportchannelmapping, /**< The custom effect must support the given channel mapping. */
        elias_error_mustsupportsamplerate, /**< The custom effect must support the given sample rate. */
        elias_error_unsupportedcompressionmethod, /**< Unsupported compression method. */
        elias_error_generatornotfound, /**< The specified generator was not found. */
        elias_error_invalidforgeneratortype, /**< The given operation cannot be performed for this type of generator. */
        elias_error_notalooptrack, /**< This is not a loop track. */
        elias_error_patchnotfound /** The specified patch was not found. */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_data_reader_functions
    {
        public elias_data_reader_functions_create_instance create_instance;
        public elias_data_reader_functions_open open;
        public elias_data_reader_functions_close close;
        public elias_data_reader_functions_read read;
        public elias_data_reader_functions_seek seek;
        public elias_data_reader_functions_get_size get_size;
        public elias_data_reader_functions_get_position get_position;
        public elias_data_reader_functions_free_instance free_instance;
        public IntPtr user;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_memory_functions
    {
        public elias_memory_functions_malloc malloc;
        public elias_memory_functions_free free;
        public elias_memory_functions_realloc realloc;
        public IntPtr user;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_audio_segment
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string filename;
        public UInt16 start_bar;
        public UInt16 length_in_bars;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_source_specifier
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string theme;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string track;
        public int level;
        public int variation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_source
    {
        public uint theme_index;
        public uint track_index;
        public uint track_type;
        public int level;
        public int variation;
        public IntPtr audio_segments;
        public uint audio_segment_count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_decoder
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string decoder_name;
        public elias_decoder_create_instance create_instance;
        public elias_decoder_open open;
        public elias_decoder_close close;
        public elias_decoder_read read;
        public elias_decoder_seek seek;
        public elias_decoder_get_position get_position;
        public elias_decoder_get_length get_length;
        public elias_decoder_get_sample_rate get_sample_rate;
        public elias_decoder_get_channels get_channels;
        public elias_decoder_free_instance free_instance;
        public IntPtr user;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct elias_effect_parameter_value
    {
        [FieldOffset(0)]
        public double double_value;
        [FieldOffset(0)]
        public int int32_value;
        [FieldOffset(0)]
        public byte bool_value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_effect_info_wrapped
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string effect_name;
        public uint effect_version;
        public elias_effect_info_versions_compatible versions_compatible;
        public byte parameter_count;
        public elias_effect_info_get_parameter_descriptive_name get_parameter_descriptive_name;
        public elias_effect_info_get_parameter_value_unit_label get_parameter_value_unit_label;
        public elias_effect_info_get_parameter_type get_parameter_type;
        public elias_effect_info_get_double_parameter_range get_double_parameter_range;
        public elias_effect_info_get_int32_parameter_range get_int32_parameter_range;
        public elias_effect_info_get_int32_option_parameter_descriptive_name get_int32_option_parameter_descriptive_name;
        public byte input_count;
        public elias_effect_info_get_input_descriptive_name get_input_descriptive_name;
        public elias_effect_info_supports_sample_rate supports_sample_rate;
        public elias_effect_info_supports_channels supports_channels;
        public IntPtr user;
    }

    /// <summary>
    /// Abstract layer for elias_effect_info_bridge returned from C++ side. Manages strings transform between C# and C++.
    /// </summary>
    public class elias_effect_info
    {
        // Constructor
        public elias_effect_info(elias_effect_info_wrapped info_bridge)
        {
            // Info
            this.info_bridge = info_bridge;
            // Fill delegates
            this.versions_compatible = info_bridge.versions_compatible;
            this.get_parameter_type = info_bridge.get_parameter_type;
            this.get_double_parameter_range = info_bridge.get_double_parameter_range;
            this.get_int32_parameter_range = info_bridge.get_int32_parameter_range;
            this.supports_sample_rate = info_bridge.supports_sample_rate;
            this.supports_channels = info_bridge.supports_channels;
        }

        // Filed that keeps informations from C++ plugin side
        private elias_effect_info_wrapped info_bridge;

        // Properties that change info_bridge
        public string effect_name
        {
            get
            {
                return info_bridge.effect_name;
            }
            set
            {
                info_bridge.effect_name = value;
            }
        }
        public uint effect_version
        {
            get
            {
                return info_bridge.effect_version;
            }
            set
            {
                info_bridge.effect_version = value;
            }
        }
        public byte parameter_count
        {
            get
            {
                return info_bridge.parameter_count;
            }
            set
            {
                info_bridge.parameter_count = value;
            }
        }
        public byte input_count
        {
            get
            {
                return info_bridge.input_count;
            }
            set
            {
                info_bridge.input_count = value;
            }
        }
        public IntPtr user
        {
            get
            {
                return info_bridge.user;
            }
            set
            {
                info_bridge.user = value;
            }
        }

        // Delegates that are equal to info_bridge.<delegate_name>
        public elias_effect_info_versions_compatible versions_compatible;
        public elias_effect_info_get_parameter_type get_parameter_type;
        public elias_effect_info_get_double_parameter_range get_double_parameter_range;
        public elias_effect_info_get_int32_parameter_range get_int32_parameter_range;
        public elias_effect_info_supports_sample_rate supports_sample_rate;
        public elias_effect_info_supports_channels supports_channels;

        // Delegates that used StringBuilder earlier, after replaced StringBuilder to IntPtr they need to manage memory itself
        // so they are changed to be methods that invoke info_bridge delegates
        public void get_parameter_descriptive_name(byte parameter_index, StringBuilder name, IntPtr user)
        {
            // Set length of string that will be returned from C++ side
            int length = STRING_LENGTH;
            // Allocate memory for the string value that will be returned from C++ side
            IntPtr out_name_b = Marshal.AllocHGlobal(length);

            // Check is memory allocated
            if (out_name_b != IntPtr.Zero)
            {
                // Use safty block to be sure that memory will always be free
                try
                {
                    // Call function from C++ native plugin
                    info_bridge.get_parameter_descriptive_name(parameter_index, out_name_b, user);
                    // Clear name
                    name.Remove(0, name.Length);
                    // Convert memory block to the StringBuilder
                    name.Insert(0, StringFromNativeUtf8(out_name_b));
                }
                finally
                {
                    // Free memory
                    Marshal.FreeHGlobal(out_name_b);
                }

            }
            else
            {
                Debug.LogError("Out of memeory");
            }

        }
        public void get_parameter_value_unit_label(byte parameter_index, StringBuilder unit, IntPtr user)
        {
            // Set length of string that will be returned from C++ side
            int length = STRING_LENGTH;
            // Allocate memory for the string value that will be returned from C++ side
            IntPtr out_name_b = Marshal.AllocHGlobal(length);

            // Check is memory allocated
            if (out_name_b != IntPtr.Zero)
            {
                // Use safty block to be sure that memory will always be free
                try
                {
                    // Call function from C++ native plugin
                    info_bridge.get_parameter_value_unit_label(parameter_index, out_name_b, user);
                    // Clear name
                    unit.Remove(0, unit.Length);
                    // Convert memory block to the StringBuilder
                    unit.Insert(0, StringFromNativeUtf8(out_name_b));
                }
                finally
                {
                    // Free memory
                    Marshal.FreeHGlobal(out_name_b);
                }

            }
            else
            {
                Debug.LogError("Out of memeory");
            }
        }
        public void get_int32_option_parameter_descriptive_name(byte parameter_index, int value, StringBuilder name, IntPtr user)
        {
            // Set length of string that will be returned from C++ side
            int length = STRING_LENGTH;
            // Allocate memory for the string value that will be returned from C++ side
            IntPtr out_name_b = Marshal.AllocHGlobal(length);

            // Check is memory allocated
            if (out_name_b != IntPtr.Zero)
            {
                // Use safty block to be sure that memory will always be free
                try
                {
                    // Call function from C++ native plugin
                    info_bridge.get_int32_option_parameter_descriptive_name(parameter_index, value, out_name_b, user);
                    // Clear name
                    name.Remove(0, name.Length);
                    // Convert memory block to the StringBuilder
                    name.Insert(0, StringFromNativeUtf8(out_name_b));
                }
                finally
                {
                    // Free memory
                    Marshal.FreeHGlobal(out_name_b);
                }

            }
            else
            {
                Debug.LogError("Out of memeory");
            }
        }
        public void get_input_descriptive_name(byte input_index, StringBuilder name, IntPtr user)
        {
            // Set length of string that will be returned from C++ side
            int length = STRING_LENGTH;
            // Allocate memory for the string value that will be returned from C++ side
            IntPtr out_name_b = Marshal.AllocHGlobal(length);

            // Check is memory allocated
            if (out_name_b != IntPtr.Zero)
            {
                // Use safty block to be sure that memory will always be free
                try
                {
                    // Call function from C++ native plugin
                    info_bridge.get_input_descriptive_name(input_index, out_name_b, user);
                    // Clear name
                    name.Remove(0, name.Length);
                    // Convert memory block to the StringBuilder
                    name.Insert(0, StringFromNativeUtf8(out_name_b));
                }
                finally
                {
                    // Free memory
                    Marshal.FreeHGlobal(out_name_b);
                }

            }
            else
            {
                Debug.LogError("Out of memeory");
            }
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_effect_parameter
    {
        public uint type;
        public elias_effect_parameter_value value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct elias_active_source
    {
        public elias_source source;
        public uint status;
        public int active_segment_index;
    }

#if !UNITY_IOS || UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void elias_audio_buffer_callback(elias_handle handle, IntPtr buffer, string bus_name, IntPtr user);


    static elias_audio_buffer_callback s_elias_audio_buffer_callback;

    [AOT.MonoPInvokeCallback(typeof(elias_audio_buffer_callback))]
    static void call_elias_audio_buffer_callback(elias_handle handle, IntPtr buffer, string bus_name, IntPtr user)
    {
        if (s_elias_audio_buffer_callback != null)
            s_elias_audio_buffer_callback(handle, buffer, bus_name, user);
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_file_reader")]
    public static extern void elias_get_file_reader(out elias_data_reader_functions reader_functions, uint abi_version);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_version_string")]
    public static extern string elias_get_version_string();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_initialize")]
    private static extern elias_handle elias_initialize(out elias_result_codes result_code, uint abi_version, IntPtr reader_functions, string base_path, uint sample_rate, byte channels, UInt16 frames_per_buffer, IntPtr memory_functions);
    public static elias_handle elias_initialize_wrapped(out elias_result_codes result_code, uint abi_version, elias_data_reader_functions? reader_functions, string base_path, uint sample_rate, byte channels, UInt16 frames_per_buffer, elias_memory_functions? memory_functions)
    {
        IntPtr reader_functions_ptr = IntPtr.Zero;
        if (reader_functions.HasValue)
        {
            reader_functions_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@reader_functions.GetType()));
            Marshal.StructureToPtr(reader_functions.Value, reader_functions_ptr, true);
        }
        IntPtr memory_functions_ptr = IntPtr.Zero;
        if (memory_functions.HasValue)
        {
            memory_functions_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@memory_functions.GetType()));
            Marshal.StructureToPtr(memory_functions.Value, memory_functions_ptr, true);
        }
        elias_handle returnValue = elias_initialize(out result_code, abi_version, reader_functions_ptr, base_path, sample_rate, channels, frames_per_buffer, memory_functions_ptr);
        if (reader_functions.HasValue)
        {
            Marshal.FreeHGlobal(reader_functions_ptr);
        }
        if (memory_functions.HasValue)
        {
            Marshal.FreeHGlobal(memory_functions_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_free")]
    public static extern void elias_free(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_archive")]
    public static extern elias_result_codes elias_set_archive(elias_handle handle, string archive_path, uint flags);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_base_path")]
    public static extern string elias_get_base_path(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_archive_path")]
    public static extern string elias_get_archive_path(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_source_count")]
    private static extern uint elias_get_source_count(elias_handle handle, IntPtr filter);
    public static uint elias_get_source_count_wrapped(elias_handle handle, elias_source_specifier? filter)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        uint returnValue = elias_get_source_count(handle, filter_ptr);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_source_info")]
    private static extern elias_result_codes elias_get_source_info(elias_handle handle, IntPtr filter, uint index, out elias_source result);
    public static elias_result_codes elias_get_source_info_wrapped(elias_handle handle, elias_source_specifier? filter, uint index, out elias_source result)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_get_source_info(handle, filter_ptr, index, out result);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_add_transition_preset")]
    public static extern elias_result_codes elias_add_transition_preset(elias_handle handle, string new_preset_name, string copy_from_existing);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_transition_option_int32")]
    private static extern elias_result_codes elias_set_transition_option_int32(elias_handle handle, IntPtr filter, uint option_id, int value, string transition_preset, byte clear_children);
    public static elias_result_codes elias_set_transition_option_int32_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, int value, string transition_preset, byte clear_children)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_set_transition_option_int32(handle, filter_ptr, option_id, value, transition_preset, clear_children);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_transition_option_double")]
    private static extern elias_result_codes elias_set_transition_option_double(elias_handle handle, IntPtr filter, uint option_id, double value, string transition_preset, byte clear_children);
    public static elias_result_codes elias_set_transition_option_double_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, double value, string transition_preset, byte clear_children)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_set_transition_option_double(handle, filter_ptr, option_id, value, transition_preset, clear_children);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_transition_option_bool")]
    private static extern elias_result_codes elias_set_transition_option_bool(elias_handle handle, IntPtr filter, uint option_id, byte value, string transition_preset, byte clear_children);
    public static elias_result_codes elias_set_transition_option_bool_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, byte value, string transition_preset, byte clear_children)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_set_transition_option_bool(handle, filter_ptr, option_id, value, transition_preset, clear_children);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_transition_option_int32_array")]
    private static extern elias_result_codes elias_set_transition_option_int32_array(elias_handle handle, IntPtr filter, uint option_id, int[] elements, uint count, string transition_preset, byte clear_children);
    public static elias_result_codes elias_set_transition_option_int32_array_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, int[] elements, uint count, string transition_preset, byte clear_children)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_set_transition_option_int32_array(handle, filter_ptr, option_id, elements, count, transition_preset, clear_children);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_transition_option_double_array")]
    private static extern elias_result_codes elias_set_transition_option_double_array(elias_handle handle, IntPtr filter, uint option_id, double[] elements, uint count, string transition_preset, byte clear_children);
    public static elias_result_codes elias_set_transition_option_double_array_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, double[] elements, uint count, string transition_preset, byte clear_children)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_set_transition_option_double_array(handle, filter_ptr, option_id, elements, count, transition_preset, clear_children);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_transition_option_bool_array")]
    private static extern elias_result_codes elias_set_transition_option_bool_array(elias_handle handle, IntPtr filter, uint option_id, byte[] elements, uint count, string transition_preset, byte clear_children);
    public static elias_result_codes elias_set_transition_option_bool_array_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, byte[] elements, uint count, string transition_preset, byte clear_children)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_set_transition_option_bool_array(handle, filter_ptr, option_id, elements, count, transition_preset, clear_children);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_remove_transition_options")]
    private static extern elias_result_codes elias_remove_transition_options(elias_handle handle, IntPtr filter, uint option_id, string transition_preset, byte find_exact);
    public static elias_result_codes elias_remove_transition_options_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_id, string transition_preset, byte find_exact)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        elias_result_codes returnValue = elias_remove_transition_options(handle, filter_ptr, option_id, transition_preset, find_exact);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_int32")]
    private static extern elias_result_codes elias_get_transition_option_int32(elias_handle handle, IntPtr source, uint option_id, out int value, string transition_preset, byte find_exact);
    public static elias_result_codes elias_get_transition_option_int32_wrapped(elias_handle handle, elias_source_specifier? source, uint option_id, out int value, string transition_preset, byte find_exact)
    {
        IntPtr source_ptr = IntPtr.Zero;
        if (source.HasValue)
        {
            source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@source.GetType()));
            Marshal.StructureToPtr(source.Value, source_ptr, true);
        }
        elias_result_codes returnValue = elias_get_transition_option_int32(handle, source_ptr, option_id, out value, transition_preset, find_exact);
        if (source.HasValue)
        {
            Marshal.FreeHGlobal(source_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_double")]
    private static extern elias_result_codes elias_get_transition_option_double(elias_handle handle, IntPtr source, uint option_id, out double value, string transition_preset, byte find_exact);
    public static elias_result_codes elias_get_transition_option_double_wrapped(elias_handle handle, elias_source_specifier? source, uint option_id, out double value, string transition_preset, byte find_exact)
    {
        IntPtr source_ptr = IntPtr.Zero;
        if (source.HasValue)
        {
            source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@source.GetType()));
            Marshal.StructureToPtr(source.Value, source_ptr, true);
        }
        elias_result_codes returnValue = elias_get_transition_option_double(handle, source_ptr, option_id, out value, transition_preset, find_exact);
        if (source.HasValue)
        {
            Marshal.FreeHGlobal(source_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_bool")]
    private static extern elias_result_codes elias_get_transition_option_bool(elias_handle handle, IntPtr source, uint option_id, out byte value, string transition_preset, byte find_exact);
    public static elias_result_codes elias_get_transition_option_bool_wrapped(elias_handle handle, elias_source_specifier? source, uint option_id, out byte value, string transition_preset, byte find_exact)
    {
        IntPtr source_ptr = IntPtr.Zero;
        if (source.HasValue)
        {
            source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@source.GetType()));
            Marshal.StructureToPtr(source.Value, source_ptr, true);
        }
        elias_result_codes returnValue = elias_get_transition_option_bool(handle, source_ptr, option_id, out value, transition_preset, find_exact);
        if (source.HasValue)
        {
            Marshal.FreeHGlobal(source_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_int32_array")]
    private static extern elias_result_codes elias_get_transition_option_int32_array(elias_handle handle, IntPtr source, uint option_id, int[] elements, out uint count, string transition_preset, byte find_exact);
    public static elias_result_codes elias_get_transition_option_int32_array_wrapped(elias_handle handle, elias_source_specifier? source, uint option_id, int[] elements, out uint count, string transition_preset, byte find_exact)
    {
        IntPtr source_ptr = IntPtr.Zero;
        if (source.HasValue)
        {
            source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@source.GetType()));
            Marshal.StructureToPtr(source.Value, source_ptr, true);
        }
        elias_result_codes returnValue = elias_get_transition_option_int32_array(handle, source_ptr, option_id, elements, out count, transition_preset, find_exact);
        if (source.HasValue)
        {
            Marshal.FreeHGlobal(source_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_double_array")]
    private static extern elias_result_codes elias_get_transition_option_double_array(elias_handle handle, IntPtr source, uint option_id, double[] elements, out uint count, string transition_preset, byte find_exact);
    public static elias_result_codes elias_get_transition_option_double_array_wrapped(elias_handle handle, elias_source_specifier? source, uint option_id, double[] elements, out uint count, string transition_preset, byte find_exact)
    {
        IntPtr source_ptr = IntPtr.Zero;
        if (source.HasValue)
        {
            source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@source.GetType()));
            Marshal.StructureToPtr(source.Value, source_ptr, true);
        }
        elias_result_codes returnValue = elias_get_transition_option_double_array(handle, source_ptr, option_id, elements, out count, transition_preset, find_exact);
        if (source.HasValue)
        {
            Marshal.FreeHGlobal(source_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_bool_array")]
    private static extern elias_result_codes elias_get_transition_option_bool_array(elias_handle handle, IntPtr source, uint option_id, byte[] elements, out uint count, string transition_preset, byte find_exact);
    public static elias_result_codes elias_get_transition_option_bool_array_wrapped(elias_handle handle, elias_source_specifier? source, uint option_id, byte[] elements, out uint count, string transition_preset, byte find_exact)
    {
        IntPtr source_ptr = IntPtr.Zero;
        if (source.HasValue)
        {
            source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@source.GetType()));
            Marshal.StructureToPtr(source.Value, source_ptr, true);
        }
        elias_result_codes returnValue = elias_get_transition_option_bool_array(handle, source_ptr, option_id, elements, out count, transition_preset, find_exact);
        if (source.HasValue)
        {
            Marshal.FreeHGlobal(source_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_count")]
    private static extern uint elias_get_transition_option_count(elias_handle handle, IntPtr filter, string transition_preset, byte find_exact);
    public static uint elias_get_transition_option_count_wrapped(elias_handle handle, elias_source_specifier? filter, string transition_preset, byte find_exact)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }
        uint returnValue = elias_get_transition_option_count(handle, filter_ptr, transition_preset, find_exact);
        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_info")]
    private static extern elias_result_codes elias_get_transition_option_info(elias_handle handle, IntPtr filter, uint option_index, string transition_preset, byte find_exact, out uint option_id, out uint option_data_type, [In, Out] IntPtr theme_name, [In, Out] IntPtr track_name, out int level, out int variation);
    public static elias_result_codes elias_get_transition_option_info_wrapped(elias_handle handle, elias_source_specifier? filter, uint option_index, string transition_preset, byte find_exact, out uint option_id, out uint option_data_type, out string theme_name, out string track_name, out int level, out int variation)
    {
        IntPtr filter_ptr = IntPtr.Zero;
        if (filter.HasValue)
        {
            filter_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@filter.GetType()));
            Marshal.StructureToPtr(filter.Value, filter_ptr, true);
        }

        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        theme_name = null;
        track_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr theme_name_b = Marshal.AllocHGlobal(length);
        IntPtr track_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if ((theme_name_b != IntPtr.Zero) && (track_name_b != IntPtr.Zero))
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_transition_option_info(handle, filter_ptr, option_index, transition_preset, find_exact, out option_id, out option_data_type, theme_name_b, track_name_b, out level, out variation);
                // Convert memory block to the string
                theme_name = StringFromNativeUtf8(theme_name_b);
                track_name = StringFromNativeUtf8(track_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(theme_name_b);
                Marshal.FreeHGlobal(track_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;

            // Check which part of memory was not allocated and free allocated memeory
            if (theme_name_b != IntPtr.Zero)
                Marshal.FreeHGlobal(theme_name_b);
            else if (track_name_b != IntPtr.Zero)
                Marshal.FreeHGlobal(track_name_b);

            // Initialize values that have to be returned
            option_id = 0;
            option_data_type = 0;
            level = 0;
            variation = 0;
        }

        if (filter.HasValue)
        {
            Marshal.FreeHGlobal(filter_ptr);
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_option_data_type")]
    public static extern elias_result_codes elias_get_transition_option_data_type(uint option_id, out uint option_data_type);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_preset_count")]
    public static extern uint elias_get_transition_preset_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_preset_name")]
    private static extern elias_result_codes elias_get_transition_preset_name(elias_handle handle, uint transition_preset_index, [In, Out] IntPtr out_name);
    public static elias_result_codes elias_get_transition_preset_name_wrapped(elias_handle handle, uint transition_preset_index, out string out_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_transition_preset_name(handle, transition_preset_index, out_name_b);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_transition_preset_index")]
    public static extern elias_result_codes elias_get_transition_preset_index(elias_handle handle, string transition_preset_name, out uint index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_action_preset_count")]
    public static extern uint elias_get_action_preset_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_action_preset_name")]
    private static extern elias_result_codes elias_get_action_preset_name(elias_handle handle, uint action_preset_index, [In, Out] IntPtr out_name);
    public static elias_result_codes elias_get_action_preset_name_wrapped(elias_handle handle, uint action_preset_index, out string out_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_action_preset_name(handle, action_preset_index, out_name_b);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_action_preset_index")]
    public static extern elias_result_codes elias_get_action_preset_index(elias_handle handle, string action_preset_name, out uint index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_action_preset_required_initial_theme")]
    private static extern elias_result_codes elias_get_action_preset_required_initial_theme(elias_handle handle, string preset_name, [In, Out] IntPtr theme_name);
    public static elias_result_codes elias_get_action_preset_required_initial_theme_wrapped(elias_handle handle, string preset_name, out string theme_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        theme_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr theme_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (theme_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_action_preset_required_initial_theme(handle, preset_name, theme_name_b);
                // Convert memory block to the string
                theme_name = StringFromNativeUtf8(theme_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(theme_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_theme_count")]
    public static extern uint elias_get_theme_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_theme_name")]
    private static extern elias_result_codes elias_get_theme_name(elias_handle handle, uint theme_index, [In, Out] IntPtr out_name);
    public static elias_result_codes elias_get_theme_name_wrapped(elias_handle handle, uint theme_index, out string out_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_theme_name(handle, theme_index, out_name_b);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_theme_index")]
    public static extern elias_result_codes elias_get_theme_index(elias_handle handle, string theme_name, out uint index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_count")]
    public static extern uint elias_get_track_count(elias_handle handle, string theme_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_name")]
    private static extern elias_result_codes elias_get_track_name(elias_handle handle, string theme_name, uint track_index, [In, Out] IntPtr out_name);
    public static elias_result_codes elias_get_track_name_wrapped(elias_handle handle, string theme_name, uint track_index, out string out_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_track_name(handle, theme_name, track_index, out_name_b);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_index")]
    public static extern elias_result_codes elias_get_track_index(elias_handle handle, string theme_name, string track_name, out uint index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_group_count")]
    public static extern uint elias_get_track_group_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_group_name")]
    private static extern elias_result_codes elias_get_track_group_name(elias_handle handle, uint track_group_index, [In, Out] IntPtr out_name);
    public static elias_result_codes elias_get_track_group_name_wrapped(elias_handle handle, uint track_group_index, out string out_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_track_group_name(handle, track_group_index, out_name_b);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_group_index")]
    public static extern elias_result_codes elias_get_track_group_index(elias_handle handle, string track_group_name, out uint index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_group_theme")]
    private static extern elias_result_codes elias_get_track_group_theme(elias_handle handle, string group_name, [In, Out] IntPtr theme_name);
    public static elias_result_codes elias_get_track_group_theme_wrapped(elias_handle handle, string group_name, out string theme_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        theme_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr theme_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (theme_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_track_group_theme(handle, group_name, theme_name_b);
                // Convert memory block to the string
                theme_name = StringFromNativeUtf8(theme_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(theme_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_count_in_track_group")]
    public static extern uint elias_get_track_count_in_track_group(elias_handle handle, string group_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_in_track_group")]
    private static extern elias_result_codes elias_get_track_in_track_group(elias_handle handle, string group_name, uint track_index, [In, Out] IntPtr track_name);
    public static elias_result_codes elias_get_track_in_track_group_wrapped(elias_handle handle, string group_name, uint track_index, out string track_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        track_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr track_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (track_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_track_in_track_group(handle, group_name, track_index, track_name_b);
                // Convert memory block to the string
                track_name = StringFromNativeUtf8(track_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(track_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_greatest_level_in_theme")]
    public static extern int elias_get_greatest_level_in_theme(elias_handle handle, string theme_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_greatest_level_on_track")]
    public static extern int elias_get_greatest_level_on_track(elias_handle handle, string theme_name, string track_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_theme_basic_info")]
    public static extern elias_result_codes elias_get_theme_basic_info(elias_handle handle, string theme_name, out double initial_bpm, out UInt16 initial_timesig_numerator, out UInt16 initial_timesig_denominator, out UInt16 bars);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_theme_length_in_seconds")]
    public static extern elias_result_codes elias_get_theme_length_in_seconds(elias_handle handle, string theme_name, out double length);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_convert_seconds_to_bars_and_beats")]
    public static extern elias_result_codes elias_convert_seconds_to_bars_and_beats(elias_handle handle, string theme_name, double seconds, out UInt16 bar, out double beat);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_track_type")]
    public static extern elias_result_codes elias_get_track_type(elias_handle handle, string theme_name, string track_name, out uint type);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_deserialize")]
    public static extern elias_result_codes elias_deserialize(elias_handle handle, byte[] buffer, uint buffer_size, uint flags);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_deserialize_from_file")]
    public static extern elias_result_codes elias_deserialize_from_file(elias_handle handle, string filename, UInt32 flags );

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_register_custom_decoder")]
    private static extern elias_result_codes elias_register_custom_decoder(elias_handle handle, IntPtr decoder_implementation);
    public static elias_result_codes elias_register_custom_decoder_wrapped(elias_handle handle, elias_decoder? decoder_implementation)
    {
        IntPtr decoder_implementation_ptr = IntPtr.Zero;
        if (decoder_implementation.HasValue)
        {
            decoder_implementation_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@decoder_implementation.GetType()));
            Marshal.StructureToPtr(decoder_implementation.Value, decoder_implementation_ptr, true);
        }
        elias_result_codes returnValue = elias_register_custom_decoder(handle, decoder_implementation_ptr);
        if (decoder_implementation.HasValue)
        {
            Marshal.FreeHGlobal(decoder_implementation_ptr);
        }
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_decoder_count")]
    public static extern uint elias_get_decoder_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_decoder")]
    public static extern elias_result_codes elias_get_decoder(elias_handle handle, uint decoder_index, out elias_decoder decoder);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_decoder_by_name")]
    public static extern elias_result_codes elias_get_decoder_by_name(elias_handle handle, string decoder_name, out elias_decoder decoder);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_effect_count")]
    public static extern uint elias_get_effect_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_effect_info")]
    public static extern elias_result_codes elias_get_effect_info(elias_handle handle, uint effect_index, out elias_effect_info_wrapped info);
    public static elias_result_codes elias_get_effect_info_wrapped(elias_handle handle, uint effect_index, out elias_effect_info info)
    {
        // Declare structure
        elias_effect_info_wrapped info_bridge;
        // Get info from C++ part
        var returnCode = elias_get_effect_info(handle, effect_index, out info_bridge);
        // Check does method success, otherwise just return error code
        if (returnCode == elias_result_codes.elias_result_success)
        {
            // Create info class
            info = new elias_effect_info(info_bridge);
        }
        else
            info = null;

        // Return code
        return returnCode;
    }


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_effect_info_by_name")]
    public static extern elias_result_codes elias_get_effect_info_by_name(elias_handle handle, string effect_name, out elias_effect_info_wrapped info);
    public static elias_result_codes elias_get_effect_info_by_name_wrapped(elias_handle handle, string effect_name, out elias_effect_info info)
    {
        // Declare structure
        elias_effect_info_wrapped info_bridge;
        // Get info from C++ part
        var returnCode = elias_get_effect_info_by_name(handle, effect_name, out info_bridge);
        // Check does method success, otherwise just return error code
        if (returnCode == elias_result_codes.elias_result_success)
        {
            // Create info class
            info = new elias_effect_info(info_bridge);
        }
        else
            info = null;

        // Return code
        return returnCode;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_event_count_in_action_preset")]
    public static extern uint elias_get_event_count_in_action_preset(elias_handle handle, string preset_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_event_in_action_preset")]
    public static extern elias_result_codes elias_get_event_in_action_preset(elias_handle handle, string preset_name, uint event_index, out elias_event_out outEvent);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_configure_cache")]
    public static extern elias_result_codes elias_configure_cache(elias_handle handle, uint page_count, uint bytes_per_page);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_cache_configuration")]
    public static extern elias_result_codes elias_get_cache_configuration(elias_handle handle, out uint page_count, out uint bytes_per_page);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_audio_file_count")]
    public static extern uint elias_get_audio_file_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_audio_file_name")]
    public static extern string elias_get_audio_file_name(elias_handle handle, uint audio_file_index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_audio_file_length_in_seconds")]
    public static extern elias_result_codes elias_get_audio_file_length_in_seconds(elias_handle handle, string filename, out double length_in_seconds);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_preload_audio_file")]
    public static extern elias_result_codes elias_preload_audio_file(elias_handle handle, string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_unload_audio_file")]
    public static extern elias_result_codes elias_unload_audio_file(elias_handle handle, string filename);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_midi_file_count")]
    public static extern uint elias_get_midi_file_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_midi_file_name")]
    public static extern string elias_get_midi_file_name(elias_handle handle, uint audio_file_index);



    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_start")]
    private static extern elias_result_codes elias_start(elias_handle handle, IntPtr start_event);
    public static elias_result_codes elias_start_wrapped(elias_handle handle, elias_event_set_level start_event)
    {
        IntPtr start_event_ptr = IntPtr.Zero;
        start_event_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@start_event.GetType()));
        Marshal.StructureToPtr(start_event, start_event_ptr, true);
        elias_result_codes returnValue = elias_start(handle, start_event_ptr);
        Marshal.FreeHGlobal(start_event_ptr);
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_start_background")]
    private static extern elias_result_codes elias_start_background(elias_handle handle, IntPtr start_event, uint output_sample_rate, uint output_buffer_size );
    public static elias_result_codes elias_start_background_wrapped(elias_handle handle, elias_event_set_level start_event, uint output_sample_rate, uint output_buffer_size)
    {
        IntPtr start_event_ptr = IntPtr.Zero;
        start_event_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@start_event.GetType()));
        Marshal.StructureToPtr(start_event, start_event_ptr, true);
        elias_result_codes returnValue = elias_start_background(handle, start_event_ptr, output_sample_rate, output_buffer_size);
        Marshal.FreeHGlobal(start_event_ptr);
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_start_with_action_preset")]
    public static extern elias_result_codes elias_start_with_action_preset(elias_handle handle, string preset_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_start_background_with_action_preset")]
    public static extern elias_result_codes elias_start_background_with_action_preset(elias_handle handle, string preset_name, uint output_sample_rate, uint output_buffer_size);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_active_theme_index")]
    public static extern int elias_get_active_theme_index(elias_handle handle, double[] cursor_in_seconds);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_queue_event")]
    private static extern elias_result_codes elias_queue_event(elias_handle handle, IntPtr @event);
    public static elias_result_codes elias_queue_event_wrapped(elias_handle handle, elias_event @event)
    {
        IntPtr event_ptr = IntPtr.Zero;
        event_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@event.GetType()));
        Marshal.StructureToPtr(@event, event_ptr, true);
        elias_result_codes returnValue = elias_queue_event(handle, @event_ptr);
        Marshal.FreeHGlobal(event_ptr);
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_can_run_action_preset")]
    public static extern elias_result_codes elias_can_run_action_preset(elias_handle handle, string preset_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_run_action_preset")]
    public static extern elias_result_codes elias_run_action_preset(elias_handle handle, string preset_name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_clear_events")]
    public static extern elias_result_codes elias_clear_events(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_event_count")]
    public static extern uint elias_get_event_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_event")]
    public static extern elias_result_codes elias_get_event(elias_handle handle, uint event_index, out elias_event_out outEvent);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_active_source_count")]
    public static extern uint elias_get_active_source_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_active_source_info")]
    public static extern elias_result_codes elias_get_active_source_info(elias_handle handle, uint source_index, out elias_active_source info);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_lock_mixer")]
    public static extern elias_result_codes elias_lock_mixer(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_unlock_mixer")]
    public static extern elias_result_codes elias_unlock_mixer(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_render_buffer")]
    private static extern elias_result_codes elias_render_buffer(elias_handle handle, [MarshalAs(UnmanagedType.FunctionPtr)] elias_audio_buffer_callback callback, IntPtr user, uint flags);
    public static elias_result_codes elias_render_buffer_wrapped(elias_handle handle, elias_audio_buffer_callback callback, IntPtr user, uint flags)
    {
        s_elias_audio_buffer_callback = callback;
        elias_result_codes returnValue = elias_render_buffer(handle, call_elias_audio_buffer_callback, user, flags);
        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_bus_count")]
    public static extern uint elias_get_bus_count(elias_handle handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_bus_name")]
    private static extern elias_result_codes elias_get_bus_name(elias_handle handle, uint bus_index, [In, Out] IntPtr out_name);
    public static elias_result_codes elias_get_bus_name_wrapped(elias_handle handle, uint bus_index, out string out_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_bus_name(handle, bus_index, out_name_b);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_bus_index")]
    public static extern elias_result_codes elias_get_bus_index(elias_handle handle, string bus_name, out uint index);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_slot_type")]
    public static extern elias_result_codes elias_get_slot_type(elias_handle handle, string bus_name, byte slot, out uint type);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_send_volume")]
    public static extern elias_result_codes elias_set_send_volume(elias_handle handle, string bus_name, byte slot, double volume_db, uint fade_time_milliseconds);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_send_volume")]
    public static extern elias_result_codes elias_get_send_volume(elias_handle handle, string bus_name, byte slot, out double volume_db, uint[] milliseconds_until_fade_completes);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_send_destination_bus_info")]
    private static extern elias_result_codes elias_get_send_destination_bus_info(elias_handle handle, string bus_name, byte slot, [In, Out] IntPtr out_name, out byte destination_slot);
    public static elias_result_codes elias_get_send_destination_bus_info_wrapped(elias_handle handle, string bus_name, byte slot, out string out_name, out byte destination_slot)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_send_destination_bus_info(handle, bus_name, slot, out_name_b, out destination_slot);
                // Convert memory block to the string
                out_name = StringFromNativeUtf8(out_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
            // Initialize values that have to be initialized before leaves this method
            destination_slot = 0;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_send_destination_effect_info")]
    private static extern elias_result_codes elias_get_send_destination_effect_info(elias_handle handle, string bus_name, byte slot, [In, Out] IntPtr out_effect_name, out byte destination_effect_input_index);
    public static elias_result_codes elias_get_send_destination_effect_info_wrapped(elias_handle handle, string bus_name, byte slot, out string out_effect_name, out byte destination_effect_input_index)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_effect_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_effect_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_effect_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_send_destination_effect_info(handle, bus_name, slot, out_effect_name_b, out destination_effect_input_index);
                // Convert memory block to the string
                out_effect_name = StringFromNativeUtf8(out_effect_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_effect_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
            // Initialize values that have to be initialized before leaves this method
            destination_effect_input_index = 0;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_effect_name")]
    private static extern elias_result_codes elias_get_effect_name(elias_handle handle, string bus_name, byte slot, [In, Out] IntPtr out_effect_name);
    public static elias_result_codes elias_get_effect_name_wrapped(elias_handle handle, string bus_name, byte slot, out string out_effect_name)
    {
        // Set length of string that will be returned from C++ side
        int length = STRING_LENGTH;
        // Code that will be returned from this method
        elias_result_codes returnValue;
        // Initialize string
        out_effect_name = null;
        // Allocate memory for the string value that will be returned from C++ side
        IntPtr out_effect_name_b = Marshal.AllocHGlobal(length);

        // Check is memory allocated
        if (out_effect_name_b != IntPtr.Zero)
        {
            // Use safty block to be sure that memory will always be free
            try
            {
                // Call function from C++ native plugin
                returnValue = elias_get_effect_name(handle, bus_name, slot, out_effect_name_b);
                // Convert memory block to the string
                out_effect_name = StringFromNativeUtf8(out_effect_name_b);
            }
            finally
            {
                // Free memory
                Marshal.FreeHGlobal(out_effect_name_b);
            }

        }
        else
        {
            // Return "out of memory" code
            returnValue = elias_result_codes.elias_error_outofmemory;
        }

        return returnValue;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_set_effect_parameter")]
    public static extern elias_result_codes elias_set_effect_parameter(elias_handle handle, string bus_name, byte slot, byte effect_parameter_index, elias_effect_parameter parameter, uint sweep_time_milliseconds);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_get_effect_parameter")]
    public static extern elias_result_codes elias_get_effect_parameter(elias_handle handle, string bus_name, byte slot, byte effect_parameter_index, out elias_effect_parameter parameter, uint[] milliseconds_until_sweep_completes);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_stop")]
    public static extern elias_result_codes elias_stop(elias_handle handle);

    //Note: This function is experimental and its signature/functionality may change between versions of the plugin.
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "elias_read_samples")]
    public static extern elias_result_codes elias_read_samples(elias_handle handle, IntPtr output_buffer);
}