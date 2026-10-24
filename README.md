# Bobbys Mods 0.7.0 beta

Windows Steam PC VR Gorilla Tag menu. Requires BepInEx 5 and your own installed game. Standalone Quest is not supported. No Utilla dependency.

## Download and install
Download [Bobbys Mods 0.7.0](https://github.com/njspy123-stack/Bobbys-Mods/releases/tag/v0.7.0): choose BobbysMods.dll, or BobbysMods-0.7.0-beta.zip for the installation folders. The automatic Source code ZIP is for developers.

Close Gorilla Tag. Put BobbysMods.dll directly in BepInEx/plugins. On first launch, the DLL automatically creates BepInEx/plugins/Bobbys Mods/config.txt from its built-in template. Existing settings and lobby selections from the old BobbysMods folder are copied when the new config is absent. Preserve your existing config when updating and add new keys from the template. Keep one DLL only; remove the historical BobbysWorldScale.dll add-on. Start through Steam in PC VR and press left X. Never double-click the DLL.

The public 0.7.0 beta includes the categorized menu and current fixes. Both Noclip buttons are marked (matinence) while repairs continue.

## Menu
Home has Movement, Platforms, Avatar, Displays and Settings categories. Touch with the right pointer. Movement has two pages; other categories fit one page. Use the separate < Previous and Next > arrow buttons, which wrap within the current category. Categories returns home. Search covers all 18 feature toggles across categories. Both hands can type and select search results. X CLOSE closes search and the menu. The neon menu has raised 3D bezels without physics colliders.

## Features

| Category | Features and controls |
| --- | --- |
| Movement | Teleport (right trigger and constant aim), Low Gravity, Grip Noclip, Noclip Fly, Fly, Speed Boost, High Jump, Zero Gravity, Iron Man, Slide Control |
| Platforms | Grip Platforms; Trigger Platforms. Teleport wins a shared trigger binding. |
| Avatar | Freeze Keybind (arm then right B freezes/restores); Smart Arms; No Finger Movement |
| Displays | FPS Display; HUD Status; Session Timer |
| Settings | Reload Config; Disable All; Build / Test Info |

Enable features in the menu before using their bindings. Freeze is only the original manual keybind. Menu Ghost, Pulse Ghost, Hold-to-Ghost, Spectral Trails, Orbiting Wisps and Checkpoint have been removed.

### Noclip
Grip Noclip suspends the local movement collision mask and enabled non-trigger colliders under your player while noclip_button (default right grip) is held. It makes no movement/teleport requests while active. noclip_hover=true cancels vertical velocity and disables gravity while held to prevent falling through the floor; false preserves gravity. Releasing, opening the menu, disabling or leaving restores the original collision and gravity state. If released inside geometry, the player returns to the last clear position. Colliders belonging to the map and other players are not changed.

Noclip Fly uses the same grip, with the left thumbstick controlling forward/back/strafe relative to the right controller. Left trigger adds up, left grip adds down. Neutral input stops, acceleration is smoothed. noclip_fly_auto_forward=false is the default. Disable overlapping grip platforms/Iron Man or customize the bindings. Noclip Fly takes priority over Grip Noclip when both are enabled.

### New movement features
Fly: hold fly_button (right A), use left thumbstick for headset-directed movement, left trigger up and left grip down. Keeps normal collisions. Speed Boost adjusts the player's jump multiplier, High Jump adjusts maximum jump speed, and Slide Control adjusts steering on slippery surfaces. Original values restore on disable/lobby change. Zero Gravity counteracts local Rigidbody gravity; global gravity is unchanged. Iron Man adds thrust opposite each controller's up axis while its grip is held. These features interact with existing game movement and require in-game testing.

### No Finger Movement
Captures finger-bone rotations and the six packed finger-bend digits on the local avatar. Held-item flag digits remain live; controller tracking and grip/trigger gameplay inputs remain active. Disable to resume animation. This does not freeze head, body or hand positions. Remote appearance is unverified and must be checked with a consenting second player.

## Config
The active file is BepInEx/plugins/Bobbys Mods/config.txt. Reload Config applies edits and disables all features; re-enable the ones you want. Common new settings:

    noclip_hover=true
    noclip_button=right_grip
    noclip_fly_speed=3
    noclip_deadzone=0.18
    noclip_acceleration=8
    noclip_fly_auto_forward=false
    fly_button=right_a
    fly_speed=3
    fly_acceleration=10
    speed_boost_multiplier=1.3
    high_jump_multiplier=1.4
    slide_control=1
    ironman_acceleration=8

Buttons: left_x, left_y, right_a, right_b, left_grip, right_grip, left_trigger, right_trigger, left_stick, right_stick, none. Avoid overlapping bindings for unrelated features.

HUD FPS/status/timer are attached to the headset view. Configure hud_enabled, hud_x/hud_y (0.5=center), hud_distance, hud_scale, hud_font_size, hud_colour, hud_show_mode, hud_show_active_count and hud_show_freeze. Default x=0.76/y=0.75 places the panel toward the top right. Session Timer counts plugin runtime, not real-world time or FPS.

save_per_lobby=false carries selections between eligible lobbies and game restarts; true uses independent room/mode profiles. Only feature permissions restore, never active frozen poses. Old removed flags are discarded during version migration. Never upload lobby-settings.txt: its room keys are encoded, not anonymized. Disable All persists zero selections.

work_in_all_lobbies=true preserves the existing setting. false limits gameplay mods to private MODDED rooms. No setting guarantees avoiding a ban; test with consenting players.

## Build and verification
Use your legally installed game assemblies with a .NET SDK:

    ./Build.ps1 -GamePath 'YOUR GORILLA TAG FOLDER'
    dotnet run --project tests/FreezeTests.csproj --configuration Release

No game assemblies, third-party mod code or assets are included. Features/category ideas were selected from ii community menu feature labels, then implemented independently. See RESEARCH.md. Automated tests do not prove Unity physics, networking, VR comfort or frame rate. See TESTING.md before describing this beta as stable.

## Troubleshooting
Report version, action and exception from BepInEx/LogOutput.log or Gorilla Tag Player.log. Do not include room profiles or account identifiers. To uninstall, close the game, remove BepInEx/plugins/BobbysMods.dll and optionally its configuration directory.

## Controller-held menu and headset display
The normal menu is now controller-relative: it follows both the left hand position and wrist rotation, with a handle under the panel. It no longer rotates itself to face the headset. menu_grip_x/y/z and menu_grip_pitch/yaw/roll adjust how it sits in your grip; menu_scale controls its size. Search keeps its stationary keyboard placement.

HUD placement now uses viewing angles rather than desktop camera aspect ratio. Defaults are near the upper left: hud_angle_x=-12, hud_angle_y=10, hud_width_degrees=18. These replace the old hud_x/y placement keys. Use negative x for left or positive x for right, with 0 centered. hud_scale still adjusts size. Defaults aim to keep the panel within both eyes' comfortable central view; adjust the angles and width for your headset if needed.

Movement reads the game's controller poller thumbstick values and runs after the game's physics update. Fly now moves forward while right A is held when the stick is neutral (fly_auto_forward=true); set false for stick-only movement. Grip Noclip/Noclip Fly and Fly may operate while the hand menu is open; search pauses them. Iron Man controls and thrust settings are unchanged. Noclip also suppresses local GTPlayer sphere/raycast collision results while active, without changing map geometry or other players. No Finger Movement now hooks the current VRRig.PostTick method rather than the absent LateUpdate method.

94 automated logic checks pass and the release build compiles cleanly. The user reports gameplay testing complete. Both Noclip features remain under maintenance.

## Noclip maintenance status
Grip Noclip now clears local Rigidbody linear and angular momentum each physics step while its grip is held. It does not issue movement requests. Collision remains suspended; room-scale headset/controller tracking remains live. Noclip Fly speed defaults to 7 m/s with acceleration 20 m/s²; edit noclip_fly_speed (0.2–15) and noclip_acceleration (1–50) to customize. Neutral input stops flight immediately. Both Noclip buttons are labeled (matinence); their behavior remains a known issue.
