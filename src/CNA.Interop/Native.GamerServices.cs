using System.Runtime.InteropServices;

namespace CNA.Interop;

/// <summary>
/// P/Invoke surface of gamer_services.h (Guide, gamers, profiles, achievements, leaderboards, avatars). Prototypes and ownership are documented in that header; tools/abi-verify proves each declaration against it.
/// </summary>
internal static partial class Native
{
    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_create_ext(CnaStringView gamertag, byte isSignedInToLive, byte isGuest, uint playerIndex, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_gamertag_size(CnaHandle gamer, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_signed_in_gamer_copy_gamertag(CnaHandle gamer, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_invite_accepted_event_info_init(CnaHandle gamer, byte isCurrentSession, ref CnaInviteAcceptedEventInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_destroy(CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_set_signed_in_gamers_ext(CnaHandle* gamers, ulong count);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_signed_in_gamer_count(out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_presence_init(ref CnaGamerPresence outPresence);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_destroy(CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_display_name_size(CnaHandle gamer, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_copy_display_name(CnaHandle gamer, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_set_display_name(CnaHandle gamer, CnaStringView displayName);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_gamertag_size(CnaHandle gamer, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_copy_gamertag(CnaHandle gamer, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_text_size(CnaHandle gamer, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_copy_text(CnaHandle gamer, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_is_disposed(CnaHandle gamer, out byte outIsDisposed);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_tag(CnaHandle gamer, out ulong outTag);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_set_tag(CnaHandle gamer, ulong tag);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_profile(CnaHandle gamer, out CnaHandle outProfile);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_begin_get_profile(CnaHandle gamer, nint callback, nint context, out CnaHandle outProfile);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_from_gamertag(CnaStringView gamertag, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_begin_get_from_gamertag(CnaStringView gamertag, nint callback, nint context, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_partner_token_size(CnaStringView audienceUri, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_copy_partner_token(CnaStringView audienceUri, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_begin_get_partner_token(CnaStringView audienceUri, nint callback, nint context, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_signed_in_gamer_at(int index, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_signed_in_index_of(CnaHandle gamer, out int outIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_signed_in_contains(CnaHandle gamer, out byte outContains);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_get_signed_in_gamer_at_player_index(uint playerIndex, out byte outHasGamer, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_is_guest(CnaHandle gamer, out byte outIsGuest);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_is_signed_in_to_live(CnaHandle gamer, out byte outIsSignedInToLive);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_party_size(CnaHandle gamer, out int outPartySize);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_set_party_size(CnaHandle gamer, int partySize);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_player_index(CnaHandle gamer, out uint outPlayerIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_presence(CnaHandle gamer, ref CnaGamerPresence outPresence);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_signed_in_gamer_set_presence(CnaHandle gamer, CnaGamerPresence* presence);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_set_presence_mode_string_ext(CnaHandle gamer, CnaStringView mode);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_privileges(CnaHandle gamer, ref CnaGamerPrivileges outPrivileges);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_is_friend(CnaHandle gamer, CnaHandle other, out byte outIsFriend);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_is_headset(CnaHandle gamer, ulong microphoneIndex, out byte outIsHeadset);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_friends(CnaHandle gamer, out CnaHandle outFriends);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_award_achievement(CnaHandle gamer, CnaStringView achievementKey);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_begin_award_achievement(CnaHandle gamer, CnaStringView achievementKey, nint callback, nint context);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_subscribe_signed_in_ext(nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_subscribe_signed_out_ext(nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_unsubscribe_ext(CnaHandle registration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_profile_get_info(CnaHandle profile, ref CnaGamerProfileInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_profile_get_motto_size(CnaHandle profile, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_profile_copy_motto(CnaHandle profile, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_profile_get_region_name_size(CnaHandle profile, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_profile_copy_region_name(CnaHandle profile, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_profile_get_picture_size(CnaHandle profile, out byte outHasPicture, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_profile_copy_picture(CnaHandle profile, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_profile_destroy(CnaHandle profile);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_friend_gamer_get_info(CnaHandle gamer, ref CnaFriendGamerInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_friend_gamer_get_presence_size(CnaHandle gamer, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_friend_gamer_copy_presence(CnaHandle gamer, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_get_count(CnaHandle collection, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_get_at(CnaHandle collection, int index, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_index_of(CnaHandle collection, CnaHandle gamer, out int outIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_contains(CnaHandle collection, CnaHandle gamer, out byte outContains);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_gamer_collection_copy_to(CnaHandle collection, CnaHandle* destination, ulong capacity, int index, out ulong outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_add(CnaHandle collection, CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_remove(CnaHandle collection, CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_clear(CnaHandle collection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_create_enumerator(CnaHandle collection, out CnaHandle outEnumerator);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_enumerator_move_next(CnaHandle enumerator, out byte outHasCurrent);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_enumerator_get_current(CnaHandle enumerator, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_enumerator_reset(CnaHandle enumerator);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_enumerator_destroy(CnaHandle enumerator);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_friend_collection_get_is_disposed(CnaHandle collection, out byte outIsDisposed);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_friend_gamer_create_ext(CnaStringView gamertag, CnaStringView displayName, byte isOnline, byte isPlaying, byte isAway, byte isBusy, byte friendRequestSentTo, byte friendRequestReceivedFrom, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_friend_collection_create_ext(CnaHandle* friends, ulong count, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_collection_destroy(CnaHandle collection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_is_screen_saver_enabled(out byte outIsEnabled);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_set_is_screen_saver_enabled(byte isEnabled);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_is_trial_mode(out byte outIsTrialMode);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_is_visible(out byte outIsVisible);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_notification_position(out uint outPosition);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_set_notification_position(uint position);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_simulate_trial_mode(out byte outSimulate);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_set_simulate_trial_mode(byte simulate);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_begin_show_keyboard_input(uint player, CnaStringView title, CnaStringView description, CnaStringView defaultText, byte usePasswordMode, nint callback, nint context);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_end_show_keyboard_input_size(out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_end_show_keyboard_input(byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_has_pending_keyboard_input_ext(out byte outHasPending);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_was_keyboard_input_canceled_ext(out byte outWasCanceled);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_pending_keyboard_input_title_size_ext(out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_copy_pending_keyboard_input_title_ext(byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_pending_keyboard_input_description_size_ext(out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_copy_pending_keyboard_input_description_ext(byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_pending_keyboard_input_display_text_size_ext(out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_copy_pending_keyboard_input_display_text_ext(byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_render_pending_keyboard_input_ext(CnaHandle device, CnaHandle spriteBatch, CnaHandle font, CnaHandle whitePixel);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_simulate_keyboard_input_cancel_ext();

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_reset_pending_keyboard_input_ext();

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_begin_show_message_box(uint player, CnaStringView title, CnaStringView text, CnaStringView* buttons, ulong buttonCount, int focusButton, uint icon, nint callback, nint context);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_end_show_message_box(out byte outHasChoice, out int outButtonIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_has_pending_message_box_ext(out byte outHasPending);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_get_pending_message_box_focus_button_ext(out int outFocusButton);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_render_pending_message_box_ext(CnaHandle device, CnaHandle spriteBatch, CnaHandle font, CnaHandle whitePixel);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_simulate_message_box_click_ext(int buttonIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_reset_pending_message_box_ext();

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_delay_notifications(long delayTicks);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_show_compose_message(uint player, CnaStringView text, CnaHandle* recipients, ulong recipientCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_friend_request(uint player, CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_friends(uint player);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_guide_show_game_invite(uint player, CnaHandle* recipients, ulong recipientCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_game_invite_for_session(CnaStringView sessionId);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_gamer_card(uint player, CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_marketplace(uint player);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_messages(uint player);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_party(uint player);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_party_sessions(uint player);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_player_review(uint player, CnaHandle gamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_players(uint player);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_sign_in(int paneCount, byte onlineOnly);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_guide_show_achievements_ext(uint player);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_get_is_initialized(out byte outIsInitialized);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_get_window_handle(out ulong outWindowHandle);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_set_window_handle(ulong windowHandle);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_initialize(CnaHandle game);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_update();

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_update_async(out byte outDidWork);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_get_freed_gamer_count_ext(out ulong outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_dispatcher_subscribe_installing_title_update_ext(nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_gamer_services_component_create(CnaHandle game, out CnaHandle outComponent);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_create_ext(CnaStringView key, CnaStringView name, CnaStringView description, byte displayBeforeEarned, byte isEarned, long earnedDateTimeTicks, out CnaHandle outAchievement);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_destroy(CnaHandle achievement);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_get_info(CnaHandle achievement, ref CnaAchievementInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_get_key_size(CnaHandle achievement, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_copy_key(CnaHandle achievement, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_get_name_size(CnaHandle achievement, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_copy_name(CnaHandle achievement, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_get_description_size(CnaHandle achievement, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_copy_description(CnaHandle achievement, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_get_how_to_earn_size(CnaHandle achievement, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_copy_how_to_earn(CnaHandle achievement, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_get_picture_size(CnaHandle achievement, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_copy_picture(CnaHandle achievement, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_equals(CnaHandle achievement, CnaHandle other, out byte outEquals);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_collection_create_ext(CnaHandle* achievements, ulong count, out CnaHandle outCollection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_destroy(CnaHandle collection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_get_count(CnaHandle collection, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_get_is_disposed(CnaHandle collection, out byte outIsDisposed);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_get_is_read_only(CnaHandle collection, out byte outIsReadOnly);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_get_at(CnaHandle collection, int index, out CnaHandle outAchievement);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_get_by_key(CnaHandle collection, CnaStringView key, out CnaHandle outAchievement);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_index_of(CnaHandle collection, CnaHandle achievement, out int outIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_contains(CnaHandle collection, CnaHandle achievement, out byte outContains);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_add(CnaHandle collection, CnaHandle achievement);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_insert(CnaHandle collection, int index, CnaHandle achievement);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_remove_at(CnaHandle collection, int index);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_remove(CnaHandle collection, CnaHandle achievement, out byte outRemoved);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_achievement_collection_clear(CnaHandle collection);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_achievement_collection_copy_to(CnaHandle collection, CnaHandle* destination, ulong capacity, int index, out ulong outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_achievements(CnaHandle gamer, out CnaHandle outAchievements);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_begin_get_achievements(CnaHandle gamer, nint callback, nint context, out CnaHandle outAchievements);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_game_defaults_init(ref CnaGameDefaults outDefaults);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_signed_in_gamer_get_game_defaults(CnaHandle gamer, ref CnaGameDefaults outDefaults);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_create_ext(out CnaHandle outDictionary);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_destroy(CnaHandle dictionary);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_count(CnaHandle dictionary, out int outCount);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_is_read_only(CnaHandle dictionary, out byte outIsReadOnly);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_contains_key(CnaHandle dictionary, CnaStringView key, out byte outContains);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_try_get_value_kind_ext(CnaHandle dictionary, CnaStringView key, out byte outFound, out uint outKind);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_date_time_ticks(CnaHandle dictionary, CnaStringView key, out long outTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_double(CnaHandle dictionary, CnaStringView key, out double outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_int32(CnaHandle dictionary, CnaStringView key, out int outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_int64(CnaHandle dictionary, CnaStringView key, out long outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_outcome(CnaHandle dictionary, CnaStringView key, out uint outOutcome);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_single(CnaHandle dictionary, CnaStringView key, out float outValue);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_stream_size_ext(CnaHandle dictionary, CnaStringView key, out byte outHasStream, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_string_size(CnaHandle dictionary, CnaStringView key, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_property_dictionary_copy_string(CnaHandle dictionary, CnaStringView key, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_time_span_ticks(CnaHandle dictionary, CnaStringView key, out long outTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_date_time_ticks(CnaHandle dictionary, CnaStringView key, long ticks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_double(CnaHandle dictionary, CnaStringView key, double value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_int32(CnaHandle dictionary, CnaStringView key, int value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_int64(CnaHandle dictionary, CnaStringView key, long value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_outcome(CnaHandle dictionary, CnaStringView key, uint outcome);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_single(CnaHandle dictionary, CnaStringView key, float value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_string(CnaHandle dictionary, CnaStringView key, CnaStringView value);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_set_time_span_ticks(CnaHandle dictionary, CnaStringView key, long ticks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_remove(CnaHandle dictionary, CnaStringView key, out byte outRemoved);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_clear(CnaHandle dictionary);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_property_dictionary_get_key_size_at(CnaHandle dictionary, int index, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_property_dictionary_copy_key_at(CnaHandle dictionary, int index, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_identity_init(uint key, int gameMode, ref CnaLeaderboardIdentity outIdentity);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_leaderboard_reader_read(CnaLeaderboardIdentity* identity, int pageStart, int pageSize, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_leaderboard_reader_read_from_pivot(CnaLeaderboardIdentity* identity, CnaHandle pivotGamer, int pageSize, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_leaderboard_reader_read_from_gamers(CnaLeaderboardIdentity* identity, CnaHandle* gamers, ulong gamerCount, CnaHandle pivotGamer, int pageSize, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_leaderboard_reader_begin_read(CnaLeaderboardIdentity* identity, int pageStart, int pageSize, nint callback, nint context, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_leaderboard_reader_begin_read_from_pivot(CnaLeaderboardIdentity* identity, CnaHandle pivotGamer, int pageSize, nint callback, nint context, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_leaderboard_reader_begin_read_from_gamers(CnaLeaderboardIdentity* identity, CnaHandle* gamers, ulong gamerCount, CnaHandle pivotGamer, int pageSize, nint callback, nint context, out CnaHandle outReader);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_destroy(CnaHandle reader);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_get_info(CnaHandle reader, ref CnaLeaderboardReaderInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_get_identity(CnaHandle reader, ref CnaLeaderboardIdentity outIdentity);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_get_entry_at(CnaHandle reader, int index, out CnaHandle outEntry);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_page_down(CnaHandle reader);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_page_up(CnaHandle reader);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_begin_page_down(CnaHandle reader, nint callback, nint context);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_reader_begin_page_up(CnaHandle reader, nint callback, nint context);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_create_ext(CnaHandle gamer, long rating, int ranking, out CnaHandle outEntry);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_destroy(CnaHandle entry);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_get_info(CnaHandle entry, ref CnaLeaderboardEntryInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_set_rating(CnaHandle entry, long rating);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_get_gamer(CnaHandle entry, out byte outHasGamer, out CnaHandle outGamer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_get_columns(CnaHandle entry, out CnaHandle outColumns);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_set_rating_changed_hook_ext(CnaHandle entry, nint callback, nint context);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_leaderboard_entry_equals(CnaHandle entry, CnaHandle other, out byte outEquals);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_expression_init(ref CnaAvatarExpression outExpression);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_avatar_description_create(byte* description, ulong byteCount, out CnaHandle outDescription);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_description_create_random(out CnaHandle outDescription);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_description_create_random_for_body_type(uint bodyType, out CnaHandle outDescription);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_description_get_from_gamer(CnaHandle gamer, nint callback, nint context, out CnaHandle outDescription);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_description_destroy(CnaHandle description);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_description_get_info(CnaHandle description, ref CnaAvatarDescriptionInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_avatar_description_copy_description(CnaHandle description, byte* destination, ulong capacity, out ulong outBytes);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_description_subscribe_changed_ext(CnaHandle description, nint callback, nint context, out CnaHandle outRegistration);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_create(uint preset, out CnaHandle outAnimation);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_destroy(CnaHandle animation);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_get_info(CnaHandle animation, ref CnaAvatarAnimationInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_set_current_position(CnaHandle animation, long positionTicks);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_get_expression(CnaHandle animation, ref CnaAvatarExpression outExpression);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_update(CnaHandle animation, long elapsedTicks, byte loop);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_animation_get_bone_transform_at(CnaHandle animation, int index, ref CnaMatrix outTransform);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_create(CnaHandle description, byte useLoadingEffect, out CnaHandle outRenderer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_destroy(CnaHandle renderer);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_get_info(CnaHandle renderer, ref CnaAvatarRendererInfo outInfo);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_get_transforms(CnaHandle renderer, ref CnaMatrix outWorld, ref CnaMatrix outView, ref CnaMatrix outProjection);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_avatar_renderer_set_transforms(CnaHandle renderer, CnaMatrix* world, CnaMatrix* view, CnaMatrix* projection);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_get_lighting(CnaHandle renderer, ref CnaVector3 outLightColor, ref CnaVector3 outLightDirection, ref CnaVector3 outAmbientLightColor);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_avatar_renderer_set_lighting(CnaHandle renderer, CnaVector3* lightColor, CnaVector3* lightDirection, CnaVector3* ambientLightColor);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_get_parent_bone_at(CnaHandle renderer, int index, out int outParentIndex);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_get_bind_pose_at(CnaHandle renderer, int index, ref CnaMatrix outTransform);

    [LibraryImport(LibraryName)]
    internal static partial CnaResult cna_avatar_renderer_draw_animation(CnaHandle renderer, CnaHandle animation);

    [LibraryImport(LibraryName)]
    internal static unsafe partial CnaResult cna_avatar_renderer_draw_bones(CnaHandle renderer, CnaMatrix* bones, ulong boneCount, CnaAvatarExpression* expression);
}
