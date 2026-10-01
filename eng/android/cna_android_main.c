/* SPDX-License-Identifier: MS-PL */

/*
 * The native entry point of a CNA.NET game on Android, built as libmain.so.
 *
 * SDL's Java activity loads libSDL3.so and libmain.so and runs SDL_main on a thread of its own,
 * where the game must run: the window, the GL context and the event pump belong to that thread.
 * A .NET app has no native main, so the managed activity hands this library its entry point before
 * SDL starts, and SDL_main calls it. The game's own Main then runs, and blocks in Game.Run, exactly
 * as it does on a desktop.
 */

typedef int (*cna_android_main_function)(int argc, char** argv);

static cna_android_main_function managed_main;

__attribute__((visibility("default"))) void cna_android_set_main(cna_android_main_function function)
{
    managed_main = function;
}

__attribute__((visibility("default"))) int SDL_main(int argc, char** argv)
{
    /* No entry point means the activity never registered one; returning ends SDL's thread. */
    return managed_main != 0 ? managed_main(argc, argv) : 1;
}
