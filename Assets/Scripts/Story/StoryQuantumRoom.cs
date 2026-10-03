using System.Collections;
using NullSignal.Gameplay;
using NullSignal.Presentation;
using UnityEngine;

namespace NullSignal.Story
{
    public enum StoryRoomKind { Maintenance, AssistedPower, Authentication, Oracle, Amplification, Overshoot, Security, Chhaya, Vault, Prime, Extraction }

    /// <summary>One teaching beat around an existing encounter. All operators remain in QuantumEncounter.</summary>
    public sealed class StoryQuantumRoom : MonoBehaviour
    {
        [SerializeField] private StoryRoomKind kind;
        [SerializeField] private int index;
        [SerializeField] private QuantumEncounter encounter;
        [SerializeField] private DoorController exit;
        [SerializeField] private MainGameDirector director;
        [SerializeField] private Transform spawn;
        [SerializeField] private GameObject hologram;
        [SerializeField] private GameObject candidateVisuals;
        [SerializeField] private Light[] practicals;
        [SerializeField] private QuantumCombatEncounter combat;
        [SerializeField] private FinaleRoomController finale;
        public FinaleRoomController Finale => finale;
        public void ConfigureFinale(FinaleRoomController value) => finale = value;
        public QuantumCombatEncounter Combat => combat;
        public void ConfigureCombat(QuantumCombatEncounter value) => combat = value;
        [SerializeField] private SubtitleCue[] entranceLines, markLines, amplificationLines, successLines, overshootLines;
        private bool current, busy, assisted, sawOvershoot, sawRecovery, demoFinished;
        private int lastIteration;
        private bool wasMarked, wasScanned;
        public bool Completed { get; private set; }
        public QuantumEncounter Encounter => encounter;
        public StoryRoomKind Kind => kind;
        public Vector3 Spawn => spawn.position;
        public Vector3 Center => transform.position;
        public bool Busy => busy;
        public DoorController Exit => exit;
        public void ConfigureVisuals(GameObject visuals) => candidateVisuals = visuals;
        public void RevealCandidates() { if (candidateVisuals != null) candidateVisuals.SetActive(true); }

        public void Configure(int roomIndex, StoryRoomKind roomKind, QuantumEncounter search, DoorController door,
            MainGameDirector owner, Transform checkpoint, GameObject recording, Light[] lights,
            SubtitleCue[] enter, SubtitleCue[] mark, SubtitleCue[] amplify, SubtitleCue[] success, SubtitleCue[] overshoot)
        {
            index = roomIndex; kind = roomKind; encounter = search; exit = door; director = owner; spawn = checkpoint;
            hologram = recording; practicals = lights; entranceLines = enter; markLines = mark;
            amplificationLines = amplify; successLines = success; overshootLines = overshoot;
        }
        private void OnEnable() { if (encounter != null) encounter.Changed += OnEncounterChanged; }
        private void OnDisable() { if (encounter != null) encounter.Changed -= OnEncounterChanged; }
        public void Enter()
        {
            current = true;
            if (finale != null) { finale.Enter(); return; }
            if (kind == StoryRoomKind.AssistedPower && !Completed)
            {
                StationPowerState power = GetComponentInChildren<StationPowerState>();
                if (power != null) power.SetPowered(false);
            }
            if (candidateVisuals != null && kind == StoryRoomKind.AssistedPower)
                candidateVisuals.SetActive((director.Anvesh.Unlocked & AnveshAbility.Scan) != 0);
            if (hologram != null) hologram.SetActive(true);
            director.Subtitles.Play("enter-" + index, entranceLines);
            if (combat != null) { combat.Enter(); return; }
            if (kind == StoryRoomKind.Authentication) director.Unlock(AnveshAbility.Mark);
            ResetEncounter();
        }
        public void Leave() { current = false; StopAllCoroutines(); busy = false; if (finale != null) finale.Leave(); if (combat != null) combat.Leave(); }
        public void ResetEncounter()
        {
            director.Anvesh.Operations?.Cancel();
            if (finale != null) { finale.ResetRoom(); return; }
            if (combat != null) { combat.Restart(); return; }
            StopAllCoroutines(); busy = false; assisted = false; wasMarked = false; wasScanned = false; lastIteration = 0;
            // Restart amplitudes, not an observation the player has already made.
            if (encounter != null) encounter.RestartEncounter();
            director.Anvesh.SetSuppressed(false);
            UpdateLockPermission();
            if (!Completed) SetObjective();
            if (kind == StoryRoomKind.Oracle && !demoFinished) StartCoroutine(OracleDemonstration());
        }
        public void Finish()
        {
            // Opening is idempotent; completed rooms must never regain a barrier.
            if (exit != null) exit.Open();
            if (Completed) return;
            Completed = true;
            if (kind == StoryRoomKind.AssistedPower)
            {
                foreach (Light light in practicals) if (light != null) light.intensity = 3f;
                StationPowerState power = GetComponentInChildren<StationPowerState>();
                if (power != null) power.SetPowered(true);
            }
            director.Subtitles.Play("success-" + index, successLines);
            director.CompletedRoom(index);
        }
        private void OnEncounterChanged()
        {
            if (!current || encounter == null || busy || combat != null || finale != null) return;
            if (kind == StoryRoomKind.AssistedPower && encounter.HasScanned && !assisted)
            {
                assisted = true; StartCoroutine(AssistedSearch()); return;
            }
            bool marked = encounter.State == QuantumEncounterState.Marked;
            if (marked && !wasMarked)
            {
                StartCoroutine(ExplainAfterChange("mark-" + index, markLines));
                if (kind == StoryRoomKind.Authentication) director.Unlock(AnveshAbility.Amplify);
                if (kind == StoryRoomKind.Oracle && demoFinished) Finish();
            }
            if (encounter.IterationCount > lastIteration)
            {
                if (kind == StoryRoomKind.Authentication) director.Unlock(AnveshAbility.Lock);
                if (!encounter.LastAmplificationOvershot) StartCoroutine(ExplainAfterChange("amplify-" + index, amplificationLines));
                if (encounter.LastAmplificationOvershot)
                {
                    sawOvershoot = true;
                    StartCoroutine(ExplainAfterChange("overshoot-" + index, overshootLines));
                }
                else if (sawOvershoot) sawRecovery = true;
            }
            wasMarked = marked; wasScanned = encounter.HasScanned; lastIteration = encounter.IterationCount;
            UpdateLockPermission();
            ReconcileCompletion();
            if (!Completed) SetObjective();
        }
        public void ReconcileCompletion()
        {
            if (Completed) { if (exit != null) exit.Open(); return; }
            if (!current || busy || combat != null || finale != null || encounter == null || encounter.MeasurementSucceeded != true) return;
            bool learned = kind != StoryRoomKind.Amplification || encounter.IterationCount >= 1;
            if (kind == StoryRoomKind.Overshoot) learned = sawOvershoot && encounter.IterationCount >= 1;
            if (learned) Finish();
        }
        private void UpdateLockPermission()
        {
            bool allow = kind != StoryRoomKind.Overshoot || sawOvershoot && encounter != null && encounter.IterationCount >= 1;
            if (kind == StoryRoomKind.Amplification) allow = encounter != null && encounter.IterationCount >= 1;
            director.Anvesh.PermitLock(allow);
        }
        private IEnumerator AssistedSearch()
        {
            busy = true; director.Anvesh.SetSuppressed(true);
            director.Objectives.Set("RESTORE LOCAL POWER", "ANVESH is assisting the first search.");
            yield return new WaitForSeconds(1.3f);
            encounter.Mark();
            yield return new WaitForSeconds(2f);
            encounter.Amplify();
            yield return new WaitForSeconds(2f);
            encounter.Lock(); // N=4, one real Grover iteration; the original weighted sampler is used.
            yield return new WaitForSeconds(.8f);
            busy = false; director.Anvesh.SetSuppressed(false);
            if (encounter.MeasurementSucceeded == true) Finish(); else SetObjective();
        }
        private IEnumerator OracleDemonstration()
        {
            busy = true; director.Anvesh.SetSuppressed(true);
            director.Objectives.Set("OBSERVE THE ORACLE", "Watch the wave direction. You can keep moving.");
            encounter.Scan();
            yield return new WaitForSeconds(1.5f);
            encounter.Mark();
            yield return new WaitForSeconds(2f);
            demoFinished = true; busy = false;
            ResetEncounter();
        }
        private IEnumerator ExplainAfterChange(string trigger, SubtitleCue[] lines)
        {
            if (lines == null || lines.Length == 0) yield break;
            int version = encounter.RunVersion;
            yield return new WaitForSeconds(.5f);
            if (current && encounter.RunVersion == version) director.Subtitles.Play(trigger, lines);
        }
        public void SetObjective()
        {
            string title, hint;
            if (kind == StoryRoomKind.Maintenance)
            { title = "ESCAPE THE MAINTENANCE SECTOR"; hint = "Find the lit manual-release console. E to interact."; }
            else if (kind == StoryRoomKind.AssistedPower)
            {
                bool found = (director.Anvesh.Unlocked & AnveshAbility.Scan) != 0;
                title = found ? "RESTORE LOCAL POWER" : "FIND THE CYAN SIGNAL";
                hint = found ? "Q / SCAN the four damaged power modules." : "Approach the glowing ANVESH device. Press E.";
            }
            else if (kind == StoryRoomKind.Oracle)
            { title = "MARK THE RESEARCH SIGNATURE"; hint = "Q / SCAN, then 1 / MARK. Watch direction, not brightness."; }
            else if (kind == StoryRoomKind.Authentication)
            { title = "OPEN THE BULKHEAD"; hint = NextQuantumHint(); }
            else if (kind == StoryRoomKind.Amplification)
            { title = "CALIBRATE EIGHT CANDIDATE STATES"; hint = NextQuantumHint(); }
            else
            {
                title = "STABILIZE REACTOR CONTROL";
                hint = !sawOvershoot ? encounter.IterationCount >= 2 ? "One extra cycle: 1 / MARK then 2 / AMPLIFY. Observe overshoot to unlock LOCK." : "Repeat 1 / MARK then 2 / AMPLIFY. Watch the signal reach its peak."
                    : encounter.IterationCount == 0 ? "Overshoot recorded. MARK / AMPLIFY twice, then 3 / LOCK to stabilize."
                    : encounter.LastAmplificationOvershot ? "Overshoot recorded. R resets amplitudes; SCAN, amplify twice, then LOCK."
                    : sawRecovery ? "Signal recovered. 3 / LOCK the target to open the eastern exit."
                    : "3 / LOCK the target to open the eastern exit. One more amplification is optional.";
                if (!encounter.HasScanned) hint = "Q / SCAN eight reactor-control possibilities.";
                else if (encounter.State == QuantumEncounterState.Marked) hint = "2 / AMPLIFY the pending phase mark. Then compare signal strength.";
            }
            if (encounter != null && encounter.HasMeasured && !Completed) hint = kind == StoryRoomKind.Overshoot
                ? "Another state survived. R resets the search; the overshoot lesson is retained."
                : "A different state survived. R / RESET this encounter and retry.";
            director.Objectives.Set(title, hint);
        }
        private string NextQuantumHint()
        {
            if (!wasScanned) return "Q / SCAN to see the possible states.";
            if (encounter.State == QuantumEncounterState.Marked) return "Try 2 / AMPLIFY. Watch the wave heights and chances.";
            return encounter.IterationCount == 0 ? "Try 1 / MARK. Watch which wave changes direction."
                : "3 / LOCK keeps one state. Or try 1 then 2 again and compare.";
        }
    }
}
