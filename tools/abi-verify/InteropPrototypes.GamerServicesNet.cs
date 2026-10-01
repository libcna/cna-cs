// SPDX-License-Identifier: MIT

static partial class InteropPrototypes
{
    /// <summary>
    /// The <see cref="ParameterOverrides"/> of gamer_services.h, net.h, net_gamers.h and
    /// net_sessions.h, kept apart because there are ninety-eight of them. Same rules: each entry is
    /// one of the four representational differences documented there, and each was produced by the
    /// compiler rejecting the managed-derived spelling, not written by hand.
    /// </summary>
    public static readonly Dictionary<string, string> GamerServicesNetParameterOverrides = new(StringComparer.Ordinal)
    {
        // devices.h/sensors.h, for CNA.PhoneCompat's accelerometer: the two reading callbacks, whose
        // shapes InteropCallbacks checks against the managed handlers.
        ["cna_accelerometer_subscribe_current_value_changed#1"] = "void (*)(const CNA_AccelerometerReading*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_accelerometer_subscribe_reading_changed#1"] = "void (*)(const CNA_AccelerometerReadingEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate

        ["cna_achievement_collection_create_ext#0"] = "const CNA_AchievementHandle*",   // const, which C# cannot put on a pointer
        ["cna_achievement_copy_description#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_achievement_copy_how_to_earn#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_achievement_copy_key#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_achievement_copy_name#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_available_network_session_collection_create_ext#0"] = "const CNA_AvailableNetworkSessionHandle*",   // const, which C# cannot put on a pointer
        ["cna_available_network_session_copy_connect_address_ext#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_available_network_session_copy_host_gamertag#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_available_network_session_create_ext#0"] = "const CNA_AvailableNetworkSessionCreateInfo*",   // const, which C# cannot put on a pointer
        ["cna_available_network_session_create_ext#1"] = "const CNA_QualityOfService*",   // const, which C# cannot put on a pointer
        ["cna_avatar_description_create#0"] = "const uint8_t*",   // const, which C# cannot put on a pointer
        ["cna_avatar_description_get_from_gamer#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_avatar_description_subscribe_changed_ext#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_avatar_renderer_draw_bones#1"] = "const CNA_Matrix*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_draw_bones#3"] = "const CNA_AvatarExpression*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_set_lighting#1"] = "const CNA_Vector3*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_set_lighting#2"] = "const CNA_Vector3*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_set_lighting#3"] = "const CNA_Vector3*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_set_transforms#1"] = "const CNA_Matrix*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_set_transforms#2"] = "const CNA_Matrix*",   // const, which C# cannot put on a pointer
        ["cna_avatar_renderer_set_transforms#3"] = "const CNA_Matrix*",   // const, which C# cannot put on a pointer
        ["cna_friend_collection_create_ext#0"] = "const CNA_GamerHandle*",   // const, which C# cannot put on a pointer
        ["cna_friend_gamer_copy_presence#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_begin_get_from_gamertag#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_gamer_begin_get_partner_token#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_gamer_begin_get_partner_token#3"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_begin_get_profile#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_gamer_copy_display_name#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_copy_gamertag#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_copy_partner_token#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_copy_text#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_profile_copy_motto#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_profile_copy_region_name#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_gamer_services_dispatcher_subscribe_installing_title_update_ext#0"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_gamer_set_signed_in_gamers_ext#0"] = "const CNA_SignedInGamerHandle*",   // const, which C# cannot put on a pointer
        ["cna_guide_begin_show_keyboard_input#5"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_guide_begin_show_message_box#3"] = "const CNA_StringView*",   // const, which C# cannot put on a pointer
        ["cna_guide_begin_show_message_box#7"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_guide_copy_pending_keyboard_input_description_ext#0"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_guide_copy_pending_keyboard_input_display_text_ext#0"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_guide_copy_pending_keyboard_input_title_ext#0"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_guide_end_show_keyboard_input#0"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_guide_show_compose_message#2"] = "const CNA_GamerHandle*",   // const, which C# cannot put on a pointer
        ["cna_guide_show_game_invite#1"] = "const CNA_GamerHandle*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_entry_set_rating_changed_hook_ext#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_leaderboard_reader_begin_page_down#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_leaderboard_reader_begin_page_up#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_leaderboard_reader_begin_read#0"] = "const CNA_LeaderboardIdentity*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_begin_read#3"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_leaderboard_reader_begin_read_from_gamers#0"] = "const CNA_LeaderboardIdentity*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_begin_read_from_gamers#1"] = "const CNA_GamerHandle*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_begin_read_from_gamers#5"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_leaderboard_reader_begin_read_from_pivot#0"] = "const CNA_LeaderboardIdentity*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_begin_read_from_pivot#3"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_leaderboard_reader_read#0"] = "const CNA_LeaderboardIdentity*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_read_from_gamers#0"] = "const CNA_LeaderboardIdentity*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_read_from_gamers#1"] = "const CNA_GamerHandle*",   // const, which C# cannot put on a pointer
        ["cna_leaderboard_reader_read_from_pivot#0"] = "const CNA_LeaderboardIdentity*",   // const, which C# cannot put on a pointer
        ["cna_local_network_gamer_enqueue_packet_ext#1"] = "const CNA_NetworkEventInfo*",   // const, which C# cannot put on a pointer
        ["cna_local_network_gamer_send_data#1"] = "const uint8_t*",   // const, which C# cannot put on a pointer
        ["cna_local_network_gamer_send_data_range#1"] = "const uint8_t*",   // const, which C# cannot put on a pointer
        ["cna_local_network_gamer_send_data_range_to#1"] = "const uint8_t*",   // const, which C# cannot put on a pointer
        ["cna_local_network_gamer_send_data_to#1"] = "const uint8_t*",   // const, which C# cannot put on a pointer
        ["cna_network_session_copy_type_name#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_network_session_create_async#3"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_create_with_local_gamers#1"] = "const CNA_Handle*",   // const, which C# cannot put on a pointer
        ["cna_network_session_create_with_local_gamers_async#1"] = "const CNA_Handle*",   // const, which C# cannot put on a pointer
        ["cna_network_session_create_with_local_gamers_async#6"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_create_with_properties_async#5"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_find_async#3"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_find_with_local_gamers#1"] = "const CNA_Handle*",   // const, which C# cannot put on a pointer
        ["cna_network_session_find_with_local_gamers_async#1"] = "const CNA_Handle*",   // const, which C# cannot put on a pointer
        ["cna_network_session_find_with_local_gamers_async#4"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_join_async#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_join_invited_async#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_join_invited_with_local_gamers#0"] = "const CNA_Handle*",   // const, which C# cannot put on a pointer
        ["cna_network_session_join_invited_with_local_gamers_async#0"] = "const CNA_Handle*",   // const, which C# cannot put on a pointer
        ["cna_network_session_join_invited_with_local_gamers_async#2"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_send_network_event_ext#1"] = "const CNA_NetworkEventInfo*",   // const, which C# cannot put on a pointer
        ["cna_network_session_subscribe_game_ended#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_GameEndedEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_game_started#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_GameStartedEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_gamer_joined#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_GamerJoinedEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_gamer_left#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_GamerLeftEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_host_changed#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_HostChangedEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_invite_accepted#0"] = "void (*)(const CNA_InviteAcceptedEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_session_ended#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_NetworkSessionEndedEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_write_arbitrated_leaderboard#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_WriteLeaderboardsEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_write_true_skill#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_WriteLeaderboardsEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_network_session_subscribe_write_unarbitrated_leaderboard#1"] = "void (*)(CNA_NetworkSessionHandle, const CNA_WriteLeaderboardsEventInfo*, void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_packet_reader_set_data_ext#1"] = "const uint8_t*",   // const, which C# cannot put on a pointer
        ["cna_property_dictionary_copy_key_at#2"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_property_dictionary_copy_string#2"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_signed_in_gamer_begin_award_achievement#2"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_signed_in_gamer_begin_get_achievements#1"] = "void (*)(void*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_signed_in_gamer_copy_gamertag#1"] = "char*",   // text buffer: C char, which C# has no type for
        ["cna_signed_in_gamer_set_presence#1"] = "const CNA_GamerPresence*",   // const, which C# cannot put on a pointer
        ["cna_signed_in_gamer_subscribe_signed_in_ext#0"] = "void (*)(void*, const CNA_SignedInGamerEventInfo*)",   // callback, declared nint; its shape is checked from the delegate
        ["cna_signed_in_gamer_subscribe_signed_out_ext#0"] = "void (*)(void*, const CNA_SignedInGamerEventInfo*)",   // callback, declared nint; its shape is checked from the delegate
    };
}
