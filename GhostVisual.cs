using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BobbysMods
{
    // Local render-only override: no rig state, colour properties or network messages are changed.
    internal sealed class GhostVisual : IDisposable
    {
        sealed class Saved
        {
            internal Renderer Renderer;
            internal Material[] Originals, Ghosts;
            internal MaterialPropertyBlock Properties;
        }
        readonly List<Saved> saved = new List<Saved>();
        Material ghost;
        internal void Show(Component rig, Color colour)
        {
            Dispose();
            if (!rig) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("GUI/Text Shader");
            if (!shader) return;
            ghost = new Material(shader) { name = "Bobbys local green ghost", renderQueue = (int)RenderQueue.Transparent };
            ghost.SetColor("_BaseColor", colour); ghost.SetColor("_Color", colour);
            if (ghost.HasProperty("_Surface")) ghost.SetFloat("_Surface", 1);
            if (ghost.HasProperty("_Blend")) ghost.SetFloat("_Blend", 0);
            if (ghost.HasProperty("_SrcBlend")) ghost.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (ghost.HasProperty("_DstBlend")) ghost.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (ghost.HasProperty("_ZWrite")) ghost.SetFloat("_ZWrite", 0);
            ghost.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            ghost.SetOverrideTag("RenderType", "Transparent");
            foreach (var skin in rig.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (!skin || !skin.sharedMesh) continue;
                var state = new Saved { Renderer = skin, Originals = skin.sharedMaterials, Properties = new MaterialPropertyBlock() };
                skin.GetPropertyBlock(state.Properties);
                state.Ghosts = new Material[state.Originals.Length];
                for (int i = 0; i < state.Ghosts.Length; i++) state.Ghosts[i] = ghost;
                skin.sharedMaterials = state.Ghosts;
                var tint = new MaterialPropertyBlock(); tint.SetColor("_BaseColor", colour); tint.SetColor("_Color", colour);
                skin.SetPropertyBlock(tint);
                saved.Add(state);
            }
        }
        internal void Maintain()
        {
            // The game can replace the skin material on a tag/state change. No allocations per frame.
            if (!ghost) return;
            foreach (var state in saved)
                if (state.Renderer && state.Renderer.sharedMaterial != ghost) state.Renderer.sharedMaterials = state.Ghosts;
        }
        public void Dispose()
        {
            foreach (var state in saved)
                if (state.Renderer) { state.Renderer.sharedMaterials = state.Originals; state.Renderer.SetPropertyBlock(state.Properties); }
            saved.Clear();
            if (ghost) UnityEngine.Object.Destroy(ghost);
            ghost = null;
        }
    }
}
