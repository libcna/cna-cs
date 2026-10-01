using System.Runtime.InteropServices;

namespace CNA.Interop;

/// <summary>
/// P/Invoke surface of net.h, net_gamers.h and net_sessions.h (packets, network gamers, sessions). Prototypes and ownership are documented in those headers; tools/abi-verify proves each declaration against them.
/// </summary>
internal static partial class Native
{
    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_quality_of_service_init(ref CnaQualityOfService outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_quality_of_service_init_measured(long roundtripTicks, ref CnaQualityOfService outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_net_get_last_join_error(out uint outJoinError, out byte outHasJoinError);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_create(out CnaHandle outProperties);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_get_count(CnaHandle properties, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_get_is_read_only(CnaHandle properties, out byte outIsReadOnly);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_get_item(CnaHandle properties, int index, ref CnaOptionalInt32 outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_set_item(CnaHandle properties, int index, CnaOptionalInt32 value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_index_of(CnaHandle properties, CnaOptionalInt32 value, out int outIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_insert(CnaHandle properties, int index, CnaOptionalInt32 value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_remove_at(CnaHandle properties, int index);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_add(CnaHandle properties, CnaOptionalInt32 value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_remove(CnaHandle properties, CnaOptionalInt32 value, out byte outRemoved);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_contains(CnaHandle properties, CnaOptionalInt32 value, out byte outContains);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_clear(CnaHandle properties);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_properties_copy_to(CnaHandle properties, CnaOptionalInt32* destination, ulong capacity, int index, out ulong outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_create_enumerator(CnaHandle properties, out CnaHandle outEnumerator);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_property_enumerator_move_next(CnaHandle enumerator, out byte outHasCurrent);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_property_enumerator_get_current(CnaHandle enumerator, ref CnaOptionalInt32 outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_property_enumerator_reset(CnaHandle enumerator);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_property_enumerator_destroy(CnaHandle enumerator);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_properties_destroy(CnaHandle properties);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_create(int capacity, out CnaHandle outWriter);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_get_length(CnaHandle writer, out int outLength);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_get_position(CnaHandle writer, out int outPosition);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_set_position(CnaHandle writer, int position);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_color(CnaHandle writer, CnaColor value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_matrix(CnaHandle writer, CnaMatrix value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_quaternion(CnaHandle writer, CnaQuaternion value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_vector2(CnaHandle writer, CnaVector2 value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_vector3(CnaHandle writer, CnaVector3 value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_vector4(CnaHandle writer, CnaVector4 value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_single(CnaHandle writer, float value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_write_double(CnaHandle writer, double value);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_packet_writer_copy_data_ext(CnaHandle writer, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_writer_destroy(CnaHandle writer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_create(int capacity, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_packet_reader_set_data_ext(CnaHandle reader, byte* data, ulong count);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_get_length(CnaHandle reader, out int outLength);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_get_position(CnaHandle reader, out int outPosition);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_set_position(CnaHandle reader, int position);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_color(CnaHandle reader, ref CnaColor outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_matrix(CnaHandle reader, ref CnaMatrix outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_quaternion(CnaHandle reader, ref CnaQuaternion outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_vector2(CnaHandle reader, ref CnaVector2 outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_vector3(CnaHandle reader, ref CnaVector3 outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_vector4(CnaHandle reader, ref CnaVector4 outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_single(CnaHandle reader, out float outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_read_double(CnaHandle reader, out double outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_packet_reader_destroy(CnaHandle reader);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_create(CnaHandle session, CnaStringView gamertag, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_has_left_session(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_set_has_left_session_ext(CnaHandle gamer, byte value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_has_voice(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_gamer_get_id(CnaHandle gamer, byte* outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_set_id_ext(CnaHandle gamer, byte value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_guest(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_host(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_set_is_host_ext(CnaHandle gamer, byte value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_local(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_muted_by_local_user(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_private_slot(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_ready(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_set_is_ready(CnaHandle gamer, byte value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_is_talking(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_copy_machine(CnaHandle gamer, out CnaHandle outMachine);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_set_machine(CnaHandle gamer, CnaHandle machine);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_roundtrip_ticks(CnaHandle gamer, out long outTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_set_roundtrip_ticks_ext(CnaHandle gamer, long ticks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_get_session(CnaHandle gamer, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_gamer_destroy(CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_machine_create(out CnaHandle outMachine);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_machine_get_gamer_count(CnaHandle machine, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_machine_get_gamer(CnaHandle machine, int index, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_machine_remove_from_session(CnaHandle machine);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_machine_destroy(CnaHandle machine);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_game_ended_event_info_init(ref CnaGameEndedEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_game_started_event_info_init(ref CnaGameStartedEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_joined_event_info_init(CnaHandle gamer, ref CnaGamerJoinedEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_left_event_info_init(CnaHandle gamer, ref CnaGamerLeftEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_host_changed_event_info_init(CnaHandle oldHost, CnaHandle newHost, ref CnaHostChangedEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_ended_event_info_init(uint endReason, ref CnaNetworkSessionEndedEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_write_leaderboards_event_info_init(CnaHandle gamer, byte isLeaving, ref CnaWriteLeaderboardsEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_available_network_session_create_ext(CnaAvailableNetworkSessionCreateInfo* createInfo, CnaQualityOfService* qualityOfService, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_current_gamer_count(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_host_gamertag_size(CnaHandle session, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_available_network_session_copy_host_gamertag(CnaHandle session, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_open_private_gamer_slots(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_open_public_gamer_slots(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_quality_of_service(CnaHandle session, ref CnaQualityOfService outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_copy_session_properties(CnaHandle session, out CnaHandle outProperties);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_equals(CnaHandle left, CnaHandle right, out byte outEqual);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_not_equals(CnaHandle left, CnaHandle right, out byte outNotEqual);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_connect_address_size_ext(CnaHandle session, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_available_network_session_copy_connect_address_ext(CnaHandle session, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_connect_port_ext(CnaHandle session, out ushort outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_get_session_type_ext(CnaHandle session, out uint outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_destroy(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_available_network_session_collection_create_ext(CnaHandle* sessions, ulong count, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_collection_get_count(CnaHandle collection, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_collection_copy_session(CnaHandle collection, int index, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_collection_get_is_disposed(CnaHandle collection, out byte outIsDisposed);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_collection_dispose(CnaHandle collection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_available_network_session_collection_destroy(CnaHandle collection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_create(uint sessionType, int maxLocalGamers, int maxGamers, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_create_with_properties(uint sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, CnaHandle sessionProperties, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_create_with_local_gamers(uint sessionType, CnaHandle* localGamers, ulong count, int maxGamers, int privateGamerSlots, CnaHandle sessionProperties, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_is_disposed(CnaHandle session, out byte outIsDisposed);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_gamer_count(CnaHandle session, uint roster, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_gamer(CnaHandle session, uint roster, int index, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_allow_host_migration(CnaHandle session, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_set_allow_host_migration(CnaHandle session, byte value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_allow_join_in_progress(CnaHandle session, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_set_allow_join_in_progress(CnaHandle session, byte value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_bytes_per_second_received(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_bytes_per_second_sent(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_host(CnaHandle session, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_is_everyone_ready(CnaHandle session, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_is_host(CnaHandle session, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_max_gamers(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_set_max_gamers(CnaHandle session, int value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_private_gamer_slots(CnaHandle session, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_set_private_gamer_slots(CnaHandle session, int value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_copy_session_properties(CnaHandle session, out CnaHandle outProperties);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_replace_session_properties(CnaHandle session, CnaHandle properties);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_session_state(CnaHandle session, out uint outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_session_type(CnaHandle session, out uint outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_simulated_latency_ticks(CnaHandle session, out long outTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_set_simulated_latency_ticks(CnaHandle session, long ticks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_simulated_packet_loss(CnaHandle session, out float outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_set_simulated_packet_loss(CnaHandle session, float value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_type_name_size(CnaHandle session, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_copy_type_name(CnaHandle session, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_update(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_add_local_gamer(CnaHandle session, CnaHandle signedInGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_find_gamer_by_id(CnaHandle session, byte gamerId, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_reset_ready(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_start_game(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_end_game(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_send_network_event_ext(CnaHandle session, CnaNetworkEventInfo* eventInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_add_remote_gamer_ext(CnaHandle session, CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_remove_gamer_ext(CnaHandle session, CnaHandle gamer, uint reason);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_owned_gamer_count_ext(CnaHandle session, out ulong outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_instance_count_ext(out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_get_active_action_count_ext(out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_dispose(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_destroy(CnaHandle session);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_create_async(uint sessionType, int maxLocalGamers, int maxGamers, nint callback, nint context, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_create_with_properties_async(uint sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, CnaHandle sessionProperties, nint callback, nint context, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_create_with_local_gamers_async(uint sessionType, CnaHandle* localGamers, ulong count, int maxGamers, int privateGamerSlots, CnaHandle sessionProperties, nint callback, nint context, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_find(uint sessionType, int maxLocalGamers, CnaHandle searchProperties, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_find_with_local_gamers(uint sessionType, CnaHandle* localGamers, ulong count, CnaHandle searchProperties, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_find_async(uint sessionType, int maxLocalGamers, CnaHandle searchProperties, nint callback, nint context, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_find_with_local_gamers_async(uint sessionType, CnaHandle* localGamers, ulong count, CnaHandle searchProperties, nint callback, nint context, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_join(CnaHandle availableSession, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_join_async(CnaHandle availableSession, nint callback, nint context, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_join_invited(int maxLocalGamers, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_join_invited_with_local_gamers(CnaHandle* localGamers, ulong count, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_join_invited_async(int maxLocalGamers, nint callback, nint context, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_network_session_join_invited_with_local_gamers_async(CnaHandle* localGamers, ulong count, nint callback, nint context, out CnaHandle outSession);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_create_ext(CnaHandle signedInGamer, CnaHandle session, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_get_is_data_available(CnaHandle gamer, out byte outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_get_signed_in_gamer(CnaHandle gamer, out CnaHandle outSignedInGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_enable_send_voice(CnaHandle gamer, CnaHandle remoteGamer, byte enable);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_send_party_invites(CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_receive_data(CnaHandle gamer, byte* destination, ulong capacity, out CnaHandle outSender, out ulong outReceived);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_receive_data_at(CnaHandle gamer, byte* destination, ulong capacity, int offset, out CnaHandle outSender, out ulong outReceived);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_receive_data_into_packet_reader(CnaHandle gamer, CnaHandle reader, out CnaHandle outSender, out ulong outReceived);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_send_data(CnaHandle gamer, byte* data, ulong count, uint options);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_send_data_range(CnaHandle gamer, byte* data, ulong count, int offset, int length, uint options);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_send_data_to(CnaHandle gamer, byte* data, ulong count, uint options, CnaHandle recipient);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_send_data_range_to(CnaHandle gamer, byte* data, ulong count, int offset, int length, uint options, CnaHandle recipient);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_send_packet_writer(CnaHandle gamer, CnaHandle writer, uint options);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_send_packet_writer_to(CnaHandle gamer, CnaHandle writer, uint options, CnaHandle recipient);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_local_network_gamer_clear_packet_queue_ext(CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_local_network_gamer_enqueue_packet_ext(CnaHandle gamer, CnaNetworkEventInfo* eventInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_game_started(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_game_ended(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_gamer_joined(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_gamer_left(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_host_changed(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_session_ended(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_write_arbitrated_leaderboard(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_write_unarbitrated_leaderboard(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_write_true_skill(CnaHandle session, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_subscribe_invite_accepted(nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_network_session_unsubscribe(CnaHandle registration);
}
