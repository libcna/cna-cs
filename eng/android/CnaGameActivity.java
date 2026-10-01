// SPDX-License-Identifier: MS-PL
package com.libcna.cna;

/**
 * The activity of a CNA.NET game: SDL's own, unchanged. SDL loads libSDL3 and libmain and runs
 * SDL_main on a thread of its own; libmain (cna_android_main.c) hands that call to the entry point
 * the app's CnaGameApplication registered when the process started.
 */
public class CnaGameActivity extends org.libsdl.app.SDLActivity {
}
