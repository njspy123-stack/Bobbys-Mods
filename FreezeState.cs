using System;

namespace BobbysMods
{
    // Packed avatar pose only. No timestamps, tags, cosmetics, room flags, or locomotion state.
    internal struct AvatarPose
    {
        internal int HeadRotation, BodyRotation, Rotation, HandPosition;
        internal long Position, LeftHand, RightHand;
        internal short LeftArm, RightArm;
        internal bool UsingNewIK;
    }
    internal sealed class FreezeState
    {
        internal bool Enabled { get; private set; }
        bool captured;
        AvatarPose snapshot;
        internal static bool RoomAllowed(bool inRoom, bool isPrivate, string mode, bool allLobbies = false)
            => inRoom && (allLobbies || (isPrivate && mode != null && mode.IndexOf("MODDED", StringComparison.OrdinalIgnoreCase) >= 0));
        internal void Enable() { Enabled = true; captured = false; }
        internal void Disable() { Enabled = false; captured = false; snapshot = default; }
        internal void Apply(ref AvatarPose pose)
        {
            if (!Enabled) return;
            if (!captured) { snapshot = pose; captured = true; }
            else pose = snapshot;
        }
    }
}

