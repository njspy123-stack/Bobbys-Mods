# Bobbys Mods 0.7.0 test record

- Release build: PASS, zero warnings or errors.
- Automated logic checks: 94 PASS (room rules, selections and migration, freeze state, touch input, search, keyboard, categories, navigation and packed finger data).
- Gameplay testing: user reports testing complete.
- Known issues: Grip Noclip (matinence) and Noclip Fly (matinence) remain under repair. Labels do not mean these features are disabled or fixed.
- Startup smoke check on the preceding development build completed without matching plugin stack traces or Harmony/InvalidProgram exceptions. This is narrower than automated end-to-end VR validation.

## Regression checklist for future fixes

Check Noclip momentum, flight speed, collision/gravity restoration, room changes, both eyes' HUD placement, controller-held menu orientation, both keyboard pointers, category arrows, finger lock and held items, movement modifiers, platforms, teleport and Iron Man. Confirm remote avatar appearance with a consenting second player and inspect game logs.

The public release is 0.7.0. Earlier local 0.8.x development labels were superseded by this requested public version number; they are not newer published releases. Passing logic checks does not guarantee every physics or multiplayer interaction.
