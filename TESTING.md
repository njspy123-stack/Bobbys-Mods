# 0.8.0 beta test record

Build: pending final run. Automated logic checks: 94 PASS before final hover/support guard changes. Headset and multiplayer checks pending.

- Category home, all five categories, next/previous wrapping, empty slots, category return, Disable All, reload and search across all 18 features.
- Both keyboard pointers, simultaneous typing, result activation and Close handling. Measure VR FPS with menu/HUD open and closed.
- Grip Noclip must not request translation. Test hover on/off, gravity/velocity, releasing outside and inside geometry, last-clear return, body scaling, map/lobby changes, menu pause and disabling.
- Noclip Fly: grip and stick, pitch/strafe, neutral stop, up/down and conflicts with platforms/Iron Man.
- No Finger Movement: local and remote bends remain fixed; held items still update; head/hand tracking and gameplay inputs remain active. Disable restores normal animation.
- Fly: normal wall collisions; speed/acceleration/up/down. Speed Boost/High Jump/Slide Control: capture/restore original values, tag-state changes, maps, disabling/restarting. Zero Gravity and Iron Man: local forces and cleanup, water/climbing/vehicles, no global gravity changes.
- Removed ghost modes absent from search/menu and old flags cannot enable new mods. Original Freeze B only; no automatic freeze caused by opening menu.
- Regression: platforms, teleport, Smart Arms, HUD/timer, lobby carryover and log exceptions.

A successful compile and pure logic checks are not headset or multiplayer validation. GitHub remains on 0.6.0.

Final build: PASS, zero warnings/errors. Final automated checks: 94 PASS. Runtime checks remain pending.

0.8.1: verify native FixedUpdate postfix and VRRig.PostTick patches load; controller-held panel orientation/handle; comfortable two-eye HUD position; Fly A with neutral stick and stick directions; Noclip through wall then collision restoration; confirm Iron Man behavior unchanged. Runtime checks pending.

0.8.1 startup smoke: fresh BepInEx log loads 0.8.1 and completes startup; game running at follow-up. No matching BobbysMods stack traces, HarmonyException or InvalidProgramException in Player.log at this check. This does not verify feature controls or headset rendering.

0.8.2: clean build; installed with game closed and matching DLL checksum. Grip momentum reset and 7 m/s Noclip Fly require in-game verification.

0.7.0 release: user reports testing complete. Both Noclip features remain known issues and are labeled (matinence). No claim is made that Noclip is repaired. Release version is 0.7.0 at the user's request, superseding local 0.8.x labels.
