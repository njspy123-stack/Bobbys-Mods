# Ghost-themed ideas — researched 22 October 2026

These are ideas for later features, not additional installed mods in version 0.7.0. No third-party DLLs or assets were downloaded.

| Idea | Reference | Visibility / implementation notes |
| --- | --- | --- |
| Ghost double or brief afterimage | https://github.com/NachoEngine/GorillaTagGhostEntity | The original describes a client-side ghost entity. A Bobbys version could fade local snapshots. Other players would need an explicitly shared implementation to see that effect. |
| Ghost-view / fade toggle | https://github.com/developer9998/GorillaInvisibility | Its README explicitly describes invisibility from the player's own point of view, not invisibility to other players. Could inspire an alternate local ghost appearance. |
| Floating ghost wisps | https://github.com/KyleTheScientist/Bark/releases | Bark documents a Fireflies feature. A separate fresh implementation could use green local particles as a ghost theme. Do not assume the old plugin works with the current game. |
| Temporary spectral trail | Original proposal inspired by ghost doubles/wisps | A short pooled local trail could show your recent movement without affecting other players. Avoid baking a mesh every frame. |
| Friendly reveal button | Original proposal | Restore normal appearance, stop ghost effects, and show a local “It was a joke” card. Sharing a reveal with others requires a deliberate supported networking design; the current menu does not broadcast that message. |

Original source summaries are not compatibility or ban-safety guarantees. Local-only effects cannot be claimed to scare or appear to other players. Bobbys Mods' existing outgoing-pose freeze is a separate, still-unverified multiplayer behavior.

The search feature searches the eight currently implemented Bobbys Mods features. It does not search the web, download mods or activate the unimplemented ideas listed here.
