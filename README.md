# Bobbys Mods 0.6.0 beta

PC VR Gorilla Tag neon menu with per-lobby saved selections. This is a development build; headset/multiplayer validation is incomplete.

## Features and controls

Press left X to open/close the menu. Touch a row using the single cyan right-controller pointer. Next Page/Previous Page switches pages; Disable All disables and saves all features off for that lobby.

### Page one

- Freeze Keybind: arms/disarms right B. B freezes/restores the outgoing pose. A local baked main-skin snapshot remains behind while your live avatar is tinted translucent green.
- Grip Platforms: hold either grip for that hand's floating platform.
- Trigger Platforms: hold either trigger. Teleport takes priority when using the same input.
- Teleport: right-hand aim line stays visible while enabled; right trigger teleports to an open flat target.
- Smart Arms: 1.5x estimated shoulder-to-controller reach with normal player size.

### Page two

- Low Gravity: compensates half of normal Rigidbody gravity on the local player by default. It does not change global Physics.gravity. This may interact with the game's other movement modes and needs testing.
- Checkpoint Keybinds: enable, press right A to save current head position, then left Y to return if the destination is clear and has ground. The checkpoint is cleared on scene/lobby changes or disabling the feature. Only permission to use it is saved; coordinates are not persisted.
- FPS Display: shows an average FPS sample in the menu footer, refreshed twice per second. This is not a GPU profiler.
- Reload Config: reloads settings and disables/saves the current lobby features off.
- Build / Test Info: shows the development build status.

Gameplay features must be enabled in the menu before their keybinds operate. Saved selections restore separately for each room name and game mode. A new lobby starts off; restoring Freeze only arms B.

## Install

Requires PC VR Gorilla Tag with BepInEx 5. Put one BobbysMods.dll in Gorilla Tag/BepInEx/plugins. Start the game once with config.txt in BepInEx/plugins/BobbysMods/config.txt.

On this PC, the current DLL and active configuration are installed under:
C:/Program Files (x86)/Steam/steamapps/common/Gorilla Tag/BepInEx/plugins

Close the game before replacing the DLL. Remove the historical BobbysWorldScale.dll add-on to avoid running two arm extensions. Utilla is not required by the current build.

## Configuration

Edit the active config.txt, not the project template, then use Reload Config on page two. Per-lobby selections are stored separately in lobby-settings.txt. Its encoded room keys are not anonymous; do not publish that file.

work_in_all_lobbies=true permits features in any joined room (existing default).
work_in_all_lobbies=false permits only private MODDED rooms.
No setting guarantees avoiding a ban. The mod does not bypass game protections.

New settings:

    platform_opacity=0.25
    gravity_multiplier=0.5
    checkpoint_save_button=right_a
    checkpoint_return_button=left_y

Existing settings control menu colours, platform dimensions, teleport limits, input bindings, ghost_colour, ghost_opacity and pointer_colour. Low gravity is clamped to 10%-100% of normal gravity. Platform opacity is clamped to 5%-75%. Use distinct inputs for unrelated actions.

## Bug fixes in 0.5.0

The frozen snapshot now bakes only VRRig.mainSkin, instead of every skinned helper mesh. Vertices are transformed once into an unscaled world-aligned root. Oversized/invalid snapshots are suppressed rather than displayed. This addresses likely causes of the reported giant skeleton; actual appearance still needs headset verification.

Platforms use a translucent unlit material, cast no shadows, and are placed below the hand follower with collision-radius clearance. This is intended to keep your hands visible; in-headset confirmation is still pending.

The menu has two pages instead of an increasingly tall panel. It changes visibility only on transitions and does not rebuild the canvas every frame.

## Build and tests

Install a .NET SDK and use your own legally installed Gorilla Tag/BepInEx files. No game assemblies are included.

    ./Build.ps1 -GamePath 'YOUR GORILLA TAG FOLDER'

    dotnet run --project tests/FreezeTests.csproj --configuration Release

If this working folder has a local .dotnet SDK, Build.ps1 uses it. Otherwise it uses the installed dotnet command. The build does not automatically update the installed game DLL.

Validation: zero build warnings/errors and 71 automated logic checks. See TESTING.md for coverage and remaining headset checks. Passing these checks does not establish release stability or that every feature works in VR.

## Credits and research

Low Gravity and Checkpoint were selected after reviewing KyleTheScientist/Bark's documented feature descriptions. These are fresh implementations for Bobbys Mods; no Bark code, assets or DLLs are included. See RESEARCH.md. The FPS display is an original local diagnostic addition.

## Repository contents

This repository contains the mod source with tests, config template and documentation. It excludes game DLLs, third-party binary dependencies, local SDKs, backups and saved room profiles. Select a project license and finish the headset checklist before publishing a stable release.

## Troubleshooting and uninstall

Logs: Gorilla Tag/BepInEx/LogOutput.log and your Windows AppData/LocalLow/Another Axiom/Gorilla Tag/Player.log. Report the plugin version, exact action and error text.

To uninstall, close the game and remove BepInEx/plugins/BobbysMods.dll. Optionally remove its configuration folder. Existing game files and BepInEx remain installed.

## Search bar and VR keyboard — 0.6.0

1. Press left X to open the menu.
2. Touch Search mods... at the top with the right-hand cyan pointer.
3. A QWERTY touch keyboard appears in front of you. The menu stays above and slightly beyond it as the search-results panel.
4. Touch letters to type. SPACE adds a space, BACK removes one letter, and CLEAR removes the query. Text is limited to 32 characters.
5. Matching installed mods appear above the keyboard. Touch a result to enable/disable that feature; its enabled colour matches the regular menu. Freeze still arms B instead of instantly freezing.
6. Use More results if more than five matches are available.
7. Touch X CLOSE on the keyboard when done. This closes both the keyboard and menu. Left X also closes the view; pressing it again returns to the normal hand menu.

Search supports partial words and aliases such as ghost (Freeze), tp (Teleport), float (Low Gravity), and long (Smart Arms). All words in the query must match. With no query, all eight installed features are listed. No matching mods is shown for unknown terms.

The search list activates the same existing mod handlers and saves selections to the same per-lobby profiles. All lobby restrictions still apply. A disabled/locked feature cannot be enabled through search. Search itself works while the gameplay controls are locked.

The search view is placed once in front of you; it does not chase your head while typing. Move back to it or close/reopen Search to reposition it. The panels and pointer are local UI, have no colliders and send no network messages. The keyboard is created once and reused.

See GHOST-IDEAS.md for sourced future ghost-themed ideas and their local/multiplayer visibility limits. These ideas have not been silently added to the installed feature list.

Validation: clean build; 71 automated checks including query matching, aliases, no results, length limits, editing, key hit-testing and Close handling. Actual headset keyboard reach, panel placement, pointer visibility and result activation still need VR testing.
