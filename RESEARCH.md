# Research and attribution

Research date: 22 October 2026.

## Feature discovery

Original source: KyleTheScientist/Bark — https://github.com/KyleTheScientist/Bark#features

Its README documents Low Gravity and Checkpoint as modules. Its release history records continued changes to those concepts: https://github.com/KyleTheScientist/Bark/releases

Bark is archived and its license is GPL-3.0. This project used feature descriptions as inspiration only. No source code, models, textures, audio, compiled DLLs or other assets were copied from Bark. The implementations and controls here differ and do not imply current Bark compatibility or endorsement.

## Rendering reference

Unity 6 SkinnedMeshRenderer.BakeMesh API: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SkinnedMeshRenderer.BakeMesh.html

Used to check renderer-local baking and the useScale parameter when revising the frozen-avatar snapshot. The main game skin is baked locally at runtime; game meshes are not distributed in the source export.

## Retrieval limits

Firecrawl could not run because the connected account had no credits. Research continued through web search and the original GitHub/Unity pages. This is a focused review, not an exhaustive survey of Gorilla Tag mods.

## Ghost ideas and search UI
Additional primary repositories reviewed: https://github.com/developer9998/GorillaInvisibility and https://github.com/NachoEngine/GorillaTagGhostEntity . See GHOST-IDEAS.md for proposals and visibility limits. No code/assets/DLLs were copied from these projects.


## 0.7.0 ideas
Reviewed feature labels only in https://github.com/SilverXdamodder/ii-s-Stupid-Reborn/blob/main/Menu/Buttons.cs (a community fork; the former upstream URL returned 404). Labels included Ghost, Non-Togglable Ghost, Menu Trail and Orbit Projectiles. Bobbys implements original hold-to-ghost, local hand ribbons and local primitive wisps; no third-party code/assets or projectile/network behavior was copied.

0.7.1: original Menu Ghost and Pulse Ghost based on previously reviewed Freeze Rig in Menu and Ghost Animations labels in the community ii menu fork. Implemented through Bobbys existing freeze handler, with no third-party code/assets.

0.8.0: reviewed feature LABELS in https://github.com/SilverXdamodder/ii-s-Stupid-Reborn/blob/main/Menu/Buttons.cs for movement/menu inspiration (fly-speed settings, speed-boost settings, hand/wrist/menu organization). Implemented original categorized navigation, movement modifiers and finger lock against local game metadata. No ii method bodies or assets copied. The original upstream URL was previously unavailable; this is a community fork, not a claim of official upstream provenance.
