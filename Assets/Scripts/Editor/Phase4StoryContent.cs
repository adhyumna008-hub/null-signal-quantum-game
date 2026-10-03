using NullSignal.Presentation;
using NullSignal.Story;

namespace NullSignal.Editor
{
    internal static class Phase4StoryContent
    {
        internal static SubtitleCue[] Entrance(StoryRoomKind kind)
        {
            switch (kind)
            {
                case StoryRoomKind.Maintenance: return new[] {
                    Cue("TARA", "Emergency impact detected.", 2.5f),
                    Cue("TARA", "Anirudh? If you're conscious, move.\nThat compartment is losing pressure.", 5f),
                    Cue("TARA", "Manual release still has power.", 3f) };
                case StoryRoomKind.AssistedPower: return new[] {
                    Cue("TARA", "Local power is failing. Follow that cyan signal.", 3.5f) };
                case StoryRoomKind.Authentication: return new[] {
                    Cue("ANVESH", "Authorization fragmented. Four candidate states.", 3.5f),
                    Cue("TARA", "Mark the desired signature. Watch what changes.", 3.5f) };
                case StoryRoomKind.Oracle: return new[] {
                    Cue("ANANYA", "Watch the wave direction. Then try it yourself.", 3f) };
                case StoryRoomKind.Amplification: return new[] {
                    Cue("TARA", "Eight possibilities this time. The same procedure applies.", 4f) };
                case StoryRoomKind.Security: return new[] { Cue("TARA", "Security lockdown. Keep moving, Anirudh.", 2.5f) };
                case StoryRoomKind.Chhaya: return new SubtitleCue[0];
                default: return new[] {
                    Cue("ANVESH", "Reactor control fragmented. Eight candidate states.", 3.5f),
                    Cue("TARA", "Track the signal through several search cycles.", 3.5f) };
            }
        }
        internal static SubtitleCue[] Mark(StoryRoomKind kind)
        {
            return new SubtitleCue[0];
        }
        internal static SubtitleCue[] Amplify(StoryRoomKind kind)
        {
            if (kind == StoryRoomKind.Authentication) return new[] { Cue("TARA", "A larger wave. A higher chance.", 3f) };
            return new SubtitleCue[0];
        }
        internal static SubtitleCue[] Success(StoryRoomKind kind)
        {
            switch (kind)
            {
                case StoryRoomKind.Maintenance: return new[] { Cue("TARA", "Manual release accepted. Keep moving.", 3f) };
                case StoryRoomKind.AssistedPower: return new[] { Cue("ANVESH", "Desired power state measured. Local systems restored.", 4f) };
                case StoryRoomKind.Authentication: return new[] {
                    Cue("ANIRUDH", "So we didn't check all four.", 2.5f), Cue("TARA", "No.", 1.4f), Cue("TARA", "We changed the odds.", 2.5f) };
                case StoryRoomKind.Oracle: return new[] { Cue("ANANYA", "Signature marked. The next lab is open.", 3f) };
                case StoryRoomKind.Amplification: return new[] { Cue("TARA", "Calibration accepted. Reactor control is ahead.", 3.5f) };
                case StoryRoomKind.Security: return new[] { Cue("ANANYA", "Core disabled. That violet interference is coming from the next sector.", 4f) };
                case StoryRoomKind.Chhaya: return new[] { Cue("LUBNA", "The manifestations have dispersed. The station is quiet again... for now.", 4f) };
                default: return new[] { Cue("LUBNA", "You saw the signal rise, fall, and return.\nMore amplification is not always better.", 5f),
                    Cue("TARA", "Reactor stabilized. Hold here, Anirudh.", 3.5f) };
            }
        }
        // ANVESH supplies the single short explanation after the waveform visibly weakens.
        internal static SubtitleCue[] Overshoot(StoryRoomKind kind) => new SubtitleCue[0];
        private static SubtitleCue Cue(string who, string text, float duration) => new SubtitleCue(who, text, duration);
    }
}
