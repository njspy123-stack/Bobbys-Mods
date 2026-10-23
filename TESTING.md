# Test record — 0.5.0 beta

## Completed

- Build against the installed Gorilla Tag/Unity 6 libraries: PASS, zero warnings/errors.
- Automated tests: 54 PASS.
- Covered: room permissions, input gating, freeze-state capture/restore, pointer hit/debounce, layer selection, independent saved profiles and parsing, feature-page mapping, gravity compensation math, FPS sampling math, frozen-body size guards.
- The installed DLL checksum is checked when copied into the game.

## Not covered by the automated checks

Unity shader transparency, actual BakeMesh output, hand collision physics, Rigidbody/game movement interaction, headset frame rate, network-visible avatar poses and headset control mapping. These require real runtime testing; no claim of complete functionality is made.

## Headset checklist before a stable GitHub release

1. Confirm version 0.5.0 loads; open/close menu repeatedly and switch pages. Measure FPS with and without the menu. No repeated canvas toggling should occur.
2. Freeze: arm B, press it, move away and inspect the normal-size frozen copy. Check for skeleton/helper artifacts. Confirm the moving green appearance and B restore. Have a friend separately confirm their network view.
3. Platforms: verify both grips and triggers, visible hands, transparency, solid support and release cleanup. Try with/without Smart Arms. Test narrow environments.
4. Teleport: valid flat ground, invalid steep target, blocked destination, local hand filtering, trigger-platform conflict and stable movement after arrival.
5. Smart Arms: shoulder alignment and no body/world scaling; verify disabling restores reach.
6. Low Gravity: compare falling/jumping to normal. Disable while airborne. Test swimming, climbing, rides and scene changes for unexpected forces.
7. Checkpoint: arm, save with right A, move, return with left Y. Block the destination and verify rejection. Change scene/lobby and verify coordinates clear.
8. FPS Display: enable and verify a plausible changing number without excessive text updates.
9. Profiles: use different selections in two named rooms; revisit both. Checkpoint coordinates and active frozen state must never restore. Test Disable All and Reload Config persistence.
10. Check latest logs for plugin exceptions/crashes after several minutes. A startup-only pass is insufficient.

Status: headset and multiplayer checklist pending. Do not label this as stable or fully tested until results are recorded.

## Startup smoke check
Fresh 0.5.0 startup completed. The game was still running after 73 seconds with no matching plugin exception/crash markers. This check did not exercise feature controls or verify headset rendering.


## 0.6.0 search tests
71 automated checks pass. Added matching/aliases, multiword queries, editing, no-match behavior, bounded input, keyboard hit boxes, letter X versus X Close, and top search-bar touch. In headset: open Search, type GHOST and TP, enable/disable a result, page all results, press BACK/CLEAR/SPACE, press X CLOSE, verify no lingering keyboard/pointer, reopen and confirm normal menu controls. These headset checks are pending.


0.6.0 startup smoke check: fresh log confirms Bobbys Mods 0.6.0 loaded and completed startup; no matching plugin exceptions or crash markers at the follow-up. Keyboard interactions still need headset testing.
