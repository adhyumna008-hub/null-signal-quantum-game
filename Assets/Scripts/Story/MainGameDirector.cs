using NullSignal.Gameplay;
using NullSignal.Player;
using NullSignal.Presentation;
using UnityEngine;

namespace NullSignal.Story
{
    [DefaultExecutionOrder(-50)]
    public sealed class MainGameDirector : MonoBehaviour
    {
        [SerializeField] private StoryQuantumRoom[] rooms;
        [SerializeField] private AnveshController anvesh;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Transform player;
        [SerializeField] private ObjectiveSystem objectives;
        [SerializeField] private SubtitleController subtitles;
        [SerializeField] private CheckpointSystem checkpoints;
        [SerializeField] private StationCamera cameraRig;
        [SerializeField] private StationFeedbackAudio feedback;
        [SerializeField] private bool waitForPlay;
        public bool HasStarted { get; private set; }
        public void ConfigureStartFlow() => waitForPlay = true;
        private int current;
        private bool released, deviceFound;
        public AnveshController Anvesh => anvesh;
        public ObjectiveSystem Objectives => objectives;
        public SubtitleController Subtitles => subtitles;
        public StoryQuantumRoom CurrentRoom => rooms[current];
        public Transform Player => player;
        public float UnlockAt { get; private set; } = -10f;
        public void Configure(StoryQuantumRoom[] sections, AnveshController controller, PlayerHealth vitals, Transform actor,
            ObjectiveSystem goals, SubtitleController dialogue, CheckpointSystem saves, StationCamera camera, StationFeedbackAudio audio)
        { rooms = sections; anvesh = controller; health = vitals; player = actor; objectives = goals; subtitles = dialogue; checkpoints = saves; cameraRig = camera; feedback = audio; }
        private void Start()
        {
            anvesh.SetUnlocked(AnveshAbility.None); anvesh.RestartRequested += ResetCurrent;
            if (!waitForPlay) BeginGameplay();
        }
        public void BeginGameplay()
        {
            if (HasStarted) return;
            HasStarted = true; Enter(0);
        }
        private void OnDestroy() { if (anvesh != null) anvesh.RestartRequested -= ResetCurrent; }
        private void Update()
        {
            if (!HasStarted || Time.timeScale <= 0) return;
            CurrentRoom.ReconcileCompletion();
            bool combat = CurrentRoom.Combat != null && CurrentRoom.Combat.Active;
            cameraRig.FrameCombat(combat);
            cameraRig.FrameEncounter(combat || !CurrentRoom.Completed && CurrentRoom.Encounter != null && CurrentRoom.Encounter.HasScanned);
            if (current >= rooms.Length - 1 || !CurrentRoom.Completed || health.Current <= 0) return;
            // Route is a continuous eastward station spine. Doors physically gate each threshold.
            if ((CurrentRoom.Exit == null || CurrentRoom.Exit.IsOpen) && player.position.x > rooms[current + 1].Center.x - 11.5f) Enter(current + 1);
        }
        private void Enter(int next)
        {
            if (next != current) rooms[current].Leave();
            current = next;
            anvesh.Configure(CurrentRoom.Encounter); anvesh.SetSuppressed(false);
            // Overshoot is introduced later than MARK/AMPLIFY; its first observation still gets one short lesson.
            anvesh.ShowTeaching = next <= (int)StoryRoomKind.Overshoot;
            objectives.Set("", "", "ASTRA-7 / " + CurrentRoom.Kind.ToString().ToUpperInvariant());
            checkpoints.Set(CurrentRoom.Kind.ToString(), CurrentRoom.Spawn);
            cameraRig.SetRoom(CurrentRoom.Center, CurrentRoom.Encounter != null ? CurrentRoom.Encounter.CandidateCount : 0);
            CurrentRoom.Enter();
        }
        public bool CanInteract(StationInteraction interaction)
        {
            if (health.Current <= 0) return false;
            return interaction == StationInteraction.ManualRelease ? current == 0 && !released : current == 1 && !deviceFound;
        }
        public void Interact(StationInteraction interaction)
        {
            if (!CanInteract(interaction)) return;
            feedback.Pulse();
            if (interaction == StationInteraction.ManualRelease) { released = true; CurrentRoom.Finish(); }
            else
            {
                deviceFound = true; Unlock(AnveshAbility.Scan);
                CurrentRoom.RevealCandidates();
                subtitles.Play("anvesh-found", new SubtitleCue("ANVESH", "Quantum search interface restored.", 3f), new SubtitleCue("TARA", "Good. You'll need that.", 2.5f));
                CurrentRoom.SetObjective();
            }
        }
        public void Unlock(AnveshAbility ability)
        {
            if ((anvesh.Unlocked & ability) != 0) return;
            anvesh.SetUnlocked(anvesh.Unlocked | ability); UnlockAt = Time.unscaledTime; feedback.Pulse();
        }
        public void CompletedRoom(int index)
        {
            if (index != current) return;
            string[] titles = { "FIND ANVESH", "OPEN THE BULKHEAD", "REACH THE ORACLE LAB", "ENTER THE AMPLIFICATION LAB", "REACH THE OVERSHOOT REACTOR", "REACTOR STABILIZED", "SECURITY CLEARED / FOLLOW THE DISTORTION", "REACH THE VAULT", "REACH ANVESHAK CORE", "ESCAPE ASTRA-7", "MISSION COMPLETE" };
            objectives.Set(titles[index], index == rooms.Length - 1
                ? (rooms.Length > 8 ? "Extraction confirmed." : rooms.Length > 6 ? "Phase 5 complete. Chhaya's eight-state manifestation is dispersed." : "Phase 4 complete. The route ends here; R reopens the current search.")
                : "EXIT OPEN / Follow the copper floor guides east into " + rooms[index + 1].Kind.ToString().ToUpperInvariant() + ".");
        }
        private void ResetCurrent()
        {
            if (health.Current <= 0) return;
            CurrentRoom.ResetEncounter();
            if (CurrentRoom.Completed) CompletedRoom(current);
        }
        public void RestoreCheckpoint()
        {
            subtitles.Clear(); CurrentRoom.ResetEncounter();
            cameraRig.SnapToPlayer();
            subtitles.Play(null, new SubtitleCue("TARA", "Checkpoint restored. You're still in the same sector.", 3f));
        }
        public void BeginRespawn() { anvesh.Operations?.Cancel(); anvesh.SetSuppressed(true); }
        public void RestartCheckpoint()
        {
            if (HasStarted) checkpoints.Restart();
        }
    }
}
